using System.Web.Mvc;
using System.Web.Routing;
using FluentFilters;

namespace FluentSecurity.SampleApplication
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			SetupApplication();
		}

		protected void Application_Error()
		{
			// TODO: Add logging here
		}

		public static void SetupApplication()
		{
			SetupContainer();
			SetControllerFactory();
			Bootstrapper.SetupFluentSecurity();
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);
			RegisterFluentFilters();
		}

		public static void SetupContainer()
		{
			// TODO: Setup the IOC-container of your choice
		}

		public static void SetControllerFactory()
		{
			// TODO: Set the controllerfactory of your choice
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			new RouteRegistrar(routes).RegisterRoutes();
		}

		public static void RegisterFluentFilters()
		{
            FluentFiltersBuilder.Current.Add<HandleSecurityAttribute>();
		}
	}
}