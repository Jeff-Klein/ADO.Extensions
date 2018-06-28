using System;

namespace ADO.Extensions.Reflection
{
    public class IsPKAttribute : Attribute
    {
        private bool v;

        public IsPKAttribute(bool v)
        {
            this.v = v;
        }
    }
}