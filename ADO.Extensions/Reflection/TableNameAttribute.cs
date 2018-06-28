using System;
using System.Collections.Generic;
using System.Text;

namespace ADO.Extensions.Reflection
{
    public class TableNameAttribute : Attribute
    {
        private string v;

        public TableNameAttribute(string v)
        {
            this.v = v;
        }
    }
}