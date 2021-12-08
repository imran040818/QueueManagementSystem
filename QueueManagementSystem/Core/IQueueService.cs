using QueueManagementSystem.Events;
using QueueManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QueueManagementSystem.Core
{
    /// <summary>
    /// Class contains all the properties, operations and events related to queue
    /// </summary>
    /// <typeparam name="T1">Generic Parameter, which is used in <see cref="TaskInformation{T1, T2}"/></typeparam>
    /// <typeparam name="T2">Generic task id</typeparam>
    public interface IQueueService<T1, T2>
    {
        /// <summary>
        /// Events will get fired, when operation get cancelled
        /// </summary>
        event TaskCancelledEventHandler<T1> TaskCancelled;

        /// <summary>
        /// Events will get fired, when exception is thrown
        /// </summary>
        event TaskFaultedEventHandler<T1> TaskFaulted;

        /// <summary>
        /// Event will get fired, when operation is complete
        /// </summary>
        event TaskCompleteEventHandler<T1> TaskComplete;

        /// <summary>
        /// Event will be fired, when all the operation is cancelled
        /// </summary>
        event TaskAllCancelledEventHandler<List<T1>> TaskAllCancel;

        /// <summary>
        /// Event will be fired, when queue is ready
        /// </summary>
        event ReadyToEnqueueEventHandler ReadyToEnqueue;

        /// <summary>
        /// Event will be fired, when opeartion is paused
        /// </summary>
        event TaskPausedEventHandler<T1> TaskPausedEvent;

        /// <summary>
        /// Event will be fired, when all operation are paused
        /// </summary>
        event TaskAllPausedEventHandler<List<T1>> TaskAllPausedEvent;

        /// <summary>
        /// Cancel all the operation
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task Cancel();

        /// <summary>
        /// Cancel an operation with matching taskId which is in progress or added to the queue
        /// </summary>
        /// <param name="taskId"><see cref="T2"/></param>
        /// <returns><see cref="Task"/></returns>
        Task Cancel(T2 taskId);

        /// <summary>
        /// Cancel the last running operation (Operation which has ran for very short interval as compared to other operation)
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task CancelLastRunningOperation();

        /// <summary>
        /// Configure the queue operation.
        /// </summary>
        /// <param name="queueInformation"><see cref="QueueInformation"></param>
        void Configure(QueueInformation queueInformation);

        /// <summary>
        /// Enqueue an item
        /// </summary>
        /// <param name="taskInformation"><see cref="TaskInformation<T1, T2>"></param>
        /// <returns><see cref="Task"/></returns>
        Task Enqueue(TaskInformation<T1, T2> taskInformation);

        /// <summary>
        /// Pause all the operation
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task Pause();

        /// <summary>
        /// Pause an operation with matching taskId which is in progress or added to the queue
        /// </summary>
        /// <param name="taskId"><see cref="T2"/></param>
        /// <returns><see cref="Task"/></returns>
        Task Pause(T2 taskId);

        /// <summary>
        /// Cancel the last running operation (Operation which has ran for very short interval as compared to other operation)
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task PauseLastRunningOperation();

        /// <summary>
        /// Start the queue service
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task StartQueue();

        /// <summary>
        /// Stop the queue service
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task StopQueue();

        /// <summary>
        /// Pause the queue service
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        void PauseQueue();
        Task<TaskInformation<T1, T2>> Dequeue();
        Task SetParallelizationFactor(int parallelizationFactor);
    }
}
