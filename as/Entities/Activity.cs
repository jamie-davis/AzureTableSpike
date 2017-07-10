using System;
using asSpike.Commands.TestCommand;
using AzureStorage.DataAccess;

namespace asSpike.Entities
{
    public class Activity : BaseTableEntity
    {
        public string Who { get; set; }
        public string What { get; set; }
        public DateTime WhenDidItStart { get; set; }
        public TimeSpan HowLong { get; set; }
        public Guid Key { get; set; }

        public override string GetPartitionKey()
        {
            return WhenDidItStart.ToString("u");
        }

        public override string GetRowKey()
        {
            return Key.ToString();
        }
    }
}
