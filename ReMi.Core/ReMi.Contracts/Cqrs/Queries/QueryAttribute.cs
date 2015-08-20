using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Contracts.Cqrs.Queries
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueryAttribute : Attribute
    {
        private readonly string _description;
        private readonly QueryGroup _group;

        public QueryAttribute(string description, QueryGroup group)
        {
            _group = group;
            _description = description;
        }

        public string Description
        {
            get { return _description; }
        }

        public QueryGroup Group
        {
            get { return _group; }
        }

        public bool IsStatic { get; set; }
    }
}
