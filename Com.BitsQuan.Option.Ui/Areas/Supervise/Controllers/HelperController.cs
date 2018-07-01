using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Other;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Models.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    [AuthViaMenu(Roles = "网站管理员")]
    [Log]
    public class HelperController : Controller
    {
        static int countPerPage = 10;
        //
        // GET: /Supervise/Helper/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HelpIndex()
        {

            return View();
        }
        DbBackModel dbm = new DbBackModel();


        //分页查询帮助
        public PartialViewResult HelpList(int pageIndex = 1, int type = 0)
        {

            if (type > 0)
            {
                var hs = dbm.Helps.OrderByDescending(a => a.Id)
               .Skip((pageIndex - 1) * countPerPage)
               .Take(countPerPage).Where(d => d.type == (ContentType)type);
                var pager = new Pager
                {
                    PageCount = (int)Math.Ceiling((double)dbm.Helps.Where(d => d.type == (ContentType)type).Count() / (double)countPerPage),
                    PageIndex = pageIndex,
                    PostAction = "HelpList",
                    TargetId = "contractquery"
                };
                ViewData["pager"] = pager;
                return PartialView(hs);
            }
            else
            {
                var hs = dbm.Helps.OrderByDescending(a => a.Id)
                  .Skip((pageIndex - 1) * countPerPage)
                  .Take(countPerPage);
                var pager = new Pager
                {
                    PageCount = (int)Math.Ceiling((double)dbm.Helps.Count() / (double)countPerPage),
                    PageIndex = pageIndex,
                    PostAction = "HelpList",
                    TargetId = "contractquery"
                };
                ViewData["pager"] = pager;
                return PartialView(hs);
            }




        }
        //添加帮助页面
        public PartialViewResult AddHelper()
        {
            return PartialView();

        }

        public JsonResult GetHelpByID(int id)
        {
            if (id.ToString() != null && id != 0)
            {

                // 根据编号查询对象（在前台显示需要为真）
                var hs = dbm.Helps.Where(a => a.Id == id).FirstOrDefault();
                return Json(hs, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return null;
            }
        }

        //添加帮助功能
        [ValidateInput(false)]
        public string AddMethod(string title, string content, string ckeShow, int type, string publishName, HttpPostedFileBase upImg)
        {
            string uploadpath = "UploadFile/New/" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "/";
            string s = "";
            DirectoryInfo info = new DirectoryInfo(base.Server.MapPath("~/Content/" + uploadpath));
            if (!info.Exists)
            {
                info.Create();
            }
            if (upImg != null)
            {
                s = UploadPicture(upImg, uploadpath);
            }
            Help h = new Help
            {
                Id = BackIdService<Help>.Instance.NewId(),//该方法不针对于数据库，是正对于List<Help>
                Htitle = title,
                Hcontent = content,
                Hdate = System.DateTime.Now.ToString(),
                HforeShow = (ckeShow == "on" ? true : false),
                Hperson = publishName,
                HlastDate = System.DateTime.Now.ToString(),
                imgSrc = s,
                type = (ContentType)type,
                ReadTime = 0
            };


            var r = dbm.AddHelper(h);
            return s;
        }
        //修改帮助功能
        [ValidateInput(false)]
        public void UpdateMethod(int id, string title, string urlPath, string content, string ckeShow, int type, string publishName, int ReadTime, HttpPostedFileBase upImg)
        {
            using (ApplicationDbContext dbm = new ApplicationDbContext())
            {
                string uploadpath = "UploadFile/New/" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "/";
                string s = "";
                DirectoryInfo info = new DirectoryInfo(base.Server.MapPath("~/Content/" + uploadpath));
                if (!info.Exists)
                {
                    info.Create();
                }
                if (upImg != null)
                {
                    s = UploadPicture(upImg, uploadpath);
                }
                else
                {
                    s = urlPath;
                }
                Help h = new Help
                {
                    Id = id,//该方法不针对于数据库，是正对于List<Help>
                    Htitle = title,
                    Hcontent = content,
                    Hdate = System.DateTime.Now.ToString(),
                    HforeShow = (ckeShow == "on" ? true : false),
                    Hperson = publishName,
                    HlastDate = System.DateTime.Now.ToString(),
                    imgSrc = s,
                    type = (ContentType)type,
                    ReadTime = ReadTime
                };
                //List<string> list = new List<string>();
                //list.Add("Htitle");
                //list.Add("Hcontent");
                //list.Add("HforeShow");
                dbm.Helps.Attach(h);

                dbm.Entry(h).State = EntityState.Modified;

                dbm.SaveChanges();
                HttpContext.Response.Write(s);
                HttpContext.Response.End();
                //return dbm.UpdateHelper(h, list);
            }
        }

        //删除功能
        public bool DeleteMethod(int id)
        {
            Help h = new Help
            {
                Id = id,
            };
            return dbm.DeleteHelper(h);
        }
        public string UploadPicture(HttpPostedFileBase fileUpload, string uploadpath)
        {
            string str = string.Empty;
            string contentType = string.Empty;
            string thumbnailPath = string.Empty;
            string str4 = string.Empty;
            string str5 = string.Empty;
            string str6 = string.Empty;
            Random random = new Random((int)DateTime.Now.Ticks);
            contentType = fileUpload.ContentType;
            str = uploadpath + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + random.Next(0x3e8, 0x270f).ToString();
            if (fileUpload.ContentLength > 0)
            {
                if (contentType == "image/gif")
                {
                    str6 = ".gif";
                }
                else if (contentType == "image/png")
                {
                    str6 = ".png";
                }
                else if (contentType == "image/jpeg" || contentType == "image/pjpeg")
                {
                    str6 = ".jpg";
                }
                else
                {
                    return "";
                }
                fileUpload.SaveAs(HttpContext.Server.MapPath("~/Content/" + str) + str6);

                return (str + str6);
            }
            return "";
        }
    }
}