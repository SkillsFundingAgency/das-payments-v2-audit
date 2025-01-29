namespace SFA.DAS.Payments.Audit.ArchiveService.Configuration
{
    public interface IAppSettingsOptions
    {
        ConnectionStrings ConnectionStrings { get; set; }
        bool IsEncrypted { get; set; }
        Values Values { get; set; }
    }
}
