using ADO.Extensions.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace ADO.Extensions.DBContext
{
    internal class QueryBuilder<T> where T : new()
    {
        internal string GetSelectAllQuery(string where, string orderBy)
        {
            if (!String.IsNullOrEmpty(where))
                where = " WHERE " + where;

            if (!String.IsNullOrEmpty(orderBy))
                where += " ORDER BY " + orderBy;

            string sqlQuery = "SELECT ";
            bool first = true;

            T obj = new T();

            PropertyReader<T> propertyReader = new PropertyReader<T>();
            var propList = propertyReader.GetAllProperties(obj);

            foreach (var prop in propList)
            {
                if (prop.IsComputed)
                    continue;

                if (first)
                    sqlQuery += prop.ColumnName;
                else
                    sqlQuery += ", " + prop.ColumnName;

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

            PropertyReader<T> propertyReader = new PropertyReader<T>();
            var propList = propertyReader.GetAllProperties(obj);

            foreach (var prop in propList)
            {

                if (prop.IsComputed)
                    continue;

                if (String.Equals(prop.Type, "string"))
                    insertCommand += " '" + prop.Value + "' ";
                else if (String.Equals(prop.Type, "date") || String.Equals(prop.Type, "datetime"))
                {
                    DateTime data = Convert.ToDateTime(prop.Value);
                    insertCommand += " TO_DATE('" + data.ToString("dd/MM/yyyy HH:mm:ss") + "', 'DD/MM/YYYY HH24:MI:SS') ";
                }
                else
                    insertCommand += prop.Value;

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

            PropertyReader<T> propertyReader = new PropertyReader<T>();
            var propList = propertyReader.GetAllProperties(obj);

            foreach (var prop in propList)
            { 
                if (prop.IsComputed)
                    continue;
                else if (prop.IsPK)
                {
                    if (String.Equals(prop.Type, "string") || String.Equals(prop.Type, "date") || String.Equals(prop.Type, "datetime"))
                        where += prop.ColumnName + " = '" + prop.Value + "' AND ";
                    else
                        where += prop.ColumnName + " = " + prop.Value + " AND ";
                }
                else
                {
                    if (String.Equals(prop.Type, "string"))
                        updateCommand += prop.ColumnName + " = '" + prop.Value + "', ";
                    else if (String.Equals(prop.Type, "date") || String.Equals(prop.Value, "datetime"))
                    {
                        DateTime data = Convert.ToDateTime(prop.Value);
                        updateCommand += prop.ColumnName + " = TO_DATE('" + data.ToString("dd/MM/yyyy HH:mm:ss") + "', 'DD/MM/YYYY HH24:MI:SS'), ";
                    }
                    else
                        updateCommand += prop.ColumnName + " = " + prop.Value + ", ";
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

            PropertyReader<T> propertyReader = new PropertyReader<T>();
            var propList = propertyReader.GetAllProperties(obj);

            foreach (var prop in propList)
            {
                if (prop.IsComputed)
                    continue;
                else if (prop.IsPK)
                {
                    if (String.Equals(prop.Type, "string") || String.Equals(prop.Type, "date") || String.Equals(prop.Type, "datetime"))
                        where += prop.ColumnName + " = '" + prop.Value + "' AND ";
                    else
                        where += prop.ColumnName + " = " + prop.Value + " AND ";
                }
            }

            deleteCommand = deleteCommand + where;

            if (deleteCommand.EndsWith(" AND "))
                deleteCommand = deleteCommand.Substring(0, deleteCommand.Length - 4);

            return deleteCommand;
        }
    }
}
