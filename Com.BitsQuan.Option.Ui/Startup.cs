using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Com.BitsQuan.Option.Ui.Startup))]
namespace Com.BitsQuan.Option.Ui
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
