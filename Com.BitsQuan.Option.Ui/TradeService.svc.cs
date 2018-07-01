using Com.BitsQuan.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Com.BitsQuan.Option.Ui
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“TradeService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 TradeService.svc 或 TradeService.svc.cs，然后开始调试。
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TradeService : ITrade
    {
        TradeSvc ts;
        public TradeService()
        {
            ts = new TradeSvc(MvcApplication.OptionService, new ConnectionMgr<ITradeCallBack>());
        }

        public void Login(string name, string pwd)
        {
            ts.Login(name, pwd);
        }

        public void QueryContract()
        {
            ts.QueryContract();
        }

        public void QueryOrder(int orderId)
        {
            ts.QueryOrder(orderId);
        }

        public void QueryPosition(List<string> contractCodes)
        {
            ts.QueryPosition(contractCodes);
        }

        public void AddOrder(int contract, Core.TradeDirectType dir, Core.OrderType orderType, Core.OrderPolicy policy, decimal count, decimal price, string userOpId = "")
        {
            ts.AddOrder(contract, dir, orderType, policy, count, price, userOpId);
        }

        public void RedoOrder(int orderId)
        {
            ts.RedoOrder(orderId);
        }
    }
}
