using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.Application.Data.EarningEvent;
using SFA.DAS.Payments.ServiceFabric.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing.EarningEvent
{
    public interface IEarningsDuplicateEliminator
    {
        List<EarningEvents.Messages.Events.EarningEvent> RemoveDuplicates(
            List<EarningEvents.Messages.Events.EarningEvent> earningEvents);
    }

    public class EarningsDuplicateEliminator: IEarningsDuplicateEliminator
    {
        private readonly IPaymentLogger logger;

        public EarningsDuplicateEliminator(IPaymentLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public List<EarningEvents.Messages.Events.EarningEvent> RemoveDuplicates(List<EarningEvents.Messages.Events.EarningEvent> earningEvents)
        {
            logger.LogDebug($"Removing duplicates from batch. Batch size: {earningEvents.Count}");

            var uniqueEvents = earningEvents
                .GroupBy(earningEvent => new EarningEventKey(earningEvent).Key)
                .Select(group => group.FirstOrDefault())
                .Where(earningEvent => earningEvent != null)
                .ToList();

            if (uniqueEvents.Count != earningEvents.Count)
                logger.LogInfo($"Removed '{earningEvents.Count - uniqueEvents.Count}' duplicates from the batch.");
            else
                logger.LogDebug("Found no duplicates in the batch.");

            return uniqueEvents;
        }
    }
}