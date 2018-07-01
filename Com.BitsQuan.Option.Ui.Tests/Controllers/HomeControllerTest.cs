using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Ui;
using Com.BitsQuan.Option.Ui.Controllers;
using Com.BitsQuan.Option.Match;

namespace Com.BitsQuan.Option.Ui.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void RefreshModel()
        {
            MvcApplication.OptionService = new MatchService();
            HomeController c = new HomeController();
            var r = c.Refresh("月115001");
            Assert.IsNotNull(r);
        }
    }
}
