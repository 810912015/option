using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Other;
using Com.BitsQuan.Option.Ui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Controllers
{
 [Authorize(Roles = "交易员")]
    public class FrontForumController : Controller
    {
        //
        // GET: /FrontForum/
        public ActionResult Index(int id=0)
        {
           BigForSmall(id);
            return View();
        }

        public ActionResult IndexHome()
        {


            return View();
        }
        ApplicationDbContext adb = new ApplicationDbContext();
    //    DbBackModel dbm = new DbBackModel();
        void HomePageIndex(int pageIndex)
        {
            int tid = Convert.ToInt32(Session["tid"]);
             var  hh= dbm.ForumHosts.Where(a => a.Smalltype == tid);
             if (Session["where"] != null)
             {
                 string where = Session["where"].ToString();
                 if (where == "1")
                 {
                     hh = hh.OrderBy(a => a.FDate)
                          .Skip((pageIndex - 1) * countPerPage)
                          .Take(countPerPage).ToList();
                 }
                 else if (where == "2")
                 {
                     hh = hh.OrderBy(a => a.replyCount)
                             .Skip((pageIndex - 1) * countPerPage)
                             .Take(countPerPage).ToList();
                 }
                 else if (where == "3")
                 {
                     hh = hh.OrderByDescending(a => a.FDate)
                             .Skip((pageIndex - 1) * countPerPage)
                             .Take(countPerPage).ToList();
                 }
                 else if (where == "4")
                 {
                     hh = hh.OrderByDescending(a => a.replyCount)
                             .Skip((pageIndex - 1) * countPerPage)
                             .Take(countPerPage).ToList();
                 }
             }
             else
             {

                 hh = hh.OrderByDescending(a => a.Id)
                            .Skip((pageIndex - 1) * countPerPage)
                            .Take(countPerPage).ToList();
             }
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)dbm.ForumHosts.Where(a => a.Smalltype == tid).Count() / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "IndexList",
                TargetId = "contractquery"
            };


        //    var hs = dbm.ForumHosts.ToList();
            foreach (var item in hh)
            {
                //根据主贴查询回帖的数量
                var rcount = dbm.ForumReplys.Where(a => a.Fid == item.Id).Count();
                ViewData["RelyCount" + item.Id] = rcount;
            }

            ViewData["Hpager"] = pager;
            ViewData["Forum"] = hh;
        }

        void SelectBySmallType(int tid,int pageIndex)
        {

            var hh=dbm.ForumHosts.Where(a => a.Smalltype == tid);
            var hs = hh.OrderByDescending(a => a.Id)
                    .Skip((pageIndex - 1) * countPerPage)
                    .Take(countPerPage).ToList();
            foreach (var item in hs)
            {
                //根据主贴查询回帖的数量
                var rcount = dbm.ForumReplys.Where(a => a.Fid == item.Id).Count();
                ViewData["RelyCount"+item.Id] = rcount;
            }
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)dbm.ForumHosts.Where(a => a.Smalltype == tid).Count() / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "IndexList",
                TargetId = "contractquery"
            };
            
            ViewData["Hpager"] = pager;
            ViewData["Forum"] = hs;
        }


        void SelectByPaiXu(int tid, int pageIndex, string where)
        {

            var hh = dbm.ForumHosts.Where(a => a.Smalltype == tid);
            var hs = hh.ToList();
            if (where == "1") {
                hs = hh.OrderBy(a => a.FDate)//按时间倒叙排序
               .Skip((pageIndex - 1) * countPerPage)
               .Take(countPerPage).ToList();
                ViewData["FDate"] = 3;//重新赋正序
            }
            else if (where == "2") {
                hs = hh.OrderBy(a => a.replyCount)//按回复数量倒叙排序
               .Skip((pageIndex - 1) * countPerPage)
               .Take(countPerPage).ToList();
                ViewData["replyCount"] = 4;
            }
            else if (where == "3")
            {
                hs = hh.OrderByDescending(a => a.FDate)//按时间正序排序
               .Skip((pageIndex - 1) * countPerPage)
               .Take(countPerPage).ToList();
                ViewData["FDate"] = 1;//重新赋倒叙
            }
            else if (where == "4")
            {
                hs = hh.OrderByDescending(a => a.replyCount)//按时间正序排序
               .Skip((pageIndex - 1) * countPerPage)
               .Take(countPerPage).ToList();
                ViewData["replyCount"] =2;
            }

           


            foreach (var item in hs)
            {
                //根据主贴查询回帖的数量
                var rcount = dbm.ForumReplys.Where(a => a.Fid == item.Id).Count();
                ViewData["RelyCount" + item.Id] = rcount;
            }
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)dbm.ForumHosts.Where(a => a.Smalltype == tid).Count() / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "IndexList",
                TargetId = "contractquery"
            };

            ViewData["Hpager"] = pager;
            ViewData["Forum"] = hs;
        }

        public ActionResult IndexList(int tid = 0, int pageIndex = 1,string where=null)
        {
            //if (where == "排序方式")
            //{
            //    Session["where"] = null;
            //}
            //else {
            //    Session["where"] = where;
            //}
            if (where != null)
            {
                Session["where"] = where;
                tid =Convert.ToInt32(Session["tid"]);
                //根据小类型查询
                SelectByPaiXu(tid, pageIndex, where);
            }
            else if (tid == 0)
            {
                HomePageIndex(pageIndex);
            }
            else {
                Session["where"] = null;//如果重新选择了小类型则清除排序条件
                Session["tid"] = tid;
                //根据小类型查询
                SelectBySmallType(tid, pageIndex);
            }

            //计算指定小类型对应主贴的数量
            var SmalHost = adb.ForumHosts.Where(a => a.Smalltype == tid).Count();
            //计算指定小类型对应回帖的数量
            var SmalRely = adb.ForumReplys.Where(a => a.Smalltype == tid).Count();
            if (tid == 0) {
                tid = Convert.ToInt32(Session["tid"]);
            }
            //根据小类型编号查询对应名称
           var SmalType = adb.SmallTypes.Where(a => a.Id == tid).FirstOrDefault();
            //根据小类型查询大类型编号，再根据大类型编号查询名称
           var bigType = adb.BigTypes.Where(a => a.Id == SmalType.BigType).FirstOrDefault();
          
            ViewData["SmalHost"] = SmalHost;
          ViewData["SmalTypeName"] = SmalType.Name;
           ViewData["BigTypeName"] = bigType.Name;
           ViewData["BigTypeId"] = bigType.Id;
            ViewData["SmalRely"] = SmalRely;
            return View();
        }


         static int countPerPage = 10;
        DbBackModel dbm = new DbBackModel();
        public ActionResult MyForumIndex(int pageIndex = 1)
        {
            selectList();
            myHost(pageIndex);
            return View();
        }

        void selectList() {
           var smalls = adb.SmallTypes.ToList();
           List<SelectListItem> list = new List<SelectListItem>();
           foreach (var smal in smalls)
           {
               SelectListItem item = new SelectListItem();
               item.Value = smal.Id.ToString();
               item.Text = smal.Name;
               list.Add(item);
           }
            ViewData["s"] = list;
        }

        //提交发帖按钮
         [HttpPost]
        public ActionResult MyForumIndex(HostModel hm)
        {

            if (ModelState.IsValid)
            {
                int id = Convert.ToInt32(hm.Htype);
                //根据小类型编号查找对应大类型
               var big = adb.SmallTypes.Where(a => a.Id ==id).FirstOrDefault();
                ForumHost fh = new ForumHost
                {
                    Id = BackIdService<ForumReply>.Instance.NewId(),
                    Fcontent = hm.Hcontent,
                    Fname = hm.Title,
                    FDate = System.DateTime.Now.ToString(),
                    FuserName = User.Identity.Name,
                    Smalltype = Convert.ToInt32(hm.Htype),
                    Bigtype=big.BigType
                };
                bool r = dbm.AddHost(fh);
                if (r)
                {
                    ViewBag.msg2 = "添加成功";
                }
                myHost();
                selectList();
                return View(r ? null : hm);
            }
            return View(hm);
        }

        void myHost(int pageIndex = 1)
        {
            var hs = dbm.ForumHosts.Where(a => a.FuserName == User.Identity.Name).OrderByDescending(a => a.Id)
                         .Skip((pageIndex - 1) * countPerPage)
                         .Take(countPerPage).ToList();
            var pager = new Pager
            {
                PageCount = (int)Math.Ceiling((double)dbm.ForumHosts.Where(a => a.FuserName == User.Identity.Name).Count() / (double)countPerPage),
                PageIndex = pageIndex,
                PostAction = "MyForumIndex",
                TargetId = "contractquery"
            };

            ViewData["pager"] = pager;
            ViewData["myHost"] = hs;
        }
        public ActionResult FrontForumReply(int fid)
        {
            ReplyShow(fid);
            return View();
        }


        //提交回复按钮
        [HttpPost]
        public ActionResult FrontForumReply(ReplyModel rm)
        {
            if (ModelState.IsValid)
            {
                var r = AddReply(rm);
                return View(r ? null : rm);
            }
            return View(rm);
        }

        public PartialViewResult FrontHost(int fid, int pageIndex = 1)
        {
            //根据主贴编号,查询主贴对象
            var host = dbm.ForumHosts.Where(a => a.Id == fid).FirstOrDefault();
            return PartialView(host);
        }


        public PartialViewResult FrontReply(int fid, int pageIndex = 1)
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
            else
            { //否则直接删主贴
                r = dbm.DeleteFHosts(host);
            }

            return PartialView();
        }

        //删除指定回帖
        public PartialViewResult DeleteReply(int id,int fid)
        {
            var Reply = dbm.ForumReplys.Where(a => a.Id == id).FirstOrDefault();
            //删除指定回帖
            dbm.DeleteFReplys(Reply);
            ViewData["backFId"] = fid;
            return PartialView();
        }



        public ActionResult ReplyList(int fid, int pageIndex = 1)
        {
            ReplyShow(fid);
            return View();
        }

        //提交回复按钮
        [HttpPost]
        public ActionResult ReplyList(ReplyModel rm)
        {
            if (ModelState.IsValid)
            {
              var r = AddReply(rm);
              return View(r ? null : rm);
            }
            return View(rm);
        }

        //添加回帖
        bool AddReply(ReplyModel rm)
        {
            //查询主贴
            var host = adb.ForumHosts.Where(a => a.Id == rm.fid).FirstOrDefault();
            //修改主贴的回复数量（没回复一条增加1）
            host.replyCount += 1;
            dbm.updateHost(host);
            

            int id = Convert.ToInt32(host.Smalltype);
              //根据小类型编号查找对应大类型
             var big = adb.SmallTypes.Where(a => a.Id == id).FirstOrDefault().BigType;
            ForumReply fr = new ForumReply
            {
                Fid = rm.fid,
                Rcontent = rm.Rcontent,
                RDate = System.DateTime.Now.ToString(),
                Uid = User.Identity.Name,
                Id = BackIdService<ForumReply>.Instance.NewId(),
                Smalltype = host.Smalltype,
                Bigtype=big

            };

            bool r = dbm.AddReply(fr);
            if (r)
            {
                ViewBag.msg2 = "添加成功";
            }
            ReplyShow(rm.fid);
            return r;
        }

        void ReplyShow(int fid, int pageIndex = 1)
        {
            //根据主贴编号,查询主贴对象
            var host = dbm.ForumHosts.Where(a => a.Id == fid).FirstOrDefault();
            //根据主贴对象查询所有回帖对象
            var hs = dbm.ForumReplys.Where(a => a.Fid == fid).OrderByDescending(a=>a.RDate).ToList();
            ViewData["HostId"] = fid;
            ViewData["ReplyList"] = hs;
            ViewData["Host"] = host;


        }


        void BigForSmall(int bid, int pageIndex=1)
        {
            //获得所有大类型
            var Bigtype = adb.BigTypes.ToList();
            if (bid > 0) {
                Bigtype = adb.BigTypes.Where(a => a.Id == bid).ToList();
            }
            foreach (var Bigs in Bigtype)
            {
                //根据大类型编号查找所有小类型
                var Small = adb.SmallTypes.Where(a => a.BigType == Bigs.Id).ToList();
                ViewData["SmaillType" + Bigs.Id] = Small;
                foreach (var item in Small)
                {
                    var scounth = adb.ForumHosts.Where(a => a.Smalltype == item.Id).Count();
                    var scountr = adb.ForumReplys.Where(a => a.Smalltype == item.Id).Count();
                    ViewData["smaillCount" + item.Id] = scounth + "/" + scountr;
                    //根据时间查询小类型最后的主贴
                    var LastContent = adb.ForumHosts.Where(a => a.Smalltype == item.Id).OrderByDescending(a => a.FDate).FirstOrDefault();
                    ViewData["LastContent" + item.Id] = LastContent;
                }
            }
           //根据小类型查询主贴和回帖的数量
                ViewData["BigTiTile"] = Bigtype;
        }

	}
}