using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.DurableTask.Client;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.Configuration;
using SFA.DAS.Payments.Model.Core.Audit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.Audit.ArchiveService.Activities
{
    public class CheckStatusActivity
    {
        private readonly IPaymentLogger _logger;
        private readonly IPeriodEndArchiveConfiguration _config;

        public CheckStatusActivity(
            IPaymentLogger logger,
            IPeriodEndArchiveConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        [Function(nameof(CheckStatusActivity))]
        public async Task<StatusHelper.ArchiveStatus> Run(
            [ActivityTrigger] string messageJson,
            [DurableClient] DurableTaskClient durableClient)
        {
            var currentRunInfo =
                await StatusHelper.GetCurrentJobs(durableClient);

            try
            {
                var client = await DataFactoryHelper.CreateClient(_config);

                var pipelineRun = await client.PipelineRuns.GetAsync(
                    _config.ResourceGroup,
                    _config.AzureDataFactoryName,
                    currentRunInfo.InstanceId);

                _logger.LogInfo(
                    "Period End Archive Status: " + pipelineRun.Status);

                if (pipelineRun.Status is "InProgress" or "Queued")
                {
                    currentRunInfo = new ArchiveRunInformation
                    {
                        JobId = currentRunInfo.JobId,
                        InstanceId = currentRunInfo.InstanceId,
                        Status = pipelineRun.Status
                    };

                    await StatusHelper.UpdateCurrentJobStatus(
                        durableClient,
                        currentRunInfo);

                    return StatusHelper.ArchiveStatus.InProgress;
                }

                var filterParams = new RunFilterParameters(
                    DateTime.UtcNow.AddMinutes(-10),
                    DateTime.UtcNow.AddMinutes(10));

                var queryResponse =
                    await client.ActivityRuns.QueryByPipelineRunAsync(
                        _config.ResourceGroup,
                        _config.AzureDataFactoryName,
                        currentRunInfo.InstanceId,
                        filterParams);

                if (pipelineRun.Status != "Succeeded")
                {
                    throw new Exception(
                        $"Error in CheckStatusActivity. " +
                        $"Pipeline run failed. Status: {pipelineRun.Status}. " +
                        $"Message: {messageJson}");
                }

                _logger.LogInfo(
                    queryResponse.Value.First().Output.ToString());

                currentRunInfo = new ArchiveRunInformation
                {
                    JobId = currentRunInfo.JobId,
                    InstanceId = currentRunInfo.InstanceId,
                    Status = pipelineRun.Status
                };

                await StatusHelper.UpdateCurrentJobStatus(
                    durableClient,
                    currentRunInfo);

                return StatusHelper.ArchiveStatus.Completed;
            }
            catch
            {
                currentRunInfo.Status = "Failed";

                await StatusHelper.UpdateCurrentJobStatus(
                    durableClient,
                    currentRunInfo);

                return StatusHelper.ArchiveStatus.Failed;
            }
        }
    }
}