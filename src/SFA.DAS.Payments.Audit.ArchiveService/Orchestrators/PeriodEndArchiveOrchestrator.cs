using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Activities;
using SFA.DAS.Payments.Audit.ArchiveService.Configuration;
using SFA.DAS.Payments.Audit.ArchiveService.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.Models;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.Orchestrators
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
            var periodEndArchiveActivityResponse = new PeriodEndArchiveActivityResponse();
            ILogger logger = context.CreateReplaySafeLogger(nameof(PeriodEndArchiveOrchestrator));
            try
            {
                using (logger.BeginScope(new Dictionary<string, object> { ["OrchestrationInstanceId"] = context.InstanceId }))
                {
                    var periodEndFcsHandOverJob = context.GetInput<RecordPeriodEndFcsHandOverCompleteJob>();
                    string msg = $"Starting Period End Archive Orchestrator for OrchestrationInstanceId: {context.InstanceId}";
                    logger.LogInformation(msg);

                    periodEndArchiveActivityResponse = await context.CallActivityAsync<PeriodEndArchiveActivityResponse>(nameof(PeriodEndArchiveActivity)
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
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while executing {nameof(PeriodEndArchiveOrchestrator)} function with InstanceId : {context.InstanceId}", ex.Message);
                await context.CallActivityAsync(nameof(ArchiveFailActivity), periodEndArchiveActivityResponse);
            }
        }
    }
}