using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.Audit.ArchiveService.UnitTests;
using SFA.DAS.Payments.Audit.ArchiveService.Activities;
using SFA.DAS.Payments.Audit.ArchiveService.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.Models;
using SFA.DAS.Payments.Model.Core.Audit;
using static SFA.DAS.Payments.Audit.ArchiveService.Helper.StatusHelper;

namespace SFA.DAS.Payments.Audit.ArchiveService.Tests.Activities
{
    [TestFixture]
    public class ArchiveFailActivityTests
    {
        private Mock<IEntityHelper> _entityHelperMock;
        private Mock<FakeDurableTaskClient> _mockDurableTaskClient;
        private Mock<ILogger<ArchiveFailActivity>> _mockLogger;
        private ArchiveFailActivity _archiveFailActivity;
        private Mock<IServiceProvider> _serviceProvider;
        private Mock<FunctionContext> _mockFunctionContext;

        [SetUp]
        public void SetUp()
        {
            _entityHelperMock = new Mock<IEntityHelper>();
            _mockDurableTaskClient = new Mock<FakeDurableTaskClient>();
            _mockLogger = new Mock<ILogger<ArchiveFailActivity>>();
            _serviceProvider = new Mock<IServiceProvider>();
            _mockFunctionContext = new Mock<FunctionContext>();

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<ArchiveFailActivity>)))
                         .Returns(_mockLogger.Object);

            _mockFunctionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _archiveFailActivity = new ArchiveFailActivity(_entityHelperMock.Object, _mockLogger.Object);
        }

        [Test]
        public async Task StartArchiveFailActivity_Should_Log_Information_And_Update_Status()
        {
            // Arrange
            var response = new PeriodEndArchiveActivityResponse
            {
                InstanceId = "test-instance-id"
            };

            var currentJob = new ArchiveRunInformation
            {
                JobId = "test-job-id",
                InstanceId = "test-instance-id"
            };

            _entityHelperMock.Setup(x => x.GetCurrentJobs(It.IsAny<DurableTaskClient>())).ReturnsAsync(currentJob);

            // Act
            await _archiveFailActivity.StartArchiveFailActivity(response, _mockFunctionContext.Object, _mockDurableTaskClient.Object);

            // Assert
            _entityHelperMock.Verify(x => x.UpdateCurrentJobStatus(It.IsAny<DurableTaskClient>(), It.IsAny<ArchiveRunInformation>(), EntityState.add), Times.Once);
        }
    }
}
