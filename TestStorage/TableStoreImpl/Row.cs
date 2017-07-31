using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestStorage.TableStoreImpl
{
    internal class Row
    {
        private Dictionary<string, EntityProperty> _properties;

        public Row Clone()
        {
            var props = CopyFields();
            return new Row {_properties = props};
        }

        public void SetFields(IDictionary<string, EntityProperty> fields)
        {
            _properties = fields.ToDictionary(p => p.Key, p => p.Value);
        }

        public void UpdateFields(IDictionary<string, EntityProperty> fields)
        {
            foreach (var property in fields)
            {
                _properties[property.Key] = property.Value;
            }
        }

        public Dictionary<string, EntityProperty> CopyFields()
        {
            return Enumerable.ToDictionary(_properties, p => p.Key, p => EntityProperty.CreateEntityPropertyFromObject(p.Value.PropertyAsObject));
        }
    }
}