using System;

namespace TestStorage.TableStoreImpl
{
    [Serializable]
    internal class RowKeyAlreadyExists : Exception
    {
        public RowKeyAlreadyExists()
        {
        }

        public RowKeyAlreadyExists(string message) : base(message)
        {
        }

        public RowKeyAlreadyExists(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}