using System.Configuration;
using System.Net;
using AutoMapper;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Automapper
{
    public class IsolateViewModelMappingAction : IsolateViewModelMappingActionBase, IMappingAction<Isolate, IsolateViewModel>, IMappingAction<IsolateViewModel, Isolate>
    {
        //TODO eventually refactor and move this to base class
        private readonly IsolateInterpretation isolateInterpretation = new IsolateInterpretation();

        public IsolateViewModelMappingAction() : base(DatabaseType.Haemophilus, EnumUtils.ParseCommaSeperatedListOfNames<Antibiotic>(ConfigurationManager.AppSettings["PrimaryAntibiotics_Haemophilus"]))
        {
        }

        public void Process(Isolate source, IsolateViewModel destination)
        {
            var sending = source.Sending;
            destination.SamplingLocation = sending.SamplingLocation == SamplingLocation.Other
                ? sending.OtherSamplingLocation
                : EnumEditor.GetEnumDescription(sending.SamplingLocation);
            destination.Material = EnumEditor.GetEnumDescription(sending.Material);
            destination.Invasive = EnumEditor.GetEnumDescription(sending.Invasive);
            destination.PatientAgeAtSampling = source.PatientAgeAtSampling();
            destination.EpsilometerTestViewModels = EpsilometerTestsModelToViewModel(source.EpsilometerTests);
            destination.SamplingDate = source.Sending.SamplingDate.ToReportFormat();
            destination.ReceivingDate = source.Sending.ReceivingDate.ToReportFormat();
            destination.PatientId = source.Sending.Patient.PatientId;
            destination.Patient = source.Sending.Patient.ToReportFormat();
            destination.PatientBirthDate = source.Sending.Patient.BirthDate.ToReportFormat();
            destination.PatientPostalCode = source.Sending.Patient.PostalCode;
            destination.SenderLaboratoryNumber = source.Sending.SenderLaboratoryNumber;
            destination.EvaluationString = source.Evaluation.ToReportFormat();
            var interpretationResult = isolateInterpretation.Interpret(source);
            destination.Interpretation = interpretationResult.Interpretation;
            destination.InterpretationPreliminary = interpretationResult.InterpretationPreliminary;
            destination.InterpretationDisclaimer = interpretationResult.InterpretationDisclaimer;
            destination.Comment = interpretationResult.Comment;
            destination.Announcement = ConfigurationManager.AppSettings["Announcement"];

            var sender = db.Senders.Find(source.Sending.SenderId);
            destination.SenderName = sender.Name;
            destination.SenderDepartment = sender.Department ?? string.Empty;
            destination.SenderStreet = sender.StreetWithNumber;
            destination.SenderCity = $"{sender.PostalCode} {sender.City}";
        }

        public void Process(IsolateViewModel source, Isolate destination)
        {
            ParseAndMapLaboratoryNumber(source, destination);

            destination.EpsilometerTests =
                EpsilometerTestsViewModelToModel(source.EpsilometerTestViewModels);
        }

    }
}