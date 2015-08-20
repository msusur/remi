using System;

namespace ReMi.Contracts.Cqrs.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        private readonly string _description;
        private readonly CommandGroup _group;

        public CommandAttribute(string description, CommandGroup group)
        {
            _group = group;
            _description = description;
        }

        public bool IsBackground { get; set; }

        public string Description
        {
            get { return _description; }
        }

        public CommandGroup Group
        {
            get { return _group; }
        }
    }
}
