using System;
using ReMi.BusinessEntities.Auth;

namespace ReMi.BusinessEntities.ReleaseExecution
{
    public class SignOff
    {
        public Account Signer { get; set; }
        public Guid ExternalId { get; set; }
        public Boolean SignedOff { get; set; }
        public String Comment { get; set; }

        public override string ToString()
        {
            return String.Format("[Signer={0},ExternalId={1},SignedOff={2},Comment={3}]", Signer, ExternalId,
                SignedOff, Comment);
        }
    }
}
