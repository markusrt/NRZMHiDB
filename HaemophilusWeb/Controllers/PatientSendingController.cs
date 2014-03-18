using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.Validators;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    public class PatientSendingController : Controller
    {
        private readonly IApplicationDbContext db;
        private readonly PatientController patientController;
        private readonly SendingController sendingController;

        public PatientSendingController()
            : this(
                new ApplicationDbContextWrapper(new ApplicationDbContext()), new PatientController(),
                new SendingController())
        {
        }

        public PatientSendingController(IApplicationDbContext applicationDbContext, PatientController patientController,
            SendingController sendingController)
        {
            db = applicationDbContext;
            this.patientController = patientController;
            this.sendingController = sendingController;
        }

        public ActionResult Create()
        {
            var patientResult = patientController.Create() as ViewResult;
            var sendingResult = sendingController.Create() as ViewResult;


            return CreateEditView(new PatientSendingViewModel()
            {
                Patient = (Patient) patientResult.Model,
                Sending = (Sending) sendingResult.Model
            });
        }

        private ViewResult CreateEditView(PatientSendingViewModel patientSending)
        {
            sendingController.AddReferenceDataToViewBag(ViewBag);
            return View(patientSending);
        }

        public ActionResult Edit(int? id)
        {
            var sendingResult = sendingController.Edit(id) as ViewResult;
            return CreateEditView(CreatePatientSending((Sending) sendingResult.Model));
        }

        [HttpPost]
        public ActionResult Edit(PatientSendingViewModel patientSending)
        {
            PerformValidations(patientSending);

            if (ModelState.IsValid)
            {
                db.Entry(patientSending.Patient).State = EntityState.Modified;
                db.Entry(patientSending.Sending).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return CreateEditView(patientSending);
        }

        [HttpPost]
        public ActionResult Create(PatientSendingViewModel patientSending)
        {
            PerformValidations(patientSending);
            ValidatePatientDoesNotAlreadyExist(patientSending.Patient);

            if (ModelState.IsValid)
            {
                db.Patients.Add(patientSending.Patient);
                db.SaveChanges();
                patientSending.Sending.PatientId = patientSending.Patient.PatientId;
                db.Sendings.Add(patientSending.Sending);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return CreateEditView(patientSending);
        }

        private void PerformValidations(PatientSendingViewModel patientSending)
        {
            ValidateModel(patientSending.Sending, new SendingValidator());
            ValidateModel(patientSending.Patient, new PatientValidator());
        }

        private void ValidatePatientDoesNotAlreadyExist(Patient patient)
        {
            var existingPatient = db.Patients.SingleOrDefault(
                p => p.Initials == patient.Initials &&
                     ((p.BirthDate.HasValue && patient.BirthDate.HasValue &&
                       p.BirthDate.Value == patient.BirthDate.Value)
                      || (!p.BirthDate.HasValue && !patient.BirthDate.HasValue))
                     && p.PostalCode == patient.PostalCode);
            if (existingPatient != null)
            {
                ModelState.AddModelError("",
                    "Ein Patient mit den selben Initialen, Geburtsdatum und Postleitzahl existiert bereits");
            }
        }

        private void ValidateModel<T>(T objectToValidate, IValidator<T> validator)
        {
            var result = validator.Validate(objectToValidate);
            if (result.IsValid)
            {
                return;
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }

        public ActionResult Index()
        {
            return View(db.Sendings.Select(CreatePatientSending).ToList());
        }

        private PatientSendingViewModel CreatePatientSending(Sending sending)
        {
            return new PatientSendingViewModel
            {
                Patient = db.Patients.FirstOrDefault(p => p.PatientId == sending.PatientId),
                Sending = sending
            };
        }
    }
}