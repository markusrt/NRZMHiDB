using System;
using System.Web;
using System.Web.Mvc;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class RkiMatchControllerTests
    {
        // specs/024 FR-010, US1.6: a file of zero length imports nothing and still reports success
        // (the database context is created but not used on this path)
        [Test]
        public void Index_FileOfZeroLength_ShowsSuccessLineOnSamePage()
        {
            var sut = new RkiMatchController();
            var file = Substitute.For<HttpPostedFileBase>();
            file.ContentLength.Returns(0);
            file.FileName.Returns("empty.csv");

            var result = sut.Index(file);

            result.Should().BeOfType<ViewResult>().Which.ViewName.Should().BeEmpty();
            ((string) sut.ViewBag.Message).Should().Be("File Uploaded Successfully!!");
            file.DidNotReceive().SaveAs(Arg.Any<string>());
        }

        // specs/024 FR-010, US1.8: a failure is shown as "File upload failed: " plus the exception message
        // on the same page; the message text follows the culture of the server
        [Test]
        public void Index_FailingUpload_ShowsFailureLineWithExceptionMessage()
        {
            var sut = new RkiMatchController();
            var file = Substitute.For<HttpPostedFileBase>();
            file.ContentLength.Returns(_ => throw new InvalidOperationException("SV-024 failure"));

            var result = sut.Index(file);

            result.Should().BeOfType<ViewResult>().Which.ViewName.Should().BeEmpty();
            ((string) sut.ViewBag.Message).Should().Be("File upload failed: SV-024 failure");
        }
    }
}
