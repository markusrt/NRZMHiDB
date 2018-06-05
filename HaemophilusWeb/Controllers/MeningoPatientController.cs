using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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

        protected override void PopulateEnumFlagProperties(MeningoPatient patient)
        {
            patient.ClinicalInformation =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<MeningoClinicalInformation>(
                    Request.Form["ClinicalInformation"]);
            patient.Epidemiology =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<Epidemiology>(
                    Request.Form["Epidemiology"]);
            patient.RiskFactors =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<RiskFactors>(
                    Request.Form["RiskFactors"]);
        }

        public override void AddReferenceDataToViewBag(dynamic viewBag)
        {
            viewBag.PossibleOtherClinicalInformation = DbSet().Where(
                    s => !string.IsNullOrEmpty(s.OtherClinicalInformation)).
                Select(s => s.OtherClinicalInformation).AsDataList();
            viewBag.PossibleTherapyDetails = DbSet().Where(
                    s => !string.IsNullOrEmpty(s.TherapyDetails)).
                Select(s => s.TherapyDetails).AsDataList();
            viewBag.PossibleOtherUnderlyingDisease = DbSet().Where(
                    s => !string.IsNullOrEmpty(s.OtherUnderlyingDisease)).
                Select(s => s.OtherUnderlyingDisease).AsDataList();
            viewBag.PossibleOtherRiskFactors = DbSet().Where(
                    s => !string.IsNullOrEmpty(s.OtherRiskFactor)).
                Select(s => s.OtherRiskFactor).AsDataList();
        }
    }
}