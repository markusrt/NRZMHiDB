﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
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

        public IsolateController(IApplicationDbContext applicationDbContext) : base(applicationDbContext)
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
            return Mapper.Map<IsolateViewModel>(isolate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IsolateViewModel isolateViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isolate =
                        db.Isolates.Include(i => i.EpsilometerTests)
                            .Single(i => i.IsolateId == isolateViewModel.IsolateId);
                    Mapper.Map(isolateViewModel, isolate);
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
                catch (DbUpdateException e)
                {
                    if (e.AnyMessageMentions("IX_StemNumber"))
                    {
                        ModelState.AddModelError("StemNumber", "Diese Stammnummer ist bereits vergeben");
                    }
                    else if (e.AnyMessageMentions("IX_LaboratoryNumber"))
                    {
                        ModelState.AddModelError("LaboratoryNumber", "Diese Labornummer ist bereits vergeben");
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
            return CreateEditView(isolateViewModel);
        }
    }
}