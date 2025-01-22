using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Models;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.EntityTrigger
{
    public class EntityDispatcher
    {
        public const string PeriodEndArchiveEntityName = "CurrentPeriodEndArchiveJobId";

        [Function(nameof(EntityDispatcher))]
        public static Task DispatchAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
        {
            return dispatcher.DispatchAsync(operation =>
            {
                if (operation.State.GetState(typeof(ArchiveRunInformation)) is null)
                {
                    operation.State.SetState(new ArchiveRunInformation());
                }

                switch (operation.Name.ToLowerInvariant())
                {
                    case "add":
                        var state = operation.State.GetState<ArchiveRunInformation>();
                        var input = operation.GetInput<ArchiveRunInformation>();
                        state = ArchiveRunInformationHelper.Merge(state, input);
                        operation.State.SetState(state);
                        return new(state);
                    case "reset":
                        operation.State.SetState(new ArchiveRunInformation());
                        break;
                    case "get":
                        return new(operation.State.GetState<ArchiveRunInformation>());
                    case "delete":
                        operation.State.SetState(null);
                        break;
                }

                return default;
            });
        }
    }
}
