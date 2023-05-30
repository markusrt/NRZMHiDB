using System;
using System.Collections.Generic;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class SendingValidatorTests : AbstractValidatorTests<SendingValidator, Sending>
    {
        protected static IEnumerable<Tuple<Sending, string[]>> InvalidModels;

        static SendingValidatorTests()
        {
            InvalidModels = CreateInvalidModels();
        }

        protected override Sending CreateValidModel()
        {
            return CreateSending();
        }

        private static Sending CreateSending()
        {
            return new Sending
            {
                SenderId = 1,
                PatientId = 1,
                SenderLaboratoryNumber = "1234",
                SamplingLocation = SamplingLocation.Other,
                OtherSamplingLocation = "Other",
            };
        }

        protected static IEnumerable<Tuple<Sending, string[]>> CreateInvalidModels()
        {
            var invalidSending = new Sending
            {
                SenderConclusion = string.Empty
            };
            yield return Tuple.Create(invalidSending, new[] {"SenderLaboratoryNumber", "SenderConclusion" });

            yield return Tuple.Create(CreateInvalidSendingWithReceivingDateBeforeReportDate(), new[] { "ReceivingDate"});

            yield return Tuple.Create(CreateInvalidSendingWithOtherSamplingLocationEmpty(), new[] { "OtherSamplingLocation" });
        }

        private static Sending CreateInvalidSendingWithOtherSamplingLocationEmpty()
        {
            var sending = CreateSending();
            sending.SamplingLocation = SamplingLocation.Other;
            sending.OtherSamplingLocation = string.Empty;
            return sending;
        }

        private static Sending CreateInvalidSendingWithReceivingDateBeforeReportDate()
        {
            var sending = CreateSending();
            sending.ReceivingDate = new DateTime(2010, 1, 1);
            return sending;
        }
    }
}