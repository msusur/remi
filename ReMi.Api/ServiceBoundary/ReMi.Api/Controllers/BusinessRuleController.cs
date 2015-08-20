using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.WebApi;
using ReMi.Queries.BusinessRules;
using System.Web.Http;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("rule")]
    public class BusinessRuleController : ApiController
    {
        public IImplementQueryAction<TestBusinessRuleRequest, TestBusinessRuleResponse> TestBusinessRuleAction { get; set; }
        public IImplementQueryAction<TriggerBusinessRuleRequest, TriggerBusinessRuleResponse> TriggerBusinessRuleAction { get; set; }
        public IImplementQueryAction<GetBusinessRuleRequest, GetBusinessRuleResponse> GetBusinessRuleAction { get; set; }
        public IImplementQueryAction<GetBusinessRulesRequest, GetBusinessRulesResponse> GetBusinessRulesAction { get; set; }
        public IImplementQueryAction<GetGeneratedRuleRequest, GetGeneratedRuleResponse> GetGeneratedCommandRuleAction { get; set; }

        [HttpPost]
        [Route("{group}/{rule}")]
        public TriggerBusinessRuleResponse TriggerRule(BusinessRuleGroup group, string rule, [FromBody]IDictionary<string, string> parameters)
        {
            var request = new TriggerBusinessRuleRequest
            {
                Group = group,
                Rule = rule,
                Parameters = parameters
            };

            return TriggerBusinessRuleAction.Handle(ActionContext, request);
        }

        [HttpPost]
        [Route("{ruleId}")]
        public TriggerBusinessRuleResponse TriggerRule(Guid ruleId, [FromBody]IDictionary<string, string> parameters)
        {
            var request = new TriggerBusinessRuleRequest
            {
                ExternalId = ruleId,
                Parameters = parameters
            };

            return TriggerBusinessRuleAction.Handle(ActionContext, request);
        }


        [HttpPost]
        [Route("test")]
        public TestBusinessRuleResponse TestRule([FromBody]BusinessRuleDescription rule)
        {
            var request = new TestBusinessRuleRequest
            {
                Rule = rule
            };

            return TestBusinessRuleAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("{group}/{rule}")]
        public GetBusinessRuleResponse GetRule(BusinessRuleGroup group, string rule)
        {
            var request = new GetBusinessRuleRequest
            {
                Group = group,
                Name = rule
            };

            return GetBusinessRuleAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("{ruleId}")]
        public GetBusinessRuleResponse GetRule(Guid ruleId)
        {
            var request = new GetBusinessRuleRequest
            {
                ExternalId = ruleId
            };

            return GetBusinessRuleAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("rules")]
        public GetBusinessRulesResponse GetRules()
        {
            var request = new GetBusinessRulesRequest();

            return GetBusinessRulesAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("generate/{ns}/{command}")]
        public GetGeneratedRuleResponse GetGeneratedCommandRule(string ns, string command)
        {
            var request = new GetGeneratedRuleRequest
            {
                Name = command,
                Namespace = ns
            };

            return GetGeneratedCommandRuleAction.Handle(ActionContext, request);
        }
    }
}
