using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Controllers
{
    public class WebHelperController : Controller
    {
        // GET: WebHelper
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Law()
        {
            return View();
        }
        public ActionResult Service()
        {
            return View();
        }
        public ActionResult Fee()
        {
            return View();

        }
        public ActionResult about()
        {
            return View();
        }

        public ActionResult Faq()
        {
            return View();
        }
        public ActionResult Rules()
        {
            return View();
        }
        public ActionResult OptionClause()
        {
            return View();
        }
        public ActionResult TheoryPriceExplain()
        {
            return View();
        }
        public ActionResult BtcOptionExplain()
        {
            return View();
        }
        public ActionResult PayAndWithdraw(int type)
        {
            ViewBag.Type = type;
            return View();
        }
    }
}