using System.Collections.Generic;

namespace ADO.Extensions.Reflection
{
    internal class CustomPropertyComparer : IEqualityComparer<CustomProperty>
    {
        public int GetHashCode(CustomProperty customProperty)
        {
            if (customProperty == null)
            {
                return 0;
            }
            return customProperty.ColumnName.GetHashCode();
        }

        public bool Equals(CustomProperty cp1, CustomProperty cp2)
        {
            if (cp1.ColumnName == cp2.ColumnName &&
                cp1.IsComputed == cp2.IsComputed &&
                cp1.TableName == cp2.TableName &&
                cp1.Type == cp2.Type &&
                cp1.Value == cp1.Value)
                return true;
            else
                return false;
        }
    }
}
