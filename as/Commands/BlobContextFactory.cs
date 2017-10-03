using AzureStorage;
using AzureStorage.DataAccess;
using TestStorage.TableStoreImpl;

namespace asSpike.Commands
{
    static class BlobContextFactory
    {
        private static BlobStore _blobStore = new BlobStore();

        public static IBlobContext Get(BaseCommand command, bool tryCreate = false)
        {
            if (!command.FakeDataStore)
            {
                var connectionString = command.UseDevStorage
                    ? Properties.Settings.Default.EmulatorConnectionString
                    : Properties.Settings.Default.ConnectionString;

                return new BlobContext(connectionString, tryCreate);
            }

            return new TestStorage.DataAccess.BlobContext(_blobStore);
        }
    }
}