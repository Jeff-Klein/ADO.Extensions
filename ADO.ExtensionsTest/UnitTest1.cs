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
            DBHandler<Person> dbHandler = new DBHandler<Person>("");
            var result = dbHandler.ExecuteReader();
        }
    }
}
