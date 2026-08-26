using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.Activities
{
    public static class CheckStatusActivity
    {
        [FunctionName(nameof(CheckStatusActivity))]
        public static async Task<StatusHelper.ArchiveStatus> Run([ActivityTrigger] string messageJson,
            [DurableClient] IDurableEntityClient entityClient)
        {
            var currentRunInfo = await StatusHelper.GetCurrentJobs(entityClient);
            // Simplified: return Completed if status indicates success, otherwise Failed
            if (currentRunInfo != null && currentRunInfo.Status == "Succeeded")
            {
                return StatusHelper.ArchiveStatus.Completed;
            }

            if (currentRunInfo != null && currentRunInfo.Status == "InProgress")
            {
                return StatusHelper.ArchiveStatus.InProgress;
            }

            return StatusHelper.ArchiveStatus.Completed;
        }
    }
}
