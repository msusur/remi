using Moq;
using NUnit.Framework;
using ReMi.Plugin.Gerrit.GerritApi;
using ReMi.TestUtils.UnitTests;

namespace ReMi.Plugin.Gerrit.Tests.UnitTests.GerritApi
{
    [TestFixture]
    public class GerritRequestTests : TestClassFor<GerritRequest>
    {
        private Mock<ISshClient> _sshClientMock;

        protected override GerritRequest ConstructSystemUnderTest()
        {
            return new GerritRequest
            {
                SshClientFactory = () => _sshClientMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _sshClientMock = new Mock<ISshClient>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void GetGitLog_ShoudGetDataThroughSSHCommand_WhenCalled()
        {
            //_sshClientMock.Setup(x => x.Connect());
            //_sshClientMock.Setup(x => x.ExecuteCommand("gitcommand log --project fop/inpus v_2.965..repo --pretty format:\"%H<sep>%p<sep>%an<sep>%ai<sep>%d<sep>%s<sep>%b<end>\""))
            //    .Returns(File.ReadAllText(@"TestData\git-logs.txt"));
            //_sshClientMock.Setup(x => x.Dispose());

            //var result = Sut.GetGitLog(new[]
            //{
            //    new ReleaseRepository { Repository = "fop/inpus", ChangesFrom = "v_2.965", ChangesTo = "repo" }
            //});

            //Assert.AreEqual(78, result.Values.First().Count());
            //Assert.AreEqual("09f94d1cc437f3c6f760e0f40e27c591da579b03", result.Values.First().First().Hash);
            //Assert.AreEqual(CommitType.Typical, result.Values.First().First().CommitType);
            //Assert.AreEqual(DateTime.Parse("2015-04-29 18:01:47 +0100", CultureInfo.InvariantCulture), result.Values.First().First().Date);
            //Assert.AreEqual(" (tag: v_2.965, repo, collections)", result.Values.First().First().Reference);
            //Assert.AreEqual("Iad9b83b3f3661c3e5a0edfe7af34ae3e932f0a6f", result.Values.First().First().ChangeId);
            //Assert.AreEqual("I7d3df3db58daf2ed7344ad47db68bce0d95eae5b", result.Values.First().ElementAt(18).ChangeId);
        }
    }
}
