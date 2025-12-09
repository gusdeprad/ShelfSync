using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using ShelfSync.Mvc.Controllers;
using ShelfSync.Mvc.Models.ViewModels;

namespace ShelfSync.Test.Controllers
{
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void Index_Returns_ViewResult()
        {
            var logger = NullLogger<HomeController>.Instance;
            var ctrl = new HomeController(logger);

            var result = ctrl.Index();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Privacy_Returns_ViewResult()
        {
            var logger = NullLogger<HomeController>.Instance;
            var ctrl = new HomeController(logger);

            var result = ctrl.Privacy();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Error_Returns_ViewResult_With_ErrorViewModel()
        {
            var logger = NullLogger<HomeController>.Instance;
            var ctrl = new HomeController(logger);
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var result = ctrl.Error();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var vr = (ViewResult)result;
            Assert.IsInstanceOfType(vr.Model, typeof(ErrorViewModel));
            var model = (ErrorViewModel)vr.Model;
            Assert.IsFalse(string.IsNullOrEmpty(model.RequestId));
        }
    }
}
