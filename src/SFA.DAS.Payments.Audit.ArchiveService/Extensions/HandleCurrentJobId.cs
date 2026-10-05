using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using SFA.DAS.Payments.Model.Core.Audit;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.Audit.ArchiveService.Extensions
{
    public class HandleCurrentJobId : TaskEntity<ArchiveRunInformation>
    {
        public const string PeriodEndArchiveEntityName =
            "CurrentPeriodEndArchiveJobId";

        public void Add(ArchiveRunInformation value)
        {
            State = value;
        }

        public void Reset()
        {
            State = new ArchiveRunInformation();
        }

        [Function(nameof(HandleCurrentJobId))]
        public static Task Run(
            [EntityTrigger] TaskEntityDispatcher dispatcher)
        {
            return dispatcher.DispatchAsync<HandleCurrentJobId>();
        }
    }
}