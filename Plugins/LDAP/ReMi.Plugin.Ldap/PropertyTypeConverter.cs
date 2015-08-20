using System;

namespace ReMi.Plugin.Ldap
{
    public interface IPropertyTypeConverter<T>
    {
        T Convert(object propertyValue);
    }

    public class ByteToGuidConverter : IPropertyTypeConverter<Guid>
    {
        public Guid Convert(object value)
        {
            return new Guid((byte[])value);

        }
    }
}
