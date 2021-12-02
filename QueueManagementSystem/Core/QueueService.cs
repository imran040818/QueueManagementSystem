using QueueManagementSystem.Events;
using QueueManagementSystem.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QueueManagementSystem.Core
{
    public class QueueService<T1, T2> : IQueueService<T1, T2>
    {
        readonly ConcurrentDictionary<T2, TaskInformation<T1, T2>> queueDictionary;
        readonly ConcurrentDictionary<T2, ProgressTask<TaskInformation<T1, T2>>> progressQueueDictionary;
        CancellationTokenSource globalCancellationTokenSource;
        bool startQueue;
        readonly object asyncLock = new object();
        public QueueService()
        {
            queueDictionary = new ConcurrentDictionary<T2, TaskInformation<T1, T2>>();
            progressQueueDictionary = new ConcurrentDictionary<T2, ProgressTask<TaskInformation<T1, T2>>>();
        }

        public event TaskCancelledEventHandler<T1> TaskCancelled;
        public event TaskFaultedEventHandler<T1> TaskFaulted;
        public event TaskCompleteEventHandler<T1> TaskComplete;
        public event TaskAllCancelledEventHandler<List<T1>> TaskAllCancel;
        public event ReadyToEnqueueEventHandler ReadyToEnqueue;
        public event TaskPausedEventHandler<T1> TaskPausedEvent;
        public event TaskAllPausedEventHandler<List<T1>> TaskAllPausedEvent;
        public QueueInformation QueueInformation { get; set; }

        ///<inheritdoc/>
        public async Task Enqueue(TaskInformation<T1, T2> taskInformation)
        {
            await Task.Run(() =>
            {
                CancellationTokenSource itemCancellationTokenSource = new CancellationTokenSource();
                CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(globalCancellationTokenSource.Token, itemCancellationTokenSource.Token);
                taskInformation.CancellationToken = linkedCancellationTokenSource;
                if (queueDictionary.TryGetValue(taskInformation.TaskId, out TaskInformation<T1, T2> _))
                {
                    throw new ArgumentException($"{taskInformation.TaskId} key is already present");
                }
                if (!queueDictionary.TryGetValue(taskInformation.TaskId, out TaskInformation<T1, T2> _))
                {
                    queueDictionary.TryAdd(taskInformation.TaskId, taskInformation);
                }
            });
        }

        ///<inheritdoc/>
        public async Task StartQueue()
        {
            // Start the task checker. it will check wheather the task is complete or not.
            startQueue = true;
            while (startQueue)
            {
                if (progressQueueDictionary.Count < QueueInformation.PreferedParallelizationFactor)
                {
                    lock (asyncLock)
                    {
                        var dequeueItem = Dequeue().GetAwaiter().GetResult();
                        if (dequeueItem != null)
                        {
                            Task task = new Task(() =>
                           {
                               try
                               {
                                   dequeueItem.RunTaskHandler(dequeueItem.Parameter, dequeueItem.CancellationToken.Token).Wait();
                                   ExecuteNormalTaskCompleteFlow(dequeueItem);
                               }
                               catch (Exception exception)
                               {
                                   ExecuteExceptionFlow(dequeueItem, exception);
                               }
                               finally
                               {
                                   if (progressQueueDictionary.ContainsKey(dequeueItem.TaskId))
                                   {
                                       progressQueueDictionary[dequeueItem.TaskId].TaskInformation.CancellationToken?.Dispose();
                                       progressQueueDictionary.TryRemove(dequeueItem.TaskId, out ProgressTask<TaskInformation<T1, T2>> _);
                                   }
                               }
                           });
                            progressQueueDictionary.TryAdd(dequeueItem.TaskId, new ProgressTask<TaskInformation<T1, T2>>
                            {
                                Task = task,
                                TaskInformation = dequeueItem
                            });
                            task.Start();
                        }
                    }
                    ReadyToEnqueue?.Invoke();
                }
                await Task.Delay(1000);
            }
        }

        ///<inheritdoc/>
        public async Task StopQueue()
        {
            startQueue = false;
            await Cancel();
        }

        ///<inheritdoc/>
        public async Task Cancel(T2 taskId)
        {
            await Task.Run(() =>
            {
                lock (asyncLock)
                {
                    if (queueDictionary.TryGetValue(taskId, out TaskInformation<T1, T2> value))
                    {
                        value.CancellationToken?.Cancel();
                        value.CancellationToken?.Dispose();
                        queueDictionary.TryRemove(taskId, out TaskInformation<T1, T2> _);
                        TaskCancelled?.Invoke(value.Parameter);
                    }
                    if (progressQueueDictionary.TryGetValue(taskId, out ProgressTask<TaskInformation<T1, T2>> progressValue)
                    && !progressValue.TaskInformation.CancellationToken.IsCancellationRequested)
                    {
                        progressValue.TaskInformation.CancellationToken?.Cancel();
                    }
                }
            });
        }

        ///<inheritdoc/>
        public async Task Cancel()
        {
            await Task.Run(() =>
            {
                lock (asyncLock)
                {
                    queueDictionary?.Select(s => s.Value?.CancellationToken)?.ToList()?.ForEach(f => f?.Dispose());
                    var queueTaskParameter = queueDictionary.Select(s => s.Value.Parameter)
                      .ToList();
                    var progressTaskParameter = progressQueueDictionary.Select(s => s.Value.TaskInformation.Parameter)
                    .ToList();
                    List<T1> parameters = new List<T1>();

                    if (progressTaskParameter != null && progressTaskParameter.Count > 0)
                        parameters.AddRange(progressTaskParameter);
                    if (queueTaskParameter != null && queueTaskParameter.Count > 0)
                        parameters.AddRange(queueTaskParameter);

                    globalCancellationTokenSource?.Cancel();
                    TaskAllCancel?.Invoke(parameters);
                    queueDictionary.Clear();
                }
            });
        }

        ///<inheritdoc/>
        public async Task CancelLastRunningOperation()
        {
            await Task.Run(() =>
            {
                lock (asyncLock)
                {
                    T2 key = progressQueueDictionary.Keys.Last();
                    if (key != null && progressQueueDictionary.TryGetValue(key, out ProgressTask<TaskInformation<T1, T2>> progressValue))
                    {
                        progressValue.TaskInformation.CancellationToken?.Cancel();
                    }
                }
            });
        }

        ///<inheritdoc/>
        public async Task Pause(T2 taskId)
        {
            await Task.Run(() =>
            {
                lock (asyncLock)
                {
                    if (queueDictionary.TryGetValue(taskId, out TaskInformation<T1, T2> value))
                    {
                        value.CancellationToken?.Cancel();
                        value.CancellationToken?.Dispose();
                        queueDictionary.TryRemove(taskId, out TaskInformation<T1, T2> _);
                        TaskPausedEvent?.Invoke(value.Parameter);
                    }
                    if (progressQueueDictionary.TryGetValue(taskId, out ProgressTask<TaskInformation<T1, T2>> progressValue)
                    && !progressValue.TaskInformation.CancellationToken.IsCancellationRequested)
                    {
                        progressValue.TaskInformation.IsPaused = true;
                        progressValue.TaskInformation.CancellationToken?.Cancel();
                    }
                }
            });
        }

        ///<inheritdoc/>
        public async Task Pause()
        {
            await Task.Run(() =>
            {
                lock (asyncLock)
                {
                    queueDictionary?.Select(s => s.Value?.CancellationToken)?.ToList()?.ForEach(f => f?.Dispose());
                    var queueTaskParameter = queueDictionary.Select(s => s.Value.Parameter)
                      .ToList();
                    var progressTaskParameter = progressQueueDictionary.Select(s => s.Value.TaskInformation.Parameter)
                    .ToList();
                    List<T1> parameters = new List<T1>();

                    if (progressTaskParameter != null && progressTaskParameter.Count > 0)
                        parameters.AddRange(progressTaskParameter);
                    if (queueTaskParameter != null && queueTaskParameter.Count > 0)
                        parameters.AddRange(queueTaskParameter);
                    progressQueueDictionary.Select(s => s.Value.TaskInformation)
                   .ToList().ForEach(f => f.IsPaused = true);
                    globalCancellationTokenSource?.Cancel();
                    TaskAllPausedEvent?.Invoke(parameters);
                    queueDictionary.Clear();
                }
            });
        }

        ///<inheritdoc/>
        public void PauseQueue()
        {
            startQueue = false;
        }

        ///<inheritdoc/>
        public async Task PauseLastRunningOperation()
        {
            await Task.Run(() =>
            {
                lock (asyncLock)
                {
                    T2 key = progressQueueDictionary.Keys.Last();
                    if (key != null && progressQueueDictionary.TryGetValue(key, out ProgressTask<TaskInformation<T1, T2>> progressValue))
                    {
                        progressValue.TaskInformation.IsPaused = true;
                        progressValue.TaskInformation.CancellationToken?.Cancel();
                    }
                }
            });
        }

        ///<inheritdoc/>
        public void Configure(QueueInformation queueInformation)
        {
            if (progressQueueDictionary.Count > 0 || queueDictionary.Count > 0)
            {
                Cancel().Wait();
            }
            QueueInformation = queueInformation;
            if (QueueInformation.PreferedParallelizationFactor == 0)
            {
                QueueInformation.PreferedParallelizationFactor = 2;
            }
            if (QueueInformation.PreferedParallelizationFactor > QueueInformation.MaxParallelizationFactor)
            {
                QueueInformation.PreferedParallelizationFactor = QueueInformation.MaxParallelizationFactor;
            }
            globalCancellationTokenSource = new CancellationTokenSource();
        }

        private async Task<TaskInformation<T1, T2>> Dequeue()
        {
            TaskInformation<T1, T2> data = default;
            await Task.Run(() =>
            {
                T2 key = queueDictionary.Keys.FirstOrDefault() ?? default;
                if (key != null && queueDictionary.TryGetValue(key, out data))
                {
                    queueDictionary.Remove(key, out TaskInformation<T1, T2> _);
                }
            });
            return data;
        }

        private void ExecuteExceptionFlow(TaskInformation<T1, T2> dequeueItem, Exception exception)
        {
            if (exception.InnerException is OperationCanceledException operationCanceledException &&
            progressQueueDictionary.TryGetValue(dequeueItem.TaskId, out ProgressTask<TaskInformation<T1, T2>> data))
            {
                if (!globalCancellationTokenSource.IsCancellationRequested)
                {
                    if (dequeueItem.IsPaused)
                    {
                        TaskPausedEvent?.Invoke(dequeueItem.Parameter);
                    }
                    else
                    {
                        progressQueueDictionary[dequeueItem.TaskId].TaskInformation.CancellationToken?.Dispose();
                        TaskCancelled?.Invoke(dequeueItem.Parameter);
                    }
                }
            }
            else
            {
                TaskFaulted?.Invoke(dequeueItem.Parameter);
            }
        }

        private void ExecuteNormalTaskCompleteFlow(TaskInformation<T1, T2> dequeueItem)
        {
            if (!globalCancellationTokenSource.IsCancellationRequested)
            {
                if (dequeueItem.CancellationToken.IsCancellationRequested)
                {
                    if (progressQueueDictionary.TryGetValue(dequeueItem.TaskId, out ProgressTask<TaskInformation<T1, T2>> data))
                    {
                        if (dequeueItem.IsPaused)
                        {
                            TaskPausedEvent?.Invoke(dequeueItem.Parameter);
                        }
                        else
                        {
                            progressQueueDictionary[dequeueItem.TaskId].TaskInformation.CancellationToken?.Dispose();
                            TaskCancelled?.Invoke(dequeueItem.Parameter);
                        }
                    }
                }
                else
                {
                    if (progressQueueDictionary.TryGetValue(dequeueItem.TaskId, out ProgressTask<TaskInformation<T1, T2>> data))
                    {
                        progressQueueDictionary[dequeueItem.TaskId].TaskInformation.CancellationToken?.Dispose();
                        TaskComplete?.Invoke(data.TaskInformation.Parameter);
                    }
                }
            }
        }
    }
}
