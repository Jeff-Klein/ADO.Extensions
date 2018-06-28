using ADO.Extensions.Reflection;
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
                PropertyReader<T> propertyReader = new PropertyReader<T>();
                T obj = propertyReader.CreateObject(result);
                returnList.Add(obj);
            }

            oracleConnection.Close();

            return returnList;
        }

        public List<T> ExecuteReader(string where, string orderBy)
        {

            QueryBuilder<T> queryBuilder = new QueryBuilder<T>();            

            List<T> returnList = new List<T>();
            if (oracleConnection.State != ConnectionState.Open)
                oracleConnection.Open();

            OracleCommand cmd = new OracleCommand();

            cmd.Connection = oracleConnection;

            cmd.CommandText = queryBuilder.GetSelectAllQuery(where, orderBy);

            cmd.CommandType = CommandType.Text;

            OracleDataReader dr = cmd.ExecuteReader();

            PropertyInfo[] properties;
            properties = typeof(T).GetProperties();


            while (dr.Read())
            {
               /* T obj = new T();

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
                }*/

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
                QueryBuilder<T> queryBuilder = new QueryBuilder<T>();

                var insertCommand = queryBuilder.GetInsertQuery(obj);

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
                QueryBuilder<T> queryBuilder = new QueryBuilder<T>();

                var updateCommand = queryBuilder.GetUpdateQuery(obj);

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

                QueryBuilder<T> queryBuilder = new QueryBuilder<T>();

                var deleteCommand = queryBuilder.GetDeleteQuery(obj);

                var result = ExecuteNonQuery(deleteCommand);

                return new DBResult(true, result);

            }
            catch (Exception ex)
            {
                return new DBResult(false, ex.Message);
            }
        }       
    }
}