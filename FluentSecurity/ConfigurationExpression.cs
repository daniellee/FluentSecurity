using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using FluentSecurity.Scanning;
using FluentSecurity.ServiceLocation;

namespace FluentSecurity
{
	public class ConfigurationExpression : Builder<IPolicyContainer>
	{
		internal Func<bool> IsAuthenticated { get; private set; }
		internal Func<IEnumerable<object>> Roles { get; private set; }
		internal ISecurityServiceLocator ExternalServiceLocator { get; private set; }
		internal bool ShouldIgnoreMissingConfiguration { get; private set; }
		private IPolicyAppender PolicyAppender { get; set; }

		public ConfigurationExpression()
		{
			PolicyAppender = new DefaultPolicyAppender();
		}

		public IPolicyContainer For<TController>(Expression<Func<TController, object>> propertyExpression) where TController : Controller
		{
			var controllerName = typeof(TController).GetControllerName();
			var actionName = propertyExpression.GetActionName();

			return AddPolicyContainerFor(controllerName, actionName);
		}

		public IConventionPolicyContainer For<TController>() where TController : Controller
		{
			var controllerType = typeof(TController);
			var controllerName = controllerType.GetControllerName();
			var actionMethods = controllerType.GetActionMethods();

			var policyContainers = new List<IPolicyContainer>();
			foreach (var actionMethod in actionMethods)
			{
				var actionName = actionMethod.Name;
				var policyContainer = AddPolicyContainerFor(controllerName, actionName);
				policyContainers.Add(policyContainer);
			}

			return new ConventionPolicyContainer(policyContainers);
		}

		public IConventionPolicyContainer ForAllControllers()
		{
			var assemblyScanner = new AssemblyScanner();
			assemblyScanner.TheCallingAssembly();
			assemblyScanner.With<ControllerTypeScanner>();
			var controllerTypes = assemblyScanner.Scan();

			var policyContainers = new List<IPolicyContainer>();
			foreach (var controllerType in controllerTypes)
			{
				var controllerName = controllerType.GetControllerName();
				var actionMethods = controllerType.GetActionMethods();

				policyContainers.AddRange(
					actionMethods.Select(actionMethod => AddPolicyContainerFor(controllerName, actionMethod.Name))
					);
			}

			return new ConventionPolicyContainer(policyContainers);
		}

		public IConventionPolicyContainer ForAllControllersInAssembly(Assembly assembly)
		{
			var assemblyScanner = new AssemblyScanner();
			assemblyScanner.Assembly(assembly);
			assemblyScanner.With<ControllerTypeScanner>();
			var controllerTypes = assemblyScanner.Scan();

			var policyContainers = new List<IPolicyContainer>();
			foreach (var controllerType in controllerTypes)
			{
				var controllerName = controllerType.GetControllerName();
				var actionMethods = controllerType.GetActionMethods();

				policyContainers.AddRange(
					actionMethods.Select(actionMethod => AddPolicyContainerFor(controllerName, actionMethod.Name))
					);
			}

			return new ConventionPolicyContainer(policyContainers);
		}

		public IConventionPolicyContainer ForAllControllersInAssemblyContainingType<TType>()
		{
			var assembly = typeof (TType).Assembly;
			return ForAllControllersInAssembly(assembly);
		}

		public IConventionPolicyContainer ForAllControllersInNamespaceContainingType<TType>()
		{
			var assembly = typeof (TType).Assembly;

			var assemblyScanner = new AssemblyScanner();
			assemblyScanner.Assembly(assembly);
			assemblyScanner.With<ControllerTypeScanner>();
			assemblyScanner.IncludeNamespaceContainingType<TType>();
			var controllerTypes = assemblyScanner.Scan();

			var policyContainers = new List<IPolicyContainer>();
			foreach (var controllerType in controllerTypes)
			{
				var controllerName = controllerType.GetControllerName();
				var actionMethods = controllerType.GetActionMethods();

				policyContainers.AddRange(
					actionMethods.Select(actionMethod => AddPolicyContainerFor(controllerName, actionMethod.Name))
					);
			}

			return new ConventionPolicyContainer(policyContainers);
		}

		private IPolicyContainer AddPolicyContainerFor(string controllerName, string actionName)
		{
			IPolicyContainer policyContainer;

			var existingContainer = _itemValues.GetContainerFor(controllerName, actionName);
			if (existingContainer != null)
			{
				policyContainer = existingContainer;
			}
			else
			{
				policyContainer = new PolicyContainer(controllerName, actionName, PolicyAppender);
				_itemValues.Add(policyContainer);
			}

			return policyContainer;
		}

		public void RemovePoliciesFor<TController>(Expression<Func<TController, object>> actionExpression) where TController : Controller
		{
			var controllerName = typeof(TController).GetControllerName();
			var actionName = actionExpression.GetActionName();

			var policyContainer = _itemValues.GetContainerFor(controllerName, actionName);
			if (policyContainer != null)
			{
				_itemValues.Remove(policyContainer);
			}
		}

		public void GetAuthenticationStatusFrom(Func<bool> isAuthenticatedFunction)
		{
			if (isAuthenticatedFunction == null)
				throw new ArgumentNullException("isAuthenticatedFunction");

			IsAuthenticated = isAuthenticatedFunction;
		}

		public void GetRolesFrom(Func<IEnumerable<object>> rolesFunction)
		{
			if (rolesFunction == null)
				throw new ArgumentNullException("rolesFunction");

			if (_itemValues.Count > 0)
				throw new ConfigurationErrorsException("You must set the rolesfunction before adding policies.");

			Roles = rolesFunction;
		}

		public void IgnoreMissingConfiguration()
		{
			ShouldIgnoreMissingConfiguration = true;
		}

		public void SetPolicyAppender(IPolicyAppender policyAppender)
		{
			if (policyAppender == null)
				throw new ArgumentNullException("policyAppender");
			
			PolicyAppender = policyAppender;
		}

		public void ResolveServicesUsing(Func<Type, IEnumerable<object>> servicesLocator, Func<Type, object> singleServiceLocator = null)
		{
			if (servicesLocator == null)
				throw new ArgumentNullException("servicesLocator");

			ExternalServiceLocator = new ExternalServiceLocator(servicesLocator, singleServiceLocator);
		}

		public void ResolveServicesUsing(ISecurityServiceLocator securityServiceLocator)
		{
			if (securityServiceLocator == null)
				throw new ArgumentNullException("securityServiceLocator");

			ExternalServiceLocator = securityServiceLocator;
		}
	}
}