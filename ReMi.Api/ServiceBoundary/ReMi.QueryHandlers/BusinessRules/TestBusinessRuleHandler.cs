using ReMi.BusinessLogic.BusinessRules;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.BusinessRules;

namespace ReMi.QueryHandlers.BusinessRules
{
    public class TestBusinessRuleHandler : IHandleQuery<TestBusinessRuleRequest, TestBusinessRuleResponse>
    {
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        public TestBusinessRuleResponse Handle(TestBusinessRuleRequest request)
        {
            return new TestBusinessRuleResponse
            {
                Result = BusinessRuleEngine.Test(request.Rule)
            };
        }
    }
}
