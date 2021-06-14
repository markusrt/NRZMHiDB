using System;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    public class QueryRecord
    {
        public string Initials { get; set; }
        public DateTime? BirthDate { get; set; }
        public string StemNumber { get; set; }
        public DateTime ReceivingDate { get; set; }
        public string SamplingLocation { get; set; }
        public string Invasive { get; set; }
        public int LaboratoryNumber { get; set; }
        public string LaboratoryNumberString { get; set; }
        public string FullTextSearch { get; set; }
        public int IsolateId { get; set; }
        public int SendingId { get; set; }
        public int PatientId { get; set; }
        public bool ReportGenerated { get; set; }
        public string PatientPostalCode { get; set; }
        public string SenderPostalCode { get; set; }
        public string SenderLaboratoryNumber { get; set; }
        public ReportStatus ReportStatus { get; internal set; }
    }
}