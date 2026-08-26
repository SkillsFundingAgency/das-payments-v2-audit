using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.Extensions
{
    public static class HandleCurrentJobId
    {
        public const string PeriodEndArchiveEntityName = "CurrentPeriodEndArchiveJobId";

        [Function(nameof(Handle))]
        public static Task Handle([EntityTrigger] TaskEntityDispatcher dispatcher)
        {
            return dispatcher.DispatchAsync(operation =>
            {
                switch (operation.Name.ToLowerInvariant())
                {
                    case "add":
                        var newJobId = operation.GetInput<ArchiveRunInformation>();
                        operation.State.SetState(newJobId);
                        break;
                    case "reset":
                        operation.State.SetState(new ArchiveRunInformation());
                        break;
                    case "get":
                        return new(operation.State.GetState<ArchiveRunInformation>());
                }

                return default;
            });
        }
    }
}
