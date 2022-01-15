using System;
using HaemophilusWeb.Models;

namespace CsvImporter
{
    public class SendingCsvRecord : IsolateBase
    {
        public int SendingId { get; set; }

        public int SenderId { get; set; }

        public int PatientId { get; set; }

        public string Initials { get; set; }

        public DateTime? BirthDate { get; set; }

        public string PostalCode { get; set; }

        public Gender? Gender { get; set; }

        public string City { get; set; }

        public string County { get; set; }

        public State State { get; set; }

        public ClinicalInformation ClinicalInformation { get; set; }

        public string OtherClinicalInformation { get; set; }

        public VaccinationStatus HibVaccination { get; set; }

        public DateTime? HibVaccinationDate { get; set; }

        public DateTime? SamplingDate { get; set; }

        public DateTime ReceivingDate { get; set; }

        public SamplingLocation SamplingLocation { get; set; }

        public string OtherSamplingLocation { get; set; }

        public Material Material { get; set; }

        public YesNo? Invasive { get; set; }

        public string SenderLaboratoryNumber { get; set; }

        public string SenderConclusion { get; set; }

        public string LaboratoryNumber { get; set; }

        public int YearlySequentialIsolateNumber { get; set; }

        public int Year { get; set; }

        public float? AmpicillinMeasurement { get; set; }

        public EpsilometerTestResult? AmpicillinEpsilometerTestResult { get; set; }

        public float? AmoxicillinClavulanateMeasurement { get; set; }

        public EpsilometerTestResult? AmoxicillinClavulanateEpsilometerTestResult { get; set; }

        public float? CefotaximeMeasurement { get; set; }

        public EpsilometerTestResult? CefotaximeEpsilometerTestResult { get; set; }

        public float? MeropenemMeasurement { get; set; }

        public EpsilometerTestResult? MeropenemEpsilometerTestResult { get; set; }

        public string EuCastVersion { get; set; }
    }
}