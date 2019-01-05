using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace HaemophilusWeb.Views.Utils
{
    public static class TestUtils
    {
        public static HtmlHelper<T> CreateHtmlHelper<T>(ViewDataDictionary viewData)
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