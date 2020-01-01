using System;
using System.Web.WebPages;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Domain
{
    public class IsolateExtensionsTests
    {
        [TestCase("2010-04-22", "2010-04-22", null, 0, Description = "Sampled newborn has age of 0")]
        [TestCase("2010-04-22", "2013-04-22", null, 3, Description = "Sampled at third birthday")]
        [TestCase("2010-04-22", "2013-04-21", null, 2, Description = "Sampled one day before third birthday")]
        [TestCase("2020-02-29", "2022-02-28", null, 1, Description = "Born in leap year and sampled one day before second birthday")]
        [TestCase("2007-12-13", null, "2019-01-12", 11, Description = "Sample date is empty but receiving date not")]
        [TestCase("2007-12-13", null, "2019-12-12", 11, Description = "Sample date is empty and receiving date is before birthday")]
        [TestCase("2007-12-13", null, "2019-12-13", 12, Description = "Sample date is empty and receiving date is at birthday")]
        public void PatientAgeAtSampling_NewBorn_ReturnsZero(string birthDate, string samplingDate, string receivingDate, int expectedAge )
        {
            var isolate = new Isolate
            {
                Sending = new Sending
                {
                    Patient = new Patient
                    {
                        BirthDate = DateTime.Parse(birthDate)
                    },
                    SamplingDate = null,
                }
            };
            if (!samplingDate.IsEmpty()) isolate.Sending.SamplingDate = DateTime.Parse(samplingDate);
            if (!receivingDate.IsEmpty()) isolate.Sending.ReceivingDate = DateTime.Parse(receivingDate);


            isolate.PatientAgeAtSampling().Should().Be(expectedAge);
        }
    }
}