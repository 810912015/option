using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Dto;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
namespace Com.BitsQuan.Option.Ui.Models
{
    public interface ITradeManager
    {
        System.Collections.Generic.List<DealDto> GetDealsInMarket(string cname);
        System.Collections.Generic.List<OrderDto> GetMyOrders(int tid);
        System.Collections.Generic.List<OrderDto> GetMyOrdersByTime_State(int tid, DateTime timeTop, DateTime timeEnd, string state, int pageIndex);
        System.Collections.Generic.List<PositionSummaryDto> GetMyPositions(int tid);
        D2Model GetOrdersInMarket(string contractCode,int count);
        OrderResultDto OrderIt(int uid, string code, Com.BitsQuan.Option.Core.OrderPolicy policy, decimal price, int count, Com.BitsQuan.Option.Core.TradeDirectType direct, Com.BitsQuan.Option.Core.OrderType openclose, string userOpId);
        MarketDto QueryMarket(string cname);
        Com.BitsQuan.Option.Imp.OperationResult Redo(int tid, int orderId);
        List<Contract> GetContracts();
        MyAccount GetMyAccount(int uid);
    }
}
