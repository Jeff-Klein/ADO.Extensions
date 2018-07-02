using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ADO.Extensions.Reflection
{
    internal class CustomProperty
    {
        internal PropertyInfo DefaultProperties { get; set; }
        internal String TableName { get; set; }
        internal String Type { get; set; }
        internal String ColumnName { get; set; }
        internal Boolean IsPK { get; set; }
        internal Boolean IsComputed { get; set; }
        internal Object Value { get; set; }
    }
}
