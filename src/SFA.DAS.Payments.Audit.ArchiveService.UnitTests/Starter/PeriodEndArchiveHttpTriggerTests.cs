using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Internal;
using SFA.DAS.Payments.Audit.ArchiveService.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.Starter;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests.Starter
{
    [TestFixture]
    public class PeriodEndArchiveHttpTriggerTests
    {
        private Mock<IEntityHelper> _entityHelper;
        private Mock<FunctionContext> _functionContext;
        private Mock<IServiceProvider> _serviceProvider;
        private Mock<ILogger<PeriodEndArchiveHttpTrigger>> _logger;
        private Mock<FakeDurableTaskClient> _mockDurableTaskClient;
        private Mock<IDurableTaskClientWrapper> _durableTaskClientWrapper;
        private string getUrl = "http://localhost:7044/api/orchestrators/PeriodEndArchiveOrchestrator?jobid=1879876";

        public PeriodEndArchiveHttpTrigger periodEndArchiveHttpTrigger { get; set; }

        [SetUp]
        public void Setup()
        {
            _entityHelper = new Mock<IEntityHelper>();
            _functionContext = new Mock<FunctionContext>();
            _serviceProvider = new Mock<IServiceProvider>();
            _logger = new Mock<ILogger<PeriodEndArchiveHttpTrigger>>();
            _mockDurableTaskClient = new Mock<FakeDurableTaskClient>();
            _durableTaskClientWrapper = new Mock<IDurableTaskClientWrapper>();

            periodEndArchiveHttpTrigger = new PeriodEndArchiveHttpTrigger(_entityHelper.Object, _logger.Object);
        }


        [Test]
        public async Task Get_CurrentJobs_IsNull_Status_Is_Queued_Test()
        {
            RecordPeriodEndFcsHandOverCompleteJob input = MockRecordPeriodEndFcsHandOverCompleteJob();

            string request = JsonSerializer.Serialize(input);
            var body = new MemoryStream(Encoding.ASCII.GetBytes(request));
            var requestData = new FakeHttpRequestData(_functionContext.Object, new Uri(getUrl), body);

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<PeriodEndArchiveHttpTrigger>)))
                           .Returns(_logger.Object);
            _functionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDurableTaskClient.Setup(client => client.ScheduleNewOrchestrationInstanceAsync(It.IsAny<TaskName>(), It.IsAny<object>(), CancellationToken.None))
                     .ReturnsAsync("instanceId");

            _durableTaskClientWrapper.Setup(wrapper => wrapper.CreateCheckStatusResponseAsync(
                It.IsAny<HttpRequestData>()
                , It.IsAny<string>()
                , CancellationToken.None)).ReturnsAsync(new FakerHttpResponseData(HttpStatusCode.OK));

            var result = periodEndArchiveHttpTrigger.HttpTriggerArchivePeriodEnd(requestData, _mockDurableTaskClient.Object, _functionContext.Object).Result;


            result.Body.Position = 0;
            using var reader = new StreamReader(result.Body);
            var responseBody = await reader.ReadToEndAsync();
            var response = JsonSerializer.Deserialize<ArchiveRunInformation>(responseBody);

            Assert.That(response.InstanceId, Is.EqualTo(string.Empty));
            Assert.That(response.Status, Is.EqualTo("Queued"));
            Assert.That(response.JobId, Is.EqualTo("1879876"));
        }

        [Test]
        public async Task Get_RequestPayload_Is_Null_Test()
        {
            RecordPeriodEndFcsHandOverCompleteJob input = null;

            string request = JsonSerializer.Serialize(input);
            var body = new MemoryStream(Encoding.ASCII.GetBytes(request));
            var requestData = new FakeHttpRequestData(_functionContext.Object, new Uri(getUrl), body, "POST");

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<PeriodEndArchiveHttpTrigger>)))
                           .Returns(_logger.Object);
            _functionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDurableTaskClient.Setup(client => client.ScheduleNewOrchestrationInstanceAsync(It.IsAny<TaskName>(), It.IsAny<object>(), CancellationToken.None))
                     .ReturnsAsync("instanceId");

            _durableTaskClientWrapper.Setup(wrapper => wrapper.CreateCheckStatusResponseAsync(
                It.IsAny<HttpRequestData>()
                , It.IsAny<string>()
                , CancellationToken.None)).ReturnsAsync(new FakerHttpResponseData(HttpStatusCode.OK));

            var result = periodEndArchiveHttpTrigger.HttpTriggerArchivePeriodEnd(requestData, _mockDurableTaskClient.Object, _functionContext.Object).Result;


            result.Body.Position = 0;
            using var reader = new StreamReader(result.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.That(responseBody, Is.EqualTo("Request payload is null"));
        }

        [Test]
        public async Task Get_MockedArchiveRunInformation_Test()
        {
            RecordPeriodEndFcsHandOverCompleteJob input = MockRecordPeriodEndFcsHandOverCompleteJob();
            var archiveInfo = ArchiveRunInformation();

            string request = JsonSerializer.Serialize(input);
            var body = new MemoryStream(Encoding.ASCII.GetBytes(request));
            var requestData = new FakeHttpRequestData(_functionContext.Object, new Uri(getUrl), body);

            _entityHelper.Setup(a => a.GetCurrentJobs(It.IsAny<FakeDurableTaskClient>())).ReturnsAsync(archiveInfo);

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<PeriodEndArchiveHttpTrigger>)))
                           .Returns(_logger.Object);
            _functionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDurableTaskClient.Setup(client => client.ScheduleNewOrchestrationInstanceAsync(It.IsAny<TaskName>(), It.IsAny<object>(), CancellationToken.None))
                     .ReturnsAsync("instanceId");

            _durableTaskClientWrapper.Setup(wrapper => wrapper.CreateCheckStatusResponseAsync(
                It.IsAny<HttpRequestData>()
                , It.IsAny<string>()
                , CancellationToken.None)).ReturnsAsync(new FakerHttpResponseData(HttpStatusCode.OK));

            var result = periodEndArchiveHttpTrigger.HttpTriggerArchivePeriodEnd(requestData, _mockDurableTaskClient.Object, _functionContext.Object).Result;


            result.Body.Position = 0;
            using var reader = new StreamReader(result.Body);
            var responseBody = await reader.ReadToEndAsync();
            var response = JsonSerializer.Deserialize<ArchiveRunInformation>(responseBody);

            Assert.That(response.InstanceId, Is.EqualTo(archiveInfo.InstanceId));
            Assert.That(response.Status, Is.EqualTo(archiveInfo.Status));
            Assert.That(response.JobId, Is.EqualTo(archiveInfo.JobId));
        }


        [Test]
        public async Task Get_MockedArchiveRunInformation_Where_JobId_Is_NotPassed_Test()
        {
            RecordPeriodEndFcsHandOverCompleteJob input = MockRecordPeriodEndFcsHandOverCompleteJob();
            var archiveInfo = ArchiveRunInformation();

            string getUrl = "http://localhost:7044/api/orchestrators/PeriodEndArchiveOrchestrator?";

            string request = JsonSerializer.Serialize(input);
            var body = new MemoryStream(Encoding.ASCII.GetBytes(request));
            var requestData = new FakeHttpRequestData(_functionContext.Object, new Uri(getUrl), body);

            _entityHelper.Setup(a => a.GetCurrentJobs(It.IsAny<FakeDurableTaskClient>())).ReturnsAsync(archiveInfo);

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<PeriodEndArchiveHttpTrigger>)))
                           .Returns(_logger.Object);
            _functionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDurableTaskClient.Setup(client => client.ScheduleNewOrchestrationInstanceAsync(It.IsAny<TaskName>(), It.IsAny<object>(), CancellationToken.None))
                     .ReturnsAsync("instanceId");

            _durableTaskClientWrapper.Setup(wrapper => wrapper.CreateCheckStatusResponseAsync(
                It.IsAny<HttpRequestData>()
                , It.IsAny<string>()
                , CancellationToken.None)).ReturnsAsync(new FakerHttpResponseData(HttpStatusCode.OK));

            var result = periodEndArchiveHttpTrigger.HttpTriggerArchivePeriodEnd(requestData, _mockDurableTaskClient.Object, _functionContext.Object).Result;


            result.Body.Position = 0;
            using var reader = new StreamReader(result.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.That(responseBody, Is.EqualTo("Method not supported in: PeriodEndArchiveHttpTrigger jobId is not been passed with the request."));
        }

        [Test]
        public async Task Post_Where_CollectionPeriod_Is_0_Test()
        {
            RecordPeriodEndFcsHandOverCompleteJob input = MockRecordPeriodEndFcsHandOverCompleteJob();
            input.CollectionPeriod = 0;

            string request = JsonSerializer.Serialize(input);
            var body = new MemoryStream(Encoding.ASCII.GetBytes(request));
            var requestData = new FakeHttpRequestData(_functionContext.Object, new Uri(getUrl), body, "POST");

            _serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<PeriodEndArchiveHttpTrigger>)))
                           .Returns(_logger.Object);
            _functionContext.Setup(c => c.InstanceServices).Returns(_serviceProvider.Object);

            _mockDurableTaskClient.Setup(client => client.ScheduleNewOrchestrationInstanceAsync(It.IsAny<TaskName>(), It.IsAny<object>(), CancellationToken.None))
                     .ReturnsAsync("instanceId");

            _durableTaskClientWrapper.Setup(wrapper => wrapper.CreateCheckStatusResponseAsync(
                It.IsAny<HttpRequestData>()
                , It.IsAny<string>()
                , CancellationToken.None)).ReturnsAsync(new FakerHttpResponseData(HttpStatusCode.OK));

            var result = periodEndArchiveHttpTrigger.HttpTriggerArchivePeriodEnd(requestData, _mockDurableTaskClient.Object, _functionContext.Object).Result;


            result.Body.Position = 0;
            using var reader = new StreamReader(result.Body);
            var responseBody = await reader.ReadToEndAsync();
            Assert.That(responseBody, Is.EqualTo("Error in PeriodEndArchiveHttpTrigger. CollectionPeriod or CollectionYear is invalid. CollectionPeriod: 0. CollectionYear: 2324"));
        }

        private static ArchiveRunInformation ArchiveRunInformation()
        {
            return new ArchiveRunInformation { InstanceId = "b0165422-1926-475d-8a28-497514fb8301", JobId = "1879876", Status = "Failed" };
        }

        private static RecordPeriodEndFcsHandOverCompleteJob MockRecordPeriodEndFcsHandOverCompleteJob()
        {
            Guid guid = Guid.NewGuid();
            return new RecordPeriodEndFcsHandOverCompleteJob
            {
                CollectionYear = 2324,
                CollectionPeriod = 8,
                StartTime = DateTime.Now,
                GeneratedMessages = new List<GeneratedMessage>
                {
                    new GeneratedMessage
                    {
                        StartTime = DateTime.Now,
                        MessageId = guid,
                        MessageName = "SFA.DAS.Payments.PeriodEnd.Messages.Events.PeriodEndFcsHandOverCompleteEvent"
                    }
                },
                CommandId = guid,
                RequestTime = DateTime.Now,
                JobId = 1879876
            };
        }
    }

}