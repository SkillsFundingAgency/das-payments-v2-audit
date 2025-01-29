using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.Audit.ArchiveService.Activities;
using SFA.DAS.Payments.Audit.ArchiveService.Configuration;
using SFA.DAS.Payments.Audit.ArchiveService.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.Models;
using SFA.DAS.Payments.Audit.ArchiveService.Orchestrators;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests.Orchestrators
{
    [TestFixture]
    public class PeriodEndArchiveOrchestratorTests
    {
        private Mock<IAppSettingsOptions> _mockAppSettingsOptions;
        private Mock<TaskOrchestrationContext> _mockOrchestrationContext;
        private Mock<ILogger> _mockLogger;
        private PeriodEndArchiveOrchestrator _orchestrator;

        [SetUp]
        public void SetUp()
        {
            _mockAppSettingsOptions = new Mock<IAppSettingsOptions>();
            _mockOrchestrationContext = new Mock<TaskOrchestrationContext>();
            _mockLogger = new Mock<ILogger>();

            _mockAppSettingsOptions.Setup(x => x.Values).Returns(new Values { SleepDelay = 1 });

            _orchestrator = new PeriodEndArchiveOrchestrator(_mockAppSettingsOptions.Object);
        }

        [Test]
        public async Task RunOrchestrator_Should_Call_Activities_And_Log_Information()
        {
            // Arrange
            var periodEndFcsHandOverJob = new RecordPeriodEndFcsHandOverCompleteJob();
            PeriodEndArchiveActivityResponse periodEndArchiveActivityResponse = PeriodEndArchiveResponse();

            _mockOrchestrationContext.Setup(x => x.GetInput<RecordPeriodEndFcsHandOverCompleteJob>())
                        .Returns(periodEndFcsHandOverJob);

            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<PeriodEndArchiveActivityResponse>(nameof(PeriodEndArchiveActivity), periodEndFcsHandOverJob, It.IsAny<TaskOptions?>()))
                        .ReturnsAsync(periodEndArchiveActivityResponse);

            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<StatusHelper.ArchiveStatus>(nameof(CheckStatusActivity), periodEndArchiveActivityResponse, It.IsAny<TaskOptions?>()))
                       .ReturnsAsync(StatusHelper.ArchiveStatus.Completed);

            _mockOrchestrationContext.Setup(x => x.CurrentUtcDateTime).Returns(DateTime.UtcNow);

            _mockOrchestrationContext.Setup(x => x.CreateReplaySafeLogger(It.IsAny<string>()))
                        .Returns(_mockLogger.Object);

            // Act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<PeriodEndArchiveActivityResponse>(nameof(PeriodEndArchiveActivity), periodEndFcsHandOverJob, It.IsAny<TaskOptions?>()), Times.Once);
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<StatusHelper.ArchiveStatus>(nameof(CheckStatusActivity), periodEndArchiveActivityResponse, It.IsAny<TaskOptions?>()), Times.Once);
        }

        [Test]
        public async Task RunOrchestrator_Should_Handle_Exception_And_Call_ArchiveFailActivity()
        {
            // Arrange
            var periodEndFcsHandOverJob = new RecordPeriodEndFcsHandOverCompleteJob();
            var periodEndArchiveActivityResponse = PeriodEndArchiveResponse();

            _mockOrchestrationContext.Setup(x => x.GetInput<RecordPeriodEndFcsHandOverCompleteJob>())
                        .Returns(periodEndFcsHandOverJob);

            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<PeriodEndArchiveActivityResponse>(nameof(PeriodEndArchiveActivity), periodEndFcsHandOverJob, It.IsAny<TaskOptions?>()))
                        .ReturnsAsync(periodEndArchiveActivityResponse);

            _mockOrchestrationContext.Setup(x => x.CallActivityAsync(nameof(ArchiveFailActivity), periodEndArchiveActivityResponse, It.IsAny<TaskOptions?>()));


            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<StatusHelper.ArchiveStatus>(nameof(CheckStatusActivity), periodEndArchiveActivityResponse, It.IsAny<TaskOptions?>()))
                       .ThrowsAsync(new Exception("Test exception"));

            _mockOrchestrationContext.Setup(x => x.CreateReplaySafeLogger(It.IsAny<string>()))
                        .Returns(_mockLogger.Object);


            // Act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync(nameof(ArchiveFailActivity), periodEndArchiveActivityResponse, It.IsAny<TaskOptions?>()), Times.Once);
        }

        private static PeriodEndArchiveActivityResponse PeriodEndArchiveResponse()
        {
            return new PeriodEndArchiveActivityResponse
            {
                InstanceId = "123",
                RunId = "456",
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}
