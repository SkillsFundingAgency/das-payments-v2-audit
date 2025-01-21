using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Configuration;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Models;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Activities
{
    public class CheckStatusActivity
    {
        private readonly IDataFactoryHelper _dataFactoryHelper;
        private readonly IAppSettingsOptions _appSettingsOption;

        public CheckStatusActivity(IDataFactoryHelper dataFactoryHelper, IAppSettingsOptions appSettingsOption)
        {
            _dataFactoryHelper = dataFactoryHelper;
            _appSettingsOption = appSettingsOption;
        }

        [Function(nameof(CheckStatusActivity))]
        public async Task<StatusHelper.ArchiveStatus> StartCheckStatusActivity([ActivityTrigger] PeriodEndArchiveActivityResponse PeriodEndArchiveActivityResponse
            , string runPipelineId
           , FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(CheckStatusActivity));
            using (logger.BeginScope(new Dictionary<string, object> { ["OrchestrationInstanceId"] = PeriodEndArchiveActivityResponse.InstanceId }))
            {
                try
                {
                    logger.LogInformation($"Starting {nameof(CheckStatusActivity)} for OrchestrationInstanceId: {PeriodEndArchiveActivityResponse.InstanceId}");

                    var datafactoryClient = await _dataFactoryHelper.CreateClientAsync();

                    var pipelineRun = await datafactoryClient.PipelineRuns.GetAsync(
                        _appSettingsOption.Values.ResourceGroup
                        , _appSettingsOption.Values.AzureDataFactoryName
                        , PeriodEndArchiveActivityResponse.RunId);


                    if (pipelineRun.Status is "InProgress" or "Queued")
                    {
                        return StatusHelper.ArchiveStatus.InProgress;
                    }

                    var filterParams = new RunFilterParameters(DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(10));

                    var queryResponse = await datafactoryClient.ActivityRuns.QueryByPipelineRunAsync(
                        _appSettingsOption.Values.ResourceGroup
                        , _appSettingsOption.Values.AzureDataFactoryName
                        , PeriodEndArchiveActivityResponse.InstanceId
                        , filterParams);

                    if (queryResponse is not null)
                        logger.LogInformation(queryResponse.Value.First().Output.ToString());

                    if (pipelineRun.Status != "Succeeded")
                    {
                        logger.LogError($"Error in {nameof(CheckStatusActivity)} for OrchestrationInstanceId: {PeriodEndArchiveActivityResponse.InstanceId}. Pipeline run failed. Status: {pipelineRun.Status}. InstanceId: {PeriodEndArchiveActivityResponse.InstanceId}");
                        return StatusHelper.ArchiveStatus.Failed;
                    }

                    return StatusHelper.ArchiveStatus.Completed;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error while executing {nameof(CheckStatusActivity)} function with InstanceId : {PeriodEndArchiveActivityResponse.InstanceId}", ex.Message);
                    return StatusHelper.ArchiveStatus.Failed;
                }
            }
        }
    }
}
