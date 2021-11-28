using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace QueueManagementSystem.Models
{
    public class QueueInformation
    {
        public int MaxParallelizationFactor { get; } = Environment.ProcessorCount * 4;
        public int PreferedParallelizationFactor { get; set; }
    }
}
