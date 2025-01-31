namespace SFA.DAS.Payments.Audit.ArchiveService.Configuration
{
    public interface IAppSettingsOptions
    {
        bool IsEncrypted { get; set; }
        Values Values { get; set; }
    }
}
