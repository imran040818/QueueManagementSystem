using System;
namespace QueueManagementSystem.Events.EventArg
{
    public class QueueEventArg<T> : EventArgs
    {
        public QueueEventArg(T data)
        {
            Data = data;
        }
        public QueueEventArg(T data, Exception exception)
        {
            Data = data;
            Exception = exception;
        }
        public T Data { get; set; }
        public Exception Exception { get; }
    }
}
