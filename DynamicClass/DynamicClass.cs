using System.Reflection;
using System.Text;

namespace DynamicClass
{
    /// <summary>
    /// Provides a base class for dynamic objects.
    /// </summary>
    public abstract class DynamicClass
    {
        /// <summary>
        /// Gets the dynamic property by name.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>T</returns>
        public T GetDynamicProperty<T>(string propertyName)
        {
            var type = GetType();
            var propInfo = type.GetProperty(propertyName);

            return (T)propInfo.GetValue(this, null);
        }

        /// <summary>
        /// Gets the dynamic property by name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>value</returns>
        public object GetDynamicProperty(string propertyName)
        {
            return GetDynamicProperty<object>(propertyName);
        }

        /// <summary>
        /// Sets the dynamic property by name.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public void SetDynamicProperty<T>(string propertyName, T value)
        {
            var type = GetType();
            var propInfo = type.GetProperty(propertyName);

            propInfo.SetValue(this, value, null);
        }

        /// <summary>
        /// Sets the dynamic property by name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public void SetDynamicProperty(string propertyName, object value)
        {
            SetDynamicProperty<object>(propertyName, value);
        }

        /// <summary>
        /// Overrides the ToString() method
        /// </summary>
        /// <returns>Returns properties in string format</returns>
        public override string ToString()
        {
            PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            StringBuilder sb = new StringBuilder();

            sb.Append("{");

            for (int i = 0; i < props.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(props[i].Name);
                sb.Append("=");
                sb.Append(props[i].GetValue(this, null));
            }

            sb.Append("}");

            return sb.ToString();
        }
    }
}
