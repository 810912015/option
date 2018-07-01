using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Match.Dto;
using Com.BitsQuan.Option.Match.Imp;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Service
{
 
    public class TradeSvc : TcpSvcBase<ITradeCallBack>, ITrade
    {
        public TradeSvc(MatchService ms, IConnectionMgr<ITradeCallBack> connmgr)
        {
            this.srv = ms;
            this.connMgr = connmgr;
            this.log = new TextLog("tradesvc.txt");
            ms.OnAfterMatch += ms_OnAfterMatch;
        }

        void ms_OnAfterMatch(Order obj)
        {
            throw new System.NotImplementedException();
        }
        public void Login(string name, string pwd)
        {
            Execute(() =>
            {
                var r = connMgr.Login(name, pwd, CurCallBack);
                CurCallBack.OnLogin(r.ResultCode, r.Desc);
                CurCallBack.ReceiveContract(this.srv.Model.Contracts.Select(a => new ContractDto(a)).ToList());
            });
        }

        public void QueryContract()
        {
            Execute(() =>
            {
                CurCallBack.ReceiveContract(srv.Model.Contracts.Select(a => new ContractDto(a)).ToList());
            }, "query contract");
        }

        public void QueryOrder(int orderId)
        {
            Execute(() =>
            {
                if (CurTrader == null)
                {
                    CurCallBack.OnOrder(null, new OperationResult { ResultCode = 1, Desc = "no user" });
                    return;
                }
                var q = CurTrader.Orders().GetById(orderId);
                if (q == null)
                {
                    CurCallBack.OnOrder(null, new OperationResult { ResultCode = 2, Desc = "无此编号的合约" });
                    return;
                }
                var qdto = new OrderDto(q);
                CurCallBack.OnOrder(qdto, new OperationResult { ResultCode = 0, Desc = "查询成功" });
            }, "query order");
        }
        Trader CurTrader
        {
            get { var trader = srv.Model.Traders.Where(a => a.Name == connMgr.CurUserName).FirstOrDefault(); return trader; }
        }
        public void QueryPosition(List<string> contractCodes)
        {
            Execute(() =>
            {
                if (CurTrader == null)
                {
                    CurCallBack.OnPosition(null, new OperationResult { ResultCode = 1, Desc = "no user" });
                    return;
                }
                var q = CurTrader.GetPositionSummaries()
                    .Where(a => contractCodes.Contains(a.CCode))
                    .Select(a => new PositionSummaryDto(a,
                    CurTrader.GetMarket().GetNewestPrice(a.Contract.Name), CurTrader.GetClosableCount(a))
                    ).ToList();
                CurCallBack.OnPosition(q, OperationResult.SuccessResult);
            }, "query position");
        }

        public void AddOrder(int contract, TradeDirectType dir, OrderType orderType, OrderPolicy policy, decimal count, decimal price, string userOpId = "")
        {
            Execute(() =>
            {
                var r = srv.AddOrder(CurTrader.Id, contract, dir, orderType, policy, (int)count, price, userOpId);
                CurCallBack.OnOrder(new OrderDto(r.Order), r);
            }, "add order");
        }

        public void RedoOrder(int orderId)
        {
            Execute(() =>
            {
                var o = CurTrader.Orders().GetById(orderId);
                var r = srv.RedoOrder(CurTrader.Id, orderId);
                CurCallBack.OnOrder(new OrderDto(o), r);
            });
        }
    }
}
