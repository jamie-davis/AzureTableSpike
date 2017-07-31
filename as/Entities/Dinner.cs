using AzureStorage;
using System;
using AzureStorage.DataAccess;

namespace asSpike.Entities
{
    public class Dinner : BaseTableEntity
    {
        public string Where { get; set; }
        public DateTime WhenDidItStart { get; set; }
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
