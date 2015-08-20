using System;
using System.Collections.Generic;

namespace ReMi.Common.Constants.BusinessRules
{
    public class RuleConstants
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ExternalId { get; set; }

        public IList<ParameterConstants> Parameters { get; set; } 
    }
}
