using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match;
using System.Collections.Generic;

namespace Com.BitsQuan.Service
{
    public class MarketSvc : TcpSvcBase<IMarketCallBack>, IMarketSvc
    {
        ISubscribeMgr smgr;
        public MarketSvc(MatchService ms, IConnectionMgr<IMarketCallBack> connmgr, ISubscribeMgr mgr)
        {
            this.srv = ms;
            this.connMgr = connMgr;
            this.smgr = mgr;
            this.log = new TextLog("marketsvc.txt");
        }
        
        public void Login(string name, string pwd)
        {
            Execute(() =>
            {
                var r = connMgr.Login(name, pwd, CurCallBack);
                CurCallBack.OnLogin(r.ResultCode, r.Desc);
            });
        }

        public void Subscribe(List<int> contractIds, bool IsSubscribe)
        {
            Execute(() =>
            {
                smgr.Subscribe(contractIds, IsSubscribe);
                CurCallBack.OnSubscribe(contractIds, IsSubscribe, OperationResult.SuccessResult);
            });
        }
    }
}
