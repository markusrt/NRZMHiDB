﻿using System;
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
using NLog;

namespace HaemophilusWeb
{
    public class MvcApplication : HttpApplication
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            Log.Info("Application start");

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
            FluentValidationModelValidatorProvider.Configure();
            OverwriteDefaultErrorMessages();
        }

        private static void OverwriteDefaultErrorMessages()
        {
            ClientDataTypeModelValidatorProvider.ResourceClassKey = "MyResources";
            DefaultModelBinder.ResourceClassKey = "MyResources";
        }

        private void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            Log.ErrorException(string.Format("Unhandled error: {0}", exception.Message), exception);

            if (IsAjaxRequest())
            {
                Response.Clear();

                var serializer = new JavaScriptSerializer();   
                var error = new Error {ErrorMessage = exception.Message};
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