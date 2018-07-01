using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Extensions
{
    public static class MvcExtension
    {
        /// <summary>
        /// 使用model渲染指定视图，返回解析结果
        /// </summary>
        /// <param name="self"></param>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string RenderRazorViewToString(this Controller self, string viewName, object model)
        {
            self.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(self.ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(self.ControllerContext, viewResult.View,
                                             self.ViewData, self.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(self.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public static bool IsTraderLogin(this Controller self)
        {
            var u = self.User.GetTraderId();
            if (u == -1)
            {
                return false;
            }
            return true;
        }
        public static string ContentFullPath(this UrlHelper url, string virtualPath)
        {
            var result = string.Empty;
            Uri requestUrl = url.RequestContext.HttpContext.Request.Url;

            result = string.Format("{0}://{1}{2}",
                                   requestUrl.Scheme,
                                   requestUrl.Authority,
                                   VirtualPathUtility.ToAbsolute(virtualPath));
            return result;
        }
    }
}