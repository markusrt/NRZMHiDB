using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Models
{
    // specs/001 FR-004, FR-005: sender validation is DataAnnotations-based (no FluentValidation validator)
    public class SenderTests
    {
        [Test]
        public void Validate_NameAndPhone1Only_IsValid()
        {
            var sender = new Sender {Name = "SV-001-Alpha", Phone1 = "0931 12345-0"};

            Validate(sender).Should().BeEmpty();
        }

        [Test]
        public void Validate_WithoutName_FailsOnName()
        {
            var sender = new Sender {Phone1 = "0931 12345-0"};

            Validate(sender).Should().BeEquivalentTo("Name");
        }

        [Test]
        public void Validate_WithoutPhone1_FailsOnPhone1()
        {
            var sender = new Sender {Name = "SV-001-Alpha"};

            Validate(sender).Should().BeEquivalentTo("Phone1");
        }

        [TestCase("0931 12345-0")]
        [TestCase("+49 (931) 12345-1")]
        [TestCase("030-18754-3535")]
        [TestCase("+49 931 31-46161")]
        public void Validate_WellFormedPhoneAndFax_IsValid(string phone)
        {
            var sender = new Sender {Name = "SV-001-Alpha", Phone1 = phone, Phone2 = phone, Fax = phone};

            Validate(sender).Should().BeEmpty();
        }

        [TestCase("abc")]
        [TestCase("fax!")]
        public void Validate_MalformedPhoneAndFax_FailsOnEachField(string phone)
        {
            var sender = new Sender {Name = "SV-001-Alpha", Phone1 = phone, Phone2 = phone, Fax = phone};

            Validate(sender).Should().BeEquivalentTo("Phone1", "Phone2", "Fax");
        }

        [Test]
        public void Validate_WellFormedEmail_IsValid()
        {
            var sender = new Sender {Name = "SV-001-Alpha", Phone1 = "0931 12345-0", Email = "sv-001-alpha@example.org"};

            Validate(sender).Should().BeEmpty();
        }

        [Test]
        public void Validate_MalformedEmail_FailsOnEmail()
        {
            var sender = new Sender {Name = "SV-001-Alpha", Phone1 = "0931 12345-0", Email = "not-an-email"};

            Validate(sender).Should().BeEquivalentTo("Email");
        }

        private static IEnumerable<string> Validate(Sender sender)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(sender, new ValidationContext(sender), results, true);
            return results.SelectMany(r => r.MemberNames).ToList();
        }
    }
}
