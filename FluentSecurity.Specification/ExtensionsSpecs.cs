using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using FluentSecurity.Specification.Helpers;
using NUnit.Framework;

namespace FluentSecurity.Specification
{
	[TestFixture]
	[Category("ExtensionsSpecs")]
	public class When_getting_the_container
	{
		private ICollection<IPolicyContainer> _containers;

		[SetUp]
		public void SetUp()
		{
			// Arrange
			_containers = new Collection<IPolicyContainer>
            {
            	TestDataFactory.CreateValidPolicyContainer("Controller", "ActionThatDoesExist")
            };
		}

		[Test]
		public void Should_return_a_container_for_Controller_ActionThatDoesExist()
		{
			// Act
			var policyContainer = _containers.GetContainerFor("Controller", "ActionThatDoesExist");

			// Assert
			Assert.That(policyContainer, Is.Not.Null);
		}

		[Test]
		public void Should_return_null_for_Controller_ActionThatDoesNotExists()
		{
			// Act
			var policyContainer = _containers.GetContainerFor("Controller", "ActionThatDoesNotExists");

			// Assert
			Assert.That(policyContainer, Is.Null);
		}

		[Test]
		public void Should_return_a_container_for_controller_ActionThatDoesExist()
		{
			// Act
			var policyContainer = _containers.GetContainerFor("controller", "ActionThatDoesExist");

			// Assert
			Assert.That(policyContainer, Is.Not.Null);
		}

		[Test]
		public void Should_return_a_container_for_Controller_actionthatdoesexist()
		{
			// Act
			var policyContainer = _containers.GetContainerFor("Controller", "actionthatdoesexist");

			// Assert
			Assert.That(policyContainer, Is.Not.Null);
		}

		[Test]
		public void Should_return_a_container_for_controller_actionthatdoesexist()
		{
			// Act
			var policyContainer = _containers.GetContainerFor("controller", "actionthatdoesexist");

			// Assert
			Assert.That(policyContainer, Is.Not.Null);
		}
	}

	[TestFixture]
	[Category("ExtensionsSpecs")]
	public class When_getting_the_are_name_from_route_data
	{
		[Test]
		public void Should_return_the_are_name_from_data_tokens()
		{
			// Arrange
			var routeData = new RouteData();
			routeData.DataTokens.Add("area", "AreaName");

			// Act
			var areaName = routeData.GetAreaName();

			// Assert
			Assert.That(areaName, Is.EqualTo("AreaName"));
		}
	}

	[TestFixture]
	[Category("ExtensionsSpecs")]
	public class When_getting_the_area_name_from_route_base
	{
		[Test]
		public void Should_return_the_are_name_from_data_tokens()
		{
			// Arrange
			var route = new Route("some-url", new MvcRouteHandler());
			route.DataTokens = new RouteValueDictionary();
			route.DataTokens.Add("area", "AreaName");

			// Act
			var areaName = route.GetAreaName();

			// Assert
			Assert.That(areaName, Is.EqualTo("AreaName"));
		}

		[Test]
		public void Should_return_the_are_name_from_IRouteWithArea()
		{
			// Arrange
			var route = new AreaRoute();

			// Act
			var areaName = route.GetAreaName();

			// Assert
			Assert.That(areaName, Is.EqualTo("AreaName"));
		}

		[Test]
		public void Should_return_emtpy_string_when_DataTokens_is_null()
		{
			// Arrange
			var route = new Route("some-url", new MvcRouteHandler());

			// Act
			var areaName = route.GetAreaName();

			// Assert
			Assert.That(areaName, Is.Empty);
		}

		private class AreaRoute : Route, IRouteWithArea
		{
			public AreaRoute() : base("some-url", new MvcRouteHandler()) { }

			public string Area
			{
				get { return "AreaName"; }
			}
		}
	}

	[TestFixture]
	[Category("ExtensionsSpecs")]
	public class When_getting_the_action_name
	{
		[Test]
		public void Should_handle_UnaryExpression()
		{
			// Arrange
			Expression<Func<TestController, object>> expression = x => x.UnaryExpression();

			// Act
			var name = expression.GetActionName();

			// Assert
			Assert.That(name, Is.EqualTo("UnaryExpression"));
		}

		[Test]
		public void Should_handle_InstanceMethodCallExpression()
		{
			// Arrange
			Expression<Func<TestController, object>> expression = x => x.InstanceMethodCallExpression();

			// Act
			var name = expression.GetActionName();

			// Assert
			Assert.That(name, Is.EqualTo("InstanceMethodCallExpression"));
		}

		private class TestController
		{
			public Boolean UnaryExpression()
			{
				return false;
			}

			public ActionResult InstanceMethodCallExpression()
			{
				return new EmptyResult();
			}
		}
	}
}