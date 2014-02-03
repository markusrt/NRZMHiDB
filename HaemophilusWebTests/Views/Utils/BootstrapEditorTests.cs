using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace HaemophilusWeb.Views.Utils
{
    public class BootstrapEditorTests
    {
        private class SimpleModel
        {
            public string SimpleProperty { get; set; }

            [Required]
            public string RequiredProperty { get; set; }
        }

        [Test]
        public void TextEditorFor_SimpleProperty_CreatesBootstrapTextEditor()
        {
            var helper = CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(new SimpleModel()));
            var textEditorHtml = helper.TextEditorFor(m => m.SimpleProperty, "Placeholder");

            textEditorHtml.ToHtmlString().Should().Match("*<label*for*SimpleProperty*");
            textEditorHtml.ToHtmlString().Should().Match("*<input*form-control*id=\"SimpleProperty\" name=\"SimpleProperty\"*");
            textEditorHtml.ToHtmlString().Should().Match("*placeholder=\"Placeholder\"*");
            
        }

        [Test]
        public void TextEditorFor_SimpleProperty_ShouldNotContainGlyphicons()
        {
            var helper = CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(new SimpleModel()));
            var textEditorHtml = helper.TextEditorFor(m => m.SimpleProperty);

            textEditorHtml.ToHtmlString().Should().NotContain("input-group-addon");
            textEditorHtml.ToHtmlString().Should().NotContain("glyphicon glyphicon-star");
        }

        [Test]
        public void TextEditorFor_RequiredProperty_ShouldContainGlyphiconStar()
        {
            var helper = CreateHtmlHelper<SimpleModel>(new ViewDataDictionary(new SimpleModel()));
            var textEditorHtml = helper.TextEditorFor(m => m.RequiredProperty);

            textEditorHtml.ToHtmlString().Should().Contain("input-group-addon");
            textEditorHtml.ToHtmlString().Should().Contain("glyphicon glyphicon-star");
        }

        public HtmlHelper<T> CreateHtmlHelper<T>(ViewDataDictionary viewData)
        {
            var cc = new Mock<ControllerContext>(
                new Mock<HttpContextBase>().Object,
                new RouteData(),
                new Mock<ControllerBase>().Object);

            var mockViewContext = new Mock<ViewContext>(
                cc.Object,
                new Mock<IView>().Object,
                viewData,
                new TempDataDictionary(),
                TextWriter.Null);

            mockViewContext.Setup(c => c.ViewData).Returns(viewData);

            mockViewContext.Object.ViewData = viewData; 

            var mockViewDataContainer = new Mock<IViewDataContainer>();

            mockViewDataContainer.Setup(v => v.ViewData).Returns(viewData);

            return new HtmlHelper<T>(
                mockViewContext.Object, mockViewDataContainer.Object);
        }
    }
}