using System;

namespace ReMi.BusinessEntities.Auth
{
    public class TokenParts
    {
        public string UserName { get; set; }
        public Guid SessionId { get; set; }
    }
}
