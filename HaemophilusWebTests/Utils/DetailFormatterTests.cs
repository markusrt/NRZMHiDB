using System;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Utils
{
    public class DetailFormatterTests
    {

        [Test]
        public void ToDetail_FormatsNull()
        {

            Sender sender = null;

            sender!.ToDetail().Should().Be("Kein Einsender");
        }

        [Test]
        public void ToDetail_FormatsSenderWithoutDepartment()
        {

            var sender = new Sender()
            {
                SenderId = 99,
                PostalCode = "91301",
                City = "Forchheim",
                Name = "Test Sender"
            };

            sender.ToDetail().Should().Be("#099: Test Sender - 91301 Forchheim");
        }

        [Test]
        public void ToDetail_FormatsSenderWithDepartment()
        {

            var sender = new Sender()
            {
                SenderId = 99,
                PostalCode = "91301",
                City = "Forchheim",
                Name = "Test Sender",
                Department = "Department"
            };

            sender.ToDetail().Should().Be("#099: Test Sender (Department) - 91301 Forchheim");
        }
        
    }
}