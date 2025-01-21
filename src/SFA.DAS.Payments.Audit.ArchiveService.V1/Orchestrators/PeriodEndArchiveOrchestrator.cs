using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Activities;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Configuration;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Models;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Orchestrators
{
    public class PeriodEndArchiveOrchestrator
    {
        private readonly IAppSettingsOptions _appSettingsOption;

        public PeriodEndArchiveOrchestrator(IAppSettingsOptions appSettingsOption)
        {
            _appSettingsOption = appSettingsOption;
        }

        [Function(nameof(PeriodEndArchiveOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(PeriodEndArchiveOrchestrator));
            using (logger.BeginScope(new Dictionary<string, object> { ["OrchestrationInstanceId"] = context.InstanceId }))
            {
                RecordPeriodEndFcsHandOverCompleteJob periodEndFcsHandOverJob = context.GetInput<RecordPeriodEndFcsHandOverCompleteJob>();
                logger.LogInformation($"Starting Period End Archive Orchestrator for OrchestrationInstanceId: {context.InstanceId}");

                PeriodEndArchiveActivityResponse periodEndArchiveActivityResponse = await context.CallActivityAsync<PeriodEndArchiveActivityResponse>(nameof(PeriodEndArchiveActivity)
                    , periodEndFcsHandOverJob);
                if (periodEndArchiveActivityResponse is not null)
                {
                    // Start polling ADF for result
                    var timeout = context.CurrentUtcDateTime.AddMinutes(_appSettingsOption.Values.SleepDelay);
                    var pollingInterval = TimeSpan.FromMinutes(1);

                    while (context.CurrentUtcDateTime < timeout)
                    {
                        var archiveStatus = await context.CallActivityAsync<StatusHelper.ArchiveStatus>(nameof(CheckStatusActivity), periodEndArchiveActivityResponse);
                        if (archiveStatus is StatusHelper.ArchiveStatus.Completed or StatusHelper.ArchiveStatus.Failed)
                        {
                            break;
                        }
                        // If not yet complete, or failed wait for the specified polling interval before the next attempt.
                        await context.CreateTimer(context.CurrentUtcDateTime.Add(pollingInterval), CancellationToken.None);
                    }
                }
            }
        }
    }
}