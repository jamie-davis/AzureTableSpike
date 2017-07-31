using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestStorage.TableStoreImpl
{
    public class TableStore
    {
        private object _lock = new object();

        private Dictionary<string, Table> _tables = new Dictionary<string, Table>(); 

        public void StoreNew(string tableName, string partitionKey, string rowKey, IDictionary<string, EntityProperty> fields)
        {
            var tableAccess = GetTableAccess(tableName);
            using (tableAccess.TableOwnership)
                tableAccess.Table.StoreNew(partitionKey, rowKey, fields);
        }

        public void StoreExisting(string tableName, string partitionKey, string rowKey, IDictionary<string, EntityProperty> fields)
        {
            var tableAccess = GetTableAccess(tableName);
            using (tableAccess.TableOwnership)
                tableAccess.Table.Update(partitionKey, rowKey, fields);
        }

        public void StoreOrReplace(string table, string partitionKey, string rowKey, IDictionary<string, EntityProperty> fields)
        {
            var tableAccess = GetTableAccess(table);
            using (tableAccess.TableOwnership)
                tableAccess.Table.StoreOrReplace(partitionKey, rowKey, fields);
        }

        public void Delete(string tableName, string entityPartitionKey, string entityRowKey)
        {
            var tableAccess = GetTableAccess(tableName);
            using (tableAccess.TableOwnership)
                tableAccess.Table.Delete(entityPartitionKey, entityRowKey);
        }

        public IDictionary<string, EntityProperty> GetFields(string tableName, string partitionKey, string rowKey)
        {
            var tableAccess = GetTableAccess(tableName);
            using (tableAccess.TableOwnership)
                return tableAccess.Table.GetFields(partitionKey, rowKey);
        }

        public void BeginUnitOfWork(string tableName)
        {
            var tableAccess = GetTableAccess(tableName);
            using (tableAccess.TableOwnership)
            {
                tableAccess.TableOwnership.Pin();
                tableAccess.Table.BeginTransaction();
            }
        }

        public void Commit(string tableName)
        {
            var tableAccess = GetTableAccess(tableName);
            using (tableAccess.TableOwnership)
            {
                tableAccess.TableOwnership.Release();
                tableAccess.Table.Commit();
            }
        }

        public void Rollback(string tableName)
        {
            var tableAccess = GetTableAccess(tableName);
            using (tableAccess.TableOwnership)
            {
                tableAccess.TableOwnership.Release();
                tableAccess.Table.Rollback();
            }
        }

        private (Table Table, TableOwnership TableOwnership) GetTableAccess(string tableName)
        {
            lock (_lock)
            {
                Table table;
                if (!_tables.TryGetValue(tableName, out table))
                {
                    table = new Table();
                    _tables[tableName] = table;
                }

                var ownership = table.TakeOwnership();

                return (table, ownership);
            }
        }

        public IEnumerable<IDictionary<string, EntityProperty>> GetAllRows(string tableName)
        {
            var tableAccess = GetTableAccess(tableName);
            using (tableAccess.TableOwnership)
            {
                var table = tableAccess.Table;
                foreach (var dictionary in table.GetRows())
                {
                    yield return dictionary;
                }
            }
        }
    }
}