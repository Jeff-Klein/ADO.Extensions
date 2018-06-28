using System;

namespace ADO.Extensions.Reflection
{
    public class ColumnNameAttribute : Attribute
    {
        private string v;

        public ColumnNameAttribute(string v)
        {
            this.v = v;
        }
    }
}