using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage.DataAccess
{
    /// <summary>
    /// Abstract base class for table entities. Handles reading and writing of types not natively handled by
    /// table storage.
    /// </summary>
    public abstract class BaseTableEntity : TableEntity, IKeyAccess
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTableEntity"/> class.
        /// </summary>
        public BaseTableEntity() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTableEntity"/> class.
        /// </summary>
        /// <param name="partitionKey">The partition key of the <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> to be initialized.</param>
        /// <param name="rowKey">The row key of the <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> to be initialized.</param>
        public BaseTableEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey)
        {
        }

        #region Support for types not natively handled by table storage

        /// <summary>
        /// Serializes the <see cref="T:System.Collections.Generic.Dictionary`2" /> of property names mapped to <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty" /> data values from this <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> instance.
        /// </summary>
        /// <param name="operationContext">An <see cref="T:Microsoft.WindowsAzure.Storage.OperationContext" /> object used to track the execution of the operation.</param>
        /// <returns>
        /// A map of property names to <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty" /> data typed values created by serializing this table entity instance.
        /// </returns>
        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var dictionaryToReturn = base.WriteEntity(operationContext);
            var propertiesWhichAreOfSpecialType = GetPropertiesOfSpecialType();

            foreach (var property in propertiesWhichAreOfSpecialType)
            {
                if (!dictionaryToReturn.ContainsKey(property.Name))
                {
                    dictionaryToReturn.Add(property.Name, new EntityProperty(property.GetValue(this).ToString()));
                }
            }
            return dictionaryToReturn;
        }

        /// <summary>
        /// De-serializes this <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> instance using the specified <see cref="T:System.Collections.Generic.Dictionary`2" /> of property names to <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty" /> data typed values.
        /// </summary>
        /// <param name="properties">The map of string property names to <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty" /> data values to de-serialize and store in this table entity instance.</param>
        /// <param name="operationContext">An <see cref="T:Microsoft.WindowsAzure.Storage.OperationContext" /> object used to track the execution of the operation.</param>
        public override void ReadEntity(IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            var propertiesWhichAreOfSpecialType = GetPropertiesOfSpecialType();

            foreach (var property in propertiesWhichAreOfSpecialType)
            {
                EntityProperty value;
                properties.TryGetValue(property.Name, out value);
                if (value == null)
                    continue;

                if (property.PropertyType == typeof(Uri))
                    property.SetValue(this, new Uri(value.StringValue));
            }
        }

        /// <summary>
        /// Gets the type of the properties of special.
        /// </summary>
        /// <returns></returns>
        private List<PropertyInfo> GetPropertiesOfSpecialType()
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            foreach (var property in this.GetType().GetTypeInfo().DeclaredProperties)
            {
                if (property.Name == "PartitionKey" || property.Name == "RowKey" || property.Name == "Timestamp" ||
                    property.Name == "ETag" || property.SetMethod == null || !property.SetMethod.IsPublic ||
                    property.GetMethod == null || !property.GetMethod.IsPublic)
                {
                    continue;
                }
                if (Helper.AllowedTypes.Contains(property.PropertyType))
                {
                    continue;
                }
                properties.Add(property);
            }
            return properties;
        }

        #endregion

        public abstract string GetPartitionKey();

        public abstract string GetRowKey();

        public void PopulateDefaultKeys()
        {
            if (PartitionKey == null)
                PartitionKey = GetPartitionKey();
            if (RowKey == null)
                RowKey = GetRowKey();
        }
    }
}