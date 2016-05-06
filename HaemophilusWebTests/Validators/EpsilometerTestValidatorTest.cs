using System;
using System.Collections.Generic;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class EpsilometerTestValidatorTest : AbstractValidatorTests<EpsilometerTestValidator, EpsilometerTestViewModel>
    {
        protected override EpsilometerTestViewModel CreateValidModel()
        {
            return new EpsilometerTestViewModel
            {
                Measurement = 1.0f,
                Antibiotic = Antibiotic.Amikacin,
                EucastClinicalBreakpointId = 1
            };
        }

        protected override IEnumerable<Tuple<EpsilometerTestViewModel, string[]>> CreateInvalidModels()
        {
            yield return Tuple.Create(new EpsilometerTestViewModel
            {
                Measurement = 1.0f,
                EucastClinicalBreakpointId = null,
            }, new[] { "EucastClinicalBreakpointId", "Antibiotic" });

            yield return Tuple.Create(new EpsilometerTestViewModel
            {
                Measurement = 0.0f,
                EucastClinicalBreakpointId = 1,
                Antibiotic = Antibiotic.Amikacin,
            }, new[] { "Measurement" });
        }
    }
}