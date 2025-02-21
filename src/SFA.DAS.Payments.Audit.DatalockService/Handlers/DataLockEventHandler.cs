using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.DataLocks.Messages.Events;

namespace SFA.DAS.Payments.Audit.DataLockService.Handlers
{
    public class DataLockEventHandler : IHandleMessageBatches<DataLockEvent>
    {
        private readonly IPaymentLogger logger;

        public DataLockEventHandler(IPaymentLogger logger)
        {
            this.logger = logger;
        }

        public async Task Handle(IList<DataLockEvent> messages, CancellationToken cancellationToken)
        {
            foreach (var message in messages)
            {
                logger.LogInfo($"Handling message {message.GetType().Name} with JobId {message.JobId} and EventId {message.EventId}");
                await Task.CompletedTask;
            }
        }
    }
}
