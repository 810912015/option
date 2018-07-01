using Com.BitsQuan.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Com.BitsQuan.Option.Ui
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“MarketService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 MarketService.svc 或 MarketService.svc.cs，然后开始调试。
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
    public class MarketService : IMarketSvc
    {
        MarketSvc ms;
        public MarketService()
        {
            ms = new MarketSvc(MvcApplication.OptionService, new ConnectionMgr<IMarketCallBack>(), new SubscribeMgr());
        }


        public void Login(string name, string pwd)
        {
            ms.Login(name, pwd);
        }

        public void Subscribe(List<int> contractIds, bool IsSubscribe)
        {
            ms.Subscribe(contractIds, IsSubscribe);
        }
    }
}
