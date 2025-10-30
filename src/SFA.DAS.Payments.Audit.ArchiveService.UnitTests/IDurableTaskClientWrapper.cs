using Microsoft.Azure.Functions.Worker.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests
{
    public interface IDurableTaskClientWrapper
    {
        Task<HttpResponseData> CreateCheckStatusResponseAsync(HttpRequestData request, string instanceId, CancellationToken cancellationToken);
    }

}