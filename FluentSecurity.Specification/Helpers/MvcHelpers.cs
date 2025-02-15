using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using Rhino.Mocks;

namespace FluentSecurity.Specification.Helpers
{
	public static class MvcHelpers
	{
		public static ActionExecutingContext GetFilterContextFor<TController>(Expression<Func<TController, object>> propertyExpression) where TController : Controller
		{
			var controllerName = typeof(TController).GetControllerName();
			var actionName = propertyExpression.GetActionName();

			var filterContext = MockRepository.GenerateMock<ActionExecutingContext>();

			var controllerDescriptor = MockRepository.GenerateMock<ControllerDescriptor>();
			controllerDescriptor.Expect(x => x.ControllerName).Return(controllerName).Repeat.Any();
			controllerDescriptor.Expect(x => x.ControllerType).Return(typeof(TController)).Repeat.Any();
			controllerDescriptor.Replay();

			var actionDescriptor = MockRepository.GenerateMock<ActionDescriptor>();
			actionDescriptor.Expect(x => x.ActionName).Return(actionName).Repeat.Any();
			actionDescriptor.Expect(x => x.ControllerDescriptor).Return(controllerDescriptor).Repeat.Any();
			actionDescriptor.Replay();

			filterContext.Expect(x => x.ActionDescriptor).Return(actionDescriptor).Repeat.Any();
			filterContext.Replay();

			return filterContext;
		}

		private static string GetControllerName(this Type controllerType)
		{
			return controllerType.Name.Replace("Controller", string.Empty);
		}

		private static string GetActionName(this LambdaExpression actionExpression)
		{
			return ((MethodCallExpression)actionExpression.Body).Method.Name;
		}
	}
}