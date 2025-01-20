using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Configuration;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Helper;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Activities
{
    public class PeriodEndArchiveActivity
    {
        private readonly IDataFactoryHelper _dataFactoryHelper;
        private readonly IAppSettingsOptions _appSettingsOption;
        public PeriodEndArchiveActivity(IDataFactoryHelper dataFactoryHelper
            , IAppSettingsOptions appSettingsOption)
        {
            _dataFactoryHelper = dataFactoryHelper;
            _appSettingsOption = appSettingsOption;
        }


        [Function(nameof(PeriodEndArchiveActivity))]
        public async Task<string> StartPeriodEndArchiveActivity(
            [ActivityTrigger] RecordPeriodEndFcsHandOverCompleteJob periodEndFcsHandOverJob
            , string InstanceId
            , FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(PeriodEndArchiveActivity));
            using (logger.BeginScope(new Dictionary<string, object> { ["OrchestrationInstanceId"] = InstanceId }))
            {
                logger.LogInformation($"Starting Period End Archive Activity for OrchestrationInstanceId: {InstanceId}");

                if (periodEndFcsHandOverJob.CollectionPeriod is 0 || periodEndFcsHandOverJob.CollectionYear is 0)
                    throw new Exception($"Error in {nameof(StartPeriodEndArchiveActivity)}. CollectionPeriod or CollectionYear is invalid. CollectionPeriod: {periodEndFcsHandOverJob.CollectionPeriod}. CollectionYear: {periodEndFcsHandOverJob.CollectionYear}");

                var datafactoryClient = await _dataFactoryHelper.CreateClientAsync();

                var parameters = new Dictionary<string, object>
                {
                    { "CollectionPeriod", periodEndFcsHandOverJob.CollectionPeriod },
                    { "AcademicYear", periodEndFcsHandOverJob.CollectionYear }
                };

                var runResponse = await datafactoryClient.Pipelines.CreateRunWithHttpMessagesAsync(_appSettingsOption.Values.ResourceGroup
                    , _appSettingsOption.Values.AzureDataFactoryName
                    , _appSettingsOption.Values.PipeLine
                    , parameters: parameters);

                if (runResponse == null)
                    throw new Exception($"Error in {nameof(StartPeriodEndArchiveActivity)}. RunResponse is null.");

                var resposne = runResponse.Body;
                var vr = StatusHelper.GetEntityId();

                //currentRunInfo = new ArchiveRunInformation
                //{
                //    JobId = message.JobId.ToString(),
                //    InstanceId = runResponse.RunId,
                //    Status = "Started"
                //};
                //await StatusHelper.UpdateCurrentJobStatus(entityClient, currentRunInfo);

                return resposne.ToString();
            }
        }
    }
}
