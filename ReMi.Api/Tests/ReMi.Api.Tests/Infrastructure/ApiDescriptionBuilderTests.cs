using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NUnit.Framework;
using ReMi.Api.Insfrastructure;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.TestUtils.UnitTests;

namespace ReMi.Api.Tests.Infrastructure
{
    public class ApiDescriptionBuilderTests : TestClassFor<ApiDescriptionBuilder>
    {
        protected override ApiDescriptionBuilder ConstructSystemUnderTest()
        {
            return new ApiDescriptionBuilder(new[] { typeof(TestController) }, new[] { typeof(Command1) });
        }

        [Test]
        public void GetApiDescriptions_ShouldReturnApiDescription_WhenInvoked()
        {
            var result = Sut.GetApiDescriptions().ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("GetCommands", result.ElementAt(0).Name);
            Assert.AreEqual("GET", result.ElementAt(0).Method);
            Assert.AreEqual("/qr1/{names}/", result.ElementAt(0).Url);
            Assert.AreEqual("{\"names\" : \"String\"}", result.ElementAt(0).InputFormat);
            Assert.AreEqual("{\"Items\" : [ {\"Name\" : \"String\", \"Value\" : \"Int32\"} ]}", result.ElementAt(0).OutputFormat);

            Assert.AreEqual("Command1", result.ElementAt(1).Name);
            Assert.AreEqual("POST", result.ElementAt(1).Method);
            Assert.AreEqual("/deliver/Command1/", result.ElementAt(1).Url);
            Assert.AreEqual(null, result.ElementAt(1).OutputFormat);
            Assert.AreNotEqual("{}", result.ElementAt(1).InputFormat);

            Assert.IsTrue(result.ElementAt(1).InputFormat.Contains("\"Str\" : \"String\""), "Str");
            Assert.IsTrue(result.ElementAt(1).InputFormat.Contains("\"Bol\" : \"Boolean\""), "Bol");
            Assert.IsTrue(result.ElementAt(1).InputFormat.Contains("\"Arr\" : \"Int32[]\""), "Arr");
            Assert.IsTrue(result.ElementAt(1).InputFormat.Contains("\"Enum\" : \"TestEnum\""), "Enum");
            Assert.IsTrue(result.ElementAt(1).InputFormat.Contains("\"Enumer\" : [ {\"Name\" : \"String\", \"Value\" : \"Int32\"} ]"), "Enumer");
            Assert.IsTrue(result.ElementAt(1).InputFormat.Contains("\"Dict\" : [ {\"Key\" : \"String\", \"Value\" : \"Boolean\"} ]"), "Dict");
            Assert.IsTrue(result.ElementAt(1).InputFormat.Contains("\"Obj\" : {\"Name\" : \"String\", \"Value\" : \"Int32\"}"), "Obj");
        }

        [Test]
        public void FormatType_ShouldReturnEmptyString_WhenTypeNull()
        {
            var result = Sut.FormatType(null, 0, null);

            Assert.IsEmpty(result);
        }

        [Test]
        public void FormatType_ShouldReturnCommandStructureWithoutContext_WhenInvoked()
        {
            var result = Sut.FormatType(typeof(Command1), 0, null);

            Assert.AreEqual("{" +
                            "\"Str\" : \"String\"," +
                            " \"Enum\" : \"TestEnum\"," +
                            " \"Bol\" : \"Boolean\"," +
                            " \"Arr\" : \"Int32[]\"," +
                            " \"Obj\" : {" +
                                "\"Name\" : \"String\"," +
                                " \"Value\" : \"Int32\"}," +
                                " \"Enumer\" : [ {" +
                                    "\"Name\" : \"String\"," +
                                    " \"Value\" : \"Int32\"" +
                                "} ]," +
                                " \"Dict\" : [ {" +
                                "\"Key\" : \"String\"," +
                                " \"Value\" : \"Boolean\"" +
                            "} ]}", result);
        }
    }

    public class TestController : ApiController
    {
        [HttpPost]
        [Route("deliver/{commandName}")]
        public HttpResponseMessage ExecCommand(string commandName, bool isSynchronous = false)
        {
            return null;
        }

        [HttpGet]
        [Route("qr1/{names}")]
        public GetCommandsResponse ExecQuery1(string names)
        {
            return null;
        }
    }

    [Command("Command1", CommandGroup.ReleasePlan)]
    public class Command1 : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public string Str { get; set; }
        public TestEnum Enum { get; set; }
        public bool Bol { get; set; }
        public int[] Arr { get; set; }
        public TestItem Obj { get; set; }
        public IEnumerable<TestItem> Enumer { get; set; }
        public IDictionary<string, bool> Dict { get; set; }
    }

    [CommandAttribute("Invisible command", CommandGroup.ReleasePlan, IsBackground = true)]
    public class Command2 : ICommand
    {
        public CommandContext CommandContext { get; set; }
    }

    public enum TestEnum
    {
        A, B
    }

    public class TestItem
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    [Query("Get commands", QueryGroup.ReleasePlan)]
    public class GetCommandsRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string Names { get; set; }
    }
    public class GetCommandsResponse
    {
        public IEnumerable<TestItem> Items { get; set; }
    }
}
