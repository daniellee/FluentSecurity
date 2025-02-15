using System;
using System.Collections.Generic;
using System.Linq;
using FluentSecurity.Policy;
using FluentSecurity.Specification.Helpers;
using FluentSecurity.Specification.TestData;
using NUnit.Framework;
using Rhino.Mocks;

namespace FluentSecurity.Specification
{
	[TestFixture]
	[Category("SecurityConfiguratorSpec")]
	public class When_setting_the_configuration_for_fluent_security
	{
		[Test]
		public void Should_have_configuration()
		{
			// Arrange
			SecurityConfigurator.Reset();
			var configuration = MockRepository.GenerateMock<ISecurityConfiguration>();

			// Act
			SecurityConfigurator.SetConfiguration(configuration);

			// Assert
			Assert.That(SecurityConfiguration.Current, Is.EqualTo(configuration));
		}

		[Test]
		public void Should_throw_ArgumentNullException_when_configuration_is_null()
		{
			// Arrange
			SecurityConfigurator.Reset();
			const ISecurityConfiguration nullConfiguration = null;

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => SecurityConfigurator.SetConfiguration(nullConfiguration));
		}
	}

	[TestFixture]
	[Category("SecurityConfiguratorSpec")]
	public class When_calling_reset_on_security_configurator
	{
		[Test]
		public void Should_create_new_configuration_instance()
		{
			// Arrange
			var configuration = MockRepository.GenerateMock<ISecurityConfiguration>();
			SecurityConfigurator.SetConfiguration(configuration);

			// Act
			SecurityConfigurator.Reset();

			// Assert
			var exception = Assert.Throws<InvalidOperationException>(() => {
				var x = SecurityConfiguration.Current;
			});
			Assert.That(exception.Message, Is.EqualTo("Security has not been configured!"));
		}
	}

	[TestFixture]
	[Category("SecurityConfiguratorSpec")]
	public class When_calling_configure_on_security_configurator
	{
		private Action<ConfigurationExpression> _configurationExpression;

		[SetUp]
		public void SetUp()
		{
			// Arrange
			SecurityConfigurator.Reset();
			_configurationExpression = delegate { TestDataFactory.CreateValidConfigurationExpression(); };
		}

		[Test]
		public void Should_return_current_configuration()
		{
			// Act
			var configuration = SecurityConfigurator.Configure(_configurationExpression);

			// Assert
			Assert.That(configuration, Is.Not.Null);
			Assert.That(configuration, Is.EqualTo(SecurityConfiguration.Current));
		}
	}

	[TestFixture]
	[Category("SecurityConfigurationSpec")]
	public class When_calling_set_configuration_on_security_configurator_passing_null_as_the_argument
	{
		[Test]
		public void Should_throw()
		{
			// Arrange
			Action<ConfigurationExpression> configurationExpression = null;

			// Act & assert
			Assert.Throws<ArgumentNullException>(() => SecurityConfigurator.Configure(configurationExpression));
		}
	}

	[TestFixture]
	[Category("SecurityConfiguratorSpec")]
	public class When_I_configure_fluent_security_for_Blog_Index_and_Blog_AddPost
	{
		private IEnumerable<IPolicyContainer> _policyContainers;
		private DefaultPolicyAppender _defaultPolicyAppender;
		private IPolicyAppender _fakePolicyAppender;
		private readonly string _controllerName = NameHelper<BlogController>.Controller();
		const string IndexActionName = "Index";
		const string AddPostActionName = "AddPost";

		[SetUp]
		public void SetUp()
		{
			// Arrange
			_defaultPolicyAppender = TestDataFactory.CreateValidPolicyAppender();
			_fakePolicyAppender = TestDataFactory.CreateFakePolicyAppender();

			SecurityConfigurator.Reset();

			// Act
			SecurityConfigurator.Configure(configuration =>
			{
				configuration.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsFalse);
				configuration.GetRolesFrom(StaticHelper.GetRolesExcludingOwner);

				configuration.SetPolicyAppender(_defaultPolicyAppender);
				configuration.For<BlogController>(x => x.Index()).DenyAnonymousAccess();

				configuration.SetPolicyAppender(_fakePolicyAppender);
				configuration.For<BlogController>(x => x.AddPost()).RequireRole(UserRole.Writer, UserRole.Publisher, UserRole.Owner);
			});

			_policyContainers = SecurityConfiguration.Current.PolicyContainers;
		}

		[Test]
		public void Should_have_two_policycontainers()
		{
			Assert.That(_policyContainers.Count(), Is.EqualTo(2));
		}

		[Test]
		public void Should_have_policycontainer_for_Blog_Index()
		{
			var container = _policyContainers.GetContainerFor(_controllerName, IndexActionName);
			Assert.That(container.ControllerName, Is.EqualTo(_controllerName));
			Assert.That(container.ActionName, Is.EqualTo(IndexActionName));
			Assert.That(container.GetPolicies().Count(), Is.EqualTo(1));
			Assert.That(container.GetPolicies().First().GetType(), Is.EqualTo(typeof(DenyAnonymousAccessPolicy)));
			Assert.That(container.PolicyAppender, Is.EqualTo(_defaultPolicyAppender));
		}

		[Test]
		public void Should_have_policycontainer_for_Blog_AddPost()
		{
			var container = _policyContainers.GetContainerFor(_controllerName, AddPostActionName);
			Assert.That(container.ControllerName, Is.EqualTo(_controllerName));
			Assert.That(container.ActionName, Is.EqualTo(AddPostActionName));
			Assert.That(container.GetPolicies().Count(), Is.EqualTo(1));
			Assert.That(container.GetPolicies().First().GetType(), Is.EqualTo(typeof(RequireRolePolicy)));
			Assert.That(container.PolicyAppender, Is.EqualTo(_fakePolicyAppender));
		}
	}

	[TestFixture]
	[Category("SecurityConfiguratorSpec")]
	public class When_adding_two_containers_with_the_same_controller_and_action_name
	{
		[Test]
		public void Should_have_1_policycontainer()
		{
			SecurityConfigurator.Reset();

			// Act
			SecurityConfigurator.Configure(configuration =>
			{
				configuration.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsFalse);
				configuration.GetRolesFrom(StaticHelper.GetRolesExcludingOwner);
				configuration.For<BlogController>(x => x.Index());
				configuration.For<BlogController>(x => x.Index());
			});

			Assert.That(SecurityConfiguration.Current.PolicyContainers.Count(), Is.EqualTo(1));
			Assert.That(SecurityConfiguration.Current.PolicyContainers.First().ControllerName, Is.EqualTo(NameHelper<BlogController>.Controller()));
			Assert.That(SecurityConfiguration.Current.PolicyContainers.First().ActionName, Is.EqualTo("Index"));
		}
	}

	[TestFixture]
	[Category("SecurityConfiguratorSpec")]
	public class When_I_remove_policies_for_Blog_Index
	{
		private IEnumerable<IPolicyContainer> _policyContainers;

		[SetUp]
		public void SetUp()
		{
			// Arrange
			SecurityConfigurator.Reset();

			// Act
			SecurityConfigurator.Configure(configuration =>
			{
				configuration.GetAuthenticationStatusFrom(StaticHelper.IsAuthenticatedReturnsFalse);
				configuration.For<BlogController>(x => x.Index());
				configuration.For<BlogController>(x => x.AddPost());
				configuration.RemovePoliciesFor<BlogController>(x => x.Index());
			});

			_policyContainers = SecurityConfiguration.Current.PolicyContainers;
		}

		[Test]
		public void Should_have_1_policycontainer()
		{
			// Assert
			Assert.That(_policyContainers.Count(), Is.EqualTo(1));
		}

		[Test]
		public void Should_not_have_policycontainer_for_Blog_Index()
		{
			// Assert
			var container = _policyContainers.GetContainerFor(NameHelper<BlogController>.Controller(), "Index");
			Assert.That(container, Is.Null);
		}
	}
}