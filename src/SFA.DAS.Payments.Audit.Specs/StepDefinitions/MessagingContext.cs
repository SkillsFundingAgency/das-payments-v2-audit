using System.Runtime.Serialization;

namespace SFA.DAS.Payments.Audit.Specs.StepDefinitions
{
    public enum EndpointLocation
    {
        RequiredPayments
    }
    public class MessagingContext
    {
        private IEndpointInstance endpointInstance;

        public MessagingContext(IEndpointInstance endpointInstance)
        {
            this.endpointInstance = endpointInstance;
        }

        public async Task Send<T>(T message, EndpointLocation location)
        {
            await endpointInstance.Send($"sfa-das-payments-audit-{location.ToString()}", message);
        }
    }
}