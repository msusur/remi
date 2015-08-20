using AutoMapper;
using Moq;
using NUnit.Framework;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using ReMi.Plugin.ZenDesk;
using ReMi.Plugin.ZenDesk.AutoMapper;
using ReMi.Plugin.ZenDesk.BusinessLogic;
using ReMi.Plugin.ZenDesk.DataAccess.Gateways;
using ReMi.TestUtils.UnitTests;

namespace ReMi.Plugin.Zendesk.Tests.IntegrationTests
{
    [TestFixture]
    public class ZenDeskServiceTest : TestClassFor<ZenDeskService>
    {
        private const string ZenDeskUser = "";
        private const string ZenDeskPassword = "";
        private const string ZenDeskUrl = "";

        private Mock<IGlobalConfigurationGateway> _globalConfigurationGatewayMock;
        private IZenDeskRequest _zenDeskRequest;
        private IMappingEngine _mappingEngine;

        protected override ZenDeskService ConstructSystemUnderTest()
        {
            return new ZenDeskService
            {
                Mapper = _mappingEngine,
                ZenDeskRequest = _zenDeskRequest
            };
        }

        private static IMappingEngine CreateMapper()
        {
            var profiles = new Profile[]
            {
                new ZenDeskModelToContractContract(),
                new ContractModelToZenDeskModel(),
                new ZenDeskBusinessEnitiesToDataEntities(),
                new ZenDeskDataEntitiesToBusinessEnities()
            };
            Mapper.Initialize(c => profiles.Each(c.AddProfile));

            return Mapper.Engine;
        }

        protected override void TestInitialize()
        {
            _globalConfigurationGatewayMock = new Mock<IGlobalConfigurationGateway>();
            _globalConfigurationGatewayMock.Setup(x => x.GetGlobalConfiguration())
                .Returns(new PluginConfigurationEntity
                {
                    ZenDeskPassword = ZenDeskPassword,
                    ZenDeskUrl = ZenDeskUrl,
                    ZenDeskUser = ZenDeskUser
                });
            _zenDeskRequest = new ZenDeskRequest
            {
                GlobalConfigurationGatewayFactory = () => _globalConfigurationGatewayMock.Object
            };
            _mappingEngine = CreateMapper();

            base.TestInitialize();
        }

        [Test, Explicit]
        public void ShouldCreateAndDeleteTicket_WhenHaveAccessToZenDesk()
        {
            var ticket = new HelpDeskTicket
            {
                Comment = "Test Ticket Comment",
                Description = "Test Ticket Description",
                Priority = "low",
                Subject = "Test Ticket Subject"
            };

            var createdTicket = Sut.CreateTicket(ticket, null);

            Assert.IsNotNullOrEmpty(createdTicket.Id);
            Assert.IsNotNullOrEmpty(createdTicket.Url);
            Assert.AreEqual("Test Ticket Description", createdTicket.Description);
            Assert.AreEqual("low", createdTicket.Priority);
            Assert.AreEqual("Test Ticket Subject", createdTicket.Subject);

            Sut.DeleteTicket(createdTicket.Id, null);
        }

        [Test, Explicit]
        public void ShouldCreateUpdateAndDeleteTicket_WhenHaveAccessToZenDesk()
        {
            var ticket = new HelpDeskTicket
            {
                Comment = "Test Ticket Comment",
                Description = "Test Ticket Description",
                Priority = "low",
                Subject = "Test Ticket Subject"
            };

            var createdTicket = Sut.CreateTicket(ticket, null);
            
            createdTicket.Subject = "Updated Subject";
            createdTicket.Description = "Updated Description";
            createdTicket.Comment = "Updated Comment";
            createdTicket.Priority = "high";
            var updatedTicket = Sut.UpdateTicket(createdTicket, null);

            Assert.AreEqual(createdTicket.Id, updatedTicket.Id);
            Assert.AreEqual(createdTicket.Url, updatedTicket.Url);
            Assert.AreEqual("high", updatedTicket.Priority);
            Assert.AreEqual("Updated Subject", updatedTicket.Subject);

            Sut.DeleteTicket(createdTicket.Id, null);
        }
    }
}
