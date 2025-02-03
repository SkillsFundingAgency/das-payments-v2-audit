using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Configuration;
using SFA.DAS.Payments.Audit.ArchiveService.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.Models;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.Activities
{
    public class PeriodEndArchiveActivity
    {
        private readonly IDataFactoryHelper _dataFactoryHelper;
        private readonly IAppSettingsOptions _appSettingsOption;
        private readonly IEntityHelper _entityHelper;
        private ILogger<PeriodEndArchiveActivity> _logger;
        public PeriodEndArchiveActivity(IDataFactoryHelper dataFactoryHelper
            , IAppSettingsOptions appSettingsOption
            , IEntityHelper entityHelper
            , ILogger<PeriodEndArchiveActivity> logger)
        {
            _dataFactoryHelper = dataFactoryHelper;
            _appSettingsOption = appSettingsOption;
            _entityHelper = entityHelper;
            _logger = logger;
        }


        [Function(nameof(PeriodEndArchiveActivity))]
        public async Task<PeriodEndArchiveActivityResponse> StartPeriodEndArchiveActivity(
            [ActivityTrigger] RecordPeriodEndFcsHandOverCompleteJob periodEndFcsHandOverJob
            , string InstanceId
            , FunctionContext executionContext
            , [DurableClient] DurableTaskClient client)
        {
            _logger = executionContext.GetLogger<PeriodEndArchiveActivity>();
            using (_logger.BeginScope(new Dictionary<string, object> { ["OrchestrationInstanceId"] = InstanceId }))
            {
                try
                {
                    _logger.LogInformation($"Starting Period End Archive Activity for OrchestrationInstanceId: {InstanceId}");

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
                    {
                        _logger.LogError($"Error in {nameof(StartPeriodEndArchiveActivity)}. RunResponse is null.");
                        return null;
                    }

                    if (runResponse.Response.StatusCode is not System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogError($"Error in {nameof(StartPeriodEndArchiveActivity)}. Error message: {runResponse.Response.Content}.");
                        return null;
                    }
                    if (runResponse.Response.StatusCode is System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogInformation($"Period end archive activity started with RunId: {runResponse.Body.RunId} status: {runResponse.Response.StatusCode}");
                    }

                    await _entityHelper.UpdateCurrentJobStatus(client, new ArchiveRunInformation
                    {
                        JobId = periodEndFcsHandOverJob.JobId.ToString(),
                        InstanceId = runResponse.Body.RunId,
                        Status = "Started"
                    }, StatusHelper.EntityState.add);

                    return new PeriodEndArchiveActivityResponse
                    {
                        RunId = runResponse.Body.RunId,
                        StatusCode = runResponse.Response.StatusCode,
                        InstanceId = InstanceId
                    };
                }
                catch (Exception ex)
                {
                    await _entityHelper.UpdateCurrentJobStatus(client, new ArchiveRunInformation
                    {
                        JobId = periodEndFcsHandOverJob.JobId.ToString(),
                        InstanceId = InstanceId,
                        Status = "Failed"
                    }, StatusHelper.EntityState.add);

                    _logger.LogError(ex, $"Error in {nameof(StartPeriodEndArchiveActivity)}. Error message: {ex.Message}.", ex.Message);
                    return null;
                }
            }
        }
    }
}
