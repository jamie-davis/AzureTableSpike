using System;
using AzureStorage.DataAccess;

namespace asSpike.Entities
{
    public class Dinner : IKeyAccess
    {
        public string Where { get; set; }
        public DateTime WhenDidItStart { get; set; }
        public Guid Key { get; set; }

        public string GetPartitionKey()
        {
            return WhenDidItStart.ToString("u");
        }

        public string GetRowKey()
        {
            return Key.ToString();
        }
    }
}
