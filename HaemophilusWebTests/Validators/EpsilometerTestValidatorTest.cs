using System;
using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class EpsilometerTestValidatorTest : AbstractValidatorTests<EpsilometerTestValidator, EpsilometerTestViewModel>
    {
        protected static IEnumerable<Tuple<EpsilometerTestViewModel, string[]>> InvalidModels;

        static EpsilometerTestValidatorTest()
        {
            InvalidModels = CreateInvalidModels();
        }

        protected override EpsilometerTestViewModel CreateValidModel()
        {
            return new EpsilometerTestViewModel
            {
                Measurement = 1.0f,
                Antibiotic = Antibiotic.Amikacin,
                EucastClinicalBreakpointId = 1
            };
        }

        [TestCase(null, 32f, 256f)]
        [TestCase(Antibiotic.Aspoxicillin, 32f, 256f)]
        [TestCase(Antibiotic.Ampicillin, 256f, 0.002f)]
        [TestCase(Antibiotic.AmoxicillinClavulanate, 256f, 0.002f)]
        [TestCase(Antibiotic.Azithromycin, 16f, 32f)]
        public void GetMhkScale_ValidAntibiotic_ReturnsSuitableList(Antibiotic? antibiotic, float contained, float notContained)
        {
            var mhkScale = EpsilometerTestValidator.GetMhkScale(antibiotic);

            mhkScale.Should().Contain(contained);
            mhkScale.Should().NotContain(notContained);
        }

        protected static IEnumerable<Tuple<EpsilometerTestViewModel, string[]>> CreateInvalidModels()
        {
            yield return Tuple.Create(new EpsilometerTestViewModel
            {
                Measurement = 1.0f,
                EucastClinicalBreakpointId = null,
            }, new[] { "EucastClinicalBreakpointId", "Antibiotic", "Measurement"});

            yield return Tuple.Create(new EpsilometerTestViewModel
            {
                Measurement = 0.0f,
                EucastClinicalBreakpointId = 1,
                Antibiotic = Antibiotic.Amikacin,
            }, new[] { "Measurement" });

            yield return Tuple.Create(new EpsilometerTestViewModel
            {
                Measurement = 0.003f, // Value not on E-Test scale for Ampicillin
                EucastClinicalBreakpointId = 1,
                Antibiotic = Antibiotic.Ampicillin,
            }, new[] { "Measurement" });

            yield return Tuple.Create(new EpsilometerTestViewModel
            {
                Measurement = 0.003f, // Value not on E-Test scale for AmoxicillinClavulanate
                EucastClinicalBreakpointId = 1,
                Antibiotic = Antibiotic.AmoxicillinClavulanate,
            }, new[] { "Measurement" });

            yield return Tuple.Create(new EpsilometerTestViewModel
            {
                Measurement = 64.0f, // Value not on E-Test scale for Meropenem
                EucastClinicalBreakpointId = 1,
                Antibiotic = Antibiotic.Meropenem,
            }, new[] { "Measurement" });

            yield return Tuple.Create(new EpsilometerTestViewModel
            {
                Measurement = 32.0f, // Value not on E-Test scale for Meropenem
                EucastClinicalBreakpointId = 1,
                Antibiotic = Antibiotic.Azithromycin,
            }, new[] { "Measurement" });
        }
    }
}