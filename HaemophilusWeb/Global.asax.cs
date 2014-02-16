using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using FluentValidation.Mvc;
using HaemophilusWeb.Migrations;
using HaemophilusWeb.Models;

namespace HaemophilusWeb
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
            FluentValidationModelValidatorProvider.Configure();
        }

        private void Application_Error(object sender, EventArgs e)
        {
            if (IsAjaxRequest())
            {
                Response.Clear();

                var serializer = new JavaScriptSerializer();
                var lastError = Server.GetLastError();
                var error = new Error {ErrorMessage = lastError.Message};
                var responseJson = serializer.Serialize(error);
                Response.Write(responseJson);
                Server.ClearError();
            }
        }

        private bool IsAjaxRequest()
        {
            var isAjaxRequest = (Request["X-Requested-With"] == "XMLHttpRequest")
                                || ((Request.Headers["X-Requested-With"] == "XMLHttpRequest"));

            if (isAjaxRequest)
            {
                return true;
            }

            try
            {
                var controllerActions = GetCurrentControllerActions();
                var actionName = GetCurrentAction();

                return controllerActions.Cast<ReflectedActionDescriptor>().Any(
                    ContainsAjaxActionWithName(actionName));
            }
            catch
            {
            }

            return false;
        }

        private static Func<ReflectedActionDescriptor, bool> ContainsAjaxActionWithName(string actionName)
        {
            return
                actionDescriptor =>
                    actionDescriptor.ActionName.Equals(actionName, StringComparison.InvariantCultureIgnoreCase)
                    && actionDescriptor.MethodInfo.ReturnType == typeof (JsonResult);
        }

        private string GetCurrentAction()
        {
            return Request.RequestContext.
                RouteData.Values["action"].ToString();
        }

        private IEnumerable<ActionDescriptor> GetCurrentControllerActions()
        {
            var controllerName = Request.RequestContext.
                RouteData.Values["controller"].ToString();
            var controllerFactory = new DefaultControllerFactory();
            var controller = controllerFactory.CreateController(
                Request.RequestContext, controllerName) as Controller;
            var controllerDescriptor = new ReflectedControllerDescriptor(controller.GetType());
            var controllerActions = controllerDescriptor.GetCanonicalActions();
            return controllerActions;
        }
    }
}