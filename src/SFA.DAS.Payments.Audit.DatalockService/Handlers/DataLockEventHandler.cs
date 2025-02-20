using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.DataLocks.Messages.Events;

namespace SFA.DAS.Payments.Audit.DataLockService.Handlers
{
    public class DataLockEventHandler : IHandleMessageBatches<DataLockEvent>
    {
        public async Task Handle(IList<DataLockEvent> messages, CancellationToken cancellationToken)
        {
            foreach (var message in messages)
            {
                Console.WriteLine($"Handling message {message.GetType().Name} with JobId {message.JobId} and EventId {message.EventId}");
                await Task.CompletedTask;
            }
        }
    }
}
