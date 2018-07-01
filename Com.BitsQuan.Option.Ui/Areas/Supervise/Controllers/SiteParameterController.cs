using Com.BitsQuan.Miscellaneous;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Models.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    [AuthViaMenu(Roles = "网站管理员")]
    [Log]
    public class SiteParameterController : Controller
    {
        static int countPerPage = 10;
        //
        // GET: /Supervise/SiteParameter/
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult WebSiteSet()
        {
            return PartialView();
        }

        public PartialViewResult EmailNotice()
        {
            return PartialView();
        }

        public PartialViewResult EmailServerSet()
        {
            return PartialView();
        }

        public PartialViewResult MassegeNotice()
        {
            return PartialView();
        }

        public PartialViewResult PaySet()
        {
            return PartialView();
        }

        public PartialViewResult StateSet()
        {
            return PartialView();
        }


        public PartialViewResult tradeSet()
        {
            return PartialView();
        }
        public PartialViewResult AdvImgs(string name, int pageIndex = 1)
        {
            ApplicationDbContext dbm = new ApplicationDbContext();

            //    name = "qq";
            if (name != null)
            {
                var coin = dbm.AdvImgs.Where(a => a.LinkName == name);
                var pager = new Pager
                {
                    PageCount = (int)Math.Ceiling((double)dbm.AdvImgs.Count() / (double)countPerPage),
                    PageIndex = pageIndex,
                    PostAction = "CoinList",
                    TargetId = "contractquery"
                };
                ViewData["pager"] = pager;
                return PartialView(coin);
            }
            else
            {
                var coin = dbm.AdvImgs.OrderByDescending(a => a.Id).Skip((pageIndex - 1) * countPerPage)
                     .Take(countPerPage);
                var pager = new Pager
                {
                    PageCount = (int)Math.Ceiling((double)dbm.AdvImgs.Count() / (double)countPerPage),
                    PageIndex = pageIndex,
                    PostAction = "CoinList",
                    TargetId = "contractquery"
                };
                ViewData["pager"] = pager;
                return PartialView(coin);


            }
        }
        public PartialViewResult FriendlyLinks(string name, int pageIndex = 1)
        {
            ApplicationDbContext dbm = new ApplicationDbContext();

            //    name = "qq";
            if (name != null)
            {
                var coin = dbm.FriendlyLinks.Where(a => a.LinkName == name);
                var pager = new Pager
                {
                    PageCount = (int)Math.Ceiling((double)dbm.FriendlyLinks.Count() / (double)countPerPage),
                    PageIndex = pageIndex,
                    PostAction = "CoinList",
                    TargetId = "contractquery"
                };
                ViewData["pager"] = pager;
                return PartialView(coin);
            }
            else
            {
                var coin = dbm.FriendlyLinks.OrderByDescending(a => a.Id).Skip((pageIndex - 1) * countPerPage)
                     .Take(countPerPage);
                var pager = new Pager
                {
                    PageCount = (int)Math.Ceiling((double)dbm.FriendlyLinks.Count() / (double)countPerPage),
                    PageIndex = pageIndex,
                    PostAction = "CoinList",
                    TargetId = "contractquery"
                };
                ViewData["pager"] = pager;
                return PartialView(coin);


            }
        }
        public JsonResult SetEmail(string from, string pwd, string smtp)
        {
            try
            {
                QQExMailSender.FromAddr = from; QQExMailSender.Pwd = pwd; QQExMailSender.Smtp = smtp;
                QQExMailSender.InitClient();
                return Json("设置成功", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult WebHelper(string name, int pageIndex = 1)
        {
            using (ApplicationDbContext dbm = new ApplicationDbContext())
            {

                //    name = "qq";
                if (name != null)
                {
                    var coin = dbm.FriendlyLinks.Where(a => a.LinkName == name);
                    var pager = new Pager
                    {
                        PageCount = (int)Math.Ceiling((double)dbm.FriendlyLinks.Count() / (double)countPerPage),
                        PageIndex = pageIndex,
                        PostAction = "CoinList",
                        TargetId = "contractquery"
                    };
                    ViewData["pager"] = pager;
                    return PartialView(coin);
                }
                else
                {
                    var coin = dbm.WebHelpers.OrderByDescending(a => a.Id).Skip((pageIndex - 1) * countPerPage)
                         .Take(countPerPage);
                    var pager = new Pager
                    {
                        PageCount = (int)Math.Ceiling((double)dbm.WebHelpers.Count() / (double)countPerPage),
                        PageIndex = pageIndex,
                        PostAction = "CoinList",
                        TargetId = "contractquery"
                    };
                    ViewData["pager"] = pager;
                    return PartialView(coin.ToList());
                }
            }
        }
        public PartialViewResult UserSet()
        {
            return PartialView();
        }
        DbBackModel dbm = new DbBackModel();

        public bool UpdateSiteAll(string par1, string par2, string par3, string par4, string par5)
        {
            //根据编号查询对象
            var item = dbm.SiteParameters.FirstOrDefault();
            bool r = DefoultUpadte(item, par1, par2, par3, par4, par5, item.OpenSite, item.siteState, item.userRegiste);
            return r;
        }
        public bool UpdateSiteAll2(bool par1, bool par2, bool par3)
        {
            //根据编号查询对象
            var item = dbm.SiteParameters.FirstOrDefault();
            bool r = DefoultUpadte(item, item.Copyright, item.Describe, item.Keyword, item.SiteName, item.SiteUrl, par1, par2, par3);
            return r;
        }

        bool DefoultUpadte(SiteParameter item, string a1, string a2, string a3, string a4, string a5, bool a6, bool a7, bool a8)
        {
            SiteParameter sp = new SiteParameter
            {
                Id = item.Id,
                Copyright = a1,
                Describe = a2,
                Keyword = a3,
                SiteName = a4,
                SiteUrl = a5,
                OpenSite = a6,
                siteState = a7,
                userRegiste = a8,

                tradePwd = item.tradePwd,
                outTime = item.outTime,
                emailUserPwd = item.emailUserPwd,
                emailUserName = item.emailUserName,
                emaiSendName = item.emaiSendName,
                SendEmail = item.SendEmail,
                sendFreezeEmail = item.sendFreezeEmail,
                sendRecoverEmail = item.sendRecoverEmail,
                sendZhuanzEmail = item.sendZhuanzEmail,
                sendWithdrawEmail = item.sendWithdrawEmail,
                sendreChargeEmail = item.sendreChargeEmail,
                sendFreezeMsg = item.sendFreezeMsg,
                sendThawMsg = item.sendThawMsg,
                sendZhuanzMsg = item.sendZhuanzMsg,
                sendWithdrawMsg = item.sendWithdrawMsg,
                sendreChargeMsg = item.sendreChargeMsg,
                BankZhuanz = item.BankZhuanz,
                OnlinePayment = item.OnlinePayment
            };
            bool r = dbm.UpdateSite(sp);
            return r;
        }

        public bool UpdateSiteAll3(bool par1, bool par2)
        {
            //根据编号查询对象
            var item = dbm.SiteParameters.FirstOrDefault();
            bool r = DefoultUpadte2(item, par1, par2, item.outTime, item.emailUserPwd, item.emailUserName, item.emaiSendName, item.SendEmail);
            return r;
        }

        public bool UpdateSiteAll4(string par1)
        {
            //根据编号查询对象
            var item = dbm.SiteParameters.FirstOrDefault();
            bool r = DefoultUpadte2(item, item.trader, item.tradePwd, par1, item.emailUserPwd, item.emailUserName, item.emaiSendName, item.SendEmail);
            return r;
        }
        public bool UpdateSiteAll5(string par1, string par2, string par3, string par4)
        {
            //根据编号查询对象
            var item = dbm.SiteParameters.FirstOrDefault();
            bool r = DefoultUpadte2(item, item.trader, item.tradePwd, item.outTime, par1, par2, par3, par4);
            return r;
        }

        bool DefoultUpadte2(SiteParameter item, bool a1, bool a2, string a3, string a4, string a5, string a6, string a7)
        {
            SiteParameter sp = new SiteParameter
            {
                Id = item.Id,
                Copyright = item.Copyright,
                Describe = item.Describe,
                Keyword = item.Keyword,
                SiteName = item.SiteName,
                SiteUrl = item.SiteUrl,
                OpenSite = item.OpenSite,
                siteState = item.siteState,
                userRegiste = item.userRegiste,

                trader = a1,
                tradePwd = a2,
                outTime = a3,

                emailUserPwd = a4,
                emailUserName = a5,
                emaiSendName = a6,
                SendEmail = a7,

                sendFreezeEmail = item.sendFreezeEmail,
                sendZhuanzEmail = item.sendZhuanzEmail,
                sendWithdrawEmail = item.sendWithdrawEmail,
                sendreChargeEmail = item.sendreChargeEmail,
                sendFreezeMsg = item.sendFreezeMsg,
                sendThawMsg = item.sendThawMsg,
                sendZhuanzMsg = item.sendZhuanzMsg,
                sendWithdrawMsg = item.sendWithdrawMsg,
                sendreChargeMsg = item.sendreChargeMsg,
                BankZhuanz = item.BankZhuanz,
                OnlinePayment = item.OnlinePayment
            };
            bool r = dbm.UpdateSite(sp);
            return r;
        }

        public bool UpdateSiteAll6(bool par1, bool par2, bool par3, bool par4, bool par5)
        {
            //根据编号查询对象
            //var item = dbm.SiteParameters.FirstOrDefault();
            //bool r = DefoultUpadte3(par1, par2, par3, par4, par5, item.sendFreezeMsg, item.sendThawMsg, item.sendZhuanzMsg, item.sendWithdrawMsg, item.sendreChargeMsg, item.BankZhuanz, item.OnlinePayment);
            //return r;
            return false;
        }
        bool DefoultUpadte3(bool a01, bool a0, bool a1, bool a2, bool a3, bool a4, bool a5, bool a6, bool a7, bool a8, bool a9, bool a10)
        {
            //根据编号查询对象
            var item = dbm.SiteParameters.FirstOrDefault();
            SiteParameter sp = new SiteParameter
            {
                Id = item.Id,
                Copyright = item.Copyright,
                Describe = item.Describe,
                Keyword = item.Keyword,
                SiteName = item.SiteName,
                SiteUrl = item.SiteUrl,
                OpenSite = item.OpenSite,
                siteState = item.siteState,
                userRegiste = item.userRegiste,
                trader = item.trader,
                tradePwd = item.tradePwd,
                outTime = item.outTime,
                emailUserPwd = item.emailUserPwd,
                emailUserName = item.emailUserName,
                SendEmail = item.SendEmail,

                sendFreezeEmail = a01,
                sendRecoverEmail = a0,
                sendZhuanzEmail = a1,
                sendWithdrawEmail = a2,
                sendreChargeEmail = a3,

                sendFreezeMsg = a4,
                sendThawMsg = a5,
                sendZhuanzMsg = a6,
                sendWithdrawMsg = a7,
                sendreChargeMsg = a8,
                BankZhuanz = a9,
                OnlinePayment = a10
            };
            bool r = dbm.UpdateSite(sp);
            return r;
        }
        public bool FriendLinks(string par1, string par2, string par3)
        {
            //根据编号查询对象
            FriendlyLink f = new FriendlyLink();
            f.LinkName = par1;
            f.LinkUrl = par2;
            f.SortId = par3;
            bool r = dbm.AddFriendlyLinks(f);
            return r;
        }

        public string DelFriendLinks(int id)
        {
            using (ApplicationDbContext dbm = new ApplicationDbContext())
            {
                FriendlyLink f = new FriendlyLink();
                f.Id = id;
                dbm.FriendlyLinks.Attach(f);
                dbm.FriendlyLinks.Remove(f);
                dbm.SaveChanges();
                return "删除成功";
            }
        }
        public string GetFriendLinksByID(int id)
        {
            using (ApplicationDbContext dbm = new ApplicationDbContext())
            {
                if (id.ToString() != null && id != 0)
                {

                    // 根据编号查询对象（在前台显示需要为真）
                    var hs = dbm.FriendlyLinks.Where(a => a.Id == id).FirstOrDefault();

                    return hs.Id + "," + hs.LinkName + "," + hs.LinkUrl + "," + hs.SortId;
                }
                else
                {
                    return null;
                }
            }
        }
        public string UpdateFriendLinks(FriendlyLink f)
        {
            using (ApplicationDbContext dbm = new ApplicationDbContext())
            {
                dbm.FriendlyLinks.Attach(f);
                dbm.Entry(f).State = EntityState.Modified;
                dbm.SaveChanges();
                return "修改成功";
            }
        }

        public void AdvImg(string par1, string par2, string par3, HttpPostedFileBase upImg)
        {
            using (ApplicationDbContext dbm = new ApplicationDbContext())
            {
                string uploadpath = "UploadFile/Picture/" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "/";
                DirectoryInfo info = new DirectoryInfo(base.Server.MapPath("~/Content/" + uploadpath));
                if (!info.Exists)
                {
                    info.Create();
                }
                string s = UploadPicture(upImg, uploadpath);

                //根据编号查询对象
                AdvImg f = new AdvImg();
                f.LinkName = par1;
                f.ImageAddress = s;
                f.LinkUrl = par2;
                f.SortId = par3;
                dbm.AdvImgs.Add(f);
                dbm.SaveChanges();
                HttpContext.Response.Write(s);
                HttpContext.Response.End();
                //根据编号查询对象
                //FriendlyLink f = new FriendlyLink();
                //f.LinkName = par1;
                //f.LinkUrl = par2;
                //f.SortId = par3;
                //bool r = dbm.AddFriendlyLinks(f);
                //return r;
            }
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
        public void UpdateAdvImg(int par4,string par1, string par2, string par3, HttpPostedFileBase upImg)
        {
            using (ApplicationDbContext dbm = new ApplicationDbContext())
            {
                string uploadpath = "UploadFile/Picture/" + DateTime.Now.Date.ToString("yyyy-MM-dd") + "/";
                DirectoryInfo info = new DirectoryInfo(base.Server.MapPath("~/Content/" + uploadpath));
                if (!info.Exists)
                {
                    info.Create();
                }
                string s = "";
                //根据编号查询对象
                AdvImg f = new AdvImg();
                f.Id = par4;
                f.LinkName = par1;
                f.ImageAddress = par2;

                f.SortId = par3;

                if (upImg != null)
                {
                    s = UploadPicture(upImg, uploadpath);
                    f.LinkUrl = s;
                }
                else
                {
                    f.LinkUrl = par2;
                }
                dbm.AdvImgs.Attach(f);

                dbm.Entry(f).State = EntityState.Modified;

                dbm.SaveChanges();
                HttpContext.Response.Write(s);
                HttpContext.Response.End();
            }
        }
        public string GetAdvImgByID(int id)
        {
            using (ApplicationDbContext dbm = new ApplicationDbContext())
            {
                if (id.ToString() != null && id != 0)
                {

                    // 根据编号查询对象（在前台显示需要为真）
                    var hs = dbm.AdvImgs.Where(a => a.Id == id).FirstOrDefault();

                    return hs.Id + "," + hs.LinkName + "," + hs.ImageAddress + "," + hs.LinkUrl + "," + hs.SortId;
                }
                else
                {
                    return null;
                }
            }
        }
        public string DelAdvImg(int id)
        {
            ApplicationDbContext dbm = new ApplicationDbContext();
            AdvImg f = new AdvImg();
            f.Id = id;
            dbm.AdvImgs.Attach(f);
            dbm.AdvImgs.Remove(f);
            dbm.SaveChanges();
            return "删除成功";
        }

        [ValidateInput(false)]
        public string WebHelpers(WebHelper webHelper)
        {
            ApplicationDbContext dbm = new ApplicationDbContext();
            dbm.WebHelpers.Add(webHelper);
            dbm.SaveChanges();
            return "添加成功";
        }
        public string GetWebHelperByID(int id)
        {
            ApplicationDbContext dbm = new ApplicationDbContext();
            if (id.ToString() != null && id != 0)
            {

                // 根据编号查询对象（在前台显示需要为真）
                var hs = dbm.WebHelpers.Where(a => a.Id == id).FirstOrDefault();

                return hs.Id + "," + hs.WebTitle + "," + hs.WebEnd + "," + hs.WebContent + "," + hs.SortId;
            }
            else
            {
                return null;
            }
        }
        public string DelWebHelper(int id)
        {
            ApplicationDbContext dbm = new ApplicationDbContext();
            WebHelper f = new WebHelper();
            f.Id = id;
            dbm.WebHelpers.Attach(f);
            dbm.WebHelpers.Remove(f);
            dbm.SaveChanges();
            return "删除成功";
        }
        [ValidateInput(false)]
        public string UpdateWebHelper(WebHelper f)
        {
            ApplicationDbContext dbm = new ApplicationDbContext();
            dbm.WebHelpers.Attach(f);
            dbm.Entry(f).State = EntityState.Modified;
            dbm.SaveChanges();
            return "修改成功";

        }
    }
}