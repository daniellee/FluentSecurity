using FluentSecurity.ServiceLocation;
using NUnit.Framework;

namespace FluentSecurity.Specification // Do not change the namespace
{
	[SetUpFixture]
	public class ResetFixture
	{
		[SetUp]
		public void Reset()
		{
			ServiceLocator.Reset();
			ExceptionFactory.Reset();
		}
	}
}