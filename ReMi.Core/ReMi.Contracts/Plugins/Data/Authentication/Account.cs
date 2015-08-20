using System;

namespace ReMi.Contracts.Plugins.Data.Authentication
{
    public class Account
    {
        public Guid AccountId { get; set; }
        public string Mail { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public long PasswordLastSet { get; set; }
        public long LastLogonTime { get; set; }
        public DateTime WhenChanged { get; set; }

        public string[] Groups { get; set; }
    }
}
