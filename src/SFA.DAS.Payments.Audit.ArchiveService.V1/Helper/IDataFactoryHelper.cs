using Microsoft.Azure.Management.DataFactory;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Helper
{
    public interface IDataFactoryHelper
    {
        Task<DataFactoryManagementClient> CreateClientAsync();
    }
}