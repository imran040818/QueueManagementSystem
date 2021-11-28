namespace QueueManagementSystem.Events
{
    public delegate void TaskCancelledHandler<T>(T data);
    public delegate void TaskAllCancelledHandler<T>(T data);
    public delegate void TaskPausedHandler<T>(T data);
    public delegate void TaskAllPausedHandler<T>(T data);
    public delegate void TaskCompleteHandler<T>(T data);
    public delegate void TaskFaultedHandler<T>(T data);
    public delegate void ReadyToAcceptTaskHandler();
}
