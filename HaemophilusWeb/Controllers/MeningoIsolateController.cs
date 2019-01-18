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
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    [Authorize(Roles = DefaultRoles.User)]
    public class MeningoIsolateController : IsolateControllerBase<MeningoIsolate, MeningoIsolateViewModel>
    {
        private readonly IApplicationDbContext db;

        public MeningoIsolateController()
            : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public MeningoIsolateController(IApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            db = applicationDbContext;
        }

        public override MeningoIsolate LoadIsolateById(int? id)
        {
            var isolate = db.MeningoIsolates.Include(i => i.Sending).SingleOrDefault(i => i.MeningoIsolateId == id);
            return isolate;
        }

        public override MeningoIsolateViewModel ModelToViewModel(MeningoIsolate isolate)
        {
            var isolateViewModel = Mapper.Map<MeningoIsolateViewModel>(isolate);
            var sending = isolate.Sending;
            //isolateViewModel.SamplingLocation = sending.SamplingLocation == SamplingLocation.Other
            //    ? WebUtility.HtmlEncode(sending.OtherSamplingLocation)
            //    : EnumEditor.GetEnumDescription(sending.SamplingLocation);
            isolateViewModel.Material = EnumEditor.GetEnumDescription(sending.Material);
            isolateViewModel.Invasive = EnumEditor.GetEnumDescription(sending.Invasive);
            //isolateViewModel.PatientAgeAtSampling = isolate.PatientAge();
           // isolateViewModel.EpsilometerTestViewModels = EpsilometerTestsModelToViewModel(isolate.EpsilometerTests);
            //isolateViewModel.SamplingDate = isolate.Sending.SamplingDate.ToReportFormat();
            //isolateViewModel.ReceivingDate = isolate.Sending.ReceivingDate.ToReportFormat();
            //isolateViewModel.Patient = isolate.Sending.Patient.ToReportFormat();
            //isolateViewModel.PatientBirthDate = isolate.Sending.Patient.BirthDate.ToReportFormat();
            //isolateViewModel.PatientPostalCode = isolate.Sending.Patient.PostalCode;
            //isolateViewModel.SenderLaboratoryNumber = isolate.Sending.SenderLaboratoryNumber;
            //isolateViewModel.EvaluationString = isolate.Evaluation.ToReportFormat();
            //var interpretationResult = IsolateInterpretation.Interpret(isolate);
            //isolateViewModel.Interpretation = interpretationResult.Interpretation;
            //isolateViewModel.InterpretationPreliminary = interpretationResult.InterpretationPreliminary;
            //isolateViewModel.InterpretationDisclaimer = interpretationResult.InterpretationDisclaimer;

            //var sender = db.Senders.Find(isolate.Sending.SenderId);
            //isolateViewModel.SenderName = sender.Name;
            //isolateViewModel.SenderStreet = sender.StreetWithNumber;
            //isolateViewModel.SenderCity = $"{sender.PostalCode} {sender.City}";

            return isolateViewModel;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MeningoIsolateViewModel isolateViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isolate =
                        db.MeningoIsolates.Include(i => i.EpsilometerTests)
                            .Single(i => i.MeningoIsolateId == isolateViewModel.MeningoIsolateId);
                    Mapper.Map(isolateViewModel, isolate);
                    db.MarkAsModified(isolate);
                    db.SaveChanges();
                    if (Request == null || Request.Form["primary-submit"] != null)
                    {
                        return RedirectToAction("Index", "MeningoPatientSending");
                    }
                    if (Request.Form["secondary-submit"] != null)
                    {
                        //TODO Fix Meningo Isolate Report
                        return RedirectToAction("Isolate", "Report", new {id = isolateViewModel.MeningoIsolateId});
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