using System.Net;
using AutoMapper;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Automapper
{
    public class MeningoIsolateViewModelMappingAction : IsolateViewModelMappingActionBase, IMappingAction<MeningoIsolate, MeningoIsolateViewModel>, IMappingAction<MeningoIsolateViewModel, MeningoIsolate>
    {
        public MeningoIsolateViewModelMappingAction() : base(DatabaseType.Meningococci)
        {
        }

        public void Process(MeningoIsolate source, MeningoIsolateViewModel destination)
        {
            var sending = source.Sending;
            destination.SamplingLocation = sending.SamplingLocation == MeningoSamplingLocation.OtherInvasive
                ? sending.OtherInvasiveSamplingLocation
                : sending.SamplingLocation == MeningoSamplingLocation.OtherNonInvasive
                ? sending.OtherNonInvasiveSamplingLocation
                : EnumEditor.GetEnumDescription(sending.SamplingLocation);
            destination.Material = EnumEditor.GetEnumDescription(sending.Material);
            destination.Invasive = EnumEditor.GetEnumDescription(sending.Invasive);
            destination.PatientAgeAtSampling = source.PatientAge();
            destination.EpsilometerTestViewModels = EpsilometerTestsModelToViewModel(source.EpsilometerTests);

            //Report fields
            destination.SamplingDate = source.Sending.SamplingDate.ToReportFormat();
            destination.ReceivingDate = source.Sending.ReceivingDate.ToReportFormat();
            destination.Patient = source.Sending.Patient.ToReportFormat();
            destination.PatientBirthDate = source.Sending.Patient.BirthDate.ToReportFormat();
            destination.PatientPostalCode = source.Sending.Patient.PostalCode;
            destination.SenderLaboratoryNumber = source.Sending.SenderLaboratoryNumber;

            //destination.EvaluationString = source.Evaluation.ToReportFormat();

            //var interpretationResult = IsolateInterpretation.Interpret(source);
            //destination.Interpretation = interpretationResult.Interpretation;
            //destination.InterpretationPreliminary = interpretationResult.InterpretationPreliminary;
            //destination.InterpretationDisclaimer = interpretationResult.InterpretationDisclaimer;

            var sender = db.Senders.Find(source.Sending.SenderId);
            if (sender != null) // special case for Meningo as old senders were not imported
            {
                destination.SenderName = sender.Name;
                destination.SenderStreet = sender.StreetWithNumber;
                destination.SenderCity = $"{sender.PostalCode} {sender.City}";
            }

        }

        public void Process(MeningoIsolateViewModel source, MeningoIsolate destination)
        {
            ParseAndMapLaboratoryNumber(source, destination);

            destination.EpsilometerTests =
                EpsilometerTestsViewModelToModel(source.EpsilometerTestViewModels);
        }
    }
}