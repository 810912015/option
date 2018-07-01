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
    public class ForumController : Controller
    {
        static int countPerPage = 10;
        //
        // GET: /Supervise/Forum/
        DbBackModel dbm = new DbBackModel();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ForumIndex(int pageIndex = 1)
        {
            var hs = dbm.ForumHosts.OrderByDescending(a => a.Id)
                   .Skip((pageIndex - 1) * countPerPage)
                   .Take(countPerPage);
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)dbm.ForumHosts.Count() / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "ForumIndex",
                TargetId = "contractquery"
            };

            ViewData["pager"] = pager;
            TypeBigBound();
            TypeSmaillBound();
            return View(hs);
        }

        //void TypeShow() { //显示类型
        //  var Bigs =  dbm.BigTypes.ToList();
        //  var Smalls = dbm.SmallTypes.ToList();
        //  ViewData["Smalls"] = Smalls;
        //  ViewData["Bigs"] = Bigs;
        //}
        
    

        public ActionResult ShowForumReply(int fid)
        {
            ViewData["HostId"] = fid;
            return View();
        }

        public PartialViewResult host(int fid, int pageIndex = 1)
        {
            //根据主贴编号,查询主贴对象
            var host = dbm.ForumHosts.Where(a => a.Id == fid).FirstOrDefault();
            return PartialView(host);
        }

        public PartialViewResult Reply(int fid, int pageIndex = 1)
        {
            //根据主贴编号,查询主贴对象
            var host = dbm.ForumHosts.Where(a => a.Id == fid).FirstOrDefault();
            //根据主贴对象查询所有回帖对象
            var hs = dbm.ForumReplys.Where(a => a.Fid == fid);

            return PartialView(hs);
        }

        //删除主贴，将其对应的回帖一并删除
        public PartialViewResult DeleteHost(int id)
        {
            var host = dbm.ForumHosts.Where(a => a.Id == id).FirstOrDefault();
            var Reply = dbm.ForumReplys.Where(a => a.Fid == id).FirstOrDefault();
            
            bool r = false;
            //有回帖先删除相应回帖再删主贴
            if (Reply != null)
            {
                r = dbm.DeleteFReplys(Reply);
                if (r)
                {
                    r = dbm.DeleteFHosts(host);
                }
            }
            else { //否则直接删主贴
              r =  dbm.DeleteFHosts(host);
            }

            return PartialView();
        }

        //删除指定回帖
        public PartialViewResult DeleteReply(int id,int fid)
        {
            var Reply = dbm.ForumReplys.Where(a => a.Id == id).FirstOrDefault();
            //删除指定回帖
            var r = dbm.DeleteFReplys(Reply);
            ViewData["backFId"] = fid;
            return PartialView();
        }

       //添加大类型
        public bool AddBigType(string bname) {
            BigType bt = new BigType
            {
                Id = BackIdService<BigType>.Instance.NewId(),
                Name = bname

            };
            var r = dbm.AddBigType(bt);
            return r;
        }
        //添加小类型
        public bool AddSmaillType(int bid,string sname,string explain) {
            SmallType st = new SmallType
            {
                Id = BackIdService<BigType>.Instance.NewId(),
                Name = sname,
                EditionUser = "比权网",
                BigType = bid,
                Explain = explain
            };
            var r = dbm.AddSmaillType(st);

            return r;
        }
        //修改大类型
        public bool UpdateBigType(int bid,string bname)
        {
            var bt = dbm.BigTypes.Where(a => a.Id == bid).FirstOrDefault();//先查询对应对象
            bt.Name = bname;//重新赋值
            var r = dbm.UpdateBigType(bt);
            return r;
          
        }

        //修改小类型
        public bool UpdateSmallType(int sid, string sname)
        {
            var bt = dbm.SmallTypes.Where(a => a.Id == sid).FirstOrDefault();//先查询对应对象
            bt.Name = sname;//重新赋值
            var r = dbm.UpdateSmallType(bt);
            return r;

        }

      public  JsonResult TypeBigBound()
        { //显示类型
            var Bigs = dbm.BigTypes.ToList();
            var Smalls = dbm.SmallTypes.ToList();
            ViewData["Smalls"] = Smalls;
            ViewData["Bigs"] = Bigs;
            return Json(Bigs, JsonRequestBehavior.AllowGet);
        }

      public JsonResult TypeSmaillBound()
      { //显示类型

          var Smalls = dbm.SmallTypes.ToList();
          ViewData["Smalls"] = Smalls;
          return Json(Smalls, JsonRequestBehavior.AllowGet);
      }

        public bool deleteBigType(int bid){
            //先删除全部小类型再删大类型
            var small = dbm.SmallTypes.Where(a => a.BigType == bid).ToList();
            foreach (var item in small)
            {
              var ss =  dbm.DeleteSmaillType(item);
            }
           
            var big = dbm.BigTypes.Where(a=>a.Id==bid).FirstOrDefault();
            return dbm.DeleteBigType(big);
        }

        public bool deleteSmallType(int sid)
        {
            var small = dbm.SmallTypes.Where(a => a.Id == sid).FirstOrDefault();
            return dbm.DeleteSmaillType(small);
        }

	}
}