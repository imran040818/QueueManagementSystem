using System.Threading;
using System.Threading.Tasks;

namespace QueueManagementSystem.Models
{
    public delegate Task RunTaskHandler<in T1>(T1 parameter, CancellationToken token);
    public class TaskInformation<T1, T2>
    {
        public CancellationTokenSource CancellationToken { get; set; }
        public T2 TaskId { get; set; }
        public T1 Parameter { get; set; }
        public RunTaskHandler<T1> RunTaskHandler { get; set; }
        public bool IsPriority { get; set; }
        public bool IsPaused { get; set; }
    }
}
