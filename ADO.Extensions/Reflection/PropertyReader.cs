using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ADO.Extensions.Reflection
{
    internal class PropertyReader<T> where T : new()
    {
        internal List<CustomProperty> GetAllProperties(object obj)
        {
            List<CustomProperty> customProperties = new List<CustomProperty>();
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                CustomProperty customProperty = new CustomProperty();
                customProperty.DefaultProperties = prop;

                customProperty.Type = obj.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance).PropertyType.Name.ToLower();
                customProperty.Value = prop.GetValue(obj, null);

                foreach (var customAtr in obj.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance).CustomAttributes)
                {
                    if (customAtr.AttributeType.Name == "ColumnNameAttribute")
                        customProperty.ColumnName = customAtr.ConstructorArguments[0].Value.ToString();
                    else if (customAtr.AttributeType.Name == "IsPKAttribute")
                        customProperty.IsPK = Convert.ToBoolean(customAtr.ConstructorArguments[0].Value);
                    else if (customAtr.AttributeType.Name == "IsComputedAttribute")
                        customProperty.IsComputed = Convert.ToBoolean(customAtr.ConstructorArguments[0].Value);
                }

                customProperties.Add(customProperty);
            }

            return customProperties;
        }

        internal T CreateObjectFromDataReader(OracleDataReader result)
        {
            PropertyInfo[] properties;
            properties = typeof(T).GetProperties();

            T obj = new T();

            foreach (var prop in properties)
            {
                var columnName = "";
                var isComputed = false;

                foreach (var customAtr in prop.CustomAttributes)
                {
                    if (customAtr.AttributeType.Name == "ColumnNameAttribute")
                        columnName = customAtr.ConstructorArguments[0].Value.ToString();
                    else if (customAtr.AttributeType.Name == "IsComputedAttribute" && Convert.ToBoolean(customAtr.ConstructorArguments[0].Value))
                        isComputed = true;
                }

                if (!isComputed)
                    TrySetProperty(obj, prop.Name, result[columnName]);
            }

            return obj;
        }

        private void TrySetProperty(object obj, string property, object value)
        {
            var prop = obj.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite && !DBNull.Value.Equals(value))
                prop.SetValue(obj, value, null);
        }
    }
}
