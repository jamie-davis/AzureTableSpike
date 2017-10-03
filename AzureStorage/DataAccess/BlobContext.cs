using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorage.DataAccess
{
    public class BlobContext : IBlobContext
    {
        private CloudStorageAccount _cloudStorageAccount;
        private CloudBlobClient _blobClient;

        public BlobContext(string connectionString, bool tryCreate)
        {
            _cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            _blobClient = _cloudStorageAccount.CreateCloudBlobClient();
        }

        #region Implementation of IBlobContext

        public IBlobContainer GetContainer(string containerName)
        {
            var reference = _blobClient.GetContainerReference(containerName);
            reference.CreateIfNotExistsAsync().Wait();

            return new BlobContainer(reference);
        }

        #endregion
    }
}
