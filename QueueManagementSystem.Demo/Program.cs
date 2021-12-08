

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
            queueService.ReadyToEnqueue += QueueService_ReadyToEnqueue;
            queueService.TaskAllCancel += QueueService_TaskAllCancel;
            queueService.TaskAllPausedEvent += QueueService_TaskAllPausedEvent;
            queueService.TaskCancelled += QueueService_TaskCancelled;
            queueService.TaskComplete += QueueService_TaskComplete;
            queueService.TaskFaulted += QueueService_TaskFaulted;

            queueService.Configure(new QueueInformation
            {
                PreferedParallelizationFactor = 4,
                QueueTimer = 5000
            });
            queueService.StartQueue();
            
            Console.ReadKey();
        }

        private static void QueueService_TaskFaulted(object sender, Events.EventArg.QueueEventArg<BackupTaskInformationParameter> eventArgs)
        {
            throw new NotImplementedException();
        }

        private static void QueueService_TaskComplete(object sender, Events.EventArg.QueueEventArg<BackupTaskInformationParameter> eventArgs)
        {
            throw new NotImplementedException();
        }

        private static void QueueService_TaskCancelled(object sender, Events.EventArg.QueueEventArg<BackupTaskInformationParameter> eventArgs)
        {
            throw new NotImplementedException();
        }

        private static void QueueService_TaskAllPausedEvent(object sender, Events.EventArg.QueueEventArg<System.Collections.Generic.List<BackupTaskInformationParameter>> eventArgs)
        {
            throw new NotImplementedException();
        }

        private static void QueueService_TaskAllCancel(object sender, Events.EventArg.QueueEventArg<System.Collections.Generic.List<BackupTaskInformationParameter>> eventArgs)
        {
            throw new NotImplementedException();
        }

        private static void QueueService_ReadyToEnqueue(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
