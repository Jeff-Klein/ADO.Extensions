using System;

namespace ADO.Extensions.Reflection
{
    public class IsComputedAttribute : Attribute
    {
        private bool v;

        public IsComputedAttribute(bool v)
        {
            this.v = v;
        }
    }
}