using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Models
{
    // specs/016 FR-012, US2.6: email, telephone and fax formats are DataAnnotations rules on the model; the
    // non-empty postal code is the FluentValidation rule (Validators/HealthOfficeValidatorTests).
    public class HealthOfficeTests
    {
        [Test]
        public void Validate_PlaceholderStyleValues_IsValid()
        {
            var office = new HealthOffice
            {
                Address = "SV-016-Amt\nTeststraße 1",
                Phone = "09191 86-3504",
                Fax = "09191-86-3508",
                Email = "Gesundheitsamt@lra-fo.de",
                PostalCode = "91301"
            };

            Validate(office).Should().BeEmpty();
        }

        // specs/016 Edge Cases: cleared telephone, fax and email pass validation and are saved without a value
        [Test]
        public void Validate_WithoutPhoneFaxAndEmail_IsValid()
        {
            var office = new HealthOffice {Address = "SV-016-Amt", PostalCode = "91301"};

            Validate(office).Should().BeEmpty();
        }

        [TestCase("Email", "kein")]
        [TestCase("Phone", "abc")]
        [TestCase("Fax", "xyz")]
        public void Validate_MalformedValue_FailsOnThatMember(string member, string value)
        {
            var office = new HealthOffice {PostalCode = "91301"};
            typeof(HealthOffice).GetProperty(member).SetValue(office, value);

            Validate(office).Should().BeEquivalentTo(member);
        }

        private static IEnumerable<string> Validate(HealthOffice office)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(office, new ValidationContext(office), results, true);
            return results.SelectMany(r => r.MemberNames).ToList();
        }
    }
}
