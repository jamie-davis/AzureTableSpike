using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestStorage.TableStoreImpl
{
    internal class Table
    {
        private object _lock = new object();
        private Dictionary<string, Partition> _partitions = new Dictionary<string, Partition>();
        private Thread _lockThread;
        private int _lockCount;
        private Table _startOfBatch;

        public Table Clone()
        {
            var partitions = Enumerable.ToDictionary(_partitions, r => r.Key, r => r.Value.Clone());
            return new Table { _partitions = partitions };
        }

        public void Swap(Table clone)
        {
            var partitions = _partitions;
            _partitions = clone._partitions;
            clone._partitions = partitions;
        }

        public TableOwnership TakeOwnership()
        {
            return new TableOwnership(this);
        }

        public void LockToThread(Thread currentThread)
        {
            do
            {
                lock (_lock)
                {
                    if (TryLockThread(currentThread))
                        return;
                }

                Thread.Sleep(20);

            } while (true);
        }

        public void ReleaseLock()
        {
            lock (_lock)
            {
                if (_lockThread == null) return;
                --_lockCount;

                if (_lockCount <= 0)
                    _lockThread = null;
            }
        }

        private bool TryLockThread(Thread currentThread)
        {
            if (_lockThread == null)
            {
                _lockThread = currentThread;
                _lockCount = 1;
                return true;
            }

            if (ReferenceEquals(_lockThread, currentThread))
            {
                ++_lockCount;
                return true;
            }

            return false;
        }

        public void StoreNew(string partitionKey, string rowKey, IDictionary<string, EntityProperty> fields)
        {
            var partition = GetPartition(partitionKey);
            partition.StoreNew(rowKey, fields);
        }

        private Partition GetPartition(string entityPartitionKey)
        {
            if (!_partitions.TryGetValue(entityPartitionKey, out Partition partition))
            {
                partition = new Partition();
                _partitions[entityPartitionKey] = partition;
            }

            return partition;
        }

        public void Update(string partitionKey, string rowKey, IDictionary<string, EntityProperty> fields)
        {
            var partition = GetPartition(partitionKey);
            partition.Update(rowKey, fields);
        }

        public void StoreOrReplace(string partitionKey, string rowKey, IDictionary<string, EntityProperty> fields)
        {
            var partition = GetPartition(partitionKey);
            partition.StoreOrReplace(rowKey, fields);
        }

        public void Delete(string entityPartitionKey, string entityRowKey)
        {
            var partition = GetPartition(entityPartitionKey);
            partition.Delete(entityRowKey);
        }

        public IDictionary<string, EntityProperty> GetFields(string partitionKey, string rowKey)
        {
            var partition = GetPartition(partitionKey);
            return partition.GetFields(rowKey);
        }

        public IEnumerable<IDictionary<string, EntityProperty>> GetRows()
        {
            foreach (var item in _partitions)
            {
                foreach (var row in item.Value.GetRows())
                {
                    row["PartitionKey"] = new EntityProperty(item.Key);
                    yield return row;
                }
            }
        }

        public void Commit()
        {
            _startOfBatch = null;
        }

        public void Rollback()
        {
            Swap(_startOfBatch);
            _startOfBatch = null;
        }

        public void BeginTransaction()
        {
            _startOfBatch = Clone();
        }
    }
}