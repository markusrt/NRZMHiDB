using System.Data.Entity;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using AutoMapper;
using FluentValidation.Mvc;
using HaemophilusWeb.Migrations;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;
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
            InitializeAutomapper();
            DisableTls1AndEnableTls12();
        }

        private static void DisableTls1AndEnableTls12()
        {
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public static void InitializeAutomapper()
        {
            Mapper.CreateMap<IsolateViewModel, Isolate>();
            Mapper.CreateMap<Isolate, IsolateViewModel>();
        }

        private static void OverwriteDefaultErrorMessages()
        {
            ClientDataTypeModelValidatorProvider.ResourceClassKey = "MyResources";
            DefaultModelBinder.ResourceClassKey = "MyResources";
        }
    }
}