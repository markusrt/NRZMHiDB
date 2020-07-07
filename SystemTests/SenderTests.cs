using System;
using System.Threading;
using SystemTests.Common;
using FluentAssertions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace SystemTests
{
    [TestFixture]
    [Explicit]
    public class SenderTests : IHaemophilusWebTest
    {
        private readonly string nameOfSenderToCreate = Guid.NewGuid().ToString();

        private const string FieldIsRequired = "Das Feld \"*\" ist erforderlich.";

        [Test]
        public void CreateSender_MissingNameAndPhone_ShowsValidationError()
        {
            GoToUrl("/Sender/Create");

            Driver.FindElementById("Name").Submit();

            VerifyValidationMessageFor("Name", FieldIsRequired);
            VerifyValidationMessageFor("Phone1", FieldIsRequired);
        }

        [TestCase("abcde")]
        [TestCase("1234")]
        public void CreateSender_InvalidPostalCode_ShowsValidationError(string invalidPostalCode)
        {
            GoToUrl("/Sender/Create");

            Driver.FindElementById("Name").SendKeys("abc");
            Driver.FindElementById("Phone1").SendKeys("123");
            var postalCode = Driver.FindElementById("PostalCode");
            postalCode.SendKeys(invalidPostalCode);
            postalCode.Submit();

            VerifyValidationMessageFor("PostalCode", "*Die Postleitzahl muss eine 5-stellige Nummer sein");
        }

        [Test]
        public void CreateSender_ValidData_CreatesSender()
        {
            GoToUrl("/Sender/Create");

            Driver.FindElementById("Name").SendKeys(nameOfSenderToCreate);
            Driver.FindElementById("Phone1").SendKeys("123456");
            var postalCodeField = Driver.FindElementById("PostalCode");
            postalCodeField.SendKeys("91301");
            postalCodeField.Submit();

            VerifySearchForSenderReturnsMatch(nameOfSenderToCreate);
        }

        private void VerifySearchForSenderReturnsMatch(string textToSearchFor)
        {
            Thread.Sleep(2000);

            var senderSearchBox = Driver.FindElementById("senders_filter").FindElement(By.TagName("input"));
            senderSearchBox.Clear();
            senderSearchBox.SendKeys(textToSearchFor);

            Thread.Sleep(1000);
            Driver.FindElementById("senders").Text.Should().Contain(textToSearchFor);
        }

        [Test]
        public void DeleteAndUndeleteSender_Succeeds()
        {
            const string senderToDelete = "Robert Koch Institut";
            GoToUrl("/Sender/Deleted");
            Driver.FindElementById("senders").Text.Should().NotContain(senderToDelete);

            GoToUrl("/Sender/Delete/2");
            var deleteButton = Driver.FindElementByXPath("//input[@name='primary-submit']");
            deleteButton.Submit();

            GoToUrl("/Sender/Deleted");
            Driver.FindElementById("senders").Text.Should().Contain(senderToDelete);

            GoToUrl("/Sender/Undelete/2");
            GoToUrl("/Sender/Deleted");
            Driver.FindElementById("senders").Text.Should().NotContain(senderToDelete);
        }

        [Test]
        public void EditSender_ChangeDepartment_UpdatesSender()
        {
            var randomDepartment = Guid.NewGuid().ToString();
            GoToUrl("/Sender/Edit/1");

            var department = Driver.FindElementById("Department");
            department.Clear();
            department.SendKeys(randomDepartment);
            department.Submit();

            VerifySearchForSenderReturnsMatch(randomDepartment);
        }

        [Test]
        public void CreateSender_PostalCodeLookup_Succeeds()
        {
            GoToUrl("/Sender/Create");

            Driver.FindElementById("PostalCode").SendKeys("91301");

            Assert.That(Driver.FindElementById("City").GetAttribute("value"), Is.EqualTo("Forchheim").After(500));
        }

        private void VerifyValidationMessageFor(string field, string validationMessage)
        {
            Driver.FindElementById(field).FindElement(By.XPath("../.."))
                .Text.Should().Match(validationMessage);
        }

        public RemoteWebDriver Driver { get; set; }

        public Action<string> GoToUrl { get; set; }
    }
}