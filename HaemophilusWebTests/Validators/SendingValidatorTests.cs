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
                SamplingLocation = SamplingLocation.Other,
                OtherSamplingLocation = "Other",
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

            yield return Tuple.Create(CreateInvalidSendingWithOtherSamplingLocationEmpty(), new[] { "OtherSamplingLocation" });
        }

        private Sending CreateInvalidSendingWithOtherSamplingLocationEmpty()
        {
            var sending = CreateValidModel();
            sending.SamplingLocation = SamplingLocation.Other;
            sending.OtherSamplingLocation = string.Empty;
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