namespace QueueManagementSystem.Events
{
    public delegate void TaskCancelledEventHandler<T>(T data);
    public delegate void TaskAllCancelledEventHandler<T>(T data);
    public delegate void TaskPausedEventHandler<T>(T data);
    public delegate void TaskAllPausedEventHandler<T>(T data);
    public delegate void TaskCompleteEventHandler<T>(T data);
    public delegate void TaskFaultedEventHandler<T>(T data);
    public delegate void ReadyToEnqueueEventHandler();
}
