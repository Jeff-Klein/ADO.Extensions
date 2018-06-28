using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ADO.Extensions.DBContext
{
    public class DBHandler<T> : IDBHandler<T> where T : new()
    {
        OracleConnection oracleConnection;

        public DBHandler(string connectionString)
        {
            oracleConnection = new OracleConnection(connectionString);
        }

        public List<T> ExecuteReader()
        {
            return ExecuteReader("", "");
        }

        public List<T> ExecuteReader(string command)
        {
            if (oracleConnection.State != ConnectionState.Open)
                oracleConnection.Open();

            List<T> returnList = new List<T>();
            OracleCommand cmd = new OracleCommand();

            cmd.Connection = oracleConnection;
            cmd.CommandText = command;
            cmd.CommandType = CommandType.Text;

            OracleDataReader result = cmd.ExecuteReader();

            PropertyInfo[] properties;
            properties = typeof(T).GetProperties();

            while (result.Read())
            {
                T obj = new T();

                foreach (var prop in properties)
                {
                    try
                    {
                        var cellValue = result[prop.CustomAttributes.First().ConstructorArguments[0].Value.ToString()];
                        TrySetProperty(obj, prop.Name, cellValue);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("Column not found: " + prop.CustomAttributes.First().ConstructorArguments[0].Value.ToString());
                        Console.Write(ex);
                        continue; //force reading the next column
                    }
                }

                returnList.Add(obj);

            }

            oracleConnection.Close();

            return returnList;
        }

        public List<T> ExecuteReader(string where, string orderBy)
        {
            if (!String.IsNullOrEmpty(where))
                where = " WHERE " + where;

            if (!String.IsNullOrEmpty(orderBy))
                where += " ORDER BY " + orderBy;

            List<T> returnList = new List<T>();
            if (oracleConnection.State != ConnectionState.Open)
                oracleConnection.Open();

            OracleCommand cmd = new OracleCommand();

            cmd.Connection = oracleConnection;

            cmd.CommandText = GetSelectAllQuery() + where;

            cmd.CommandType = CommandType.Text;

            OracleDataReader dr = cmd.ExecuteReader();

            PropertyInfo[] properties;
            properties = typeof(T).GetProperties();


            while (dr.Read())
            {
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
                        TrySetProperty(obj, prop.Name, dr[columnName]);
                }

                returnList.Add(obj);

            }

            oracleConnection.Close();

            return returnList;
        }

        public int ExecuteNonQuery(string command)
        {
            if (oracleConnection.State != ConnectionState.Open)
                oracleConnection.Open();

            OracleCommand cmd = new OracleCommand();

            cmd.Connection = oracleConnection;

            cmd.CommandText = command;

            cmd.CommandType = CommandType.Text;

            int affectedRow = cmd.ExecuteNonQuery();

            oracleConnection.Close();

            return affectedRow;
        }

        public object ExecuteScalar(string command)
        {
            if (oracleConnection.State != ConnectionState.Open)
                oracleConnection.Open();

            OracleCommand cmd = new OracleCommand();

            cmd.Connection = oracleConnection;

            cmd.CommandText = command;

            cmd.CommandType = CommandType.Text;

            var result = cmd.ExecuteScalar();

            oracleConnection.Close();

            return result;
        }

        public DBResult Insert(object obj)
        {
            try
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

                var result = ExecuteNonQuery(insertCommand);

                return new DBResult(true, result);
            }
            catch (Exception ex)
            {
                return new DBResult(false, ex.Message);
            }
        }

        public DBResult Store(object obj)
        {
            var result = Insert(obj);

            if (result.Success == false)
                result = Update(obj);

            return result;
        }

        public DBResult Update(object obj)
        {
            try
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

                //when all columns are PK
                if (updateCommand.Contains("SET  WHERE"))
                    return new DBResult(true, 0);

                var result = ExecuteNonQuery(updateCommand);

                return new DBResult(true, result);
            }
            catch (Exception ex)
            {
                return new DBResult(false, ex.Message);
            }
        }

        public DBResult Delete(object obj)
        {
            try
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


                var result = ExecuteNonQuery(deleteCommand);

                return new DBResult(true, result);

            }
            catch (Exception ex)
            {
                return new DBResult(false, ex.Message);
            }
        }

        private void TrySetProperty(object obj, string property, object value)
        {
            var prop = obj.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite && !DBNull.Value.Equals(value))
                prop.SetValue(obj, value, null);
        }

        private string GetSelectAllQuery()
        {
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

            sqlQuery += " FROM " + tableName;

            return sqlQuery;
        }
    }
}
