using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.DataAccess;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TestStorage.FilterStrings;
using TestStorage.TableStoreImpl;

[assembly: DebuggerDisplay("EntityProperty: {PropertyAsObject}", Target = typeof(EntityProperty))]

namespace TestStorage.DataAccess
{
    public class TableContext : ITableContext
    {
        private readonly string _table;
        private readonly TableStore _tableStore;

        private List<Action> _batchedOperations = new List<Action>();

        public TableContext(string table, TableStore tableStore)
        {
            _table = table;
            _tableStore = tableStore;
        }

        #region Implementation of ITableContext

        public Task AddAsync<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            return Task.Run(() => DoWrite(entity, true));
        }

        public Task AddOrReplaceAsync<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            return Task.Run(() => DoWriteOrReplace(entity));
        }

        private void DoWrite<T>(T entity, bool isNew) where T : BaseTableEntity
        {
            var context = new OperationContext();
            var fields = entity.WriteEntity(context);
            if (isNew)
                _tableStore.StoreNew(_table, entity.PartitionKey, entity.RowKey, fields);
            else
                _tableStore.StoreExisting(_table, entity.PartitionKey, entity.RowKey, fields);
        }

        private void DoWriteOrReplace<T>(T entity) where T : BaseTableEntity
        {
            var context = new OperationContext();
            var fields = entity.WriteEntity(context);
            _tableStore.StoreOrReplace(_table, entity.PartitionKey, entity.RowKey, fields);
        }

        public IEnumerable<DynamicTableEntity> CreateDynamicQuery(TableQuery<DynamicTableEntity> query, Action<Exception> queryExceptionHandler)
        {
            foreach (var row in InternalQuery(query.FilterString, query.SelectColumns))
            {
                var context = new OperationContext();
                var entity = new DynamicTableEntity();
                entity.ReadEntity(row, context);
                yield return entity;
            }
        }

        public Task<T> GetAsync<T>(string partitionKey, string rowKey) where T : BaseTableEntity, new()
        {
            return Task.Run(() => DoGet<T>(partitionKey, rowKey));
        }

        private T DoGet<T>(string partitionKey, string rowKey) where T : BaseTableEntity, new()
        {
            var operationContext = new OperationContext();
            var fields = _tableStore.GetFields(_table, partitionKey, rowKey);
            if (fields == null)
                return null;

            var entity = new T();
            entity.ReadEntity(fields, operationContext);
            return entity;
        }

        public Task UpdateAsync<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            return Task.Run(() => DoWrite(entity, false));
        }

        public Task DeleteAsync<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            return Task.Run(() => DoDelete(entity));
        }

        private void DoDelete<T>(T entity) where T : BaseTableEntity
        {
            _tableStore.Delete(_table, entity.PartitionKey, entity.RowKey);
        }

        public void BatchAdd<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            _batchedOperations.Add(() => DoWrite(entity, true));
        }

        public void BatchUpdate<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            _batchedOperations.Add(() => DoWrite(entity, false));
        }

        public void BatchDelete<T>(T entity) where T : BaseTableEntity
        {
            entity.PopulateDefaultKeys();
            _batchedOperations.Add(() => DoDelete(entity));
        }

        public Task BatchExecuteAsync()
        {
            var batch = _batchedOperations;
            _batchedOperations = new List<Action>();
            return Task.Run(() => DoBatch(batch));
        }

        private void DoBatch(List<Action> batch)
        {
            try
            {
                _tableStore.BeginUnitOfWork(_table);

                foreach (var action in batch)
                {
                    action();
                }

                _tableStore.Commit(_table);
            }
            catch
            {
                _tableStore.Rollback(_table);
                throw;
            }
        }

        public IEnumerable<T> Query<T>(TableQuery<T> query, Action<Exception> exceptionHandler) where T : BaseTableEntity, new()
        {
            foreach (var row in InternalQuery(query.FilterString, query.SelectColumns))
            {
                var context = new OperationContext();
                var entity = new T();
                entity.ReadEntity(row, context);
                yield return entity;
            }
        }

        private IEnumerable<IDictionary<string, EntityProperty>> InternalQuery(string filterString, IList<string> selectColumns)
        {
            Debug.WriteLine($"Q: {filterString}");

            var parseResult = FilterStringParser.Parse(filterString);
            if (!parseResult.Success)
                throw new InvalidFilterString(parseResult.Error);

            foreach (var row in _tableStore.GetAllRows(_table))
            {
                if (!parseResult.Root.Execute(row, out string error))
                {
                    if (error != null)
                        throw new QueryFailed(error);
                }
                else
                {
                    if (selectColumns != null)
                        yield return row.Where(p => selectColumns.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value);
                    else
                        yield return row;
                }
            }
        }

        #endregion
    }

}
