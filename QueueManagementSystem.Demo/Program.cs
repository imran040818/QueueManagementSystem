

using System;
using System.Threading;
using System.Threading.Tasks;
using QueueManagementSystem.Core;
using QueueManagementSystem.Demo;
using QueueManagementSystem.Models;

namespace QueueManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            var queueService = new QueueService<BackupTaskInformationParameter, int>();
            queueService.ReadyToEnqueue += QueueService_ReadyToAcceptTaskEventHandler;
            queueService.TaskAllCancel += QueueService_TaskAllCancelEventHandler;
            queueService.TaskComplete += QueueService_TaskCompleteEventHandler;
            queueService.TaskFaulted += QueueService_TaskFaultedEventHandler;
            queueService.TaskCancelled += QueueService_TaskCancelledEventHandler;
            queueService.TaskAllPausedEvent += QueueService_TaskAllPausedEventHandler;
            queueService.TaskPausedEvent += QueueService_TaskPausedEventHandler;

            queueService.Configure(new QueueInformation
            {
                PreferedParallelizationFactor = 4
            });
            queueService.StartQueue();
            queueService.Enqueue(new TaskInformation<BackupTaskInformationParameter, int>
            {
                TaskId = 1,
                Parameter = new BackupTaskInformationParameter
                {
                    Id = 1
                },
                RunTaskHandler = Run
            }).Wait();
            queueService.Enqueue(new TaskInformation<BackupTaskInformationParameter, int>
            {
                TaskId = 2,
                Parameter = new BackupTaskInformationParameter
                {
                    Id = 2
                },
                RunTaskHandler = Run
            }).Wait();
            queueService.Enqueue(new TaskInformation<BackupTaskInformationParameter, int>
            {
                TaskId = 3,
                Parameter = new BackupTaskInformationParameter
                {
                    Id = 3
                },
                RunTaskHandler = RunComplete
            }).Wait();
            queueService.Enqueue(new TaskInformation<BackupTaskInformationParameter, int>
            {
                TaskId = 4,
                Parameter = new BackupTaskInformationParameter
                {
                    Id = 4
                },
                RunTaskHandler = RunComplete
            }).Wait();
            queueService.Enqueue(new TaskInformation<BackupTaskInformationParameter, int>
            {
                TaskId = 5,
                Parameter = new BackupTaskInformationParameter
                {
                    Id = 5
                },
                RunTaskHandler = RunException
            }).Wait();
            queueService.Enqueue(new TaskInformation<BackupTaskInformationParameter, int>
            {
                TaskId = 6,
                Parameter = new BackupTaskInformationParameter
                {
                    Id = 6
                },
                RunTaskHandler = Run
            }).Wait();

            Thread.Sleep(2000);
            queueService.Cancel(1);
            Thread.Sleep(1000);
            queueService.Pause(2);
            Thread.Sleep(3000);
            queueService.Pause();

            Console.ReadKey();
        }

        private static void QueueService_TaskPausedEventHandler(BackupTaskInformationParameter data)
        {
            Console.WriteLine($"Paused {data.Id}");
        }

        private static void QueueService_TaskCancelledEventHandler(BackupTaskInformationParameter data)
        {
            Console.WriteLine($"Cancel {data.Id}");
        }

        private static void QueueService_TaskFaultedEventHandler(BackupTaskInformationParameter data)
        {
            Console.WriteLine($"Faulted {data.Id}");
        }

        private static void QueueService_TaskCompleteEventHandler(BackupTaskInformationParameter data)
        {
            Console.WriteLine($"Completed {data.Id}");
        }

        private static void QueueService_TaskAllPausedEventHandler(System.Collections.Generic.List<BackupTaskInformationParameter> data)
        {
            Console.WriteLine($"All Paused");
            foreach (var item in data)
            {
                Console.WriteLine($"Id = {item.Id}");
            }
        }

        private static void QueueService_TaskAllCancelEventHandler(System.Collections.Generic.List<BackupTaskInformationParameter> data)
        {
            Console.WriteLine($"All Cancelled");
            foreach (var item in data)
            {
                Console.WriteLine($"Id = {item.Id}");
            }
        }

        private static void QueueService_ReadyToAcceptTaskEventHandler()
        {

        }

        public static async Task Run(BackupTaskInformationParameter taskInformationParameter, CancellationToken cancellationToken)
        {
            //Console.WriteLine($"Paramater id {taskInformationParameter?.Id}");
            for (int i = 0; i < 1000; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                    //cancellationToken.ThrowIfCancellationRequested();
                }
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId } - Item {i}");
                await Task.Delay(500);
            }
        }
        public static async Task RunComplete(BackupTaskInformationParameter taskInformationParameter, CancellationToken cancellationToken)
        {
            //Console.WriteLine($"Paramater id {taskInformationParameter?.Id}");
            for (int i = 0; i < 10; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await Task.Delay(500);
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId } - Item {i}");
            }
        }
        public static async Task RunException(BackupTaskInformationParameter taskInformationParameter, CancellationToken cancellationToken)
        {
            await Task.Delay(5000);

            throw new NotImplementedException();
        }
    }
}
