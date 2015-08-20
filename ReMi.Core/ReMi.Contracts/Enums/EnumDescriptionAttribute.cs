using System;

namespace ReMi.Contracts.Enums
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDescriptionAttribute : Attribute
    {
        private readonly string _description;

        public EnumDescriptionAttribute(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException("description");

            _description = description;
        }

        public string Description { get { return _description; } }

        public string Annotation { get; set; }
    }
}
