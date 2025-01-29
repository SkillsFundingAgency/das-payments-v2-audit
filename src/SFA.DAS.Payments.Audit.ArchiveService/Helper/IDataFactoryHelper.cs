using Microsoft.Azure.Management.DataFactory;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helper
{
    public interface IDataFactoryHelper
    {
        Task<DataFactoryManagementClient> CreateClientAsync();
    }
}