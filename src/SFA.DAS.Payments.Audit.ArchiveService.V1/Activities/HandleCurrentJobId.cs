using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Activities
{
    public static class HandleCurrentJobId
    {
        public const string PeriodEndArchiveEntityName = "CurrentPeriodEndArchiveJobId";

        [Function(nameof(Handle))]
        public static void Handle([EntityTrigger] FunctionContext context)
        {
            var state = context.BindingContext.BindingData["state"] as ArchiveRunInformation;
            var operation = context.BindingContext.BindingData["operationName"].ToString().ToLowerInvariant();

            switch (operation)
            {
                case "add":
                    var newJobId = context.BindingContext.BindingData["input"] as ArchiveRunInformation;
                    //context.BindingContext.BindingData["state"] = newJobId;
                    break;
                case "reset":
                    //context.BindingContext.BindingData["state"] = new ArchiveRunInformation();
                    break;
                case "get":
                    //context.BindingContext.BindingData["return"] = state;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown operation: {operation}");
            }
        }
    }
}
