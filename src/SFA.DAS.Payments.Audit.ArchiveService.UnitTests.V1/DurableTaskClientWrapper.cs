using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests.V1
{
    public class DurableTaskClientWrapper : IDurableTaskClientWrapper
    {
        private readonly DurableTaskClient _durableTaskClient;

        public DurableTaskClientWrapper(DurableTaskClient durableTaskClient)
        {
            _durableTaskClient = durableTaskClient;
        }

        public Task<HttpResponseData> CreateCheckStatusResponseAsync(HttpRequestData request, string instanceId, CancellationToken cancellationToken)
        {
            return _durableTaskClient.CreateCheckStatusResponseAsync(request, instanceId, cancellationToken);
        }
    }

}