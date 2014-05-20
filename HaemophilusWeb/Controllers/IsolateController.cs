using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class IsolateController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var isolate = db.Isolates.Include(i => i.Sending).SingleOrDefault(i => i.IsolateId == id);
            if (isolate == null)
            {
                return HttpNotFound();
            }

            var isolateViewModel = ModelToViewModel(isolate);
            return CreateEditView(isolateViewModel);
        }

        public ActionResult Report(int? id)
        {
            return Edit(id);
        }

        private static IsolateViewModel ModelToViewModel(Isolate isolate)
        {
            var isolateViewModel = Mapper.Map<IsolateViewModel>(isolate);
            var sending = isolate.Sending;
            isolateViewModel.Material = sending.Material == Material.Other
                ? WebUtility.HtmlEncode(sending.OtherMaterial)
                : EnumEditor.GetEnumDescription(sending.Material);
            isolateViewModel.Invasive = EnumEditor.GetEnumDescription(sending.Invasive);
            if (isolate.Sending.Patient.BirthDate.HasValue)
            {
                var birthday = isolate.Sending.Patient.BirthDate.Value;
                var samplingDate = isolate.Sending.SamplingDate;
                var ageAtSampling = samplingDate.Year - birthday.Year;
                if (birthday > samplingDate.AddYears(-ageAtSampling))
                {
                    ageAtSampling--;
                }
                isolateViewModel.PatientAgeAtSampling = ageAtSampling;
            }
            ModelToViewModel(isolate.EpsilometerTests, isolateViewModel.EpsilometerTestViewModels);
            return isolateViewModel;
        }

        private static void ModelToViewModel(ICollection<EpsilometerTest> epsilometerTests,
            IEnumerable<EpsilometerTestViewModel> epsilometerTestViewModels)
        {
            foreach (var epsilometerTestViewModel in epsilometerTestViewModels)
            {
                var epsilometerTest =
                    epsilometerTests.SingleOrDefault(
                        e => e.EucastClinicalBreakpoint.Antibiotic == epsilometerTestViewModel.Antibiotic);
                if (epsilometerTest != null)
                {
                    epsilometerTestViewModel.EucastClinicalBreakpointId = epsilometerTest.EucastClinicalBreakpointId;
                    epsilometerTestViewModel.Measurement = epsilometerTest.Measurement;
                    epsilometerTestViewModel.Result = epsilometerTest.Result;
                }
            }
        }

        private void ViewModelToModel(IEnumerable<EpsilometerTestViewModel> eTestViewModels,
            ICollection<EpsilometerTest> eTests)
        {
            var allInitialETests = eTests.ToList();
            foreach (var eTestViewModel in eTestViewModels)
            {
                var eTest = allInitialETests.SingleOrDefault(
                    e => e.EucastClinicalBreakpoint.Antibiotic == eTestViewModel.Antibiotic);

                if (HasValidETestValue(eTestViewModel))
                {
                    AddNewOrUpdateExistingETest(eTests, eTest, eTestViewModel);
                }
                else
                {
                    if (eTest != null)
                    {
                        allInitialETests.Remove(eTest);
                        eTests.Remove(eTest);
                        db.EpsilometerTests.Remove(eTest);
                    }
                }
            }
        }

        private static bool HasValidETestValue(EpsilometerTestViewModel eTestViewModel)
        {
            return eTestViewModel.EucastClinicalBreakpointId.HasValue && eTestViewModel.Result.HasValue;
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
            eTest.Measurement = eTestViewModel.Measurement;
            eTest.Result = eTestViewModel.Result.Value;
        }

        private ActionResult CreateEditView(IsolateViewModel isolateViewModel)
        {
            ViewBag.ClinicalBreakpoints = db.EucastClinicalBreakpoints.OrderByDescending(b => b.ValidFrom);
            return View(isolateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IsolateViewModel isolateViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isolate = db.Isolates.Include(i => i.EpsilometerTests).Single(i => i.IsolateId == isolateViewModel.IsolateId);
                    Mapper.Map(isolateViewModel, isolate);

                    ViewModelToModel(isolateViewModel.EpsilometerTestViewModels, isolate.EpsilometerTests);
                    db.Entry(isolate).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", "PatientSending");
                }
                catch (DbUpdateException e)
                {
                    if (e.AnyMessageMentions("IX_StemNumber"))
                    {
                        ModelState.AddModelError("StemNumber", "Diese Stammnummer ist bereits vergeben");
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
            return CreateEditView(isolateViewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}