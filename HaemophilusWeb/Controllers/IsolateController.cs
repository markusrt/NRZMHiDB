using System;
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
    public class IsolateController : Controller
    {
        private readonly IApplicationDbContext db;
        private readonly AntibioticPriorityListComparer antibioticPriorityListComparer;
        private readonly List<Antibiotic> primaryAntibiotics;

        public IsolateController()
            : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public IsolateController(IApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
            antibioticPriorityListComparer = new AntibioticPriorityListComparer(ConfigurationManager.AppSettings["AntibioticsOrder"]);
            primaryAntibiotics = EnumUtils.ParseCommaSeperatedListOfNames<Antibiotic>(ConfigurationManager.AppSettings["PrimaryAntibiotics"]);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var isolate = LoadIsolateById(id);
            if (isolate == null)
            {
                return HttpNotFound();
            }

            var isolateViewModel = ModelToViewModel(isolate);
            return CreateEditView(isolateViewModel);
        }

        public Isolate LoadIsolateById(int? id)
        {
            var isolate = db.Isolates.Include(i => i.Sending).SingleOrDefault(i => i.IsolateId == id);
            return isolate;
        }



        public IsolateViewModel ModelToViewModel(Isolate isolate)
        {
            var isolateViewModel = Mapper.Map<IsolateViewModel>(isolate);
            var sending = isolate.Sending;
            isolateViewModel.SamplingLocation = sending.SamplingLocation == SamplingLocation.Other
                ? WebUtility.HtmlEncode(sending.OtherSamplingLocation)
                : EnumEditor.GetEnumDescription(sending.SamplingLocation);
            isolateViewModel.Material = EnumEditor.GetEnumDescription(sending.Material);
            isolateViewModel.Invasive = EnumEditor.GetEnumDescription(sending.Invasive);
            isolateViewModel.PatientAgeAtSampling = isolate.PatientAge();


            EpsilometerTestsModelToViewModel(isolate, isolateViewModel);

            isolateViewModel.SamplingDate = isolate.Sending.SamplingDate.ToReportFormat();
            isolateViewModel.ReceivingDate = isolate.Sending.ReceivingDate.ToReportFormat();
            isolateViewModel.Patient = isolate.Sending.Patient.ToReportFormat();
            isolateViewModel.PatientBirthDate = isolate.Sending.Patient.BirthDate.ToReportFormat();
            isolateViewModel.PatientPostalCode = isolate.Sending.Patient.PostalCode;
            isolateViewModel.SenderLaboratoryNumber = isolate.Sending.SenderLaboratoryNumber;
            isolateViewModel.EvaluationString = isolate.Evaluation.ToReportFormat();
            var interpretationResult = IsolateInterpretation.Interpret(isolate);
            isolateViewModel.Interpretation = interpretationResult.Interpretation;
            isolateViewModel.InterpretationPreliminary = interpretationResult.InterpretationPreliminary;
            isolateViewModel.InterpretationDisclaimer = interpretationResult.InterpretationDisclaimer;

            var sender = db.Senders.Find(isolate.Sending.SenderId);
            isolateViewModel.SenderName = sender.Name;
            isolateViewModel.SenderStreet = sender.StreetWithNumber;
            isolateViewModel.SenderCity = string.Format("{0} {1}", sender.PostalCode, sender.City);

            return isolateViewModel;
        }

        private void EpsilometerTestsModelToViewModel(Isolate isolate, IsolateViewModel isolateViewModel)
        {
            var epsilometerTestViewModels = new List<EpsilometerTestViewModel>();
            var availableAntibiotics = AvailableAntibiotics;
            var missingPrimaryAntibiotics = new List<Antibiotic>(primaryAntibiotics.Where(a => availableAntibiotics.Contains(a)));

            isolateViewModel.EpsilometerTestViewModels.Clear();

            foreach (var epsilometerTest in isolate.EpsilometerTests.ToList().OrderBy(e => e.EucastClinicalBreakpoint.Antibiotic, antibioticPriorityListComparer))
            {
                var epsilometerTestViewModel = new EpsilometerTestViewModel
                {
                    EucastClinicalBreakpointId = epsilometerTest.EucastClinicalBreakpointId,
                    Measurement = epsilometerTest.Measurement,
                    Result = epsilometerTest.Result,
                    ReadonlyAntibiotic = true
                };

                var eucastClinicalBreakpoint = epsilometerTest.EucastClinicalBreakpoint;
                if (eucastClinicalBreakpoint != null)
                {
                    epsilometerTestViewModel.MicBreakpointResistent =
                        eucastClinicalBreakpoint.MicBreakpointResistent;
                    epsilometerTestViewModel.MicBreakpointSusceptible =
                        eucastClinicalBreakpoint.MicBreakpointSusceptible;
                    if (eucastClinicalBreakpoint.ValidFrom.HasValue)
                    {
                        epsilometerTestViewModel.ValidFromYear = eucastClinicalBreakpoint.ValidFrom.Value.Year;
                    }

                    epsilometerTestViewModel.Antibiotic = eucastClinicalBreakpoint.Antibiotic;
                    missingPrimaryAntibiotics.Remove(eucastClinicalBreakpoint.Antibiotic);
                }

                epsilometerTestViewModels.Add(epsilometerTestViewModel);
            }

            epsilometerTestViewModels.AddRange(
                missingPrimaryAntibiotics.Select(
                    missingPrimaryAntibiotic => new EpsilometerTestViewModel
                    {
                        Antibiotic = missingPrimaryAntibiotic,
                        ReadonlyAntibiotic = true
                    }
            ));

            isolateViewModel.EpsilometerTestViewModels.AddRange(epsilometerTestViewModels.OrderBy(e => e.Antibiotic.Value, antibioticPriorityListComparer));

            isolateViewModel.EpsilometerTestViewModels.Insert(0, new EpsilometerTestViewModel());
        }

        private void EpsilometerTestsViewModelToModel(IsolateViewModel isolateViewModel, Isolate isolate)
        {
            isolate.EpsilometerTests.Clear();

            foreach (var epsilometerTest in isolate.EpsilometerTests.ToList())
            {
                db.EpsilometerTests.Remove(epsilometerTest);
            }
            foreach (var eTestViewModel in isolateViewModel.EpsilometerTestViewModels)
            {
                if (HasValidETestValue(eTestViewModel))
                {
                    AddNewOrUpdateExistingETest(isolate.EpsilometerTests, null, eTestViewModel);
                }
            }

        }

        private static bool HasValidETestValue(EpsilometerTestViewModel eTestViewModel)
        {
            return eTestViewModel.Antibiotic.HasValue && eTestViewModel.Measurement.HasValue;
        }

        private static void AddNewOrUpdateExistingETest(ICollection<EpsilometerTest> eTests, EpsilometerTest eTest,
            EpsilometerTestViewModel eTestViewModel)
        {
            if (eTest == null)
            {
                eTest = new EpsilometerTest();
                eTests.Add(eTest);
            }

            eTest.EucastClinicalBreakpointId = eTestViewModel.EucastClinicalBreakpointId.Value;
            eTest.Measurement = eTestViewModel.Measurement.Value;
            eTest.Result = eTestViewModel.Result.Value;
        }

        private ActionResult CreateEditView(IsolateViewModel isolateViewModel)
        {
            ViewBag.ClinicalBreakpoints = db.EucastClinicalBreakpoints.OrderByDescending(b => b.ValidFrom);
            ViewBag.Antibiotics = AvailableAntibiotics.OrderBy(a => a, antibioticPriorityListComparer);
            return View(isolateViewModel);
        }

        private List<Antibiotic> AvailableAntibiotics
        {
            get
            {
                return db.EucastClinicalBreakpoints.Select(e => e.Antibiotic)
                    .Distinct()
                    .ToList();
            }
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
                    ParseAndMapLaboratoryNumber(isolateViewModel, isolate);
                    isolate.TypeOfGrowth =
                        EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<GrowthType>(Request.Form["TypeOfGrowth"]);
                    EpsilometerTestsViewModelToModel(isolateViewModel, isolate);
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

        internal static void ParseAndMapLaboratoryNumber(IsolateViewModel isolateViewModel, Isolate isolate)
        {
            int decade;
            int yearlySequentialIsolateNumber;
            var decadeAndNumber = isolateViewModel.LaboratoryNumber.Split('/');
            int.TryParse(decadeAndNumber[0], out yearlySequentialIsolateNumber);
            int.TryParse(decadeAndNumber[1], out decade);
            isolate.Year = (DateTime.Now.Year/100)*100 + decade;
            isolate.YearlySequentialIsolateNumber = yearlySequentialIsolateNumber;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult CarCreate()
        {
            return new JsonResult();
            //throw new NotImplementedException();
        }
    }
}