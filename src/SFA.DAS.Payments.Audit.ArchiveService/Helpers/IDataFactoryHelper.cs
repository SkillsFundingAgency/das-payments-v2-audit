using Microsoft.Azure.Management.DataFactory;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helpers
{
    public interface IDataFactoryHelper
    {
        Task<DataFactoryManagementClient> CreateClientAsync();
    }
}