using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Ui.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Com.BitsQuan.Option.Models.Security
{

    /// <summary>
    /// 菜单扩展:用于判断当前菜单
    /// </summary>
    public static class MenuExtension
    {
        static Menu curTopMenu;
        static Menu curSubMenu;
        /// <summary>
        /// 子菜单中的当前菜单
        /// </summary>
        /// <param name="m"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsActive(this Menu m, string path)
        {
            try
            {
                var btm = MenuManager.Instance.GetSingleByUrl(path);
                if (btm != null && btm != curSubMenu)
                    curSubMenu = btm;
                var cmpt = btm == null ? curSubMenu : btm;
                return cmpt == m;
            }
            catch(Exception ex) {
                Singleton<TextLog>.Instance.Error(ex, "menu is active");
                return false; }
        }
        /// <summary>
        /// 主菜单中的当前菜单
        /// </summary>
        /// <param name="m"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsMainActive(this Menu m, string path)
        {
            try
            {
                var cur = MenuManager.Instance.GetSingleByUrl(path);
                if (cur == null)
                {
                    return m == curTopMenu;
                }
                var btm = cur.BelongTo;
                if (btm != null && btm != curTopMenu)
                    curTopMenu = btm;
                var cmpt = btm == null ? curTopMenu : btm;
                return cmpt == m;
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "menu");
                return false;
            }
        }
        /// <summary>
        /// 是否是上级菜单
        /// </summary>
        /// <param name="my"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        static bool IsAssenstor(this Menu my, Menu m)
        {
            if (my == null || m == null) return false;
            var t = my;
            while (t.BelongTo != null)
            {
                if (t == m)
                {
                    return true;
                }
                t = t.BelongTo;
            }
            return false;
        }
        public static bool IsAssenstor(this Menu my, string path)
        {
            var m = MenuManager.Instance.GetSingleByUrl(path);
            return IsAssenstor(m, my);

            //if (my == null || m == null) return false;
            //var t = m;
            //while (t.BelongTo != null)
            //{
            //    if (t == my)
            //    {
            //        return true;
            //    }
            //    t = t.BelongTo;
            //}
            //return false;
        }
        public static bool IsMiddleactive(this Menu m, string path, string sid)
        {
            var cur = MenuManager.Instance.GetSingleByUrl(path);
            if (cur != null)
            {
                if ((cur != null && m == cur) || cur.IsAssenstor(m))// (cur.BelongTo != null && m == cur.BelongTo))
                {
                    return true;
                }
            }
            var curMiddle = MenuManager.Instance.GetCur(sid);
            return (curMiddle != null && m == curMiddle)
                || (curMiddle.BelongTo != null && m == curMiddle.BelongTo);
        }

        public static string GetUrl(this Menu m)
        {
            if (m.Url != null) return m.Url;
            var t = m;

            while (t.SubMenus != null && t.SubMenus.Count > 0)
            {
                t = t.SubMenus[0];
                if (t.Url != null) break;
            }

            if (t.Url != null) return t.Url;
            return "/aged/index";
        }

        public static string GetBlongToStr(this Menu m)
        {
            if (m.BelongTo == null) return "!";
            string r = "";
            var t = m;
            while (t.BelongTo != null)
            {
                r += "_" + t.BelongTo.Id;
                t = t.BelongTo;
            }
            return r;
        }
    }
    /// <summary>
    /// 菜单项实际上代表了一个需要控制权限的资源
    ///     用户访问一个url,如果菜单项明确允许,那么允许
    ///     如果此url没有对应菜单项,那么找出其controller,
    ///     以菜单中的controller来决定其是否允许访问
    /// </summary>
    public static class MenuAuthExtension
    {
        public static string GetControllerName(this Menu m)
        {
            if (m.Url == null) return null;
            var ua = m.Url.Split('/');
            if (ua == null || ua.Length < 2) return null;
            return ua[1];
        }
    }
    public class IsAllowHelper
    {
        List<string> urls;
        List<string> controllers;

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("urls:");
            foreach (var v in urls) sb.AppendFormat("{0},", v);
            sb.Append("controllers:");
            foreach (var v in controllers) sb.AppendFormat("{0},", v);
            return sb.ToString();
        }

        public IsAllowHelper(List<Menu> l)
        {
            urls = new List<string>();
            controllers = new List<string>();
            if (l == null) return;
            foreach (var v in l)
            {
                Init(v);
            }
        }
        void Init(Menu v)
        {
            if (v == null) return;

            //加入自己
            if (v.Url != null)
            {
                var u = v.Url.ToLower();
                if (!urls.Contains(u))
                {
                    urls.Add(v.Url.ToLower());
                }

            }
            var c = v.GetControllerName();
            if (c != null)
            {
                var cc = c.ToLower();
                if (!controllers.Contains(cc))
                    controllers.Add(cc);
            }

            //如果有子节点,递归加入
            if (v.SubMenus == null || v.SubMenus.Count == 0) return;
            foreach (var m in v.SubMenus)
            {
                Init(m);
            }
        }


        public bool ContainsUrl(string url)
        {
            if (url == null) return false;

            var tar = url.ToLower().Split('/');
            if (tar.Length >= 3)
            {
                var tt = string.Format("/{0}/{1}", tar[1], tar[2]);
                return urls.Contains(tt);
            }
            else return urls.Contains(url.ToLower());
        }
        public bool ContainsController(string controllerName)
        {
            if (controllerName == null) return false;
            return controllers.Contains(controllerName.ToLower());
        }
    }
    class RoleUserMap
    {
        Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
        public void Add(string un, string rn)
        {
            if (rn == null || un == null) return;
            if (!dic.ContainsKey(rn)) dic.Add(rn, new List<string> { un });
            else dic[rn].Add(un);
        }
        public void Clear()
        {
            foreach (var v in dic.Values) v.Clear();
            dic.Clear();
        }
        public bool IsInRole(string un, string rn)
        {
            if (dic.ContainsKey(rn) && dic[rn].Contains(un)) return true;
            return false;
        }
    }
    /// <summary>
    /// 菜单管理:
    /// 用户的菜单由用户角色决定
    ///     菜单项代表了一个需要控制权限的资源
    ///     用户访问一个url,如果菜单项明确允许,那么允许
    ///     如果此url没有对应菜单项(存在菜单项url等于请求的url),那么找出其controller,
    ///     以菜单中是否含有此url的controller来决定其是否允许访问
    /// </summary>
    public class MenuManager : Singleton<MenuManager>
    {
        List<Menu> menus;
        Dictionary<string, Menu> dicByUrl;
        Dictionary<string, List<Menu>> roleMenu;
        public Dictionary<string, string> userRole;
        SessionMenus dicByUser;
        object sync;
        RoleUserMap rum;

        public void UpdateRoleMenu(string roleId, string[] ms)
        {
            if (roleId == null || ms.Length == 0) return;
            var lms = menus.Where(a => ms.Contains(a.Id.ToString())).ToList();
            if (roleMenu.ContainsKey(roleId))
                roleMenu[roleId] = lms;
            else roleMenu.Add(roleId, lms);
        }

        public MenuManager()
        {
            sync = new object();
            dicByUrl = new Dictionary<string, Menu>();
            dicByUser = new SessionMenus();
            rum = new RoleUserMap();
            using (var db = new ApplicationDbContext())
            {
                //menus = db.Menus.Include("SubMenus").Include("BelongTo").Include("Roles").ToList<Menu>();
                menus = new List<Menu>();
                foreach (var v in db.Menus.Include("Roles"))
                {
                    var m = new Menu
                    {
                        SubId = v.SubId,
                        Url = v.Url,
                        Name = v.Name,
                        Id = v.Id,
                        Roles = new List<ApplicationRole>()
                    };
                    foreach (var r in v.Roles)
                    {
                        m.Roles.Add(new ApplicationRole { Id = r.Id, Name = r.Name });
                    }
                    menus.Add(m);
                }
                foreach (var v in menus)
                {
                    if (v.SubId == null) continue;
                    var s = menus.Where(a => a.Id == v.SubId).FirstOrDefault<Menu>();
                    if (s == null) continue;
                    v.BelongTo = s;
                    if (s.SubMenus == null) s.SubMenus = new List<Menu>();
                    s.SubMenus.Add(v);
                }
                foreach (var v in menus)
                {
                    var k = v.Url == null ? v.Name : v.Url;
                    if (dicByUrl.ContainsKey(k))
                        dicByUrl[k] = v;
                    else dicByUrl.Add(k, v);
                }
                Refresh(db);
            }
        }
        /// <summary>
        /// 从数据库中初始化读取菜单数据
        /// </summary>
        void Refresh(ApplicationDbContext db)
        {

            lock (sync)
            {
                try
                {
                    rum.Clear();
                    dicAllow.Clear();
                    dicByUser.Clear();
                    if (roleMenu != null)
                    {
                        foreach (var v in roleMenu)
                            v.Value.Clear();
                        roleMenu.Clear();
                    }
                    else roleMenu = new Dictionary<string, List<Menu>>();


                    if (userRole == null) userRole = new Dictionary<string, string>();
                    else userRole.Clear();

                    var ur = db.Set<IdentityUserRole>();
                    var q = (from a in db.Users
                             from b in db.Roles
                             from c in ur
                             where a.Id == c.UserId && b.Id == c.RoleId
                             select new UserRoleRelation { UserName = a.UserName, RoleId = b.Id, RoleName = b.Name }).ToList<UserRoleRelation>();
                    foreach (var v in q)
                    {
                        if (userRole.ContainsKey(v.UserName))
                            userRole[v.UserName] = v.RoleId;
                        else userRole.Add(v.UserName, v.RoleId);
                        rum.Add(v.UserName, v.RoleName);
                    }
                }
                catch (Exception e)
                {
                    Singleton<TextLog>.Instance.Error(e, "用户组菜单出错");
                    //GlobalObjects.Instance.Log.Error(e, "用户组菜单出错");
                }
            }

        }

        List<Menu> tops = null;
        public List<Menu> Tops
        {
            get
            {
                if (tops != null) return tops;
                tops = menus.Where(a => a.BelongTo == null).ToList<Menu>();
                return tops;
            }
        }

        public void UpdateUserRole()
        {
            //await Task.Factory.StartNew(() => {
            using (var db = new ApplicationDbContext())
            {
                Refresh(db);
            }
            // });
        }
        public class UserRoleRelation
        {
            public string UserName { get; set; }
            public string RoleId { get; set; }
            public string RoleName { get; set; }
        }
        /// <summary>
        /// 根据用户名获取菜单列表--通过用户的角色
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public List<Menu> GetByUserName(string userName)
        {
            if (userName == null || !userRole.ContainsKey(userName)) return new List<Menu>();
            string roleId = userRole[userName];
            return GetByRoleId(roleId);
        }
        Dictionary<string, IsAllowHelper> dicAllow = new Dictionary<string, IsAllowHelper>();



        /// <summary>
        /// 用户请求url权限是否被允许
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="url"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public bool IsAllow(string userName, string url, string controllerName)
        {
           // if (rum.IsInRole(userName, "网站管理员")) return true;
            IsAllowHelper h;
            if (dicAllow.ContainsKey(userName)) h = dicAllow[userName];
            else
            {
                var ml = GetByUserName(userName);
                if (ml == null || ml.Count == 0)
                {
                    //GlobalObjects.Instance.Log.Info(string.Format("{0},{1},用户权限为空", userName, url));
                    return false;
                }
                h = new IsAllowHelper(ml);
            }
            if (h.ContainsUrl(url)) return true;
            if (h.ContainsController(controllerName)) return true;
            return false;
        }

        /// <summary>
        /// 根据角色id从缓存中获取菜单列表
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<Menu> GetByRoleId(string roleId)
        {
            if (roleId == null || roleMenu == null) return new List<Menu>();
            if (!roleMenu.ContainsKey(roleId))
            {
                var r = GetMenuStrByRoleId(roleId);
                if (!roleMenu.ContainsKey(roleId))
                    roleMenu.Add(roleId, r);
            }

            return roleMenu[roleId];
        }
        /// <summary>
        /// 根据角色id从缓存中读取菜单列表
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        List<Menu> GetMenuStrByRoleId(string RoleId)
        {
            if (RoleId == null) return null;
            var q = menus
                .Select(a =>
                { var a1 = a.Roles.Where(b => b.Id.ToString() == RoleId).FirstOrDefault<ApplicationRole>(); if (a1 != null) return a; else return null; })
                .Where(a => a != null && a.BelongTo == null).ToList<Menu>();
            return q;
        }
        /// <summary>
        /// 根据请求的url获取菜单
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Menu GetSingleByUrl(string url)
        {
            if (url == null) return null;
            if (dicByUrl.ContainsKey(url))
            {
                var m = dicByUrl[url];
                return m;
            }

            else return null;
        }
        public List<Menu> GetBread(string url, string sid)
        {
            List<Menu> l = new List<Menu>();
            var m = GetSingleByUrl(url);
            if (m == null)
            {
                m = dicByUser.GetCur(sid);
                if (m == null) return l;
            }
            l.Add(m);
            while (m.BelongTo != null)
            {
                l.Add(m.BelongTo);
                m = m.BelongTo;
            }
            l.Reverse();
            return l;
        }



        List<Menu> GetAsenstorSub(Menu m)
        {
            Menu t = m;
            while (t.BelongTo != null)
                t = t.BelongTo;
            return t.SubMenus.ToList();

        }

        /// <summary>
        /// 每session的菜单缓存
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public List<Menu> GetByUrl(string url, string sessionId)
        {
            if (sessionId == null) return new List<Menu>();

            if (dicByUrl.ContainsKey(url))
            {
                List<Menu> r;
                var m = dicByUrl[url];
                r = GetAsenstorSub(m);
                dicByUser.Add(sessionId, r);
                dicByUser.SetCur(sessionId, m);
                return r;
            }

            else
            {

                return dicByUser.GetMenu(sessionId);
            }
        }
        public Menu GetCur(string sid) { return dicByUser.GetCur(sid); }
        /// <summary>
        /// 当sesseion终结时清除菜单缓存
        /// </summary>
        /// <param name="sessionId"></param>
        public void ClearMenu(string sessionId)
        {
            dicByUser.ClearMenu(sessionId);
        }
    }

    public class MenuOfASession
    {
        public List<Menu> Menus { get; set; }
        public Menu Cur { get; set; }
    }
    public class SessionMenus
    {
        Dictionary<string, MenuOfASession> dicByUser = new Dictionary<string, MenuOfASession>();
        public void Clear()
        {
            foreach (var v in dicByUser.Values)
                v.Menus.Clear();
            dicByUser.Clear();
        }

        public void Add(string sid, List<Menu> ml)
        {
            if (sid == null) return;
            if (dicByUser.ContainsKey(sid))
            {
                dicByUser[sid] = new MenuOfASession { Menus = ml };
            }
            else
            {
                dicByUser.Add(sid, new MenuOfASession { Menus = ml });
            }
        }
        public List<Menu> GetMenu(string sid)
        {
            if (sid == null) return new List<Menu>();
            if (dicByUser.ContainsKey(sid)) return dicByUser[sid].Menus;
            else return new List<Menu>();
        }
        public void ClearMenu(string sessionId)
        {
            if (sessionId == null) return;
            if (dicByUser.ContainsKey(sessionId))
            {
                dicByUser[sessionId].Menus.Clear();
            }
            dicByUser.Remove(sessionId);
        }
        public Menu GetCur(string sid)
        {
            if (dicByUser.ContainsKey(sid)) return dicByUser[sid].Cur;
            return null;
        }
        public void SetCur(string sid, Menu m)
        {
            if (dicByUser.ContainsKey(sid)) dicByUser[sid].Cur = m;

        }
    }
}