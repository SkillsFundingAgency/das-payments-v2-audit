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
        public async Task<StatusHelper.ArchiveStatus> StartCheckStatusActivity([ActivityTrigger] string InstanceId
           , FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(CheckStatusActivity));
            using (logger.BeginScope(new Dictionary<string, object> { ["OrchestrationInstanceId"] = InstanceId }))
            {
                logger.LogInformation($"Starting CheckStatusActivity for OrchestrationInstanceId: {InstanceId}");

                var datafactoryClient = await _dataFactoryHelper.CreateClientAsync();

                var pipelineRun = await datafactoryClient.PipelineRuns.GetAsync(
                    _appSettingsOption.Values.ResourceGroup
                    , _appSettingsOption.Values.AzureDataFactoryName
                    , InstanceId);


                if (pipelineRun.Status is "InProgress" or "Queued")
                {
                    return StatusHelper.ArchiveStatus.InProgress;
                }

                var filterParams = new RunFilterParameters(DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(10));

                var queryResponse = await datafactoryClient.ActivityRuns.QueryByPipelineRunAsync(
                    _appSettingsOption.Values.ResourceGroup
                    , _appSettingsOption.Values.AzureDataFactoryName
                    , InstanceId
                    , filterParams);


                if (pipelineRun.Status != "Succeeded")
                {
                    throw new Exception($"Error in CheckStatusActivity. Pipeline run failed. Status: {pipelineRun.Status}. InstanceId: {InstanceId}");
                }

                logger.LogInformation(queryResponse.Value.First().Output.ToString());

                return StatusHelper.ArchiveStatus.Completed;
            }
        }
    }
}
