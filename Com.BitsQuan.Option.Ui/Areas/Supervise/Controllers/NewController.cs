using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Other;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    [AuthViaMenu(Roles = "网站管理员")]
    [Log]
    public class NewController : Controller
    {
        ApplicationDbContext dbm = new ApplicationDbContext();
        static int countPerPage = 10;
        //
        // GET: /Supervise/New/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewIndex()
        {

            return View();
        }

        //  public ISuperviseModel Model { get; private set; } 


        //分页查询帮助
        public PartialViewResult NewList(int pageIndex = 1)
        {

            var hs = dbm.Users.OrderByDescending(a => a.Id)
                .Skip((pageIndex - 1) * countPerPage)
                .Take(countPerPage);
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)dbm.Users.Count() / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "HelpList",
                TargetId = "contractquery"
            };
            ViewData["pager"] = pager;
            return PartialView(hs);
        }
        //添加帮助页面
        public PartialViewResult AddNew()
        {
            return PartialView();

        }

        public string GetHelpByID(int id)
        {
            if (id.ToString() != null && id != 0)
            {

                // 根据编号查询对象（在前台显示需要为真）
                var hs = dbm.Helps.Where(a => a.Id == id).FirstOrDefault();

                return hs.Htitle + "," + hs.Hcontent + "," + hs.HforeShow;
            }
            else
            {
                return null;
            }
        }

        //添加帮助功能
        //public int AddMethod(string title, string content, bool ckeShow)
        //{
        //    Help h = new Help
        //    {
        //        Id = BackIdService<Help>.Instance.NewId(),//该方法不针对于数据库，是正对于List<Help>
        //        Htitle = title,
        //        Hcontent = content,
        //        Hdate = System.DateTime.Now.ToString(),
        //        HforeShow = ckeShow,
        //        Hperson = "杨瑶",
        //        HlastDate = System.DateTime.Now.ToString(),
        //        type = ContentType.新闻
        //    };

        //    var r = dbm.AddHelper(h);
        //    return r;
        //}
        //修改帮助功能
        //public bool UpdateMethod(int id, string title, string content, bool ckeShow)
        //{
        //    Help h = new Help
        //    {
        //        Id = id,
        //        Htitle = title,
        //        Hcontent = content,
        //        HforeShow = ckeShow,
        //        HlastDate = System.DateTime.Now.ToString()
        //    };
        //    List<string> list = new List<string>();
        //    list.Add("Htitle");
        //    list.Add("Hcontent");
        //    list.Add("HforeShow");
        //    return dbm.UpdateHelper(h, list);
        //}

        ////删除功能
        //public bool DeleteMethod(int id)
        //{
        //    Help h = new Help
        //    {
        //        Id = id,
        //    };
        //    return dbm.DeleteHelper(h);


        //} 
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (dbm != null)
                {
                    dbm.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}