using Com.BitsQuan.Miscellaneous;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Ui.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace Com.BitsQuan.Option.Ui.Controllers
{
    [Authorize(Roles = "交易员")]
    public class SecureController : Controller
    {
        OptionDbCtx db = new OptionDbCtx();
        ApplicationDbContext adb = new ApplicationDbContext();
        DbBackModel dbm = new DbBackModel();
        public string path = System.Configuration.ConfigurationManager.AppSettings["webSite"];
        public SecureController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }
        public SecureController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (UserManager != null)
                {
                    UserManager.Dispose();
                    UserManager = null;
                }
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
                if (adb != null)
                {
                    adb.Dispose();
                    adb = null;
                }
                if (dbm != null)
                {
                    dbm.Dispose();
                    dbm = null;
                }
            }
            base.Dispose(disposing);
        }
        public UserManager<ApplicationUser> UserManager { get; private set; }

        //
        // GET: /Secure/
        public ActionResult Index(int ph = 0)
        {
            var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            if (ph == 1)
            {
                ViewData["AccNum"] = "1";
            }
            else if (u != null)
            {
                ViewData["AccNum"] = u.IdNumber;

            }
            return View();
        }

        public PartialViewResult AccountInfo()
        {

            AccountByInfo();
            return PartialView();
        }


        void AccountByInfo()
        {
            var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            ViewData["Users"] = u;
        }

        public PartialViewResult BindPhone()
        {
            var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            if (u != null)
            {
                ViewData["Phone_jue"] = u.PhoneNumber;
            }

            return PartialView();
        }

        /// <summary>
        /// 设置手机号
        /// </summary>
        /// <returns></returns>
        public PartialViewResult BindPhoneSet()
        {
            return PartialView();
        }
        [HttpPost]
        public PartialViewResult BindPhoneSet(setPhoneModel mm)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                if (ModelState.IsValid && IsCodeTrue1(mm.Code))
                {
                    var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                    u.PhoneNumber = mm.Phone;
                    u.PhoneNumberConfirmed = true;
                    adb.Set<ApplicationUser>().Attach(u);
                    adb.Entry<ApplicationUser>(u).State = EntityState.Modified;
                    var r = adb.SaveChanges() > 0;
                    if (r == true)
                    {
                        clearSession_Code();
                        ViewData["massagePset"] = "修改成功";
                        ViewBag.Success = true;
                        RewardInvitorIfNeed(u);
                    }
                    else
                    {
                        clearSession_Code();
                        ViewData["massagePset"] = "修改失败";
                    }
                    return PartialView();
                }
                return PartialView(mm);
            }
            else
            {

                return PartialView("Error");
            }
        }

        /// <summary>
        /// 修改手机号
        /// </summary>
        /// <returns></returns>
        public PartialViewResult BindPhoneUpd()
        {
            var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            if (u != null)
            {
                ViewData["Phone"] = u.PhoneNumber;
            }

            return PartialView();
        }
        [HttpPost]
        public PartialViewResult BindPhoneUpd(Upd_Phone mm)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                if (ModelState.IsValid && (IsCodeTrue1(mm.Code) && IsCodeTrue_new1(mm.newPhoneCode)))
                {
                    var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                    adb.Set<ApplicationUser>().Attach(u);
                    adb.Entry<ApplicationUser>(u).State = EntityState.Modified;
                    var r = adb.SaveChanges() > 0;
                    if (r == true)
                    {
                        clearSession_Code();
                        ViewData["Phone"] = mm.newPhone;
                        ViewData["massageUset"] = "修改成功";
                    }
                    else
                    {
                        clearSession_Code();
                        ViewData["Phone"] = u.PhoneNumber;
                        ViewData["massageUset"] = "修改失败";
                    }

                    return PartialView();
                }
                return PartialView(mm);
            }
            else
            {

                return PartialView("Error");
            }
        }
        public bool UpdatePhone(string phone)
        {
            //   string id = Session["UserID"].ToString();
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                if (u == null)
                {
                    return false;
                }
                bool r = DefaultUpdate(u, u.TradePwd, u.Email, u.PasswordHash, phone, u.IdNumberType, u.IdNumber, u.RealityName, u.IdentiTime, u.tradePwdCount);
                return r;
            }
            else
            {
                return false;
            }
        }

        QQExMailSender qes = new QQExMailSender();
        //获得旧手机的验证码:type有old(原手机验证码)与new(新手机验证码)
        public void getOldCode(string phone)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer == null) || (!referer.Trim().StartsWith(path)))
            {
                return;
            }
            var u = adb.Users.Where(a => a.PhoneNumber == phone && a.UserName == User.Identity.Name).FirstOrDefault();
            if (u != null)
            {
                int code = qes.SendMassage(phone);
                Session["code"] = code + "," + DateTime.Now.ToString();
            }

        }
        public void getCodeFirst(string phone)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer == null) || (!referer.Trim().StartsWith(path)))
            {
                return;
            }
            var u = adb.Users.Where(a => a.PhoneNumber == phone && a.UserName == User.Identity.Name).FirstOrDefault();
            if (u == null)
            {
                int code = qes.SendMassage(phone);
                Session["code"] = code + "," + DateTime.Now.ToString();
            }

        }
        public void getNewCode(string phone)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer == null) || (!referer.Trim().StartsWith(path)))
            {
                return;
            }
            var u = adb.Users.Where(a => a.PhoneNumber == phone && a.UserName == User.Identity.Name).FirstOrDefault();
            if (u == null)
            {
                int code = qes.SendMassage(phone);
                Session["codenew"] = code + "," + DateTime.Now.ToString();
            }
            else
            {
                HttpContext.Response.Write("1");
            }
        }

        /// <summary>
        /// 判断手机号是否已被绑定
        /// </summary>
        /// <param name="phoneNum"></param>
        /// <returns></returns>
        public JsonResult IsPhoneBound(string Phone)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer == null) || (!referer.Trim().StartsWith(path)))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrWhiteSpace(Phone)) return Json(false, JsonRequestBehavior.AllowGet);

            var u = adb.Users.Where(a => a.PhoneNumber == Phone && a.PhoneNumberConfirmed == true).FirstOrDefault();
            if (u != null)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            //return Json(UserManager.Users.Any(a => !(a.PhoneNumber == newPhone && a.PhoneNumberConfirmed)), JsonRequestBehavior.AllowGet);
        }
        public JsonResult IsPhoneBoundnewPhone(string newPhone)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer == null) || (!referer.Trim().StartsWith(path)))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrWhiteSpace(newPhone)) return Json(false, JsonRequestBehavior.AllowGet);
            var u = adb.Users.Where(a => a.PhoneNumber == newPhone && a.PhoneNumberConfirmed == true).FirstOrDefault();
            if (u != null)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            //return Json(UserManager.Users.Any(a => !(a.PhoneNumber == newPhone && a.PhoneNumberConfirmed)), JsonRequestBehavior.AllowGet);
        }
        public JsonResult IsCodeTrue(string Code)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer == null) || (!referer.Trim().StartsWith(path)))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(Code)) return Json(false, JsonRequestBehavior.AllowGet);
            if (Session["code"] == null)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            string gCode = Session["code"].ToString();

            string[] gCodes = gCode.Split(',');
            if (gCodes.Length == 2)
            {
                DateTime d = DateTime.Parse(gCodes[1]);
                if (d.AddMinutes(3) < DateTime.Now)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }

            if (gCodes[0] == Code.ToLower()) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsCodeTrue1(string newPhoneCode)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer == null) || (!referer.Trim().StartsWith(path)))
            {
                return false;
            }
            if (string.IsNullOrEmpty(newPhoneCode)) return false;
            if (Session["code"] == null)
            {
                return false;
            }
            string gCode = Session["code"].ToString();
            string[] gCodes = gCode.Split(',');
            if (gCodes.Length == 2)
            {
                DateTime d = DateTime.Parse(gCodes[1]);
                if (d.AddMinutes(3) < DateTime.Now)
                {
                    return false;
                }
            }

            if (gCodes[0] == newPhoneCode.ToLower()) { return true; }
            else return false;
        }
        public JsonResult IsCodeTrue_new(string newPhoneCode)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer == null) || (!referer.Trim().StartsWith(path)))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(newPhoneCode)) return Json(false, JsonRequestBehavior.AllowGet);
            if (Session["codenew"] == null)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            string gCode = Session["codenew"].ToString();
            string[] gCodes = gCode.Split(',');
            if (gCodes.Length == 2)
            {
                DateTime d = DateTime.Parse(gCodes[1]);
                if (d.AddMinutes(3) < DateTime.Now)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            if (gCodes[0] == newPhoneCode) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsCodeTrue_new1(string newPhoneCode)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer == null) || (!referer.Trim().StartsWith(path)))
            {
                return false;
            }
            if (string.IsNullOrEmpty(newPhoneCode)) return false;
            if (Session["codenew"] == null)
            {
                return false;
            }
            string gCode = Session["codenew"].ToString();
            string[] gCodes = gCode.Split(',');
            if (gCodes.Length == 2)
            {
                DateTime d = DateTime.Parse(gCodes[1]);
                if (d.AddMinutes(3) < DateTime.Now)
                {
                    return false;
                }
            }
            if (gCodes[0] == newPhoneCode) { return true; }
            else return false;
        }

        public PartialViewResult LoginPwd()
        {
            return PartialView();
        }

        /// <summary>
        /// 修改登录密码
        /// </summary>
        /// <returns></returns>
        public PartialViewResult LoginPwdUpd()
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                if (u != null)
                {
                    ViewData["Phone_td2"] = u.PhoneNumber;
                }
            }
            return PartialView();
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        /// <summary>
        /// 修改登录密码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public PartialViewResult LoginPwdUpd(UpdL_PwdModel pm)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                if (ModelState.IsValid && (IsLoginPwdTrue1(pm.Lpwd) && IsCodeTrue1(pm.Code) && IsTranPwd1(pm.Lnpwd)))
                {
                    var uu = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                    ViewData["Phone_td2"] = uu.PhoneNumber;

                    var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                    var result = UserManager.CreateAsync(u, pm.Lnpwd);//密码转码准备添加
                    bool r = DefaultUpdate(u, u.TradePwd, u.Email, u.PasswordHash, u.PhoneNumber, u.IdNumberType, u.IdNumber, u.RealityName, u.IdentiTime, u.tradePwdCount);
                    if (r)
                    {
                        AuthenticationManager.SignOut();
                        clearSession_Code();
                        ViewBag.msg = "修改成功";
                        return PartialView();
                    }
                    else
                    {
                        clearSession_Code();
                        ViewBag.msg = "修改失败";
                        return PartialView();
                    }
                }
                ViewBag.msg = "修改失败";
                return PartialView();
            }
            else
            {
                ViewBag.msg = "修改失败";
                return PartialView("Error");
            }
        }
        //async Task<IdentityResult> Ctrate(ApplicationUser u, string Lnpwd)
        //{

        //    return result;
        //}

        //公用的修改
        bool DefaultUpdate(ApplicationUser User, string tpwd, string email, string lpwd, string phone, string idenNumType, string idenNum, string Realityname, string rzTime, string tcount)
        {
            ApplicationUser u = new ApplicationUser
            {
                Id = User.Id,
                IdNumber = idenNum,//身份证号
                IsAllowToTrade = User.IsAllowToTrade,
                RegisterTime = User.RegisterTime,
                TradePwd = tpwd,//交易密码
                Email = email,//邮箱
                EmailConfirmed = User.EmailConfirmed,
                PasswordHash = lpwd,//登录密码
                SecurityStamp = User.SecurityStamp,
                PhoneNumber = phone,//手机号
                PhoneNumberConfirmed = User.PhoneNumberConfirmed,
                TwoFactorEnabled = User.TwoFactorEnabled,
                LockoutEnabled = User.LockoutEnabled,
                AccessFailedCount = User.AccessFailedCount,
                UserName = User.UserName,
                IdNumberType = idenNumType,//证件类型
                RealityName = Realityname,//真实姓名
                IdentiTime = rzTime,//认证时间
                tradePwdCount = tcount,
                Uiden = User.Uiden,
                InviteBonusSum = User.InviteBonusSum,
                InviteCount = User.InviteCount,
                InviteFeeSum = User.InviteFeeSum,
                InvitorFeeRatio = User.InvitorFeeRatio,
                InvitorId = User.InvitorId,
                IsInvitor = User.IsInvitor,
                LastTransferFeeTime = User.LastTransferFeeTime
            };
            bool r = dbm.UpdateUser(u);
            return r;
        }

        public PartialViewResult TradePwd()
        {
            return PartialView();
        }

        /// <summary>
        /// 设置交易密码
        /// </summary>
        /// <returns></returns>
        public PartialViewResult TradePwdSet()
        {
            return PartialView();
        }

        public int TradePwdSetaa()
        {
            return 22;
        }
        /// <summary>
        /// 修改交易密码
        /// </summary>
        /// <returns></returns>
        public PartialViewResult TradePwdUpd()
        {
            var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            ViewData["Phone_td"] = u.PhoneNumber;
            return PartialView();
        }

        [HttpPost]
        public PartialViewResult TradePwdUpd(UpdL_PwdModel pm)
        {
            //  string id = Session["UserID"].ToString();
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                if (ModelState.IsValid && (IsCodeTrue1(pm.Code) && IsLoginPwd1(pm.Lnpwd)))
                {
                    var uu = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                    ViewData["Phone_td"] = uu.PhoneNumber;

                    var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();

                    bool r = DefaultUpdate(u, pm.Lnpwd, u.Email, u.PasswordHash, u.PhoneNumber, u.IdNumberType, u.IdNumber, u.RealityName, u.IdentiTime, u.tradePwdCount);

                    if (r)
                    {
                        clearSession_Code();
                        ViewBag.msg = "修改成功";
                        return PartialView();
                    }
                    else
                    {
                        clearSession_Code();
                        ViewBag.msg = "修改失败";
                        return PartialView();
                    }
                }
                return PartialView();
            }
            else
            {

                return PartialView("Error");
            }
        }
        //清楚验证码session
        public void clearSession_Code()
        {
            if (Session["code"] != null)
            {
                Session["code"] = "";
            }
            if (Session["codenew"] != null)
            {
                Session["codenew"] = "";
            }
        }

        //修改邮箱
        public PartialViewResult EmailUpd()
        {
            return PartialView();
        }

        public bool updateEmail(string email)
        {
            //  string id = Session["UserID"].ToString();
            var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            bool r = DefaultUpdate(u, u.TradePwd, email, u.PasswordHash, u.PhoneNumber, u.IdNumberType, u.IdNumber, u.RealityName, u.IdentiTime, u.tradePwdCount);
            return r;
        }

        public PartialViewResult TradeInputSet()
        {
            return PartialView();
        }
        /// <summary>
        /// 设置交易密码输入次数
        /// </summary>
        public bool SetInputTradeCount(string count, string tpwd)
        {
            if ((count == "0" || count == "1" || count == "n") && IsTranPwdTrue1(tpwd))
            {
                var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                //判断交易密码是否正确
                if (u == null || u.TradePwd != tpwd)
                {
                    return false;
                }
                else
                {
                    //修改用户信息
                    bool r = DefaultUpdate(u, u.TradePwd, u.Email, u.PasswordHash, u.PhoneNumber, u.IdNumberType, u.IdNumber, u.RealityName, System.DateTime.Now.ToString(), count);
                    return r;
                }
            }

            else
            {
                return false;
            }
        }


        public PartialViewResult AccountIdentifi()
        {
            var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
            if (u != null)
            {
                ViewData["AccNum"] = u.IdNumber;
            }
            selectBind();
            return PartialView();

        }

        /// <summary>
        /// 判断被邀请用户是否完成相应操作，如果是则对邀请者进行相应的操作
        /// </summary>
        /// <param name="invited">被邀请用户</param>
        void RewardInvitorIfNeed(ApplicationUser invited)
        {
            if (invited.PhoneNumberConfirmed && !string.IsNullOrWhiteSpace(invited.IdNumber))
            {
                var invitor = invited.GetInvitor();
                if (invitor != null)
                {
                    var t = invitor.GetTrader();
                    //增加邀请计数和奖金数量
                    ++invitor.InviteCount;
                    invitor.InviteBonusSum += MvcApplication.ifm.TransBonus(t, new List<string> { invited.UserName });
                    UserManager.Update(invitor);
                }
            }
        }

        [HttpPost]
        public ActionResult AccountIdentifi(Account_iden acc)
        {
            if (ModelState.IsValid)
            {
                var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                if (u != null)
                {
                    ViewData["AccNum"] = u.IdNumber;
                }
                if (acc.IdentityType == "身份证")
                {
                    if ((!Regex.IsMatch(acc.IdentityId, @"^(^\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$", RegexOptions.IgnoreCase)))
                    {
                        ViewData["errorSe"] = "请输入正确的身份证!";
                        selectBind();
                        return PartialView();
                    }
                }

                u.IdNumberType = acc.IdentityType;
                u.IdNumber = acc.IdentityId;
                u.RealityName = acc.RealityName;

                //修改用户信息
                //    var u = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();
                adb.Set<ApplicationUser>().Attach(u);
                adb.Entry<ApplicationUser>(u).State = EntityState.Modified;
                var r = adb.SaveChanges() > 0;
                if (r)
                {
                    RewardInvitorIfNeed(u);
                    //if()
                    // return PartialView();
                    return RedirectToAction("AccountInfo", "Secure");
                }
                else
                {
                    selectBind();
                    return PartialView();
                }
            }
            return PartialView(acc);
        }

        public static readonly String[] IdentityTypes = new String[] { 
            "身份证","军官证","护照","台湾居民通行证","港澳居民通行证"
        };
        void selectBind()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            SelectListItem item = new SelectListItem();
            item.Value = "身份证";
            item.Text = "身份证";
            list.Add(item);

            SelectListItem item2 = new SelectListItem();
            item2.Value = "军官证";
            item2.Text = "军官证";
            list.Add(item2);

            SelectListItem item3 = new SelectListItem();
            item3.Value = "护照";
            item3.Text = "护照";
            list.Add(item3);

            SelectListItem item4 = new SelectListItem();
            item4.Value = "台湾居民通行证";
            item4.Text = "台湾居民通行证";
            list.Add(item4);

            SelectListItem item5 = new SelectListItem();
            item5.Value = "港澳居民通行证";
            item5.Text = "港澳居民通行证";
            list.Add(item5);

            //SelectListItem item6 = new SelectListItem();
            //item6.Value = "其他";
            //item6.Text = "其他";
            //list.Add(item6);
            ViewData["type"] = list;
        }
        public PartialViewResult GoogleValidate()
        {

            return PartialView();
        }

        public PartialViewResult GoogleValidate2()
        {

            return PartialView();
        }

        //public JsonResult IsLoginPwd(string Lnpwd)
        //{
        //    if (string.IsNullOrEmpty(Lnpwd)) return Json(false, JsonRequestBehavior.AllowGet);
        //    //获得用户的登录密码
        //    var user = await UserManager.FindAsync(User.Identity.Name, Lpwd);//密码解码查询
        //    if (user != null) { return Json(true, JsonRequestBehavior.AllowGet); }
        //    else return Json(false, JsonRequestBehavior.AllowGet);
        //}


        public async Task<JsonResult> IsLoginPwd(string Lnpwd)
        {
            if (string.IsNullOrEmpty(Lnpwd)) return Json(false, JsonRequestBehavior.AllowGet);
            //根据用户名与交易密码查询是否有该用户，如果有，代表交易密码与登录密码一致，则返回false
            var user = await UserManager.FindAsync(User.Identity.Name, Lnpwd);//密码解码查询
            if (user == null) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsLoginPwd1(string Lnpwd)
        {
            if (string.IsNullOrEmpty(Lnpwd)) return false; ;
            //根据用户名与交易密码查询是否有该用户，如果有，代表交易密码与登录密码一致，则返回false
            var user = UserManager.Find(User.Identity.Name, Lnpwd);//密码解码查询
            if (user == null) { return true; }
            else return false;
        }
        /// <summary>
        /// 登陆密码和交易密码不能相同
        /// </summary>
        /// <param name="Lpwd"></param>
        /// <returns></returns>
        public async Task<JsonResult> IsTranPwdTrue(string Lpwd)
        {
            if (string.IsNullOrEmpty(Lpwd)) return Json(false, JsonRequestBehavior.AllowGet);
            //根据用户名与密码查询是否有该用户,如果不等于null代表登录成功
            var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();//密码解码查询
            if (user != null)
            {
                if (user.TradePwd == Lpwd)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsTranPwdTrue1(string Lpwd)
        {
            if (string.IsNullOrEmpty(Lpwd)) return false;
            //根据用户名与密码查询是否有该用户,如果不等于null代表登录成功
            var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();//密码解码查询
            if (user != null)
            {
                if (user.TradePwd == Lpwd)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else return false;
        }
        public async Task<JsonResult> IsLoginPwdTrue(string Lpwd)
        {
            if (string.IsNullOrEmpty(Lpwd)) return Json(false, JsonRequestBehavior.AllowGet);
            //根据用户名与密码查询是否有该用户,如果不等于null代表登录成功
            var user = await UserManager.FindAsync(User.Identity.Name, Lpwd);//密码解码查询
            if (user != null) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsLoginPwdTrue1(string Lpwd)
        {
            if (string.IsNullOrEmpty(Lpwd)) return false;
            //根据用户名与密码查询是否有该用户,如果不等于null代表登录成功
            var user = UserManager.Find(User.Identity.Name, Lpwd);//密码解码查询
            if (user != null) { return true; }
            else return false;
        }

        public async Task<JsonResult> IsTranPwd(string Lnpwd)
        {
            if (string.IsNullOrEmpty(Lnpwd)) return Json(false, JsonRequestBehavior.AllowGet);
            //根据用户名与密码查询是否有该用户,如果不等于null代表登录成功
            var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();//密码解码查询
            if (user != null)
            {
                if (user.TradePwd == Lnpwd)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsTranPwd1(string Lnpwd)
        {
            if (string.IsNullOrEmpty(Lnpwd)) return false;
            //根据用户名与密码查询是否有该用户,如果不等于null代表登录成功
            var user = adb.Users.Where(a => a.UserName == User.Identity.Name).FirstOrDefault();//密码解码查询
            if (user != null)
            {
                if (user.TradePwd == Lnpwd)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else return false;
        }

    }
}