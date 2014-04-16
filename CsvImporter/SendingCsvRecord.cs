using System;
using HaemophilusWeb.Models;

namespace CsvImporter
{
    public class SendingCsvRecord
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

        public YesNoUnknown HibVaccination { get; set; }

        public DateTime? HibVaccinationDate { get; set; }

        public DateTime SamplingDate { get; set; }

        public DateTime ReceivingDate { get; set; }

        public Material Material { get; set; }

        public string OtherMaterial { get; set; }

        public YesNo? Invasive { get; set; }

        public string SenderLaboratoryNumber { get; set; }

        public string SenderConclusion { get; set; }

        public Evaluation Evaluation { get; set; }

        public string Remark { get; set; }

        public DateTime? ReportDate { get; set; }

        public int IsolateId { get; set; }

        public int YearlySequentialIsolateNumber { get; set; }

        public int Year { get; set; }

        public FactorTest? FactorTest { get; set; }

        public Serotype? Agglutination { get; set; }

        public TestResult? BetaLactamase { get; set; }

        public TestResult? Oxidase { get; set; }

        public TestResult? OuterMembraneProteinP2 { get; set; }

        public TestResult FuculoKinase { get; set; }

        public TestResult OuterMembraneProteinP6 { get; set; }

        public TestResult? BexA { get; set; }

        public Serotype? SerotypePcr { get; set; }

        public TestResult RibosomalRna16S { get; set; }

        public TestResult? ApiNh { get; set; }

        public double? AmpicillinMeasurement { get; set; }

        public EpsilometerTestResult? AmpicillinEpsilometerTestResult { get; set; }

        public double? AmoxicillinClavulanateMeasurement { get; set; }

        public EpsilometerTestResult? AmoxicillinClavulanateEpsilometerTestResult { get; set; }

        public double? CefotaximeMeasurement { get; set; }

        public EpsilometerTestResult? CefotaximeEpsilometerTestResult { get; set; }

        public double? MeropenemMeasurement { get; set; }

        public EpsilometerTestResult? MeropenemEpsilometerTestResult { get; set; }

        public string EuCastVersion { get; set; }
    }
}