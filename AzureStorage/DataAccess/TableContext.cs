using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Exceptions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage.DataAccess
{
    public class TableContext : ITableContext
    {
        private CloudTable _table;
        private CloudStorageAccount _cloudStorageAccount;
        private CloudTableClient _cloudTableClient;
        private TableBatchOperation _batch;
        private string _batchPartition;

        public TableContext(string tableName, string connectionString, bool tryCreate = false)
        {
            _cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            _cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
            _table = _cloudTableClient.GetTableReference(tableName);
            if (tryCreate)
                _table.CreateIfNotExistsAsync();
        }

        public Task AddFlattenAsync<T>(T entity) where T : IKeyAccess
        {
            var opcon = new OperationContext();
            var dyn = new DynamicTableEntity(entity.GetPartitionKey(), entity.GetRowKey())
            {
                Properties = TableEntity.Flatten(entity, opcon)
            };
            var op = TableOperation.Insert(dyn);
            return _table.ExecuteAsync(op);
        }

        public Task AddAsync<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            var op = TableOperation.Insert(entity);
            return _table.ExecuteAsync(op);
        }

        public Task AddOrReplaceAsync<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            var op = TableOperation.InsertOrMerge(entity);
            return _table.ExecuteAsync(op);
        }

        public IEnumerable<DynamicTableEntity> CreateDynamicQuery(TableQuery<DynamicTableEntity> query, Action<Exception> queryExceptionHandler)
        {
            var result = new BlockingCollection<DynamicTableEntity>();

            RunQuery(query, queryExceptionHandler, result);
            return result.GetConsumingEnumerable();
        }

        public Task<T> GetAsync<T>(string partitionKey, string rowKey) where T : BaseTableEntity, new()
        {
            var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);

            return _table.ExecuteAsync(operation)
                .ContinueWith(t => (T)t.Result.Result, TaskContinuationOptions.NotOnFaulted);
        }

        public Task UpdateAsync<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            var op = TableOperation.InsertOrMerge(entity);
            return _table.ExecuteAsync(op);
        }

        public Task DeleteAsync<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            var op = TableOperation.Delete(entity);
            return _table.ExecuteAsync(op);
        }

        public void BatchAdd<T>(T entity) where T : BaseTableEntity
        {
            CheckBatch(entity);

            _batch.InsertOrMerge(entity);
        }

        public void BatchUpdate<T>(T entity) where T : BaseTableEntity
        {
            CheckBatch(entity);

            _batch.Merge(entity);
        }

        public void BatchDelete<T>(T entity) where T : BaseTableEntity
        {
            CheckBatch(entity);

            _batch.Delete(entity);
        }

        private void CheckBatch<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();

            if (_batch == null)
            {
                _batch = new TableBatchOperation();
                _batchPartition = entity.PartitionKey;
            }
            else if (entity.PartitionKey != _batchPartition)
                throw new InconsistentBatchPartitionKey();
        }

        public Task BatchExecuteAsync()
        {
            if (_batch == null)
            {
                throw new NoBatchedOperationsPending();
            }

            var batch = _batch;
            _batch = null;
            return _table.ExecuteBatchAsync(batch);
        }

        public IEnumerable<T> Query<T>(TableQuery<T> query, Action<Exception> exceptionHandler) where T : BaseTableEntity, new()
        {
            var result = new BlockingCollection<T>();

            RunQuery(query, exceptionHandler, result);
            return result.GetConsumingEnumerable();
        }

        private void RunQuery<T>(TableQuery<T> query, Action<Exception> exceptionHandler, BlockingCollection<T> result)
            where T : ITableEntity, new()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    TableContinuationToken token = null;
                    do
                    {
                        var task = _table.ExecuteQuerySegmentedAsync(query, token);
                        var segment = task.Result;
                        token = segment.ContinuationToken;
                        foreach (var entity in segment.Results)
                        {
                            result.Add(entity);
                        }
                    } while (token != null);
                }
                catch (Exception e)
                {
                    exceptionHandler(e);
                }
                finally
                {
                    result.CompleteAdding();
                }
            });
        }
    }
}
