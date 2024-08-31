using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using AutoMapper;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;
using QRCoder;

namespace HaemophilusWeb.Automapper
{
    public class MeningoIsolateViewModelMappingAction : IsolateViewModelMappingActionBase, IMappingAction<MeningoIsolate, MeningoIsolateViewModel>, IMappingAction<MeningoIsolateViewModel, MeningoIsolate>
    {
        public MeningoIsolateViewModelMappingAction() : base(DatabaseType.Meningococci, EnumUtils.ParseCommaSeperatedListOfNames<Antibiotic>(ConfigurationManager.AppSettings["PrimaryAntibiotics_Meningococci"]))
        {
        }

        public void Process(MeningoIsolate source, MeningoIsolateViewModel destination)
        {
            var sending = source.Sending;
            ParseAndMapLaboratoryNumber(source, destination);
            destination.SamplingLocation = sending.SamplingLocation == MeningoSamplingLocation.OtherInvasive
                ? sending.OtherInvasiveSamplingLocation
                : sending.SamplingLocation == MeningoSamplingLocation.OtherNonInvasive
                ? sending.OtherNonInvasiveSamplingLocation
                : EnumEditor.GetEnumDescription(sending.SamplingLocation);
            destination.Material = EnumEditor.GetEnumDescription(sending.Material);
            destination.Invasive = EnumEditor.GetEnumDescription(sending.Invasive);
            destination.PatientAgeAtSampling = source.PatientAgeAtSampling();
            destination.EpsilometerTestViewModels = EpsilometerTestsModelToViewModel(source.EpsilometerTests);

            //Report fields
            destination.SamplingDate = source.Sending.SamplingDate.ToReportFormat();
            destination.ReceivingDate = source.Sending.ReceivingDate.ToReportFormat();
            destination.PatientId = source.Sending.Patient.PatientId;
            destination.Patient = source.Sending.Patient.ToReportFormat();
            destination.PatientBirthDate = source.Sending.Patient.BirthDate.ToReportFormat();
            destination.PatientPostalCode = source.Sending.Patient.PostalCode;
            destination.SenderLaboratoryNumber = source.Sending.SenderLaboratoryNumber;

            //destination.EvaluationString = source.Evaluation.ToReportFormat();

            var isolateInterpretation = new MeningoIsolateInterpretation();
            isolateInterpretation.Interpret(source);
            destination.Report = isolateInterpretation.Result.Report;
            destination.Typings = isolateInterpretation.Typings;
            destination.Comment = isolateInterpretation.Result.Comment;
            destination.Announcement = ConfigurationManager.AppSettings["Announcement"];

            if (!string.IsNullOrEmpty(source.Sending.DemisId))
            {
                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(source.Sending.DemisId, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var qrCodeAsBitmap = qrCode.GetGraphic(10);
                var base64String = Convert.ToBase64String(ImageToByteArray(qrCodeAsBitmap));
                destination.DemisIdQrImageUrl = "data:image/png;base64," + base64String;
            }

            var sender = db.Senders.Find(source.Sending.SenderId);
            if (sender != null) // special case for Meningo as old senders were not imported
            {
                destination.SenderName = sender.Name;
                destination.SenderDepartment = sender.Department ?? string.Empty;
                destination.SenderStreet = sender.StreetWithNumber;
                destination.SenderCity = $"{sender.PostalCode} {sender.City}";
            }
        }

        public void Process(MeningoIsolateViewModel source, MeningoIsolate destination)
        {
            ParseAndMapLaboratoryNumber(source, destination);
            SetSendingNoGrowthAccordingToGrowthOnAgar(source, destination);

            destination.EpsilometerTests =
                EpsilometerTestsViewModelToModel(source.EpsilometerTestViewModels);
        }

        private byte[] ImageToByteArray(Image image)
        {
            using var memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Png);
            return memoryStream.ToArray();
        }

        private void SetSendingNoGrowthAccordingToGrowthOnAgar(MeningoIsolateViewModel source, MeningoIsolate destination)
        {
            if (source.GrowthOnBloodAgar == Growth.No 
                && source.GrowthOnMartinLewisAgar == Growth.No
                && !destination.Sending.Material.IsNativeMaterial())
            {
                destination.Sending.Material = MeningoMaterial.NoGrowth;
            }
        }
    }
}