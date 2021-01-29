using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Tools;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class MeningoSendingController : SendingControllerBase<MeningoSending, MeningoPatient>
    {
        public MeningoSendingController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public MeningoSendingController(IApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        protected override int GetNextSequentialStemNumber()
        {
            var lastSequentialStemNumber =
                db.MeningoIsolates.DefaultIfEmpty()
                    .Max(i => i == null || !i.StemNumber.HasValue ? 0 : i.StemNumber.Value);
            return lastSequentialStemNumber + 1;
        }

        protected override int GetNextSequentialIsolateNumber()
        {
            var lastSequentialIsolateNumber =
                db.MeningoIsolates.Where(i => i.Year == CurrentYear)
                    .DefaultIfEmpty()
                    .Max(i => i == null ? 0 : i.YearlySequentialIsolateNumber);
            return lastSequentialIsolateNumber + 1;
        }

        protected override void AddAdditionalReferenceDataToViewBag(dynamic viewBag, MeningoSending sending)
        {
            viewBag.PossibleOtherInvasiveSamplingLocations = SendingDbSet().Where(
                s => !string.IsNullOrEmpty(s.OtherInvasiveSamplingLocation)).Select(s => s.OtherInvasiveSamplingLocation).AsDataList();
            viewBag.PossibleOtherNonInvasiveSamplingLocations = SendingDbSet().Where(
                s => !string.IsNullOrEmpty(s.OtherNonInvasiveSamplingLocation)).Select(s => s.OtherNonInvasiveSamplingLocation).AsDataList();
        }

        protected override IDbSet<MeningoSending> SendingDbSet()
        {
            return db.MeningoSendings;
        }

        protected override IDbSet<MeningoPatient> PatientDbSet()
        {
            return db.MeningoPatients;
        }
    }
}
