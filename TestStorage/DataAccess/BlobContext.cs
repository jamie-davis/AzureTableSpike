using AzureStorage;
using TestStorage.TableStoreImpl;

namespace TestStorage.DataAccess
{
    public class BlobContext : IBlobContext
    {
        private BlobStore _blobStore;

        public BlobContext(BlobStore blobStore)
        {
            _blobStore = blobStore;
        }

        #region Implementation of IBlobContext

        public IBlobContainer GetContainer(string containerName)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
