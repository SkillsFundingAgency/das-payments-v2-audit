using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Azure.Core.Serialization;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using SFA.DAS.Payments.Audit.ArchiveService.Triggers;
using SFA.DAS.Payments.Audit.ArchiveService.UnitTests.TestDoubles;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests.Triggers
{
    [TestFixture]
    public class PeriodEndArchiveHttpTriggerTests
    {
        [SetUp]
        public void Setup()
        {
            mocker = AutoMock.GetLoose();
            mockClient = new Mock<DurableTaskClient>("TestClient");
            mockEntityClient = new Mock<DurableEntityClient>("TestClient");
            mockClient.Setup(x => x.Entities).Returns(mockEntityClient.Object);
            logger = mocker.Mock<IPaymentLogger>();
            triggerHelper = new TriggerHelper();
        }

        private Mock<IPaymentLogger> logger;
        private Mock<DurableTaskClient> mockClient;
        private Mock<DurableEntityClient> mockEntityClient;
        private AutoMock mocker;
        private ITriggerHelper triggerHelper;

        private PeriodEndArchiveHttpTrigger CreateTrigger()
        {
            return new PeriodEndArchiveHttpTrigger(triggerHelper, logger.Object);
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesPostRequest_ThenOrchestratorIsStarted()
        {
            var req = SetupHttpPostRequest();

            SetupRunningInstances(false);
            SetupStartOrchestration("1234");
            SetupMockRunInformation();

            var response = await CreateTrigger().HttpStart(req, mockClient.Object);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Test]
        public void WhenHttpTrigger_ReceivesPostRequest_WithoutContent_ThenOrchestratorIsNotStarted()
        {
            var req = SetupHttpRequest(HttpMethod.Post, "orchestrators/PeriodEndArchiveOrchestrator", body: null);

            SetupMockRunInformation();

            Func<Task> act = async () => await CreateTrigger().HttpStart(req, mockClient.Object);
            act.Should().ThrowAsync<Exception>()
                .WithMessage("Error in PeriodEndArchiveHttpTrigger. Request content is null. Request: *");
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesPostRequest_AndInstancesAlreadyExist_ThenOrchestratorIsNotStarted()
        {
            var req = SetupHttpPostRequest();

            SetupStartOrchestration("1234");
            SetupRunningInstances(true);
            SetupMockRunInformation();

            var response = await CreateTrigger().HttpStart(req, mockClient.Object);
            var content = ReadBody(response);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            content.Should().Be("An instance of PeriodEndArchiveOrchestrator is already running.");
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesPostRequest_AndInstanceFailsToReturn_ThenErrorIsReceived()
        {
            var req = SetupHttpPostRequest();
            const string orchestratorName = nameof(Orchestrators.PeriodEndArchiveOrchestrator);

            SetupRunningInstances(false);
            SetupStartOrchestration(string.Empty);
            SetupMockRunInformation();

            var response = await CreateTrigger().HttpStart(req, mockClient.Object);
            var content = ReadBody(response);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            content.Should().Be($"An error occurred starting [{orchestratorName}], no instance id was returned.");
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesGetRequest_AndJobId_DoesNotMatch_ShouldReturn_QueuedStatus()
        {
            var req = SetupHttpGetRequest("2345");

            SetupRunningInstances(false);
            SetupMockRunInformation();

            var response = await CreateTrigger().HttpStart(req, mockClient.Object);
            var content = ReadBody(response);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should()
                .Be("{\"InstanceId\":\"\",\"JobId\":\"2345\",\"Status\":\"Queued\"}");
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesGetRequest_AndJobId_DoesMatch_ShouldReturn_CurrentStatus()
        {
            var req = SetupHttpGetRequest("1234");

            SetupRunningInstances(false);
            SetupMockRunInformation();

            var response = await CreateTrigger().HttpStart(req, mockClient.Object);
            var content = ReadBody(response);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should()
                .Be("{\"InstanceId\":null,\"JobId\":\"1234\",\"Status\":\"Success\"}");
        }

        [Test]
        public async Task
            WhenHttpTrigger_ReceivesGetRequest_AndJobIdArgument_HasNotBeenPassed_ShouldThrowException()
        {
            var req = SetupHttpGetRequest(null);

            SetupRunningInstances(false);
            SetupMockRunInformation();

            var response = await CreateTrigger().HttpStart(req, mockClient.Object);
            var content = ReadBody(response);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            content.Should().Contain("Error in PeriodEndArchiveHttpTrigger. Invalid jobId.");
        }

        [Test]
        public async Task
            WhenHttpTrigger_ReceivesGetRequest_AndJobIdValue_IsNotValidLong_ShouldThrowException()
        {
            var req = SetupHttpGetRequest("abcd");

            SetupRunningInstances(false);
            SetupMockRunInformation();

            var response = await CreateTrigger().HttpStart(req, mockClient.Object);
            var content = ReadBody(response);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            content.Should().Contain("Error in PeriodEndArchiveHttpTrigger. Invalid jobId.");
        }

        private static FakeFunctionContext CreateFunctionContext()
        {
            var services = new ServiceCollection();
            services.Configure<WorkerOptions>(o => o.Serializer = new JsonObjectSerializer());
            return new FakeFunctionContext(services.BuildServiceProvider());
        }

        private static FakeHttpRequestData SetupHttpRequest(string method, string route, string body)
        {
            var uri = new Uri($"http://localhost:7071/{route}");
            Stream stream = body == null ? null : new MemoryStream(Encoding.UTF8.GetBytes(body));
            return new FakeHttpRequestData(CreateFunctionContext(), uri, stream, method);
        }

        private static class HttpMethod
        {
            public const string Post = "POST";
            public const string Get = "GET";
        }

        private static FakeHttpRequestData SetupHttpPostRequest()
        {
            var model = new RecordPeriodEndFcsHandOverCompleteJob { CollectionPeriod = 11, CollectionYear = 2223 };
            return SetupHttpRequest(HttpMethod.Post, "orchestrators/PeriodEndArchiveOrchestrator",
                JsonConvert.SerializeObject(model));
        }

        private static FakeHttpRequestData SetupHttpGetRequest(string jobId)
        {
            var route = string.IsNullOrEmpty(jobId)
                ? "orchestrators/PeriodEndArchiveOrchestrator"
                : $"orchestrators/PeriodEndArchiveOrchestrator?jobId={jobId}";
            return SetupHttpRequest(HttpMethod.Get, route, null);
        }

        private static string ReadBody(Microsoft.Azure.Functions.Worker.Http.HttpResponseData response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            return reader.ReadToEnd();
        }

        private void SetupStartOrchestration(string runId)
        {
            mockClient
                .Setup(x => x.ScheduleNewOrchestrationInstanceAsync(
                    It.IsAny<TaskName>(), It.IsAny<object>(), It.IsAny<StartOrchestrationOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(runId);
        }

        private void SetupRunningInstances(bool hasRunningInstances)
        {
            var metadata = hasRunningInstances
                ? new[] { new OrchestrationMetadata("PeriodEndArchiveOrchestrator", "Instance01") }
                : Array.Empty<OrchestrationMetadata>();

            mockClient
                .Setup(x => x.GetAllInstancesAsync(It.IsAny<OrchestrationQuery>()))
                .Returns(Pageable.Create<OrchestrationMetadata>((_, __) =>
                    Task.FromResult(new Page<OrchestrationMetadata>(metadata))));
        }

        private void SetupMockRunInformation(string jobId = "1234")
        {
            mockEntityClient
                .Setup(x => x.GetEntityAsync<ArchiveRunInformation>(It.IsAny<EntityInstanceId>(),
                    It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EntityMetadata<ArchiveRunInformation>(
                    StatusHelper.GetEntityId(), new ArchiveRunInformation { JobId = jobId, Status = "Success" }));
        }
    }
}
