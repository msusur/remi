using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ReMi.Contracts.Enums;

namespace ReMi.Common.Utils.Enums
{
    public static class EnumDescriptionHelper
    {
        public static string GetDescription(Type enumType, object value)
        {
            return enumType.GetFields()
                    .Where(x => x.Name.Equals(value.ToString()))
                    .Select(x =>
                    {
                        var enumDescriptionAttr =
                            x.CustomAttributes.FirstOrDefault(
                                a => a.AttributeType == typeof(EnumDescriptionAttribute));

                        return enumDescriptionAttr == null ? x.Name : (string)enumDescriptionAttr.ConstructorArguments.First().Value;
                    }).First();
        }

        public static string GetDescription<TEnum>(TEnum value)
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumType = GetEnumType<TEnum>();
            return GetDescription(enumType, value);
        }

        public static string ToDescriptionString<TEnum>(this TEnum value)
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            return GetDescription(value);
        }

        public static T GetEnumDescription<TEnum, T>(TEnum value)
            where T : EnumDescription, new()
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumType = GetEnumType<TEnum>();
            return enumType.GetFields()
                    .Where(x => x.Name.Equals(value.ToString(CultureInfo.InvariantCulture)))
                    .Select(CreateDescription<T>).First();
        }

        public static T ToEnumDescription<TEnum, T>(this TEnum value)
            where TEnum : struct, IConvertible, IComparable, IFormattable
            where T : EnumDescription, new()
        {
            return GetEnumDescription<TEnum, T>(value);
        }

        public static T[] GetEnumDescriptions<TEnum, T>()
            where T : EnumDescription, new()
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumType = GetEnumType<TEnum>();
            return enumType.GetFields()
                    .Where(x => !x.Name.Equals("value__"))
                    .Select(CreateDescription<T>).ToArray();
        }

        public static int GetOrder<TEnum>(TEnum value)
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumType = GetEnumType<TEnum>();
            return enumType.GetFields()
                    .Where(x => x.Name.Equals(value.ToString(CultureInfo.InvariantCulture)))
                    .Select(x =>
                    {
                        var enuOrderAttr =
                            x.CustomAttributes.FirstOrDefault(
                                a => a.AttributeType == typeof(EnumOrderAttribute));

                        if (enuOrderAttr == null)
                            return -1;

                        return Convert.ToInt32(enuOrderAttr.ConstructorArguments.First().Value);
                    }).First();
        }

        public static T GetOrderedEnumDescription<TEnum, T>(TEnum value)
            where T : OrderedEnumDescription, new()
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumType = GetEnumType<TEnum>();
            return enumType.GetFields()
                    .Where(x => x.Name.Equals(value.ToString(CultureInfo.InvariantCulture)))
                    .Select(CreateOrderedDescription<T>).First();
        }

        public static T ToEnumOrderDescription<TEnum, T>(this TEnum value)
            where TEnum : struct, IConvertible, IComparable, IFormattable
            where T : OrderedEnumDescription, new()
        {
            return GetOrderedEnumDescription<TEnum, T>(value);
        }

        public static T[] GetOrderedEnumDescriptions<TEnum, T>()
            where T : OrderedEnumDescription, new()
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumType = GetEnumType<TEnum>();
            return enumType.GetFields()
                    .Where(x => !x.Name.Equals("value__"))
                    .Select(CreateOrderedDescription<T>).ToArray();
        }

        private static Type GetEnumType<TEnum>() where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
                throw new InvalidEnumArgumentException(string.Format("{0} this is not enum type", enumType.Name));
            return enumType;
        }

        private static T CreateDescription<T>(FieldInfo x) where T : EnumDescription, new()
        {
            var enumDescriptionAttr =
                x.CustomAttributes.FirstOrDefault(
                    a => a.AttributeType == typeof(EnumDescriptionAttribute));

            return new T
            {
                Id = (int)x.GetRawConstantValue(),
                Name = x.Name,
                Description =
                    enumDescriptionAttr == null ? x.Name : (string)enumDescriptionAttr.ConstructorArguments.First().Value,
                Annotation = enumDescriptionAttr == null || enumDescriptionAttr.NamedArguments == null || enumDescriptionAttr.NamedArguments.Count == 0
                    ? null
                    : (string)enumDescriptionAttr.NamedArguments.First(n => n.MemberName == "Annotation").TypedValue.Value
            };
        }

        private static T CreateOrderedDescription<T>(FieldInfo x) where T : OrderedEnumDescription, new()
        {
            var enumOrderAttr =
                x.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof (EnumOrderAttribute));
            var result = CreateDescription<T>(x);
            result.Order = enumOrderAttr == null
                ? (int) x.GetRawConstantValue()
                : (int) enumOrderAttr.ConstructorArguments.First().Value;

            return result;
        }
    }
}
