using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests
{
    public class FakerHttpResponseData : HttpResponseData
    {
        public FakerHttpResponseData(HttpStatusCode statusCode) : base(new Mock<FunctionContext>().Object)
        {

        }

        public override HttpStatusCode StatusCode { get; set; }
        public override HttpHeadersCollection Headers { get; set; }
        public override Stream Body { get; set; }

        public override HttpCookies Cookies => throw new NotImplementedException();
    }

}