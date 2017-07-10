namespace AzureStorage.DataAccess
{
    public interface IKeyAccess
    {
        string GetPartitionKey();
        string GetRowKey();
    }
}