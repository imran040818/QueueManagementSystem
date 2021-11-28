using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QueueManagementSystem.Models
{
    public class ProgressTask<T>
    {
        public Task Task { get; set; }
        public T TaskInformation { get; set; }
    }
}
