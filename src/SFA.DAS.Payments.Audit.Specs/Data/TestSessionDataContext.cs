using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Audit.Specs.Data.Configurations;
using SFA.DAS.Payments.Audit.Specs.Models;
using SFA.DAS.Payments.Audit.Specs.StepDefinitions;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.Audit.Specs.Data;

public class TestSessionDataContext : DbContext
{
    private readonly string connectionString;

    public virtual DbSet<Provider> Providers { get; set; }
    public virtual DbSet<PaymentModel> Payment { get; set; }
    public virtual DbSet<RequiredPaymentEventModel> RequiredPaymentEvent { get; set; }

    public TestSessionDataContext(string connectionString)
    {
        this.connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(connectionString, options => options.CommandTimeout(600));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Payments2");
        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentModelConfiguration());
        modelBuilder.ApplyConfiguration(new RequiredPaymentEventModelConfiguration());
    }

    public Provider LeastRecentlyUsed() =>
        Providers.OrderBy(x => x.LastUsed).FirstOrDefault()
        ?? throw new InvalidOperationException("There are no UKPRNs available in the well-known Providers pool.");

    private const string DeleteRequiredPaymentTestData = @"
           DELETE FROM [SFA.DAS.Payments.Database].[Payments2].[RequiredPaymentEvent] WHERE Ukprn = {0} AND LearnerUln = {1} AND JobId = {2}
        ";


    public async Task ClearRequiredPaymentTestData(TestSession testSession)
    {
        await Database.ExecuteSqlRawAsync(DeleteRequiredPaymentTestData, testSession.Learner.Ukprn, testSession.Learner.Uln, testSession.JobId);
    }

}