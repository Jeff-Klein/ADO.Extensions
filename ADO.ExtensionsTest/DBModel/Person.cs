using ADO.Extensions.Reflection;
using System;

namespace ADO.ExtensionsTest.DBModel
{
    [TableName("PERSON")]
    public class Person
    {
        [ColumnName("ID"), IsPK(true)]
        public decimal Id { get; set; }

        [ColumnName("FULL_NAME")]
        public string FullName { get; set; }

        [ColumnName("BIRTHDAY")]
        public DateTime Birthday { get; set; }

        [ColumnName("AGE"), IsComputed(true)]
        public decimal Age { get; set; }

    }
}
