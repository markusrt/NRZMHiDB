using System;
using System.Collections.Generic;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class SendingValidatorTests : AbstractValidatorTests<SendingValidator, Sending>
    {
        protected override Sending CreateValidModel()
        {
            return new Sending
            {
                SenderId = 1,
                PatientId = 1,
                SenderLaboratoryNumber = "1234",
                Material = Material.Other,
                OtherMaterial = "Other",
                Invasive = YesNo.No
            };
        }

        protected override IEnumerable<Tuple<Sending, string[]>> CreateInvalidModels()
        {
            var invalidSending = new Sending
            {
                SenderConclusion = string.Empty
            };
            yield return Tuple.Create(invalidSending, new[] { "Invasive", "SenderLaboratoryNumber", "SenderConclusion" });

            yield return Tuple.Create(CreateInvalidSendingWithReceivingDateBeforeReportDate(), new[] { "ReceivingDate"});

            yield return Tuple.Create(CreateInvalidSendingWithOtherMaterialEmpty(), new[] { "OtherMaterial" });
        }

        private Sending CreateInvalidSendingWithOtherMaterialEmpty()
        {
            var sending = CreateValidModel();
            sending.Material = Material.Other;
            sending.OtherMaterial = string.Empty;
            return sending;
        }

        private Sending CreateInvalidSendingWithReceivingDateBeforeReportDate()
        {
            var sending = CreateValidModel();
            sending.ReceivingDate = new DateTime(2010, 1, 1);
            return sending;
        }
    }
}