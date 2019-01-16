using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class MeningoPatientController : PatientControllerBase<MeningoPatient>
    {
        public MeningoPatientController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public MeningoPatientController(IApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        protected override IDbSet<MeningoPatient> DbSet()
        {
            return db.MeningoPatients;
        }

        public override void PopulateEnumFlagProperties(MeningoPatient patient, HttpRequestBase request)
        {
            patient.ClinicalInformation =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<MeningoClinicalInformation>(
                    request.Form["ClinicalInformation"]);
            patient.RiskFactors =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<RiskFactors>(
                    request.Form["RiskFactors"]);
        }

        public override void AddReferenceDataToViewBag(dynamic viewBag)
        {
            viewBag.PossibleOtherClinicalInformation = DbSet().Where(
                    s => !string.IsNullOrEmpty(s.OtherClinicalInformation)).
                Select(s => s.OtherClinicalInformation).AsDataList();
            viewBag.PossibleOtherRiskFactors = DbSet().Where(
                    s => !string.IsNullOrEmpty(s.OtherRiskFactor)).
                Select(s => s.OtherRiskFactor).AsDataList();
        }
    }
}