using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.DataAccess;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage
{
    public interface ITableContext
    {
        Task AddAsync<T>(T entity) where T : BaseTableEntity;

        Task AddOrReplaceAsync<T>(T entity) where T : BaseTableEntity;

        IEnumerable<DynamicTableEntity> CreateDynamicQuery(TableQuery<DynamicTableEntity> query, Action<Exception> queryExceptionHandler);

        Task<T> GetAsync<T>(string partitionKey, string rowKey) where T : BaseTableEntity, new();

        Task UpdateAsync<T>(T entity) where T : BaseTableEntity;

        Task DeleteAsync<T>(T entity) where T : BaseTableEntity;

        void BatchAdd<T>(T entity) where T : BaseTableEntity;

        void BatchUpdate<T>(T entity) where T : BaseTableEntity;

        void BatchDelete<T>(T entity) where T : BaseTableEntity;

        Task BatchExecuteAsync();

        IEnumerable<T> Query<T>(TableQuery<T> query, Action<Exception> exceptionHandler) where T : BaseTableEntity, new();
    }
}
