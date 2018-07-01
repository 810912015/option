using System.Web;
using System.Web.Optimization;

namespace Com.BitsQuan.Option.Ui
{
    public class BundleConfig
    {
        // 有关绑定的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/j1").Include(
                "~/Scripts/jq/jquery.1.11.2.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // 使用要用于开发和学习的 Modernizr 的开发版本。然后，当你做好
            // 生产准备时，请使用 http://modernizr.com 上的生成工具来仅选择所需的测试。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));


            bundles.Add(new ScriptBundle("~/bundles/option").Include(
                "~/Scripts/mustache.js",
                "~/Scripts/hs/highstock.src.js",
                "~/Scripts/jquery.signalR-2.1.2.js",
                //"~/signalr/hubs",
                "~/Scripts/trade/cookie.js",
                "~/Scripts/trade/ohlc.js",
                "~/Scripts/trade/deep.js",
                "~/Scripts/trade/model.js",
                "~/Scripts/trade/order.js",
                "~/Scripts/trade/sel.js",
                //"~/Scripts/trade/sclient.js",
                "~/Scripts/trade/option.js"
                ));
            
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            BundleTable.EnableOptimizations = true;
        }
    }
}
