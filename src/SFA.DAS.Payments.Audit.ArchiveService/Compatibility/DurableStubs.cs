using System;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class FunctionNameAttribute : Attribute
    {
        public FunctionNameAttribute(string name) { }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class ActivityTriggerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class OrchestrationTriggerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class EntityTriggerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class DurableClientAttribute : Attribute { }
}

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask
{
    public class EntityId
    {
        public EntityId(string functionName, string entityName) { }
    }

    public class EntityStateResponse<T>
    {
        public bool EntityExists { get; set; }
        public T EntityState { get; set; }
    }

    public class DurableOrchestrationStatus
    {
        public DateTime CreatedTime { get; set; }
        public string Name { get; set; }
    }

    public class OrchestrationStatusQueryResult
    {
        public List<DurableOrchestrationStatus> DurableOrchestrationState { get; set; } = new List<DurableOrchestrationStatus>();
    }

    public class OrchestrationStatusQueryCondition
    {
        public string InstanceIdPrefix { get; set; }
        public OrchestrationRuntimeStatus[] RuntimeStatus { get; set; }
    }

    public enum OrchestrationRuntimeStatus
    {
        Pending,
        Running,
        ContinuedAsNew
    }

    public interface IDurableEntityClient
    {
        Task SignalEntityAsync(EntityId entityId, string operationName, object input = null);
        Task<EntityStateResponse<T>> ReadEntityStateAsync<T>(EntityId entityId);
        Task<EntityStateResponse<T>> ReadEntityStateAsync<T>(EntityId entityId, object a, object b);
    }

    public interface IDurableEntityContext { }
    public interface IDurableOrchestrationContext
    {
        T GetInput<T>();
        Task<T> CallActivityAsync<T>(string name, object input);
        Task CallActivityAsync(string name, object input);
        DateTime CurrentUtcDateTime { get; }
        Task CreateTimer(DateTime fireAt, CancellationToken cancellationToken);
    }

    public interface IDurableOrchestrationClient
    {
        Task<string> StartNewAsync(string functionName, string instanceId = null);
        Task<string> StartNewAsync(string functionName, string instanceId, string input);
        HttpResponseMessage CreateCheckStatusResponse(HttpRequestMessage req, string instanceId, bool flag = false);
        Task<OrchestrationStatusQueryResult> ListInstancesAsync(OrchestrationStatusQueryCondition condition, CancellationToken cancellationToken);
    }
}
