namespace SFA.DAS.Payments.Audit.ArchiveService.Configuration
{
    public class AppSettingsOptions : IAppSettingsOptions
    {
        public bool IsEncrypted { get; set; }
        public Values Values { get; set; }
    }

    public class Values
    {
        public string AzureWebJobsStorage { get; set; }
        public string FUNCTIONS_WORKER_RUNTIME { get; set; }
        public string ApplicationInsightsInstrumentationKey { get; set; }
        public string ResourceGroup { get; set; }
        public string AzureDataFactoryName { get; set; }
        public string PipeLine { get; set; }
        public string SubscriptionId { get; set; }
        public string TenantId { get; set; }
        public string ApplicationId { get; set; }
        public string AuthenticationKey { get; set; }
        public int SleepDelay { get; set; }
        public string AuthorityUri { get; set; }
        public string ManagementUri { get; set; }
    }
}
