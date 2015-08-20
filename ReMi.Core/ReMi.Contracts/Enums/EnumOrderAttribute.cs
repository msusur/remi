using System;

namespace ReMi.Contracts.Enums
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumOrderAttribute : Attribute
    {
        private readonly int _order;

        public EnumOrderAttribute(int order)
        {
            if (order < 1)
                throw new ArgumentNullException("order");

            _order = order;
        }

        public int Order { get { return _order; } }
    }
}
