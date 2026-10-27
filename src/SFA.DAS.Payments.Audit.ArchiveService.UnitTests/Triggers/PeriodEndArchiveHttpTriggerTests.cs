using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using SFA.DAS.Payments.Audit.ArchiveService.Triggers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests.Triggers
{
    [TestFixture]
    public class PeriodEndArchiveHttpTriggerTests
    {
        private Mock<IPaymentLogger> _logger = null!;
        private Mock<ITriggerHelper> _triggerHelper = null!;
        private Mock<DurableTaskClient> _durableClient = null!;

        private PeriodEndArchiveHttpTrigger _sut = null!;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<IPaymentLogger>();
            _triggerHelper = new Mock<ITriggerHelper>();

            // DurableTaskClient is abstract and requires a client name.
            _durableClient = new Mock<DurableTaskClient>("TestClient");

            _sut = new PeriodEndArchiveHttpTrigger(
                _logger.Object,
                _triggerHelper.Object);
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesPostRequest_ThenTriggerHelperIsCalled()
        {
            // Arrange
            var request = CreateRequest(
                "POST",
                "http://localhost:7071/api/orchestrators/PeriodEndArchiveOrchestrator",
                "{}");

            var expectedResponse = CreateResponse(
                request.FunctionContext,
                HttpStatusCode.Accepted);

            _triggerHelper
                .Setup(x => x.StartOrchestrator(
                    request.Request.Object,
                    _durableClient.Object,
                    _logger.Object))
                .ReturnsAsync(expectedResponse.Response.Object);

            // Act
            var response = await _sut.HttpStart(
                request.Request.Object,
                _durableClient.Object);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            _triggerHelper.Verify(
                x => x.StartOrchestrator(
                    request.Request.Object,
                    _durableClient.Object,
                    _logger.Object),
                Times.Once);
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesPostRequest_ThenGetStatusLogicIsNotExecuted()
        {
            // Arrange
            var request = CreateRequest(
                "POST",
                "http://localhost:7071/api/orchestrators/PeriodEndArchiveOrchestrator",
                "{}");

            var expectedResponse = CreateResponse(
                request.FunctionContext,
                HttpStatusCode.Accepted);

            _triggerHelper
                .Setup(x => x.StartOrchestrator(
                    It.IsAny<HttpRequestData>(),
                    It.IsAny<DurableTaskClient>(),
                    It.IsAny<IPaymentLogger>()))
                .ReturnsAsync(expectedResponse.Response.Object);

            // Act
            var response = await _sut.HttpStart(
                request.Request.Object,
                _durableClient.Object);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            _triggerHelper.Verify(
                x => x.StartOrchestrator(
                    It.IsAny<HttpRequestData>(),
                    It.IsAny<DurableTaskClient>(),
                    It.IsAny<IPaymentLogger>()),
                Times.Once);
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesGetRequest_WithoutJobId_ShouldReturnInternalServerError()
        {
            // Arrange
            var request = CreateRequest(
                "GET",
                "http://localhost:7071/api/orchestrators/PeriodEndArchiveOrchestrator");

            // Act
            var response = await _sut.HttpStart(
                request.Request.Object,
                _durableClient.Object);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should()
                .Be(HttpStatusCode.InternalServerError);

            var content = await ReadResponseBody(response);

            content.Should().Be(
                "Error in PeriodEndArchiveHttpTrigger. Invalid jobId.");

            _triggerHelper.Verify(
                x => x.StartOrchestrator(
                    It.IsAny<HttpRequestData>(),
                    It.IsAny<DurableTaskClient>(),
                    It.IsAny<IPaymentLogger>()),
                Times.Never);
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesGetRequest_WithEmptyJobId_ShouldReturnInternalServerError()
        {
            // Arrange
            var request = CreateRequest(
                "GET",
                "http://localhost:7071/api/orchestrators/PeriodEndArchiveOrchestrator?jobId=");

            // Act
            var response = await _sut.HttpStart(
                request.Request.Object,
                _durableClient.Object);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should()
                .Be(HttpStatusCode.InternalServerError);

            var content = await ReadResponseBody(response);

            content.Should().Be(
                "Error in PeriodEndArchiveHttpTrigger. Invalid jobId.");
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesGetRequest_WithNonNumericJobId_ShouldReturnInternalServerError()
        {
            // Arrange
            var request = CreateRequest(
                "GET",
                "http://localhost:7071/api/orchestrators/PeriodEndArchiveOrchestrator?jobId=abcd");

            // Act
            var response = await _sut.HttpStart(
                request.Request.Object,
                _durableClient.Object);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should()
                .Be(HttpStatusCode.InternalServerError);

            var content = await ReadResponseBody(response);

            content.Should().Be(
                "Error in PeriodEndArchiveHttpTrigger. Invalid jobId.");
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesGetRequest_WithInvalidJobId_ShouldLogError()
        {
            // Arrange
            var request = CreateRequest(
                "GET",
                "http://localhost:7071/api/orchestrators/PeriodEndArchiveOrchestrator?jobId=abcd");

            // Act
            await _sut.HttpStart(
                request.Request.Object,
                _durableClient.Object);

            // Assert
            _logger.Verify(
            x => x.LogError(
                "Error in PeriodEndArchiveHttpTrigger",
                It.Is<Exception>(e =>
                    e.Message ==
                    "Error in PeriodEndArchiveHttpTrigger. Invalid jobId."),
                It.IsAny<object[]>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
        }

        [Test]
        public async Task WhenHttpTrigger_ReceivesGetRequest_ThenTriggerHelperIsNotCalled()
        {
            // Arrange
            var request = CreateRequest(
                "GET",
                "http://localhost:7071/api/orchestrators/PeriodEndArchiveOrchestrator?jobId=abcd");

            // Act
            await _sut.HttpStart(
                request.Request.Object,
                _durableClient.Object);

            // Assert
            _triggerHelper.Verify(
                x => x.StartOrchestrator(
                    It.IsAny<HttpRequestData>(),
                    It.IsAny<DurableTaskClient>(),
                    It.IsAny<IPaymentLogger>()),
                Times.Never);
        }

        private static RequestTestContext CreateRequest(
            string method,
            string url,
            string? body = null)
        {
            var functionContext = new Mock<FunctionContext>();

            var request = new Mock<HttpRequestData>(
                functionContext.Object);

            request
                .SetupGet(x => x.Method)
                .Returns(method);

            request
                .SetupGet(x => x.Url)
                .Returns(new Uri(url));

            request
                .SetupGet(x => x.Body)
                .Returns(new MemoryStream(
                    Encoding.UTF8.GetBytes(body ?? string.Empty)));

            request
                .Setup(x => x.CreateResponse())
                .Returns(() =>
                    CreateResponse(
                        functionContext,
                        HttpStatusCode.OK)
                    .Response.Object);

            return new RequestTestContext(
                functionContext,
                request);
        }

        private static ResponseTestContext CreateResponse(
            Mock<FunctionContext> functionContext,
            HttpStatusCode statusCode)
        {
            var response = new Mock<HttpResponseData>(
                functionContext.Object);

            response
                .SetupProperty(
                    x => x.StatusCode,
                    statusCode);

            response
                .SetupProperty(
                    x => x.Body,
                    new MemoryStream());

            response
                .SetupProperty(
                    x => x.Headers,
                    new HttpHeadersCollection());

            return new ResponseTestContext(response);
        }

        private static async Task<string> ReadResponseBody(
            HttpResponseData response)
        {
            response.Body.Position = 0;

            using var reader = new StreamReader(
                response.Body,
                Encoding.UTF8,
                leaveOpen: true);

            return await reader.ReadToEndAsync();
        }

        private sealed class RequestTestContext
        {
            public RequestTestContext(
                Mock<FunctionContext> functionContext,
                Mock<HttpRequestData> request)
            {
                FunctionContext = functionContext;
                Request = request;
            }

            public Mock<FunctionContext> FunctionContext { get; }

            public Mock<HttpRequestData> Request { get; }
        }

        private sealed class ResponseTestContext
        {
            public ResponseTestContext(
                Mock<HttpResponseData> response)
            {
                Response = response;
            }

            public Mock<HttpResponseData> Response { get; }
        }
    }
}