using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ADO.Extensions.DBContext
{
    internal class QueryBuilder<T>
    {
        internal string GetSelectAllQuery(string where, string orderBy)
        {
            if (!String.IsNullOrEmpty(where))
                where = " WHERE " + where;

            if (!String.IsNullOrEmpty(orderBy))
                where += " ORDER BY " + orderBy;

            string sqlQuery = "SELECT ";
            bool first = true;
            PropertyInfo[] properties;
            properties = typeof(T).GetProperties();

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

                if (isComputed)
                    continue;

                if (first)
                    sqlQuery += columnName;
                else
                    sqlQuery += ", " + columnName;

                first = false;
            }

            var tableName = typeof(T).CustomAttributes.First().ConstructorArguments[0].Value;

            sqlQuery += " FROM " + tableName + where;

            return sqlQuery;
        }

        internal string GetInsertQuery(object obj)
        {
            string insertCommand = "INSERT INTO ";

            var tableName = typeof(T).CustomAttributes.First().ConstructorArguments[0].Value;
            insertCommand += tableName + " VALUES(";

            var propList = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in propList)
            {
                bool isComputed = false;

                foreach (var customAtr in obj.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance).CustomAttributes)
                    if (customAtr.AttributeType.Name == "IsComputedAttribute")
                        isComputed = Convert.ToBoolean(customAtr.ConstructorArguments[0].Value);

                if (isComputed)
                    continue;

                var propType = obj.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance).PropertyType.Name;

                if (String.Equals(propType.ToLower(), "string"))
                    insertCommand += " '" + prop.GetValue(obj, null) + "' ";
                else if (String.Equals(propType.ToLower(), "date") || String.Equals(propType.ToLower(), "datetime"))
                {
                    DateTime data = Convert.ToDateTime(prop.GetValue(obj, null));
                    insertCommand += " TO_DATE('" + data.ToString("dd/MM/yyyy HH:mm:ss") + "', 'DD/MM/YYYY HH24:MI:SS') ";
                }
                else
                    insertCommand += prop.GetValue(obj, null);

                if (!propList.Last().Equals(prop))
                    insertCommand += ", ";
            }

            if (insertCommand.EndsWith(", "))
                insertCommand = insertCommand.Substring(0, insertCommand.Length - 2);

            insertCommand += ")";

            return insertCommand;
        }

        internal string GetUpdateQuery(object obj)
        {
            string updateCommand = "UPDATE ";
            string where = " WHERE ";

            var tableName = typeof(T).CustomAttributes.First().ConstructorArguments[0].Value;
            updateCommand += tableName + " SET ";

            var propList = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in propList)
            {
                var propType = obj.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance).PropertyType.Name;
                bool isPK = false;
                bool isComputed = false;
                var propDBName = "";

                foreach (var customAtr in obj.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance).CustomAttributes)
                {
                    if (customAtr.AttributeType.Name == "ColumnNameAttribute")
                        propDBName = customAtr.ConstructorArguments[0].Value.ToString();
                    else if (customAtr.AttributeType.Name == "IsPKAttribute")
                        isPK = Convert.ToBoolean(customAtr.ConstructorArguments[0].Value);
                    else if (customAtr.AttributeType.Name == "IsComputedAttribute")
                        isComputed = Convert.ToBoolean(customAtr.ConstructorArguments[0].Value);
                }

                if (isComputed)
                    continue;
                else if (isPK)
                {
                    if (String.Equals(propType.ToLower(), "string") || String.Equals(propType.ToLower(), "date") || String.Equals(propType.ToLower(), "datetime"))
                        where += propDBName + " = '" + prop.GetValue(obj, null) + "' AND ";
                    else
                        where += propDBName + " = " + prop.GetValue(obj, null) + " AND ";
                }
                else
                {
                    if (String.Equals(propType.ToLower(), "string"))
                        updateCommand += propDBName + " = '" + prop.GetValue(obj, null) + "', ";
                    else if (String.Equals(propType.ToLower(), "date") || String.Equals(propType.ToLower(), "datetime"))
                    {
                        DateTime data = Convert.ToDateTime(prop.GetValue(obj, null));
                        updateCommand += propDBName + " = TO_DATE('" + data.ToString("dd/MM/yyyy HH:mm:ss") + "', 'DD/MM/YYYY HH24:MI:SS'), ";
                    }
                    else
                        updateCommand += propDBName + " = " + prop.GetValue(obj, null) + ", ";
                }
            }

            updateCommand = updateCommand + where;

            updateCommand = updateCommand.Replace(",  WHERE ", " WHERE ");

            if (updateCommand.EndsWith(" AND "))
                updateCommand = updateCommand.Substring(0, updateCommand.Length - 4);

            return updateCommand;
        }

        internal string GetDeleteQuery(object obj)
        {
            string deleteCommand = "DELETE FROM ";
            string where = " WHERE ";

            var tableName = typeof(T).CustomAttributes.First().ConstructorArguments[0].Value;
            deleteCommand += tableName;

            var propList = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in propList)
            {
                var propType = obj.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance).PropertyType.Name;
                bool isPK = false;
                bool isComputed = false;
                var propDBName = "";

                foreach (var customAtr in obj.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance).CustomAttributes)
                {
                    if (customAtr.AttributeType.Name == "ColumnNameAttribute")
                        propDBName = customAtr.ConstructorArguments[0].Value.ToString();
                    else if (customAtr.AttributeType.Name == "IsPKAttribute")
                        isPK = Convert.ToBoolean(customAtr.ConstructorArguments[0].Value);
                    else if (customAtr.AttributeType.Name == "IsComputedAttribute")
                        isComputed = Convert.ToBoolean(customAtr.ConstructorArguments[0].Value);
                }

                if (isComputed)
                    continue;
                else if (isPK)
                {
                    if (String.Equals(propType.ToLower(), "string") || String.Equals(propType.ToLower(), "date") || String.Equals(propType.ToLower(), "datetime"))
                        where += propDBName + " = '" + prop.GetValue(obj, null) + "' AND ";
                    else
                        where += propDBName + " = " + prop.GetValue(obj, null) + " AND ";
                }
            }

            deleteCommand = deleteCommand + where;

            if (deleteCommand.EndsWith(" AND "))
                deleteCommand = deleteCommand.Substring(0, deleteCommand.Length - 4);

            return deleteCommand;
        }
    }
}
