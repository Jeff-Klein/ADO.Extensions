using ADO.Extensions.Reflection;
using ADO.ExtensionsTest.DBModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ADO.ExtensionsTest.Reflection
{
    public class PropertyReaderTest
    {
        PropertyReader<Person> propertyReader = new PropertyReader<Person>();
        Person person = new Person()
        {
            Id = 1,
            FullName = "Jeff Klein",
            Birthday = new DateTime(1989, 02, 02),
            Age = 29
        };

        [Fact]
        private void GetAllPropertiesTest()
        {
            List<CustomProperty> customProperties = new List<CustomProperty>()
            {
                new CustomProperty() { ColumnName = "ID", IsComputed = false, IsPK = true, TableName = null, Type = "decimal", Value = 1 },
                new CustomProperty() { ColumnName = "FULL_NAME", IsComputed = false, IsPK = false, TableName = null, Type = "string", Value = "Jeff Klein" },
                new CustomProperty() { ColumnName = "BIRTHDAY", IsComputed = false, IsPK = false, TableName = null, Type = "datetime", Value = new DateTime(1989, 02, 02)},
                new CustomProperty() { ColumnName = "AGE", IsComputed = true, IsPK = false, TableName = null, Type = "decimal", Value = 29 }
            };

            var result = propertyReader.GetAllProperties(person);

            var differences = customProperties.Except(result, new CustomPropertyComparer());

            if (differences.Count() > 0)
                Assert.False(true);
        }

    }
}
