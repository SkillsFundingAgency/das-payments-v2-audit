using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;

namespace SFA.DAS.Payments.Audit.ArchiveService.Activities
{
    public static class ArchiveFailActivity
    {
        [FunctionName(nameof(ArchiveFailActivity))]
        public static Task Run([ActivityTrigger] string messageJson,
            [DurableClient] IDurableEntityClient entityClient)
        {
            var current = StatusHelper.GetCurrentJobs(entityClient).Result;
            current.Status = StatusHelper.ArchiveStatus.Failed.ToString();
            StatusHelper.UpdateCurrentJobStatus(current);
            return Task.CompletedTask;
        }
    }
}
