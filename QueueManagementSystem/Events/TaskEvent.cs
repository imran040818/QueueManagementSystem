using QueueManagementSystem.Events.EventArg;
using System;

namespace QueueManagementSystem.Events
{
    public delegate void TaskCancelledEventHandler<T>(object sender, QueueEventArg<T> eventArgs);
    public delegate void TaskAllCancelledEventHandler<T>(object sender, QueueEventArg<T> eventArgs);
    public delegate void TaskPausedEventHandler<T>(object sender, QueueEventArg<T> eventArgs);
    public delegate void TaskAllPausedEventHandler<T>(object sender, QueueEventArg<T> eventArgs);
    public delegate void TaskCompleteEventHandler<T>(object sender, QueueEventArg<T> eventArgs);
    public delegate void TaskFaultedEventHandler<T>(object sender, QueueEventArg<T> eventArgs);
    public delegate void ReadyToEnqueueEventHandler(object sender, System.EventArgs eventArgs);
}
