using System;
using System.Threading;

namespace QueueManagementSystem.Models
{
    internal class QueueTaskInformation<T1, T2>
    {
        public QueueTaskInformation(TaskInformation<T1, T2> taskInformation)
        {
            TaskInformation = taskInformation;
        }
        public bool IsPaused { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }
        public TaskInformation<T1, T2> TaskInformation { get; }
    }
}
