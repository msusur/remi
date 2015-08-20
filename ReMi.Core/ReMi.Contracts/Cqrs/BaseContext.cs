using System;
namespace ReMi.Contracts.Cqrs
{
    public abstract class BaseContext
    {
        public Guid Id { get; set; }

        public string UserHostAddress { get; set; }

        public string UserHostName { get; set; }

        public string UserName { get; set; }

        public Guid UserId { get; set; }

        public string UserEmail { get; set; }

        public string UserRole { get; set; }

        public Guid ParentId { get; set; }

        public override string ToString()
        {
            return string.Format("[Id={0}, UserName={1}, UserId={2}, UserEmail={3}, UserHostAddress={4}, UserHostName={5}, UserHostName={6}, ParentId={7}]",
                Id, UserName, UserId, UserEmail, UserHostAddress, UserHostName, UserRole, ParentId);
        }
    }
}
