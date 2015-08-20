using System;
using AutoMapper;
using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.AutoMapper;
using ReMi.Commands.Configuration;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.CommandHandlers.Tests.AutoMapper
{
    [TestFixture]
    public class ApiCommandToBusinessEntityMappingProfileTests : TestClassFor<IMappingEngine>
    {
        [TestFixtureSetUp]
        public static void AutoMapperInitialize()
        {
            Mapper.Initialize(
                c => c.AddProfile<ApiCommandToBusinessEntityMappingProfile>());

        }

        protected override IMappingEngine ConstructSystemUnderTest()
        {
            return Mapper.Engine;
        }

        [Test]
        public void Map_ShouldReturnReleaseWindow_WhenBookReleaseWindowRequest()
        {
            var input = new BookReleaseWindowCommand
            {
                ReleaseWindow = Builder<ReleaseWindow>.CreateNew()
                    .With(x => x.Products, new[] { RandomData.RandomString(5) })
                    .Build()
            };

            var result = Sut.Map<BookReleaseWindowCommand, ReleaseWindow>(input);

            AssertBusinessEntities.PropertiesAreSame(input.ReleaseWindow, result);
        }

        [Test]
        public void Map_ShouldReturnReleaseWindow_WhenCancelReleaseWindowRequest()
        {
            var input = Builder<CancelReleaseWindowCommand>.CreateNew().Build();

            var result = Sut.Map<CancelReleaseWindowCommand, ReleaseWindow>(input);

            Assert.AreEqual(input.ExternalId, result.ExternalId);
        }

        [Test]
        public void Map_ShouldReturnReleaseWindow_WhenUpdateReleaseWindowRequest()
        {
            var input = new UpdateReleaseWindowCommand
            {
                ReleaseWindow = Builder<ReleaseWindow>.CreateNew()
                    .With(x => x.Products, new[] { RandomData.RandomString(5) })
                    .Build()
            };

            var result = Sut.Map<UpdateReleaseWindowCommand, ReleaseWindow>(input);

            AssertBusinessEntities.PropertiesAreSame(input.ReleaseWindow, result);
        }

        [Test]
        public void Map_ShouldReturnBusinessTicket_WhenUpdateTicketCommentCommand()
        {
            var input = Builder<UpdateTicketCommentCommand>.CreateNew()
                .With(x => x.Comment = RandomData.RandomString(100))
                .With(x => x.TicketId = new Guid())
                .With(x => x.TicketKey = RandomData.RandomString(10))
                .With(x => x.CommandContext, Builder<CommandContext>.CreateNew()
                    .With(y => y.UserId, Guid.NewGuid())
                    .Build())
                .Build();

            var result = Sut.Map<UpdateTicketCommentCommand, ReleaseContentTicket>(input);

            Assert.AreEqual(input.Comment, result.Comment);
            Assert.AreEqual(input.TicketId, result.TicketId);
            Assert.AreEqual(input.TicketKey, result.TicketName);
            Assert.AreEqual(input.CommandContext.UserId, result.LastChangedByAccount);
        }

        [Test]
        public void Map_ShouldReturnBusinessTicket_WhenUpdateTicketRiksCommand()
        {
            var input = Builder<UpdateTicketRiskCommand>.CreateNew()
                .With(x => x.Risk = RandomData.RandomEnum<TicketRisk>().ToString())
                .With(x => x.TicketId = Guid.NewGuid())
                .With(x => x.TicketKey = RandomData.RandomString(10))
                .With(x => x.CommandContext, Builder<CommandContext>.CreateNew()
                    .With(y => y.UserId, Guid.NewGuid())
                    .Build())
                .Build();

            var result = Sut.Map<UpdateTicketRiskCommand, ReleaseContentTicket>(input);

            Assert.AreEqual(input.Risk, result.Risk.ToString());
            Assert.AreEqual(input.TicketId, result.TicketId);
            Assert.AreEqual(input.TicketKey, result.TicketName);
            Assert.AreEqual(input.CommandContext.UserId, result.LastChangedByAccount);
        }

        [Test]
        public void Map_ShouldReturnProduct_WhenAddProductCommand()
        {
            var input = Builder<AddProductCommand>.CreateNew()
                .With(x => x.ExternalId = Guid.NewGuid())
                .With(x => x.BusinessUnitId = Guid.NewGuid())
                .With(x => x.ChooseTicketsByDefault = RandomData.RandomBool())
                .With(x => x.Description = RandomData.RandomString(10))
                .With(x => x.ReleaseTrack = RandomData.RandomEnum<ReleaseTrack>())
                .Build();

            var result = Sut.Map<AddProductCommand, Product>(input);

            Assert.AreEqual(input.ExternalId, result.ExternalId);
            Assert.AreEqual(input.BusinessUnitId, result.BusinessUnit.ExternalId);
            Assert.AreEqual(input.ChooseTicketsByDefault, result.ChooseTicketsByDefault);
            Assert.AreEqual(input.Description, result.Description);
            Assert.AreEqual(input.ReleaseTrack, result.ReleaseTrack);
        }

        [Test]
        public void Map_ShouldReturnProduct_WhenUpdateProductCommand()
        {
            var input = Builder<UpdateProductCommand>.CreateNew()
                .With(x => x.ExternalId = Guid.NewGuid())
                .With(x => x.BusinessUnitId = Guid.NewGuid())
                .With(x => x.ChooseTicketsByDefault = RandomData.RandomBool())
                .With(x => x.Description = RandomData.RandomString(10))
                .With(x => x.ReleaseTrack = RandomData.RandomEnum<ReleaseTrack>())
                .Build();

            var result = Sut.Map<UpdateProductCommand, Product>(input);

            Assert.AreEqual(input.ExternalId, result.ExternalId);
            Assert.AreEqual(input.BusinessUnitId, result.BusinessUnit.ExternalId);
            Assert.AreEqual(input.ChooseTicketsByDefault, result.ChooseTicketsByDefault);
            Assert.AreEqual(input.Description, result.Description);
            Assert.AreEqual(input.ReleaseTrack, result.ReleaseTrack);
        }
    }
}
