using ReMi.DataEntities.BusinessRules;
using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.Exceptions
{
    public class BusinessRuleTestDataNotNotFoundException : EntityNotFoundException
    {
        public BusinessRuleTestDataNotNotFoundException(Guid testDataId) :
            base("BusinessRuleTestData", testDataId)
        {
        }

        public BusinessRuleTestDataNotNotFoundException(Guid testDataId, Exception innerException) :
            base("BusinessRuleTestData", testDataId, innerException)
        {
        }

        public BusinessRuleTestDataNotNotFoundException(List<BusinessRuleTestData> testData) :
            base("BusinessRuleTestData", testData)
        {
        }

        public BusinessRuleTestDataNotNotFoundException(List<BusinessRuleTestData> testData, Exception innerException) :
            base("BusinessRuleTestData", testData, innerException)
        {
        }
    }
}
