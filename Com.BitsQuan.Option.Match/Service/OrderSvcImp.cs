using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    public class OrderSvcImp : SvcImpBase
    { 
        IOptionModel Model;
        OrderPreHandler preHandler;
        OrderPostHandler postHandler;
        OrderMatcher Matcher;
        TextLog log;
        Func<Order, bool> IsAcceptByFuse;
        public OrderSvcImp(IOptionModel Model,
        OrderPreHandler preHandler,
        OrderPostHandler postHandler,
        OrderMatcher Matcher,
        TextLog log,
        Func<Order, bool> IsAcceptByFuse)
        {
            this.Model = Model; this.preHandler = preHandler; this.postHandler = postHandler;
            this.Matcher = Matcher; this.log = log; this.IsAcceptByFuse = IsAcceptByFuse;
        }
        public OrderResult AddOrder(int who, int contract, TradeDirectType dir, OrderType orderType, OrderPolicy policy, int count, decimal price,
         string userOpId = "")
        {
            if (IsStoped) return new OrderResult { ResultCode = 330, Desc = "系统维护中,请稍后重试" };
            var o = Model.CreateOrder(who, contract, dir, orderType, policy, count, price);
            if (o == null) return new OrderResult { ResultCode = 10, Desc = "不能创建委托,请检查参数", UserOpId = userOpId };
            o.State = OrderState.等待中;

            if (!IsAcceptByFuse(o))
            {
                return new OrderResult
                {
                    UserOpId = userOpId,
                    Desc = "价格不能超出熔断范围",
                    Order = o,
                    ResultCode = 401
                };
            }
            var r = preHandler.CouldOrder(o);
            if (r.ResultCode == 10)
            {
                log.Info(string.Format("异常:人{0}-约{1}-向{2}-开{3}-策{4}-量{5}-价{6}-{7}", who, contract, dir, orderType, policy, count, price, userOpId));
                return new OrderResult
                {
                    UserOpId = userOpId,
                    Desc = "下单未知错误",
                    Order = o,
                    ResultCode = 401
                };
            }
            log.Info(string.Format("下单前检查=={0}=={1}", o, r));
            if (r.IsSuccess)
            {
                var br = postHandler.Handle(o);
                if (!br)
                {
                    return new OrderResult { UserOpId = userOpId, Desc = "保证金不足", Order = o, ResultCode = 5 };
                }
                log.Info(string.Format("下单后处理=={0}=={1}", o, r));
                Matcher.Handle(o);
                log.Info(string.Format("撮合后=={0}=={1}", o, r));
            }
            return new OrderResult { UserOpId = userOpId, Desc = r.Desc, Order = o, ResultCode = r.ResultCode };

        }
        public OperationResult RedoOrder(int who, int orderId)
        {
            return Operate(() =>
            {
                var t = Model.Traders.Where(a => a.Id == who).FirstOrDefault();
                if (t == null) return OperationResult.SuccessResult;

                var o = t.Orders().GetById(orderId);
                if (o == null) return OperationResult.SuccessResult;
                Matcher.Redo(o);
                log.Info(string.Format("撤单后=={0}=={1}", o, OperationResult.SuccessResult));
                return OperationResult.SuccessResult;
            });
        }
    }
}
