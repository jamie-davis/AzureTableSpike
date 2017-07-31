using System;

namespace TestStorage.TableStoreImpl
{
    [Serializable]
    internal class PartitionKeyMissing : Exception
    {
        public PartitionKeyMissing()
        {
        }

        public PartitionKeyMissing(string message) : base(message)
        {
        }

        public PartitionKeyMissing(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}