using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Ui.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Controllers
{
    public class NewNoticesController : Controller
    {
        static int countPerPage = 10;
        //
        // GET: /NewNotices/
        public ActionResult Index(int id = 1, int type = 1)
        {
            ContentType ctntType;
            if (Enum.IsDefined(typeof(ContentType), type))
            {
                ctntType = (ContentType)type;
            }
            else
            {
                ctntType = ContentType.新闻中心;
            }
            Show(ctntType, id);
            ViewBag.Type = ctntType;
            return View();
        }
        DbBackModel dbm = new DbBackModel();
        void Show(ContentType type, int pageIndex = 1)
        {
            var hs = dbm.Helps.Where(a => a.type == type).OrderByDescending(a => a.Id)
                     .Skip((pageIndex - 1) * countPerPage)
                     .Take(countPerPage).ToList();
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)dbm.Helps.Where(a => a.type == (ContentType)type).Count() / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "News",
                TargetId = "contractquery"
            };
            ViewBag.Pager = pager;
            ViewBag.News = hs;
        }

        public ActionResult New(int id)
        {
            ShowHelp(id);
            return View();
        }

        void ShowHelp(int id)
        {
            ApplicationDbContext dbm = new ApplicationDbContext();
            var help = dbm.Helps.FirstOrDefault<Help>(i => i.Id == id);
            if (help != null)
            {
                ViewBag.Type = (ContentType)help.type;
                help.ReadTime += 1;
                dbm.Helps.Attach(help);
                dbm.Entry(help).State = EntityState.Modified;
                dbm.SaveChanges();
            }
            ViewBag.Help = help;
        }
    }
}