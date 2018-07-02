using ADO.Extensions.DBContext;
using ADO.ExtensionsTest.DBModel;
using System;
using Xunit;

namespace ADO.ExtensionsTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            DBHandler<Person> dbHandler = new DBHandler<Person>("Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = cordas)(PORT = 1521))(CONNECT_DATA = (SERVICE_NAME = cordas))); User ID = rtbaux; Password = rtbaux");
            var result = dbHandler.ExecuteReader();
        }
    }
}
