using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.Audit.ArchiveService.Activities;
using SFA.DAS.Payments.Audit.ArchiveService.Configuration;
using SFA.DAS.Payments.Audit.ArchiveService.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.Models;
using SFA.DAS.Payments.Model.Core.Audit;
using System.Net;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests.Activities
{
    [Ignore("Due to extention method in CheckStatusActivity class which GetAsync not be able to mock.")]
    public class CheckStatusActivityTests
    {
        private Mock<IDataFactoryHelper> _mockDataFactoryHelper;
        private Mock<IAppSettingsOptions> _mockAppSettingsOptions;
        private Mock<IEntityHelper> _mockEntityHelper;
        private Mock<FunctionContext> _mockFunctionContext;
        private Mock<FakeDurableTaskClient> _mockDurableTaskClient;
        private Mock<ILogger<CheckStatusActivity>> _mockLogger;
        private CheckStatusActivity _activity;
        private Mock<IServiceProvider> _serviceProvider;
        private Mock<IPipelineRunsOperations> _mockPipelineRunsOperations;

        [SetUp]
        public void SetUp()
        {
            _mockDataFactoryHelper = new Mock<IDataFactoryHelper>();
            _mockAppSettingsOptions = new Mock<IAppSettingsOptions>();
            _mockEntityHelper = new Mock<IEntityHelper>();
            _mockFunctionContext = new Mock<FunctionContext>();
            _mockDurableTaskClient = new Mock<FakeDurableTaskClient>();
            _mockLogger = new Mock<ILogger<CheckStatusActivity>>();
            _serviceProvider = new Mock<IServiceProvider>();
            _mockPipelineRunsOperations = new Mock<IPipelineRunsOperations>();

            _mockAppSettingsOptions.Setup(x => x.Values).Returns(new Values
            {
                ResourceGroup = "ResourceGroup",
                AzureDataFactoryName = "AzureDataFactoryName",
                PipeLine = "PipeLine"
            });

            _activity = new CheckStatusActivity(_mockDataFactoryHelper.Object, _mockAppSettingsOptions.Object, _mockEntityHelper.Object, _mockLogger.Object);
        }

        [Test]
        public async Task StartCheckStatusActivity_Should_Return_InProgress_When_PipelineRun_Is_InProgress()
        {
            // Arrange
            var periodEndArchiveActivityResponse = BuildPeriodEndArchiveActivityResponse();
            var dataFactoryClient = new Mock<DataFactoryManagementClient>(new HttpClient(), false);
            dataFactoryClient.SetupGet(x => x.PipelineRuns).Returns(_mockPipelineRunsOperations.Object);
            _mockPipelineRunsOperations.Setup(x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new PipelineRun());

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<CheckStatusActivity>)))
                          .Returns(_mockLogger.Object);
            _mockFunctionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDataFactoryHelper.Setup(x => x.CreateClientAsync()).ReturnsAsync(dataFactoryClient.Object);
            _mockEntityHelper.Setup(x => x.GetCurrentJobs(_mockDurableTaskClient.Object)).ReturnsAsync(new ArchiveRunInformation());

            // Act
            var result = await _activity.StartCheckStatusActivity(
                periodEndArchiveActivityResponse
                , _mockFunctionContext.Object
                , _mockDurableTaskClient.Object);

            // Assert
            Assert.That(result, Is.EqualTo(StatusHelper.ArchiveStatus.InProgress));
            _mockEntityHelper.Verify(x => x.UpdateCurrentJobStatus(_mockDurableTaskClient.Object, It.IsAny<ArchiveRunInformation>(), StatusHelper.EntityState.add), Times.Once);
        }

        [Test]
        public async Task StartCheckStatusActivity_Should_Return_Completed_When_PipelineRun_Is_Succeeded()
        {
            // Arrange
            var periodEndArchiveActivityResponse = BuildPeriodEndArchiveActivityResponse();
            var pipelineRun = new PipelineRun();
            var dataFactoryClient = new Mock<DataFactoryManagementClient>(new HttpClient(), false);
            dataFactoryClient.SetupGet(x => x.PipelineRuns).Returns(_mockPipelineRunsOperations.Object);
            _mockPipelineRunsOperations.Setup(x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(pipelineRun);

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<CheckStatusActivity>)))
                          .Returns(_mockLogger.Object);
            _mockFunctionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDataFactoryHelper.Setup(x => x.CreateClientAsync()).ReturnsAsync(dataFactoryClient.Object);
            _mockEntityHelper.Setup(x => x.GetCurrentJobs(_mockDurableTaskClient.Object)).ReturnsAsync(new ArchiveRunInformation());

            // Act
            var result = await _activity.StartCheckStatusActivity(
                periodEndArchiveActivityResponse
                , _mockFunctionContext.Object
                , _mockDurableTaskClient.Object);

            // Assert
            Assert.That(result, Is.EqualTo(StatusHelper.ArchiveStatus.Completed));
            _mockEntityHelper.Verify(x => x.UpdateCurrentJobStatus(_mockDurableTaskClient.Object, It.IsAny<ArchiveRunInformation>(), StatusHelper.EntityState.add), Times.Once);
        }

        [Test]
        public async Task StartCheckStatusActivity_Should_Return_Failed_When_PipelineRun_Is_Not_Succeeded()
        {
            // Arrange
            var periodEndArchiveActivityResponse = BuildPeriodEndArchiveActivityResponse();
            var pipelineRun = new PipelineRun();
            var dataFactoryClient = new Mock<DataFactoryManagementClient>(new HttpClient(), false);
            dataFactoryClient.SetupGet(x => x.PipelineRuns).Returns(_mockPipelineRunsOperations.Object);
            _mockPipelineRunsOperations.Setup(x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(pipelineRun);

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<CheckStatusActivity>)))
                          .Returns(_mockLogger.Object);
            _mockFunctionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDataFactoryHelper.Setup(x => x.CreateClientAsync()).ReturnsAsync(dataFactoryClient.Object);
            _mockEntityHelper.Setup(x => x.GetCurrentJobs(_mockDurableTaskClient.Object)).ReturnsAsync(new ArchiveRunInformation());

            // Act
            var result = await _activity.StartCheckStatusActivity(
                periodEndArchiveActivityResponse
                , _mockFunctionContext.Object
                , _mockDurableTaskClient.Object);

            // Assert
            Assert.That(result, Is.EqualTo(StatusHelper.ArchiveStatus.Failed));
            _mockEntityHelper.Verify(x => x.UpdateCurrentJobStatus(_mockDurableTaskClient.Object, It.IsAny<ArchiveRunInformation>(), StatusHelper.EntityState.add), Times.Once);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Test]
        public async Task StartCheckStatusActivity_Should_Handle_Exception()
        {
            // Arrange
            var periodEndArchiveActivityResponse = BuildPeriodEndArchiveActivityResponse();
            var dataFactoryClient = new Mock<DataFactoryManagementClient>(new HttpClient(), false);
            dataFactoryClient.SetupGet(x => x.PipelineRuns).Returns(_mockPipelineRunsOperations.Object);
            _mockPipelineRunsOperations.Setup(x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new Exception("Test exception"));

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<CheckStatusActivity>)))
                          .Returns(_mockLogger.Object);
            _mockFunctionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDataFactoryHelper.Setup(x => x.CreateClientAsync()).ReturnsAsync(dataFactoryClient.Object);
            _mockEntityHelper.Setup(x => x.GetCurrentJobs(_mockDurableTaskClient.Object)).ReturnsAsync(new ArchiveRunInformation());

            // Act
            var result = await _activity.StartCheckStatusActivity(
                periodEndArchiveActivityResponse
                , _mockFunctionContext.Object
                , _mockDurableTaskClient.Object);

            // Assert
            Assert.That(result, Is.EqualTo(StatusHelper.ArchiveStatus.Failed));
            _mockEntityHelper.Verify(x => x.UpdateCurrentJobStatus(_mockDurableTaskClient.Object, It.IsAny<ArchiveRunInformation>(), StatusHelper.EntityState.add), Times.Once);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        private static PeriodEndArchiveActivityResponse BuildPeriodEndArchiveActivityResponse()
        {
            return new PeriodEndArchiveActivityResponse
            {
                RunId = "RunId",
                InstanceId = "InstanceId",
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}


