using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class SenderControllerTests
    {
        private const int IdOfSenderWithoutSendings = 42;
        private readonly SenderController senderController = new SenderController(SendingControllerTests.DbMock);

        static SenderControllerTests()
        {
            SendingControllerTests.DbMock.Senders.Add(new Sender {SenderId = IdOfSenderWithoutSendings});
        }

        [Test]
        public void Delete_AddsExistingSendingsToViewBag()
        {
            const int senderToDelete = SendingControllerTests.FirstId;
            
            var result = senderController.Delete(senderToDelete) as ViewResult;

            var sendingsInViewBag = (List<Sending>)result.ViewBag.Sendings;
            var sendingsOfSender = SendingControllerTests.DbMock.Sendings.Where(s => s.SenderId == senderToDelete).ToList();
            sendingsInViewBag.Count.Should().Be(1);
            CollectionAssert.AreEquivalent(sendingsOfSender, sendingsInViewBag);
        }

        [Test]
        public void Delete_IdOfSenderWithoutSendings_SendingsInViewBagAreEmpty()
        {
            var result = senderController.Delete(IdOfSenderWithoutSendings) as ViewResult;

            Assert.IsTrue(result.ViewBag.Sendings.Count == 0);
        }
    }
}