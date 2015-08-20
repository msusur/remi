using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using ReMi.Common.Utils.Enums;

namespace ReMi.Common.Utils.Repository
{
    public static class DbSetExtensions
    {
        public static void AddOrUpdateEnum<TEnum, T>(this IDbSet<T> dbSet)
            where T : EnumDescription, new()
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumDescriptions = EnumDescriptionHelper.GetEnumDescriptions<TEnum, T>();

            dbSet.AddOrUpdate(
                x => x.Id,
                enumDescriptions);
        }

        public static void AddOrUpdateOrderedEnum<TEnum, T>(this IDbSet<T> dbSet)
            where T : OrderedEnumDescription, new()
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            var enumDescriptions = EnumDescriptionHelper.GetOrderedEnumDescriptions<TEnum, T>();

            if (!enumDescriptions.All(d => enumDescriptions.Where(e => e.Name != d.Name).All(e => e.Order != d.Order)))
            {
                throw new Exception("Metric type orders should be unique");
            }

            dbSet.AddOrUpdate(
                x => x.Id,
                enumDescriptions);
        }
    }
}
