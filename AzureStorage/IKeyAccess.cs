namespace AzureStorage
{
    public interface IKeyAccess
    {
        string GetPartitionKey();
        string GetRowKey();
    }
}