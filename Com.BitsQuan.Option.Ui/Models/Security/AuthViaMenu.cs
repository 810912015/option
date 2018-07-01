using Com.BitsQuan.Option.Models.Security;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Models.Security
{
    public class AuthViaMenu : AuthorizeAttribute
    {
        bool IsAllow(AuthorizationContext httpContext)
        {
            var user = httpContext.RequestContext.HttpContext.User.Identity.Name;
            var url = httpContext.RequestContext.HttpContext.Request.Url.AbsolutePath.ToString();
            var cn = httpContext.RequestContext.RouteData.Values["controller"].ToString();
            var r = MenuManager.Instance.IsAllow(user, url, cn);
            return r;
        }


        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
                                      || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);
            if (skipAuthorization) return;
            var my = IsAllow(filterContext);
            if (!my)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    } 
}