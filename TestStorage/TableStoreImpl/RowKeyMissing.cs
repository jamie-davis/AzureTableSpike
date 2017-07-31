using System;

namespace TestStorage.TableStoreImpl
{
    [Serializable]
    internal class RowKeyMissing : Exception
    {
        public RowKeyMissing()
        {
        }

        public RowKeyMissing(string message) : base(message)
        {
        }

        public RowKeyMissing(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}