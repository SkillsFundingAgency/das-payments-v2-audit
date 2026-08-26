using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.Payments.Audit.ArchiveService.UnitTests.TestDoubles
{
    public class FakeFunctionContext : FunctionContext
    {
        public FakeFunctionContext(IServiceProvider instanceServices)
        {
            InstanceServices = instanceServices;
        }

        public override string InvocationId { get; } = Guid.NewGuid().ToString();
        public override string FunctionId { get; } = Guid.NewGuid().ToString();
        public override TraceContext TraceContext { get; }
        public override BindingContext BindingContext { get; }
        public override RetryContext RetryContext { get; }
        public override IServiceProvider InstanceServices { get; set; }
        public override FunctionDefinition FunctionDefinition { get; }
        public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
        public override IInvocationFeatures Features { get; }
    }
}
