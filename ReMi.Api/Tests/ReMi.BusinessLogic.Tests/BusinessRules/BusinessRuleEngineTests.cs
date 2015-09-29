using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.BusinessRules;
using ReMi.TestUtils.UnitTests;

namespace ReMi.BusinessLogic.Tests.BusinessRules
{
    [TestFixture]
    public class BusinessRuleEngineTests : TestClassFor<BusinessRuleEngine>
    {
        private Mock<IBusinessRuleGateway> _businessRuleGatewayMock;
        private Mock<ISerialization> _serializationMock;
        private Mock<IApplicationSettings> _applicationSettingsMock;
        private Mock<IAccountsGateway> _accountGatewayMock;

        private readonly Account _account = new Account { FullName = "Test Account", ExternalId = Guid.NewGuid() };

        protected override void TestInitialize()
        {
            _businessRuleGatewayMock = new Mock<IBusinessRuleGateway>(MockBehavior.Strict);
            _serializationMock = new Mock<ISerialization>(MockBehavior.Strict);
            _applicationSettingsMock = new Mock<IApplicationSettings>(MockBehavior.Strict);
            _accountGatewayMock = new Mock<IAccountsGateway>(MockBehavior.Strict);
            _accountGatewayMock.Setup(x => x.Dispose());
            _businessRuleGatewayMock.Setup(x => x.Dispose());

            base.TestInitialize();
        }

        protected override BusinessRuleEngine ConstructSystemUnderTest()
        {
            return new BusinessRuleEngine
            {
                BusinessRuleGatewayFactory = () => _businessRuleGatewayMock.Object,
                AccountGatewayFactory = () => _accountGatewayMock.Object,
                Serialization = _serializationMock.Object,
                ApplicationSettings = _applicationSettingsMock.Object
            };
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleEmptyException))]
        public void Execute_ShouldThrowException_WhenRuleDescriptionNotFound()
        {
            var ruleId = Guid.NewGuid();

            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(ruleId))
                .Returns((BusinessRuleDescription)null);
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);

            Sut.Execute(_account.ExternalId, ruleId, new Dictionary<string, string>());

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(ruleId), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleEmptyException))]
        public void Execute_ShouldThrowException_WhenScriptIsEmpty()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Release,
                Script = string.Empty
            };
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(rule.ExternalId))
                .Returns(rule);
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);

            Sut.Execute(_account.ExternalId, rule.ExternalId, new Dictionary<string, string>());

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(rule.ExternalId), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleParamtersMissmatchException))]
        public void Execute_ShouldThrowException_WhenReceivedEmptyListOfParametersAndRuleRequireNotEmpty()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Permissions,
                Script = "return string.Empty;",
                Parameters = new[] { new BusinessRuleParameter { Name = "Parameter1" } }
            };
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(rule.ExternalId))
                .Returns(rule);
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);

            Sut.Execute(_account.ExternalId, rule.ExternalId, new Dictionary<string, string>());

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(rule.ExternalId), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleParamtersMissmatchException))]
        public void Execute_ShouldThrowException_WhenReceivedNotEmptyListOfParametersAndRuleRequireEmpty()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Permissions,
                Script = "return string.Empty;"
            };
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(rule.ExternalId))
                .Returns(rule);
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);

            Sut.Execute(_account.ExternalId, rule.ExternalId, new Dictionary<string, string> { { "Parameter1", "1" } });

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(rule.ExternalId), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleParamtersMissmatchException))]
        public void Execute_ShouldThrowException_WhenReceivedParameterHasDifferentNameThenRequired()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Permissions,
                Script = "return string.Empty;",
                Parameters = new[]
                {
                    new BusinessRuleParameter { Name = "Parameter1" },
                    new BusinessRuleParameter { Name = "Parameter2" }
                }
            };
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(rule.ExternalId))
                .Returns(rule);
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);

            Sut.Execute(_account.ExternalId, rule.ExternalId, new Dictionary<string, string>
            {
                { "Parameter1", "1" },
                { "Parameter3", "1" }
            });

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(rule.ExternalId), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleParamterTypeMissmatchException))]
        public void ExecuteGeneric_ShouldThrowException_WhenReceivedParameterHasDifferentTypeThenRequired()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Permissions,
                Script = "return string.Empty;",
                Parameters = new[]
                {
                    new BusinessRuleParameter { Name = "Parameter1", Type = "int"},
                    new BusinessRuleParameter { Name = "Parameter2", Type = "string" }
                }
            };
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(rule.ExternalId))
                .Returns(rule);

            Sut.Execute<object>(_account.ExternalId, rule.ExternalId, new Dictionary<string, object>
            {
                { "Parameter1", "1" },
                { "Parameter2", "1" }
            });

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(rule.ExternalId), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleParamterTypeMissmatchException))]
        public void Execute_ShouldThrowException_WhenReceivedParameterValueCannotBeDeserialized()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Permissions,
                Script = "return string.Empty;",
                Parameters = new[]
                {
                    new BusinessRuleParameter { Name = "Parameter1", Type = "int" },
                    new BusinessRuleParameter { Name = "Parameter2", Type = "string" }
                }
            };
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(rule.ExternalId))
                .Returns(rule);
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);

            Sut.Execute(_account.ExternalId, rule.ExternalId, new Dictionary<string, string>
            {
                { "Parameter1", "abc" },
                { "Parameter2", "abc" }
            });

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(rule.ExternalId), Times.Once);
        }

        [Test]
        public void Execute_ShouldReturnScriptResult_WhenExecuted()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Permissions,
                Script = "return string.Format(\"{0} {1}\", Parameter2, Parameter1);",
                Parameters = new[]
                {
                    new BusinessRuleParameter { Name = "Parameter1", Type = "int"},
                    new BusinessRuleParameter { Name = "Parameter2", Type = "string"}
                }
            };
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(rule.ExternalId))
                .Returns(rule);
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);
            _serializationMock.Setup(x => x.FromJson("5", typeof(int)))
                .Returns(5);
            _serializationMock.Setup(x => x.FromJson("test", typeof(string)))
                .Returns("test");
            _applicationSettingsMock.SetupGet(x => x.LogJsonFormatted).Returns(true);
            _serializationMock.Setup(x => x.ToJson(It.IsAny<object>(), It.Is<IEnumerable<string>>(s => s.Count() == 1 && s.First() == "Password"), true))
                .Returns("{}");

            var result = Sut.Execute(_account.ExternalId, rule.ExternalId, new Dictionary<string, string>
            {
                { "Parameter1", "5" },
                { "Parameter2", "test" }
            });

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(rule.ExternalId), Times.Once);
            Assert.AreEqual(result, "test 5");
        }

        [Test]
        public void ExecuteByName_ShouldReturnScriptResult_WhenExecuted()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Permissions,
                Script = "return string.Format(\"{0} {1}\", Parameter2, Parameter1);",
                Parameters = new[]
                {
                    new BusinessRuleParameter { Name = "Parameter1", Type = "int"},
                    new BusinessRuleParameter { Name = "Parameter2", Type = "string"}
                }
            };
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(rule.Group, rule.Name))
                .Returns(rule);
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);
            _serializationMock.Setup(x => x.FromJson("5", typeof(int)))
                .Returns(5);
            _serializationMock.Setup(x => x.FromJson("test", typeof(string)))
                .Returns("test");
            _applicationSettingsMock.SetupGet(x => x.LogJsonFormatted).Returns(true);
            _serializationMock.Setup(x => x.ToJson(It.IsAny<object>(), It.Is<IEnumerable<string>>(s => s.Count() == 1 && s.First() == "Password"), true))
                .Returns("{}");

            var result = Sut.Execute(_account.ExternalId, BusinessRuleGroup.Permissions, rule.Name, new Dictionary<string, string>
            {
                { "Parameter1", "5" },
                { "Parameter2", "test" }
            });

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(rule.Group, rule.Name), Times.Once);
            Assert.AreEqual(result, "test 5");
        }
        [Test]
        public void Execute_ShouldRunScriptWithComplexParameter_WhenExecuted()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Release,
                Script = "return string.Format(\"{0:dd-MM-yyyy} {1}\", Parameter2.First().CreatedOn, Parameter1);",
                Parameters = new[]
                {
                    new BusinessRuleParameter { Name = "Parameter1", Type = "int"},
                    new BusinessRuleParameter
                    {
                        Name = "Parameter2",
                        Type = "System.Collections.Generic.IEnumerable<ReMi.BusinessEntities.Auth.Account>"
                    }
                }
            };
            const string parameter2 = "[{ \"CreatedOn\": \"2010-01-01T10:00:00.000\" }]";
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(rule.ExternalId))
                .Returns(rule);
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);
            _serializationMock.Setup(x => x.FromJson("5", typeof(int)))
                .Returns(5);
            _serializationMock.Setup(x => x.FromJson(parameter2, typeof(IEnumerable<Account>)))
                .Returns(new[] { new Account { CreatedOn = new DateTime(2010, 1, 1, 10, 0, 0) } });
            _applicationSettingsMock.SetupGet(x => x.LogJsonFormatted).Returns(false);
            _serializationMock.Setup(x => x.ToJson(It.IsAny<object>(), It.Is<IEnumerable<string>>(s => s.Count() == 1 && s.First() == "Password"), false))
                .Returns("{}");

            var result = Sut.Execute(_account.ExternalId, rule.ExternalId, new Dictionary<string, string>
            {
                { "Parameter1", "5" },
                { "Parameter2", parameter2 }
            });

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(rule.ExternalId), Times.Once);
            Assert.AreEqual(result, "01-01-2010 5");
        }

        [Test]
        public void ExecuteGeneric_ShouldReturnScriptResult_WhenExecuted()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Permissions,
                Script = "return string.Format(\"{0} {1} {2}\", Parameter2, Parameter1, account.FullName);",
                Parameters = new[]
                {
                    new BusinessRuleParameter { Name = "Parameter1", Type = "int"},
                    new BusinessRuleParameter { Name = "Parameter2", Type = "string"}
                }
            };
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(rule.ExternalId))
                .Returns(rule);
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);
            _applicationSettingsMock.SetupGet(x => x.LogJsonFormatted).Returns(true);
            _serializationMock.Setup(x => x.ToJson(It.IsAny<object>(), It.Is<IEnumerable<string>>(s => s.Count() == 1 && s.First() == "Password"), true))
                .Returns("{}");

            var result = Sut.Execute<string>(_account.ExternalId, rule.ExternalId, new Dictionary<string, object>
            {
                { "Parameter1", 5 },
                { "Parameter2", "test" }
            });

            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(rule.ExternalId), Times.Once);
            Assert.AreEqual(result, "test 5 Test Account");
        }

        [Test]
        public void Test_ShouldCompileScript_WhenExecuted()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Permissions,
                AccountTestData = new BusinessRuleAccountTestData
                {
                    JsonData = "{ CreatedOn: \"2014-05-28T08:30:39.017\"}"
                },
                Script = "return string.Format(\"{0} {1}\", Parameter2, Parameter1);",
                Parameters = new[]
                {
                    new BusinessRuleParameter {
                        Name = "Parameter1",
                        Type = "int",
                        TestData = new BusinessRuleTestData
                        {
                            JsonData = "2"
                        }
                    },
                    new BusinessRuleParameter
                    {
                        Name = "Parameter2",
                        Type = "string",
                        TestData = new BusinessRuleTestData
                        {
                            JsonData = "TEXT"
                        }
                    }
                }
            };

            _serializationMock.Setup(x => x.FromJson<Account>(rule.AccountTestData.JsonData))
                .Returns(new Account());
            _serializationMock.Setup(x => x.FromJson("2", typeof(int)))
                .Returns(2);
            _applicationSettingsMock.SetupGet(x => x.LogJsonFormatted).Returns(true);
            _serializationMock.Setup(x => x.ToJson(It.IsAny<object>(), It.Is<IEnumerable<string>>(s => s.Count() == 1 && s.First() == "Password"), true))
                .Returns("{}");

            var result = (string)Sut.Test(rule);

            Assert.AreEqual("TEXT 2", result);
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleCompilationException))]
        public void Test_ShouldThrowCompilationException_WhenScriptCannotBeCompiled()
        {
            var rule = new BusinessRuleDescription
            {
                Name = "RuleName",
                ExternalId = Guid.NewGuid(),
                Group = BusinessRuleGroup.Permissions,
                AccountTestData = new BusinessRuleAccountTestData
                {
                    JsonData = "{ CreatedOn: \"2014-05-28T08:30:39.017\"}"
                },
                Script = "return string.Forma(\"{0} {1}\", Parameter2, Parameter1);", //Forma method does not exist
                Parameters = new[]
                {
                    new BusinessRuleParameter {
                        Name = "Parameter1",
                        Type = "int",
                        TestData = new BusinessRuleTestData
                        {
                            JsonData = "2"
                        }
                    },
                    new BusinessRuleParameter
                    {
                        Name = "Parameter2",
                        Type = "string",
                        TestData = new BusinessRuleTestData
                        {
                            JsonData = "TEXT"
                        }
                    }
                }
            };


            _serializationMock.Setup(x => x.FromJson<Account>(rule.AccountTestData.JsonData))
                .Returns(new Account());
            _serializationMock.Setup(x => x.FromJson("2", typeof(int)))
                .Returns(2);

            Sut.Test(rule);
        }

        [Test]
        public void GetType_ShouldResolveTypeInt32_WhenGivenTypeNameInt()
        {
            const string typeString = "int";
            var result = Sut.GetType(typeString);

            Assert.AreEqual(typeof(int), result);
        }

        [Test]
        public void GetType_ShouldResolveGenericType_WhenGivenTypeName()
        {
            const string typeString = "IDictionary<List<int>, string[]>";
            var result = Sut.GetType(typeString);

            Assert.AreEqual(typeof(IDictionary<List<int>, string[]>), result);
        }
    }
}
