using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Com.BitsQuan.Option.Ui.Models;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Miscellaneous;
using Com.BitsQuan.Option.Ui.CreatCode;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Provider;
using System.Text;
using System.Data.Entity;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Com.BitsQuan.Option.Ui.Extensions;
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers;
using System.Configuration;
using System.Web.Security;

namespace Com.BitsQuan.Option.Ui.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        #region Fields





        #endregion

        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }
        ApplicationDbContext adb = new ApplicationDbContext();
        DbBackModel dbm = new DbBackModel();
        OptionDbCtx db = new OptionDbCtx();
        public string path = System.Configuration.ConfigurationManager.AppSettings["webSite"];
        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.UserName = "";
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByName(model.UserName);
                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        if (!LoginFailManager.IsLocked(user.Id))
                        {
                            if (UserManager.CheckPassword<ApplicationUser, string>(user, model.Password))
                            {
                                UpdateTradeCount(user);
                                await SignInAsync(user, true);
                                LoginFailManager.Reset(user.Id);
                                return RedirectToLocal(returnUrl);
                            }
                            else
                            {
                                ViewBag.UserName = model.UserName;
                                LoginFailManager.Fail(user.Id);
                                if (LoginFailManager.IsLocked(user.Id))
                                {
                                    ViewBag.Error = String.Format("由于错误次数达到上限，帐号已被锁定，解锁时间：{0}", LoginFailManager.Get(user.Id).UnlockTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                }
                                else
                                    ViewBag.Error = String.Format("密码错误，您还有{0}次机会。", LoginFailManager.MaxFailedAccessAttempts - LoginFailManager.Get(user.Id).Times);
                            }

                        }
                        else
                        {
                            ViewBag.Error = String.Format("由于错误次数达到上限，帐号已被锁定，解锁时间：{0}", LoginFailManager.Get(user.Id).UnlockTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                    }
                    else
                    {
                        ViewBag.Error = "该帐户未通过邮箱激活";
                    }
                }
                else
                    ViewBag.Error = "登录密码或用户名输入错误";
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }


        //修改密码输入次数（作用于登录只输入一次）
        [AllowAnonymous]
        public void UpdateTradeCount(ApplicationUser user)
        {
            var uu = adb.Users.Where(a => a.UserName == user.UserName).FirstOrDefault();
            //先判断该用户的交易密码输入次数是不是为11，如果为11，代表以前是登录只输入一次密码，所以需要更改为1
            if (uu.tradePwdCount == "11")
            {
                uu.tradePwdCount = "1";
                adb.SaveChanges();
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AjaxLogin(string userName, string pwd)
        {
            var LoginTimes = adb.Users.Where(a => a.UserName == userName).FirstOrDefault();
            if (LoginTimes != null)
            {
                if (LoginTimes.EnderrorTime != null)
                {
                    DateTime dtEnderrorTime = (DateTime)LoginTimes.EnderrorTime;
                    if (dtEnderrorTime.AddHours(2) < DateTime.Now)
                    {
                        LoginTimes.EnderrorTime = null;
                        LoginTimes.error = 0;
                        adb.Set<ApplicationUser>().Attach(LoginTimes);
                        adb.Entry<ApplicationUser>(LoginTimes).State = EntityState.Modified;
                        adb.SaveChanges();
                    }
                    else
                    {
                        if (LoginTimes.error > 5)
                        {
                            return Json(2, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
            }

            var user1 = UserManager.Find(userName, pwd);
            var user = false;
            if (user1 != null)
            {
                user = user1.EmailConfirmed;
            }

            if (user && user1.UserName == userName)
            {
                UpdateTradeCount(user1);
                Session["UserID"] = user1.Id;
                await SignInAsync(user1, false);
                return Json(0, JsonRequestBehavior.AllowGet);

            }
            else
            {
                LoginTimes.EnderrorTime = DateTime.Now;
                LoginTimes.error += 1;
                adb.Set<ApplicationUser>().Attach(LoginTimes);
                adb.Entry<ApplicationUser>(LoginTimes).State = EntityState.Modified;
                adb.SaveChanges();
                return Json(1, JsonRequestBehavior.AllowGet);
            }
        }
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string inviteby)
        {
            if (!string.IsNullOrWhiteSpace(inviteby))
                Session[IdentityExtension.INVITEBY_KEY] = IdentityExtension.UserNameFromInvitationCode(inviteby);
            return View();
        }
        [AllowAnonymous]
        public FileContentResult getCode()
        {
            VerifyCode v = new VerifyCode();
            byte[] bytes = v.BuildImg();
            Session["vcode"] = v.checkCode.ToLower();
            return File(bytes, @"image/jpeg");
        }
        [AllowAnonymous]
        [HttpPost]
        public bool validateCode(string code)
        {
            string vcode = Session["vcode"].ToString();
            if (vcode != code.ToLower())
            {
                return false;
            }
            return true;
        }
        [AllowAnonymous]
        [HttpPost]
        public bool IsTranPwdp(string email, string Lnpwd)
        {
            if (string.IsNullOrEmpty(Lnpwd)) return false;
            //根据用户名与密码查询是否有该用户,如果不等于null代表登录成功
            var user = adb.Users.Where(a => a.Email == email).FirstOrDefault();//密码解码查询
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
        [Log]
        [ValidateAntiForgeryToken]
        public JsonResult ResetPwd(Guid uid, string oldpwd, string newpwd)
        {
            var usr = UserManager.FindById(uid.ToString());
            if (usr != null)
            {
                var r = UserManager.ChangePassword(uid.ToString(), oldpwd, newpwd);
                if (r.Succeeded)
                {
                    return Json(new OperationResult(0, "重置密码成功"));
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var v in r.Errors) sb.Append(v);
                    return Json(new OperationResult(1, sb.ToString()));
                }
            }
            return Json(null);
        }
        [Log]
        [ValidateAntiForgeryToken]
        public JsonResult ResetTradePwd(Guid uid, string oldpwd, string newpwd)
        {
            var usr = adb.Users.Where(a => a.Id == uid.ToString()).FirstOrDefault();
            if (usr != null)
            {
                usr.TradePwd = newpwd;


                var r = adb.SaveChanges();
                if (r > 0)
                {
                    return Json(new OperationResult(0, "重置授权码成功"));
                }
                else
                {
                    return Json(new OperationResult(0, "重置授权码成功"));
                }
            }
            return Json(null);
        }

        [AllowAnonymous]
        public JsonResult IsCodeTrue(string Code)
        {
            if (string.IsNullOrEmpty(Code)) return Json(false, JsonRequestBehavior.AllowGet);
            string gCode = Session["vcode"].ToString();
            if (gCode == Code.ToLower()) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsCodeTrue1(string Code)
        {
            if (string.IsNullOrEmpty(Code)) return false;
            string gCode = Session["vcode"].ToString();
            if (gCode == Code.ToLower()) { return true; }
            else return false;
        }

        static readonly DateTime d70 = new DateTime(2000, 1, 1);
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Errors = new List<string>();
            string uid = DateTime.Now.Subtract(d70).Milliseconds.ToString();
            if (ModelState.IsValid)
            {
                if (!IsUserNameUsable1(model.UserName))
                    ViewBag.Errors.Add("用户名已被注册");
                else if (!IsUserEmailUsable1(model.Email))
                    ViewBag.Errors.Add("邮箱已被注册");
                else
                {
                    var User = adb.Users.Where(a => a.Email == model.Email || a.UserName == model.UserName).FirstOrDefault();
                    if (User == null)
                    {
                        var user = new ApplicationUser()
                        {
                            UserName = model.UserName,
                            Email = model.Email,
                            RegisterTime = DateTime.Now,
                            TradePwd = model.TradePassword,
                            // IsAllowToTrade=true,
                            Uiden = uid,
                            tradePwdCount = "n",//交易密码输入提现次数
                        };
                        var invitorUserName = Session[IdentityExtension.INVITEBY_KEY] as string;
                        if (invitorUserName != null)
                        {
                            var invitor = UserManager.FindByName<ApplicationUser, string>(invitorUserName);
                            if (invitor != null)
                            {
                                user.InvitorId = invitor.Id;
                            }
                        }
                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            Session.Remove(IdentityExtension.INVITEBY_KEY);
                            registerUserRoles(user);
                            string message = this.RenderRazorViewToString("ConfirmRegisterEmailContent",
                                new EmailContentViewModel { Email = model.Email, Url = path + "/Account/RegisterSuccess?email=" + model.Email });
                            QQExMailSender qes = new QQExMailSender();
                            await qes.SendToWait(model.Email, "比权网注册激活邮件", message);
                            //await SignInAsync(user, isPersistent: false);
                            return RedirectToAction("SendEmail", "Account", new { Email = model.Email });
                        }
                        else
                        {
                            AddErrors(result);
                        }
                    }
                }
            }
            else
            {
                foreach (var i in ModelState.Values)
                {
                    foreach (var j in i.Errors)
                    {
                        ViewBag.Errors.Add(j.ErrorMessage);
                    }
                }
            }
            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }
        public JsonResult ReSendConfirmEmail(string userName)
        {
            try
            {
                var u = adb.Users.Where(a => a.UserName == userName).FirstOrDefault();// UserManager.FindByName(userName);
                if (u == null) return Json(new OperationResult(1, "无此用户"), JsonRequestBehavior.AllowGet);

                u.RegisterTime = DateTime.Now;
                u.EmailConfirmed = false;
                var f= adb.SaveChanges();


                string message = this.RenderRazorViewToString("ConfirmRegisterEmailContent",
                                    new EmailContentViewModel { Email = u.Email, Url = path + "/Account/RegisterSuccess?email=" + u.Email });
                QQExMailSender qes = new QQExMailSender();
                qes.SendToWait(u.Email, "比权网注册激活邮件", message)
                    .ContinueWith((a) =>
                    {
                        Singleton<TextLog>.Instance.Info(string.Format("resend to {0},cancel:{1},completed:{2},faulted:{3}", u.UserName, a.IsCanceled, a.IsCompleted, a.IsFaulted));
                    });
                return Json(OperationResult.SuccessResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "resend confirm eamil");
                return Json(new OperationResult(2, "未能重新发送邮件"));
            }
        }
        //将注册用户默认为交易员
        void registerUserRoles(ApplicationUser user)
        {
            //var RoleManager = new RoleManager<ApplicationRole>(new
            //                              RoleStore<ApplicationRole>(adb));
            UserManager.AddToRole(user.Id, "交易员");
            var r = MvcApplication.OptionService.esi.CreateTrader(user.UserName, false, false);
            user.IsAllowToTrade = true;
            db.SaveChanges();
        }
        [AllowAnonymous]
        public JsonResult IsUserNameUsable(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return Json(false, JsonRequestBehavior.AllowGet);
            var q = UserManager.FindByName(userName);
            if (q == null) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }
        public bool IsUserNameUsable1(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return false;
            var q = UserManager.FindByName(userName);
            if (q == null) { return true; }
            else return false;
        }

        [AllowAnonymous]
        public JsonResult IsUserEmailUsable(string Email)
        {
            if (string.IsNullOrEmpty(Email)) return Json(false, JsonRequestBehavior.AllowGet);
            var q = UserManager.FindByEmail(Email);
            if (q == null) { return Json(true, JsonRequestBehavior.AllowGet); }
            else return Json(false, JsonRequestBehavior.AllowGet);
        }

        public bool IsUserEmailUsable1(string Email)
        {
            if (string.IsNullOrEmpty(Email)) return false;
            var q = UserManager.FindByEmail(Email);
            if (q == null) { return true; }
            else return false;
        }
        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "你的密码已更改。"
                : message == ManageMessageId.SetPasswordSuccess ? "已设置你的密码。"
                : message == ManageMessageId.RemoveLoginSuccess ? "已删除外部登录名。"
                : message == ManageMessageId.Error ? "出现错误。"
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // 请求重定向到外部登录提供程序
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // 从外部登录提供程序获取有关用户的信息
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            //  RedirectToLocal("/Home/Index");
            return RedirectToAction("Login");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult FindPassword()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult SendEmail(string Email)
        {
            // var user = UserManager.FindAsync(userName, pwd);

            ViewData["Email"] = Email;
            Session["Email"] = Email;
            ViewBag.msg = Email;
            return View();
        }
        [AllowAnonymous]
        public async Task<ActionResult> RegisterSuccess(string email = "")
        {
            if (email != "")
            {
                var User = adb.Users.Where(a => a.Email == email).FirstOrDefault();
                if (User.EmailConfirmed == true)
                {
                    ViewData["reg"] = null;
                }
                else
                {
                    if (User.RegisterTime.Value.AddHours(1) >= DateTime.Now)
                    {
                        if (User != null)
                        {
                            User.EmailConfirmed = true;
                            adb.Set<ApplicationUser>().Attach(User);
                            adb.Entry<ApplicationUser>(User).State = EntityState.Modified;
                            adb.SaveChanges();
                            await SignInAsync(User, false);

                        }
                        ViewData["reg"] = true;
                    }
                }

            }
            return View();
        }
        public void SendEmail2()
        {
            string email = Session["Email"].ToString();
            QQExMailSender qes = new QQExMailSender();
            qes.SendTo(email, "hello" + DateTime.Now.ToString(), "恭喜您注册成功,请点击<a href='" + path + "/Secure/Index'>" + path + "/Secure/Index</a>进入安全中心设置基本信息");

        }
        [AllowAnonymous]
        public JsonResult SendEmail3(string email, string code)
        {
            string referer = HttpContext.Request.Headers["Referer"];
            if ((referer != null) && (referer.Trim().StartsWith(path)))
            {
                var User = adb.Users.Where(a => a.Email == email).FirstOrDefault();
                var msg = "";
                if (User == null)
                {
                    return Json(new OperationResult(1, "该邮箱未注册"), JsonRequestBehavior.AllowGet);
                }

                string vcode = Session["vcode"].ToString();
                if (vcode != code.ToLower())
                {
                    return Json(new OperationResult(2, "验证码错误"), JsonRequestBehavior.AllowGet);
                }

                QQExMailSender qes = new QQExMailSender();
                Session["emailSet"] = email;
                string type = Encode(DateTime.Now.ToString());
                string message = SendMessage(email, path + "/Account/SetLoginPass?email=" + email + "&type=" + Server.UrlEncode(type), "重置密码");
                qes.SendTo(email, "比权网重置密码通知", message);
                return Json(new OperationResult(0, ""), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new OperationResult(12, "Error"), JsonRequestBehavior.AllowGet);

            }

        }
        const string KEY_64 = "VavicApp";//注意了，是8个字符，64位

        const string IV_64 = "VavicApp";
        public string Encode(string data)
        {
            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);

            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();
            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);

        }

        public string Decode(string data)
        {
            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV_64);

            byte[] byEnc;
            try
            {
                byEnc = Convert.FromBase64String(data);
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "account decode");
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }

        [AllowAnonymous]
        public bool EmailIsNot(string email)
        {
            var User = adb.Users.Where(a => a.Email == email).FirstOrDefault();
            if (User == null)
            {
                return false;

            }
            return true;
        }



        [AllowAnonymous]
        public ActionResult SetLoginPass(string email, string type)
        {
            if (email != "" && type != "")
            {
                DateTime d = DateTime.Now;
                bool b = DateTime.TryParse(Decode(Server.UrlDecode(type)), out d);
                if (d != null && d.AddHours(2) > DateTime.Now)
                {
                    ViewData["email"] = email;
                }
                else
                {
                    return Content("该链接无效，或者已经过期");
                }
            }

            return View();
        }
        [AllowAnonymous]
        //[HttpPost]
        public string UpdateLoginPwd(string email, string pwd)
        {
            //根据邮箱获得用户
            if (Session["emailSet"] != null)
            {
                if (Session["emailSet"].ToString() == email)
                {
                    var User = adb.Users.Where(a => a.Email == email).FirstOrDefault();
                    if (pwd == User.TradePwd)
                    {
                        return "交易密码和登陆密码不能相同！";
                    }
                    var result = UserManager.CreateAsync(User, pwd);//密码转码准备添加
                    ApplicationUser u = new ApplicationUser
                    {
                        Id = User.Id,
                        IdNumber = User.IdNumber,//身份证号
                        IsAllowToTrade = User.IsAllowToTrade,
                        RegisterTime = User.RegisterTime,
                        TradePwd = User.TradePwd,//交易密码
                        Email = email,//邮箱
                        EmailConfirmed = User.EmailConfirmed,
                        PasswordHash = User.PasswordHash,//登录密码
                        SecurityStamp = User.SecurityStamp,
                        PhoneNumber = User.PhoneNumber,//手机号
                        PhoneNumberConfirmed = User.PhoneNumberConfirmed,
                        TwoFactorEnabled = User.TwoFactorEnabled,
                        LockoutEnabled = User.LockoutEnabled,
                        AccessFailedCount = User.AccessFailedCount,
                        UserName = User.UserName,
                        IdNumberType = User.IdNumberType,//证件类型
                        RealityName = User.RealityName,//真实姓名
                        IdentiTime = User.IdentiTime//认证时间
                    };

                    bool r = dbm.UpdateUser(u);
                    if (r == true)
                    {
                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                    //if (r.ToString() == "True" || r == true)
                    //{
                    //    return RedirectToAction("Login");
                    //}
                    //return View();
                }
                else
                {
                    return "0";
                }
            }
            else
            {
                return "0";
            }

        }


        //发送邮件只邮箱
        //public void SendToEmail(string Email)
        //{
        //    ViewData["Email"] = Email;

        //}
        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
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
            }
            base.Dispose(disposing);
        }



        #region 帮助程序
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties()
            {
                IsPersistent = isPersistent,
                IssuedUtc = DateTime.Now,
                AllowRefresh = true,
                ExpiresUtc = DateTime.Now.AddMinutes(30)
            }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        public string SendMessage(string qq, string url, string message)
        {
            return this.RenderRazorViewToString("EmailContent", new EmailContentViewModel { Email = qq, Url = url, Message = message });
        }
    }
}