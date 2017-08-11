using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestStorage.TableStoreImpl
{
    internal class Partition
    {
        private Dictionary<string, Row> _rows = new Dictionary<string, Row>();

        public Partition Clone()
        {
            var rows = Enumerable.ToDictionary(_rows, r => r.Key, r => r.Value.Clone());
            return new Partition {_rows = rows};
        }

        private Row GetRow(string rowKeyString)
        {
            if (_rows.TryGetValue(rowKeyString, out Row row))
                return row;

            return null;
        }

        public void StoreNew(string rowKey, IDictionary<string, EntityProperty> fields)
        {
            var row = GetRow(rowKey);
            if (row != null)
                throw new RowKeyAlreadyExists();

            row = new Row();
            row.SetFields(fields);
            _rows[rowKey] = row;
        }

        public void Update(string rowKey, IDictionary<string, EntityProperty> fields)
        {
            var row = GetRow(rowKey);
            if (row == null)
                throw new RowKeyNotFound();
            row.UpdateFields(fields);
        }

        public void StoreOrReplace(string rowKey, IDictionary<string, EntityProperty> fields)
        {
            var row = GetRow(rowKey);
            if (row == null)
            {
                row = new Row();
                _rows[rowKey] = row;
            }
            row.SetFields(fields);
        }

        public void Delete(string entityRowKey)
        {
            var row = GetRow(entityRowKey);
            if (row == null)
                throw new RowKeyNotFound();

            _rows.Remove(entityRowKey);
        }

        public Dictionary<string, EntityProperty> GetFields(string rowKey)
        {
            var row = GetRow(rowKey);
            if (row == null)
                return null;

            return row.CopyFields();
        }

        public IEnumerable<Dictionary<string, EntityProperty>> GetRows()
        {
            foreach (var row in _rows)
            {
                var properties = row.Value.CopyFields();
                properties["RowKey"] = new EntityProperty(row.Key);
                yield return properties;
            }
        }
    }
}