using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Newtonsoft.Json;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.Configuration;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.Activities
{
    public class StartPeriodEndArchiveActivity
    {
        private readonly IPaymentLogger _logger;
        private readonly IPeriodEndArchiveConfiguration _config;

        public StartPeriodEndArchiveActivity(
            IPaymentLogger logger,
            IPeriodEndArchiveConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        [Function(nameof(StartPeriodEndArchiveActivity))]
        public async Task Run(
            [ActivityTrigger] string messageJson,
            [DurableClient] DurableTaskClient durableClient)
        {
            var currentRunInfo =
                await StatusHelper.GetCurrentJobs(durableClient);

            try
            {
                var message =
                    JsonConvert.DeserializeObject<RecordPeriodEndFcsHandOverCompleteJob>(messageJson)
                    ?? throw new Exception(
                        $"Error in StartPeriodEndArchiveActivity. Message is null. Message: {messageJson}");

                if (message.CollectionPeriod is 0 || message.CollectionYear is 0)
                {
                    throw new Exception(
                        $"Error in StartPeriodEndArchiveActivity. " +
                        $"CollectionPeriod or CollectionYear is invalid. " +
                        $"CollectionPeriod: {message.CollectionPeriod}. " +
                        $"CollectionYear: {message.CollectionYear}");
                }

                _logger.LogInfo("Starting Period End Archive Activity");

                var client = await DataFactoryHelper.CreateClient(_config);

                _logger.LogInfo("Creating pipeline run...");

                var parameters = new Dictionary<string, object>
                {
                    { "CollectionPeriod", message.CollectionPeriod },
                    { "AcademicYear", message.CollectionYear }
                };

                var response = await client.Pipelines.CreateRunWithHttpMessagesAsync(
                    _config.ResourceGroup,
                    _config.AzureDataFactoryName,
                    _config.PipeLine,
                    parameters: parameters);

                var runResponse = response.Body;

                _logger.LogInfo("Pipeline run ID: " + runResponse.RunId);

                _logger.LogInfo(
                    $"PeriodEndArchive CollectionPeriod: {message.CollectionPeriod}. " +
                    $"AcademicYear: {message.CollectionYear}");

                currentRunInfo = new ArchiveRunInformation
                {
                    JobId = message.JobId.ToString(),
                    InstanceId = runResponse.RunId,
                    Status = "Started"
                };

                await StatusHelper.UpdateCurrentJobStatus(
                    durableClient,
                    currentRunInfo);
            }
            catch (Exception ex)
            {
                currentRunInfo.Status = "Failed";

                await StatusHelper.UpdateCurrentJobStatus(
                    durableClient,
                    currentRunInfo);

                _logger.LogError(
                    "Error in StartPeriodEndArchiveActivity",
                    ex);

                throw;
            }
        }
    }
}