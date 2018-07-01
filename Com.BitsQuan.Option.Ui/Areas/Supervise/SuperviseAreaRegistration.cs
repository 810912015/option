using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise
{
    public class SuperviseAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Supervise";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Supervise_default",
                "Supervise/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}