using System;
using System.Collections.Generic;
using System.Text;

namespace ADO.Extensions.DBContext
{
    internal interface IDBHandler<T>
    {
        List<T> ExecuteReader();
        List<T> ExecuteReader(string command);
        List<T> ExecuteReader(String where, string orderBy);
        Object ExecuteScalar(string command);
        int ExecuteNonQuery(string command);
        DBResult Store(object obj);
        DBResult Insert(object obj);
        DBResult Update(object obj);

        DBResult Delete(object obj);
    }
}
