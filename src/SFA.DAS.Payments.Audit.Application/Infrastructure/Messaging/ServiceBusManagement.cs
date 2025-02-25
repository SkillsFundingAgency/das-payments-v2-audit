using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.Audit.Application.Infrastructure.Messaging;

public interface IServiceBusManagement
{
    Task EnsureSubscriptionRule<T>(CancellationToken cancellationToken);
    Task EnsureSubscriptionRule(Type type, CancellationToken cancellationToken);
}

public class ServiceBusManagement : IServiceBusManagement
{
    private readonly ManagementClient managementClient;
    private readonly string endpointName;
    private readonly IPaymentLogger logger;
    private const string TopicPath = "bundle-1";

    public ServiceBusManagement(string connectionString, string endpointName, IPaymentLogger logger)
    {
        this.managementClient = new ManagementClient(connectionString);
        this.endpointName = endpointName ?? throw new ArgumentNullException(nameof(endpointName));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EnsureSubscriptionRule<T>(CancellationToken cancellationToken)
    {
        await EnsureSubscriptionRule(typeof(T), cancellationToken).ConfigureAwait(false);
    }

    public async Task EnsureSubscriptionRule(Type type, CancellationToken cancellationToken)
    {
        var subscription = await GetOrCreateSubscription(cancellationToken).ConfigureAwait(false);
        var rules = await GetExistingRules(cancellationToken).ConfigureAwait(false);

        if (rules.Any(rule => rule.Name.Equals(type.Name)))
        {
            logger.LogDebug($"The subscription rule for type {type.Name} already exists.");
            return;
        }

        await CreateNewSubscriptionRule(type, cancellationToken).ConfigureAwait(false);
        logger.LogInfo($"Created a new subscription rule for type {type.Name}");
    }

    private async Task CreateNewSubscriptionRule(Type type, CancellationToken cancellationToken)
    {
        var ruleDescription = new RuleDescription
        {
            Filter = new SqlFilter($"[NServiceBus.EnclosedMessageTypes] LIKE '%{type.FullName}%'"),
            Name = type.Name
        };

        await managementClient.CreateRuleAsync(TopicPath, endpointName, ruleDescription, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IList<RuleDescription>> GetExistingRules(CancellationToken cancellationToken)
    {
        return await managementClient.GetRulesAsync(TopicPath, endpointName, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<SubscriptionDescription> GetOrCreateSubscription(CancellationToken cancellationToken)
    {
        if (!await managementClient.TopicExistsAsync(TopicPath, cancellationToken).ConfigureAwait(false))
        {
            logger.LogDebug($"Creating new topic: {TopicPath}.");
            await managementClient.CreateTopicAsync(new TopicDescription(TopicPath)
            {
                AutoDeleteOnIdle = TimeSpan.FromDays(3),
                DefaultMessageTimeToLive = TimeSpan.FromDays(3),
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10),
                EnableBatchedOperations = true,
                EnablePartitioning = false,
                Path = TopicPath,
                SupportOrdering = false,
                MaxSizeInMB = 1024,
                RequiresDuplicateDetection = false,
            }, cancellationToken).ConfigureAwait(false);
            logger.LogInfo($"Created new topic: {TopicPath}");
        }

        SubscriptionDescription subscriptionDescription;
        if (!await managementClient.SubscriptionExistsAsync(TopicPath, endpointName, cancellationToken).ConfigureAwait(false))
        {
            logger.LogDebug("Subscription not found");
            subscriptionDescription = new SubscriptionDescription(TopicPath, endpointName)
            {
                ForwardTo = endpointName,
                UserMetadata = endpointName,
                EnableBatchedOperations = true,
                MaxDeliveryCount = Int32.MaxValue,
                EnableDeadLetteringOnFilterEvaluationExceptions = false,
                LockDuration = TimeSpan.FromMinutes(5)
            };
            var defaultRule = new RuleDescription("$default") { Filter = new SqlFilter("1=0") };
            await managementClient.CreateSubscriptionAsync(subscriptionDescription, defaultRule, cancellationToken).ConfigureAwait(false);
            logger.LogInfo($"Created new subscription for endpoint: {endpointName}");
        }
        else
        {
            subscriptionDescription = await managementClient.GetSubscriptionAsync(TopicPath, endpointName, cancellationToken).ConfigureAwait(false);
        }
        logger.LogInfo($"Finished ensuring the topic and subscription for endpoint: {endpointName}");
        return subscriptionDescription;
    }
}
