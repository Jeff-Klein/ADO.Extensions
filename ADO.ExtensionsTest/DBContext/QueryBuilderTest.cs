using ADO.Extensions.DBContext;
using ADO.ExtensionsTest.DBModel;
using System;
using Xunit;

namespace ADO.ExtensionsTest.DBContext
{

    public class QueryBuilderTest
    {
        QueryBuilder<Person> queryBuilder = new QueryBuilder<Person>();
        Person person = new Person()
        {
            Id = 1,
            FullName = "Jeff Klein",
            Birthday = new DateTime(1989, 02, 02),
            Age = 29
        };

        [Fact]
        private void GetSelectAllQueryTest()
        {
            var selectQuery = "SELECT ID, FULL_NAME, BIRTHDAY FROM PERSON WHERE  FULL_NAME = 'Jeff Klein' ORDER BY ID DESC";    
            
            var result = queryBuilder.GetSelectAllQuery(" FULL_NAME = 'Jeff Klein'", "ID DESC");

            if (!selectQuery.Equals(result))
                Assert.False(true);
        }

        [Fact]
        private void GetInsertQueryTest()
        {
            var insertQuery = "INSERT INTO PERSON VALUES(1,  'Jeff Klein' ,  TO_DATE('02/02/1989 00:00:00', 'DD/MM/YYYY HH24:MI:SS') )";            

            var result = queryBuilder.GetInsertQuery(person);

            if (!insertQuery.Equals(result))
                Assert.False(true);
        }

        [Fact]
        private void GetUpdateQueryTest()
        {
            var updateQuery = "UPDATE PERSON SET FULL_NAME = 'Jeff Klein', BIRTHDAY = TO_DATE('02/02/1989 00:00:00', 'DD/MM/YYYY HH24:MI:SS') WHERE ID = 1 ";

            var result = queryBuilder.GetUpdateQuery(person);

            if (!updateQuery.Equals(result))
                Assert.False(true);
        }

        [Fact]
        private void GetDeleteQueryTest()
        {
            var deleteQuery = "DELETE FROM PERSON WHERE ID = 1 ";

            var result = queryBuilder.GetDeleteQuery(person);

            if (!deleteQuery.Equals(result))
                Assert.False(true);
        }
    }
}
