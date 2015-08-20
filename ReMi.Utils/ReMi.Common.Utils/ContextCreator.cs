using System;
using ReMi.Contracts.Cqrs;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Common.Utils
{
    public static class ContextCreator
    {
        public static CommandContext CreateChild<T>(this T context)
            where T : CommandContext, new()
        {
            var childContext = CreateChildContext<CommandContext>(context);
            childContext.IsSynchronous = context.IsSynchronous;
            return childContext;
        }

        public static T CreateChild<T>(this BaseContext context) where T : BaseContext, new()
        {
            return CreateChildContext<T>(context);
        }

        private static T CreateChildContext<T>(BaseContext context) where T : BaseContext, new()
        {
            var childContext = new T {Id = Guid.NewGuid()};
            if (context == null)
                return childContext;

            childContext.UserHostAddress = context.UserHostAddress;
            childContext.UserEmail = context.UserEmail;
            childContext.UserHostName = context.UserHostName;
            childContext.UserId = context.UserId;
            childContext.UserName = context.UserName;
            childContext.UserRole = context.UserRole;
            childContext.ParentId = context.Id;

            return childContext;
        }
    }
}
