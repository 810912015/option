using Com.BitsQuan.Option.Ui.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    public class MainController : Controller
    {
        //
        // GET: /Supervise/Main/
        [AuthViaMenu(Roles = "网站管理员")]
        [Log]
        public ActionResult Index()
        {
            return View();
        }
	}
}