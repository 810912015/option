using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Provider;
using System.Collections.Generic;
using System.Linq;
using System;
namespace Com.BitsQuan.Option.Imp
{

    public class OrderRepo : BaseRepository<Order>
    {
        public OrderRepo(OptionDbCtx db) { this.dbContext = db; }

        public Order  CreateOrder(int who, int contract, TradeDirectType dir,OrderPolicy policy, int count,decimal price)
        {
            var w = dbContext.Set<Trader>().Where(a => a.Id == who).FirstOrDefault();
            if (w == null) return null;
            var c = dbContext.Set<Contract>().Where(a => a.Id == contract).FirstOrDefault();
            if (c == null) return null;
            Order o = new Order { Contract=c ,Trader=w, OrderPolicy=policy,
                                  Id = IdService<Contract>.Instance.NewId(),
             Count =count, Direction=dir,  Price=price , State=OrderState.等待中, OrderTime=DateTime.Now };
            return o;
        }
        public override bool Add(Order entity)
        {
            if(entity.Id==0)
                entity.Id = IdService<Order>.Instance.NewId();
            return base.Add(entity);
        }
        public override bool AddRanged(IEnumerable<Order> entities)
        {
            foreach (var v in entities)
            {
                v.Id = IdService<Order>.Instance.NewId();
            }
            return base.AddRanged(entities);
        }
    }
}
