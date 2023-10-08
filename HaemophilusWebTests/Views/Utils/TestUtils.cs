using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NSubstitute;

namespace HaemophilusWeb.Views.Utils
{
    public static class TestUtils
    {
        public static HtmlHelper<T> CreateHtmlHelper<T>(ViewDataDictionary viewData)
        {
            var cc = Substitute.For<ControllerContext>(
                Substitute.For<HttpContextBase>(),
                new RouteData(),
                Substitute.For<ControllerBase>());

            var mockViewContext = Substitute.For<ViewContext>(
                cc,
                Substitute.For<IView>(),
                viewData,
                new TempDataDictionary(),
                TextWriter.Null);

            mockViewContext.ViewData.Returns(viewData);

            mockViewContext.ViewData = viewData;

            var mockViewDataContainer = Substitute.For<IViewDataContainer>();

            mockViewDataContainer.ViewData.Returns(viewData);

            return new HtmlHelper<T>(
                mockViewContext, mockViewDataContainer);
        }
    }
}