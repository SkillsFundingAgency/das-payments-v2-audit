using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.Audit.RequiredPaymentService.Handlers
{
    public class PeriodisedRequiredPaymentEventHandler : IHandleMessageBatches<PeriodisedRequiredPaymentEvent>
    {
        private readonly IPaymentLogger logger;

        public PeriodisedRequiredPaymentEventHandler(IPaymentLogger logger)
        {
            this.logger = logger;
        }

        public async Task Handle(IList<PeriodisedRequiredPaymentEvent> messages, CancellationToken cancellationToken)
        {
            foreach (var message in messages)
            {
                logger.LogInfo($"Handling message {message.GetType().Name} with JobId {message.JobId} and EventId {message.EventId}");
                await Task.CompletedTask;
            }
        }
    }
}
