using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Migrations;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    [Authorize(Roles = DefaultRoles.User)]
    public class IsolateController : IsolateControllerBase<Isolate, IsolateViewModel>
    {
        private readonly IApplicationDbContext db;

        public IsolateController()
            : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public IsolateController(IApplicationDbContext applicationDbContext) : base(applicationDbContext, DatabaseType.Haemophilus)
        {
            db = applicationDbContext;
        }

        public override Isolate LoadIsolateById(int? id)
        {
            var isolate = db.Isolates.Include(i => i.Sending).SingleOrDefault(i => i.IsolateId == id);
            return isolate;
        }

        public override IsolateViewModel ModelToViewModel(Isolate isolate)
        {
            return MvcApplication.Mapper.Map<IsolateViewModel>(isolate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IsolateViewModel isolateViewModel)
        {
            CreateAndEditPreparations(isolateViewModel);
            if (ModelState.IsValid)
            {
                try
                {
                    var isolate =
                        db.Isolates.Include(i => i.EpsilometerTests)
                            .Single(i => i.IsolateId == isolateViewModel.IsolateId);
                    MvcApplication.Mapper.Map(isolateViewModel, isolate);
                    isolate.TypeOfGrowth =
                        EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<GrowthType>(Request.Form["TypeOfGrowth"]);

                    db.MarkAsModified(isolate);
                    db.SaveChanges();
                    if (Request == null || Request.Form["primary-submit"] != null)
                    {
                        return RedirectToAction("Index", "PatientSending");
                    }
                    if (Request.Form["secondary-submit"] != null)
                    {
                        return RedirectToAction("Isolate", "Report", new {id = isolateViewModel.IsolateId});
                    }
                }
                catch (DbUpdateException exception)
                {
                    HandleDbUpdateException(exception);
                }
            }
            return CreateEditView(isolateViewModel);
        }
    }
}