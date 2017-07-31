using AzureStorage;
using TestStorage.DataAccess;
using TestStorage.TableStoreImpl;
using TableContext = AzureStorage.DataAccess.TableContext;

namespace asSpike.Commands
{
    static class TableContextFactory
    {
        private static TableStore _tableStore = new TableStore();

        public static ITableContext Get(BaseCommand command, string table, bool tryCreate = false)
        {
            if (!command.FakeDataStore)
            {
                var connectionString = command.UseDevStorage
                    ? Properties.Settings.Default.EmulatorConnectionString
                    : Properties.Settings.Default.ConnectionString;

                return new TableContext(table, connectionString, tryCreate);
            }

            return new TestStorage.DataAccess.TableContext(table, _tableStore);
        }
    }
}
