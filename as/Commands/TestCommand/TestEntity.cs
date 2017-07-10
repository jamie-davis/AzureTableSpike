using AzureStorage.DataAccess;

namespace asSpike.Commands.TestCommand
{
    public class TestEntity : BaseTableEntity
    {
        public override string GetPartitionKey()
        {
            return string.Empty;
        }

        public override string GetRowKey()
        {
            return string.Empty;
        }

        public string Alpha { get; set; }
        public string Beta { get; set; }
        public string Charlie { get; set; }
        public string Delta { get; set; }
        public string Echo { get; set; }
    }
}