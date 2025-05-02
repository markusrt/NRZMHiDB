using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Services;
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

        public MeningoIsolateController(IApplicationDbContext applicationDbContext) : base(applicationDbContext, DatabaseType.Meningococci)
        {
            db = applicationDbContext;
        }

        public override MeningoIsolate LoadIsolateById(int? id)
        {
            var isolate = db.MeningoIsolates
                .Include(i => i.Sending).Include(i => i.NeisseriaPubMlstIsolate).SingleOrDefault(i => i.MeningoIsolateId == id);
            return isolate;
        }

        public override MeningoIsolateViewModel ModelToViewModel(MeningoIsolate isolate)
        {
            var isolateViewModel = MvcApplication.Mapper.Map<MeningoIsolateViewModel>(isolate);
            var sending = isolate.Sending;
            isolateViewModel.Material = EnumEditor.GetEnumDescription(sending.Material);
            isolateViewModel.Invasive = EnumEditor.GetEnumDescription(sending.Invasive);
            if (isolateViewModel.NeisseriaPubMlstIsolate == null)
            {
                isolateViewModel.NeisseriaPubMlstIsolate = new NeisseriaPubMlstIsolate();
            }
            return isolateViewModel;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MeningoIsolateViewModel isolateViewModel)
        {
            CreateAndEditPreparations(isolateViewModel);
            if (ModelState.IsValid)
            {
                try
                {
                    var meningoIsolateId = isolateViewModel.MeningoIsolateId;
                    var isolate =
                        db.MeningoIsolates.Include(i => i.EpsilometerTests).Include(i => i.Sending)
                            .Single(i => i.MeningoIsolateId == meningoIsolateId);

                    //TODO replace AutoMapper by https://github.com/Dotnet-Boxed/Framework and pass db context to mapper
                    MvcApplication.Mapper.Map(isolateViewModel, isolate);
                    MapPubMlstData(isolateViewModel, isolate);

                    db.MarkAsModified(isolate);
                    db.SaveChanges();
                    if (Request == null || Request.Form["primary-submit"] != null)
                    {
                        return RedirectToAction("Index", "MeningoPatientSending");
                    }
                    if (Request.Form["secondary-submit"] != null)
                    {
                        return RedirectToAction("Isolate", "MeningoReport", new {id = meningoIsolateId});
                    }
                }
                catch (DbUpdateException exception)
                {
                    HandleDbUpdateException(exception);
                }
            }
            return CreateEditView(isolateViewModel);
        }

        public ActionResult PubMlstMatch()
        {
            var lastYear = DateTime.Now.Year;
            return View(new FromToQuery
            {
                From = new DateTime(lastYear, 1, 1),
                To = new DateTime(lastYear, 12, 31)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PubMlstMatch(FromToQuery fromTo)
        {
            var matcher = new PubMlstMatcher(db, new NeisseriaPubMlstService(), new IrisPubMlstService(ConfigurationManager.AppSettings.GetIrisAuthentication()));
            var result = matcher.Match(fromTo);
            return View("PubMlstMatchResult", result);
        }

        private void MapPubMlstData(MeningoIsolateViewModel isolateViewModel, MeningoIsolate isolate)
        {
            var pubMlstViewModel = isolateViewModel.NeisseriaPubMlstIsolate;
            if (pubMlstViewModel.PubMlstId != 0)
            {
                var neisseriaPubMlstIsolate =
                    db.NeisseriaPubMlstIsolates.SingleOrDefault(n =>
                        n.PubMlstId == pubMlstViewModel.PubMlstId);
                if (neisseriaPubMlstIsolate == null)
                {
                    neisseriaPubMlstIsolate = pubMlstViewModel;
                    db.NeisseriaPubMlstIsolates.Add(neisseriaPubMlstIsolate);
                }
                else
                {
                    neisseriaPubMlstIsolate.Database = pubMlstViewModel.Database;
                    neisseriaPubMlstIsolate.PorAVr1 = pubMlstViewModel.PorAVr1;
                    neisseriaPubMlstIsolate.PorAVr2 = pubMlstViewModel.PorAVr2;
                    neisseriaPubMlstIsolate.FetAVr = pubMlstViewModel.FetAVr;
                    neisseriaPubMlstIsolate.PorB = pubMlstViewModel.PorB;
                    neisseriaPubMlstIsolate.Fhbp = pubMlstViewModel.Fhbp;
                    neisseriaPubMlstIsolate.Nhba = pubMlstViewModel.Nhba;
                    neisseriaPubMlstIsolate.NadA = pubMlstViewModel.NadA;
                    neisseriaPubMlstIsolate.PenA = pubMlstViewModel.PenA;
                    neisseriaPubMlstIsolate.GyrA = pubMlstViewModel.GyrA;
                    neisseriaPubMlstIsolate.ParC = pubMlstViewModel.ParC;
                    neisseriaPubMlstIsolate.ParE = pubMlstViewModel.ParE;
                    neisseriaPubMlstIsolate.RpoB = pubMlstViewModel.RpoB;
                    neisseriaPubMlstIsolate.RplF = pubMlstViewModel.RplF;
                    neisseriaPubMlstIsolate.SequenceType = pubMlstViewModel.SequenceType;
                    neisseriaPubMlstIsolate.ClonalComplex = pubMlstViewModel.ClonalComplex;
                    neisseriaPubMlstIsolate.BexseroReactivity = pubMlstViewModel.BexseroReactivity;
                    neisseriaPubMlstIsolate.TrumenbaReactivity = pubMlstViewModel.TrumenbaReactivity;
                    isolate.NeisseriaPubMlstIsolate = neisseriaPubMlstIsolate;
                }
            }
        }
    }
}