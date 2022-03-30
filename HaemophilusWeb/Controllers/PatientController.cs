using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class PatientController : PatientControllerBase<Patient>
    {
        public PatientController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public PatientController(IApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        protected override IDbSet<Patient> DbSet()
        {
            return db.Patients;
        }
        
        public override void PopulateEnumFlagProperties(Patient patient, HttpRequestBase request)
        {
            patient.ClinicalInformation =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<ClinicalInformation>(
                    request.Form["ClinicalInformation"]);
        }

        public override void AddReferenceDataToViewBag(dynamic viewBag)
        {
            viewBag.PossibleOtherClinicalInformation = DbSet().Where(
                    s => !string.IsNullOrEmpty(s.OtherClinicalInformation)).
                Select(s => s.OtherClinicalInformation).AsDataList();
        }
    }
}