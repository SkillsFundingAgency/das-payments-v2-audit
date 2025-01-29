using Microsoft.Azure.Functions.Worker.Http;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests
{
    public interface IDurableTaskClientWrapper
    {
        Task<HttpResponseData> CreateCheckStatusResponseAsync(HttpRequestData request, string instanceId, CancellationToken cancellationToken);
    }

}