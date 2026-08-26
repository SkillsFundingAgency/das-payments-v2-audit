using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using SFA.DAS.Payments.Application.Infrastructure.Logging;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helpers
{
    public interface ITriggerHelper
    {
        Task<HttpResponseData> StartOrchestrator(
            HttpRequestData req,
            DurableTaskClient starter,
            IPaymentLogger log);
    }
}
