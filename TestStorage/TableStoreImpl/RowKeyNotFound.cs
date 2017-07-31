using System;

namespace TestStorage.TableStoreImpl
{
    [Serializable]
    internal class RowKeyNotFound : Exception
    {
        public RowKeyNotFound()
        {
        }

        public RowKeyNotFound(string message) : base(message)
        {
        }

        public RowKeyNotFound(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}