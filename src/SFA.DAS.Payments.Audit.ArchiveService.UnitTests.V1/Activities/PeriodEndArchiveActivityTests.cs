using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Activities;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Configuration;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Helper;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests.V1.Activities
{
    [TestFixture]
    public class PeriodEndArchiveActivityTests
    {
        private Mock<IDataFactoryHelper> _mockDataFactoryHelper;
        private Mock<IAppSettingsOptions> _mockAppSettingsOptions;
        private Mock<IEntityHelper> _mockEntityHelper;
        private Mock<FunctionContext> _mockFunctionContext;
        private Mock<FakeDurableTaskClient> _mockDurableTaskClient;
        private Mock<ILogger<PeriodEndArchiveActivity>> _mockLogger;
        private PeriodEndArchiveActivity _activity;
        private Mock<IServiceProvider> _serviceProvider;

        private string InstanceId = "InstanceId";

        [SetUp]
        public void SetUp()
        {
            _mockDataFactoryHelper = new Mock<IDataFactoryHelper>();
            _mockAppSettingsOptions = new Mock<IAppSettingsOptions>();
            _mockEntityHelper = new Mock<IEntityHelper>();
            _mockFunctionContext = new Mock<FunctionContext>();
            _mockDurableTaskClient = new Mock<FakeDurableTaskClient>();
            _mockLogger = new Mock<ILogger<PeriodEndArchiveActivity>>();
            _serviceProvider = new Mock<IServiceProvider>();

            _mockAppSettingsOptions.Setup(x => x.Values).Returns(new Values
            {
                ResourceGroup = "ResourceGroup",
                AzureDataFactoryName = "AzureDataFactoryName",
                PipeLine = "PipeLine"
            });

            _activity = new PeriodEndArchiveActivity(_mockDataFactoryHelper.Object, _mockAppSettingsOptions.Object, _mockEntityHelper.Object, _mockLogger.Object);
        }

        [Test]
        public async Task StartPeriodEndArchiveActivity_Should_Start_Archive_And_Return_Response()
        {
            // Arrange
            RecordPeriodEndFcsHandOverCompleteJob periodEndFcsHandOverJob = BuildPeriodEndFcsHandOverCompleteJob();
            CreateRunResponse runResponse = BuildRunResponse();

            var dataFactoryClient = new Mock<DataFactoryManagementClient>(new HttpClient(), false);
            dataFactoryClient.Setup(x => x.Pipelines.CreateRunWithHttpMessagesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool?>(),
                It.IsAny<string>(),
                It.IsAny<bool?>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new Microsoft.Rest.Azure.AzureOperationResponse<CreateRunResponse>
            {
                Body = runResponse,
                Response = new HttpResponseMessage(HttpStatusCode.OK)
            });

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<PeriodEndArchiveActivity>)))
                          .Returns(_mockLogger.Object);
            _mockFunctionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDataFactoryHelper.Setup(x => x.CreateClientAsync()).ReturnsAsync(dataFactoryClient.Object);

            // Act
            var result = await _activity.StartPeriodEndArchiveActivity(
                periodEndFcsHandOverJob
                , InstanceId
                , _mockFunctionContext.Object
                , _mockDurableTaskClient.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.RunId, Is.EqualTo("RunId"));
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.InstanceId, Is.EqualTo("InstanceId"));

            _mockEntityHelper.Verify(x => x.UpdateCurrentJobStatus(_mockDurableTaskClient.Object, It.IsAny<ArchiveRunInformation>(), StatusHelper.EntityState.add), Times.Once);
        }

        [Test]
        public async Task StartPeriodEndArchiveActivity_Should_Start_Archive_And_Return_NullResponse_StatusIsNotOk()
        {
            // Arrange
            RecordPeriodEndFcsHandOverCompleteJob periodEndFcsHandOverJob = BuildPeriodEndFcsHandOverCompleteJob();
            CreateRunResponse runResponse = BuildRunResponse();

            var dataFactoryClient = new Mock<DataFactoryManagementClient>(new HttpClient(), false);
            dataFactoryClient.Setup(x => x.Pipelines.CreateRunWithHttpMessagesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool?>(),
                It.IsAny<string>(),
                It.IsAny<bool?>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new Microsoft.Rest.Azure.AzureOperationResponse<CreateRunResponse>
            {
                Body = null,
                Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            });

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<PeriodEndArchiveActivity>)))
                          .Returns(_mockLogger.Object);
            _mockFunctionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDataFactoryHelper.Setup(x => x.CreateClientAsync()).ReturnsAsync(dataFactoryClient.Object);

            // Act
            var result = await _activity.StartPeriodEndArchiveActivity(
                periodEndFcsHandOverJob
                , InstanceId
                , _mockFunctionContext.Object
                , _mockDurableTaskClient.Object);

            // Assert
            Assert.IsNull(result);
        }

        private static CreateRunResponse BuildRunResponse()
        {
            return new CreateRunResponse
            {
                RunId = "RunId",
            };
        }

        private static RecordPeriodEndFcsHandOverCompleteJob BuildPeriodEndFcsHandOverCompleteJob()
        {
            return new RecordPeriodEndFcsHandOverCompleteJob
            {
                CollectionPeriod = 1,
                CollectionYear = 2021,
                JobId = 123
            };
        }
    }
}

