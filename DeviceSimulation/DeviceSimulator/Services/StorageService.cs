using DeviceSimulator.Interfaces;
using Microsoft.WindowsAzure.Storage;
using System.Fabric;

namespace DeviceSimulator.Services
{
    public class StorageService
        : IStorageService
    {
        public StorageService(StatelessServiceContext context, string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
        }
    }
}
