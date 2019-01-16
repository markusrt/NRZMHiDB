﻿using System.Net;
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
        public void Process(Isolate source, IsolateViewModel destination)
        {
            var sending = source.Sending;
            destination.SamplingLocation = sending.SamplingLocation == SamplingLocation.Other
                ? WebUtility.HtmlEncode(sending.OtherSamplingLocation)
                : EnumEditor.GetEnumDescription(sending.SamplingLocation);
            destination.Material = EnumEditor.GetEnumDescription(sending.Material);
            destination.Invasive = EnumEditor.GetEnumDescription(sending.Invasive);
            destination.PatientAgeAtSampling = source.PatientAge();
            destination.EpsilometerTestViewModels = EpsilometerTestsModelToViewModel(source.EpsilometerTests);
            destination.SamplingDate = source.Sending.SamplingDate.ToReportFormat();
            destination.ReceivingDate = source.Sending.ReceivingDate.ToReportFormat();
            destination.Patient = source.Sending.Patient.ToReportFormat();
            destination.PatientBirthDate = source.Sending.Patient.BirthDate.ToReportFormat();
            destination.PatientPostalCode = source.Sending.Patient.PostalCode;
            destination.SenderLaboratoryNumber = source.Sending.SenderLaboratoryNumber;
            destination.EvaluationString = source.Evaluation.ToReportFormat();
            var interpretationResult = IsolateInterpretation.Interpret(source);
            destination.Interpretation = interpretationResult.Interpretation;
            destination.InterpretationPreliminary = interpretationResult.InterpretationPreliminary;
            destination.InterpretationDisclaimer = interpretationResult.InterpretationDisclaimer;

            var sender = db.Senders.Find(source.Sending.SenderId);
            destination.SenderName = sender.Name;
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