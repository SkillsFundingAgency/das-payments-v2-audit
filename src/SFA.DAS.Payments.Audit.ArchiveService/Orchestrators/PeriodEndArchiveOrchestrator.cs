using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Activities;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.Audit.ArchiveService.Orchestrators
{
    public static class PeriodEndArchiveOrchestrator
    {
        [Function(nameof(PeriodEndArchiveOrchestrator))]
        public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var input = context.GetInput<PeriodEndArchiveOrchestrationInput>()
                        ?? throw new Exception("Error in PeriodEndArchiveOrchestrator. Input is null.");

            var messageJson = input.MessageJson;

            try
            {
                var log = context.CreateReplaySafeLogger(nameof(PeriodEndArchiveOrchestrator));

                log.LogInformation("Starting Period End Archive Orchestrator");

                await context.CallActivityAsync(nameof(StartPeriodEndArchiveActivity), messageJson);

                var timeout = context.CurrentUtcDateTime.AddMinutes(input.SleepDelay);
                var pollingInterval = TimeSpan.FromMinutes(1);

                while (context.CurrentUtcDateTime < timeout)
                {
                    var status = await context.CallActivityAsync<StatusHelper.ArchiveStatus>(nameof(CheckStatusActivity), messageJson);

                    if (status is StatusHelper.ArchiveStatus.Completed ||
                        status is StatusHelper.ArchiveStatus.Failed)
                    {
                        break;
                    }

                    await context.CreateTimer(context.CurrentUtcDateTime.Add(pollingInterval), CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                await context.CallActivityAsync(nameof(ArchiveFailActivity), messageJson);

                throw new Exception("Error in PeriodEndArchiveOrchestrator", ex);
            }
        }
    }
}