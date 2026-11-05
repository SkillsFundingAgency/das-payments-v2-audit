using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.Audit.ArchiveService.Orchestrators
{
    public class PeriodEndArchiveOrchestrationInput
    {
        public string MessageJson { get; set; }
        public int SleepDelay { get; set; }
    }
}
