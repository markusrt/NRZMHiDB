using System;
using System.Collections.Generic;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class HealthOfficeValidatorTests : AbstractValidatorTests<HealthOfficeValidator, HealthOffice>
    {
        protected static readonly IEnumerable<Tuple<HealthOffice, string[]>> InvalidModels;

        static HealthOfficeValidatorTests()
        {
            InvalidModels = CreateInvalidModels();
        }

        protected override HealthOffice CreateValidModel()
        {
            return CreateHealthOffice();
        }

        private static HealthOffice CreateHealthOffice()
        {
            return new HealthOffice
            {
                PostalCode = "78987",
            };
        }

        private static IEnumerable<Tuple<HealthOffice, string[]>> CreateInvalidModels()
        {
            yield return Tuple.Create(new HealthOffice
            {
            }, new[] { "PostalCode" });
        }
    }
}