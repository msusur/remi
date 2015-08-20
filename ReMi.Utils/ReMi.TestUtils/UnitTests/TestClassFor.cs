using System;
using NUnit.Framework;

namespace ReMi.TestUtils.UnitTests
{
    [TestFixture]
	public abstract class TestClassFor<TSut> where TSut : class
    {
		[SetUp]
		public void TestSetup()
		{
			TestInitialize();
		}

		[TearDown]
		public void TestTearDown()
		{
			TestCleanup();
		}

		//NOTE: fixture init and cleanup have to be static

		protected TSut Sut { get; private set; }

		protected void CreateSut()
		{
			Sut = ConstructSystemUnderTest();
		}

		protected TResult Act<TResult>(Func<TSut, TResult> act)
		{
			Sut = ConstructSystemUnderTest();

			return act.Invoke(Sut);
		}

		protected void Act(Action<TSut> act)
		{
			Sut = ConstructSystemUnderTest();

			act.Invoke(Sut);
		}

		protected abstract TSut ConstructSystemUnderTest();

		protected virtual void TestInitialize()
		{
			CreateSut();
		}

		protected virtual void TestCleanup()
		{
			return;
		}

    }
}
