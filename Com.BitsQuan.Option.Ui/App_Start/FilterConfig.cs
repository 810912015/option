using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Models;
using System;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui
{
    public class FilterConfig
    {

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            var useHttps = Com.BitsQuan.Miscellaneous.AppSettings.Read<bool>("requireHttpsWhenRemote", true);
            if (useHttps)
            {
                filters.Add(new RequreSecureConnectionFilter());
            }
            filters.Add(new HandleErrorAttribute());
        }
    }

    public class RequreSecureConnectionFilter : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.HttpContext.Request.IsLocal)
            {
                // when connection to the application is local, don't do any HTTPS stuff
                return;
            }

            base.OnAuthorization(filterContext);
        }
    }
}
