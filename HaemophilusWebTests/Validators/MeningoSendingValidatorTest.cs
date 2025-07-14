using System;
using System.Collections.Generic;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class MeningoSendingValidatorTest : AbstractValidatorTests<MeningoSendingValidator, MeningoSending>
    {
        protected static IEnumerable<Tuple<MeningoSending, string[]>> InvalidModels;

        static MeningoSendingValidatorTest()
        {
            InvalidModels = CreateInvalidModels();
        }

        protected override MeningoSending CreateValidModel()
        {
            return CreateSending();
        }

        private static MeningoSending CreateSending()
        {
            return new MeningoSending
            {
                SenderId = 1,
                MeningoPatientId = 1,
                SenderLaboratoryNumber = "1234",
                SamplingLocation = MeningoSamplingLocation.OtherInvasive,
                OtherInvasiveSamplingLocation = "Other",
            };
        }

        protected static IEnumerable<Tuple<MeningoSending, string[]>> CreateInvalidModels()
        {
            var invalidSending = new MeningoSending
            {
                SenderSpecies = string.Empty
            };
            yield return Tuple.Create(invalidSending, new[] {"SenderLaboratoryNumber", "SenderSpecies" });

            yield return Tuple.Create(CreateInvalidSendingWithReceivingDateBeforeReportDate(), new[] { "ReceivingDate"});

            yield return Tuple.Create(CreateInvalidSendingWithOtherSamplingLocationEmpty(MeningoSamplingLocation.OtherNonInvasive), new[] { "OtherNonInvasiveSamplingLocation" });
            
            yield return Tuple.Create(CreateInvalidSendingWithOtherSamplingLocationEmpty(MeningoSamplingLocation.OtherInvasive), new[] { "OtherInvasiveSamplingLocation" });
        }

        private static MeningoSending CreateInvalidSendingWithOtherSamplingLocationEmpty(MeningoSamplingLocation samplingLocation)
        {
            var sending = CreateSending();
            sending.SamplingLocation = samplingLocation; 
            sending.OtherInvasiveSamplingLocation = string.Empty;
            sending.OtherNonInvasiveSamplingLocation = string.Empty;
            return sending;
        }

        private static MeningoSending CreateInvalidSendingWithReceivingDateBeforeReportDate()
        {
            var sending = CreateSending();
            sending.ReceivingDate = new DateTime(2010, 1, 1);
            return sending;
        }
    }
}