using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Dto;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Match.Spot;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Timers;
using System.Web.Http;

namespace Com.BitsQuan.Option.Ui.Controllers
{
    /// <summary>
    /// 交易接口
    /// </summary>
    public class TradeController : ApiController
    {
        static ITradeManager itm = new TradeManager();

        static IdManager im = new IdManager();

        /// <summary>
        /// 执行<paramref name="f"/>前根据用户id检查交易号是否存在，如果不存在则不执行<paramref name="f"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uid"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        OpResult<T> Handle<T>(int uid, Func<T> f)
        {
            try
            {
                var tid = im.GetTraderId(uid);
                if (tid == -1)
                {
                    return new OpResult<T> { Desc = "请首先登录", ResultCode = 2, Result = default(T) };
                }
                var r = f();
                return new OpResult<T> { Result = r, ResultCode = 0, Desc = "操作成功" };
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                return new OpResult<T> { Desc = "操作失败", Result = default(T), ResultCode = 1 };
            }
        }
        #region option
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns>登录结果:如果成功Result是大于0的整数,用做后续操作的用户标志(uid);否则为-1;</returns>
        [HttpGet]
        public OpResult<int> Login(string name, string pwd)
        {
            try
            {
                var r = im.LogIn(name, pwd);
                if (r == -1)
                    return new OpResult<int> { Desc = "登录失败", ResultCode = 1, Result = -1 };
                return new OpResult<int> { Desc = "登录成功", Result = r, ResultCode = 0 };
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "登录失败");
                return new OpResult<int> { Desc = "登录失败", ResultCode = 1, Result = -1 };
            }
        }
        /// <summary>
        /// 获取一个合约的成交记录
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <param name="cname">合约名称</param>
        /// <returns>合约的成交记录列表</returns>
        [HttpGet]
        public OpResult<List<DealDto>> GetDealsInMarket(int uid, string cname)
        {
            return Handle<List<DealDto>>(uid, () =>
            {
                return itm.GetDealsInMarket(cname);
            });

        }
        /// <summary>
        /// 获取自己的委托列表
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <returns>当前委托列表</returns>
        [HttpGet]
        public OpResult<List<OrderDto>> GetMyOrders(int uid)
        {
            return Handle<List<OrderDto>>(uid, () =>
            {
                var tid = im.GetTraderId(uid);
                return itm.GetMyOrders(tid);
            });

        }

        /// <summary>
        /// 获取自己的持仓列表
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <returns>当前持仓列表</returns>
        [HttpGet]
        public OpResult<List<PositionSummaryDto>> GetMyPositions(int uid)
        {
            return Handle<List<PositionSummaryDto>>(uid, () =>
            {
                var tid = im.GetTraderId(uid);
                return itm.GetMyPositions(tid);
            });

        }

        /// <summary>
        /// 撤销用户的所有未成交委托
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        public OpResult<int> RedoMyOrders(int uid)
        {
            return Handle<int>(uid, () =>
            {
                var trader  = MvcApplication.OptionService.Model.Traders.FirstOrDefault(_ => _.Id == im.GetTraderId(uid));
                if (trader != null)
                {
                    var items = trader.Orders().Items;
                    int c = 0;
                    foreach (var v in items)
                    {
                        MvcApplication.OptionService.RedoOrder(trader.Id, v.Id);
                        c++;
                    }
                    return c;

                }
                return 0;
            });
        }
        /// <summary>
        /// 获取一个合约的所有委托
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <param name="contractCode">合约代码</param>
        /// <returns>合约的所有委托</returns>
        [HttpGet]
        public OpResult<D2Model> GetOrdersInMarket(int uid, string contractCode)
        {
            return Handle<D2Model>(uid, () =>
            {
                return itm.GetOrdersInMarket(contractCode, 8);
            });
        }
        /// <summary>
        /// 下单(委托)
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <param name="code">合约代码</param>
        /// <param name="policy">交易策略</param>
        /// <param name="price">价格</param>
        /// <param name="count">数量</param>
        /// <param name="direct">买卖</param>
        /// <param name="openclose">开平</param>
        /// <param name="userOpId">用户操作id--用户自己定义</param>
        /// <returns>下单结果</returns>
        [HttpGet]
        public OpResult<OrderResultDto> OrderIt(int uid, string code, Core.OrderPolicy policy,
            decimal price, int count, Core.TradeDirectType direct,
            Core.OrderType openclose, string userOpId)
        {
            return Handle<OrderResultDto>(uid, () =>
            {
                var tid = im.GetTraderId(uid);
                var r = itm.OrderIt(tid, code, policy, price, count, direct, openclose, userOpId);
                return r;
            });

        }
        /// <summary>
        /// 查询一个合约的市场信息
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <param name="cname">合约名称</param>
        /// <returns>一个合约的当前市场信息</returns>
        [HttpGet]
        public OpResult<MarketDto> QueryMarket(int uid, string cname)
        {
            return Handle<MarketDto>(uid, () =>
            {
                return itm.QueryMarket(cname);
            });

        }

        [HttpGet]
        public OpResult<List<MarketDto>> AllMarket(int uid)
        {
            return Handle<List<MarketDto>>(uid, () =>
            {
                var r = MvcApplication.OptionService.MarketBoard.Board.Values.Select(a => new MarketDto(
                    a,
                    MvcApplication.OptionService.Matcher.Container.Get1PriceAndCount
                    )).ToList<MarketDto>();
                return r;
            });

        }
        /// <summary>
        /// 撤单
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <param name="orderId">委托编号</param>
        /// <returns>撤单结果</returns>
        [HttpGet]
        public OpResult<OperationResult> Redo(int uid, int orderId)
        {
            return Handle<OperationResult>(uid, () =>
            {
                var tid = im.GetTraderId(uid);
                return itm.Redo(tid, orderId);

            });

        }
        /// <summary>
        /// 撤单多个
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <param name="orderId">委托编号集合，用逗号隔开</param>
        [HttpGet]
        public OpResult<String> RedoMany(int uid, string orderIds)
        {
            return Handle<String>(uid, () =>
            {
                if (!String.IsNullOrWhiteSpace(orderIds))
                {
                    var tid = im.GetTraderId(uid);
                    foreach (var i in orderIds.Split(',').Select(_ => Convert.ToInt32(_)))
                    {
                        itm.Redo(tid, i);
                    }
                }
                return "";
            });

        }

        /// <summary>
        /// 获取所有合约
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <returns>合约列表</returns>
        [HttpGet]
        public OpResult<List<Contract>> GetContracts(int uid)
        {
            return Handle<List<Contract>>(uid, () =>
            {
                return itm.GetContracts();
            });

        }

        /// <summary>
        /// 获取自己的账户信息
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <returns>账户信息</returns>
        [HttpGet]
        public OpResult<MyAccount> GetMyAccount(int uid)
        {
            return Handle<MyAccount>(uid, () =>
            {
                var tid = im.GetTraderId(uid);
                return itm.GetMyAccount(tid);
            });

        }
        #endregion


        #region spot
        [HttpGet]
        /// <summary>
        /// 现货撤单
        /// </summary>
        /// <param name="id">用户标志</param>
        /// <param name="soId">要撤的单号</param>
        /// <returns>撤单是否成功</returns>
        public OpResult<bool> SpotRedo(int id, int soId)
        {
            return Handle<bool>(id, () =>
            {
                var r = MvcApplication.SpotService.Redo(soId);
                return r;
            });
        }
        [HttpGet]
        /// <summary>
        /// 现货下单
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <param name="coinId">虚拟币编号:必须为2(当前只提供比特币)</param>
        /// <param name="dir">买卖</param>
        /// <param name="pwd">交易密码</param>
        /// <param name="count">数量</param>
        /// <param name="price">价格</param>
        /// <returns>下单结果</returns>
        public OpResult<SpotOrderResultDto> SpotOrderIt(int uid, int coinId, Core.TradeDirectType dir, OrderPolicy policy,
           string pwd,
           decimal count, decimal price)
        {
            return Handle<SpotOrderResultDto>(uid, () =>
            {
                try
                {
                    if (count <= 0 || price <= 0)
                    {
                        return new SpotOrderResultDto { Desc = "价格或数量不能小于等于0", ResultCode = 101, Spot = null };
                    }
                    var tid = im.GetTraderId(uid);

                    var r = MvcApplication.SpotService.AddOrder(tid, coinId, dir, policy, count, price);
                    var rr = new SpotOrderResultDto { Desc = r.Desc, IsSuccess = r.IsSuccess, ResultCode = r.ResultCode, Spot = new SpotOrderDto(r.Spot) };
                    return rr;
                }
                catch (Exception e)
                {
                    Singleton<TextLog>.Instance.Error(e, "spotapi");
                    return new SpotOrderResultDto { Desc = "服务器错误", ResultCode = 100, Spot = null };
                }
            });
        }


        CoinOrderContainer GetBtcOrderContainer()
        {
            return MvcApplication.SpotService.Container.Get(
               MvcApplication.SpotService.Model.Model.Coins.Where(a => a.Id == 2).FirstOrDefault()
               );
        }
        SpotMarketItem Getbm()
        {
            return MvcApplication.SpotService.Market.Get(MvcApplication.SpotService.Model.Model.Coins.Where(a => a.Id == 2).FirstOrDefault());
        }
        [HttpGet]
        /// <summary>
        /// 获取比特币市场信息
        /// </summary>
        /// <param name="uid">用户标志</param>
        /// <returns></returns>
        public OpResult<BtcMarket> GetBtcMarket(int uid)
        {
            return Handle<BtcMarket>(uid, () =>
                {
                    var q = GetBtcOrderContainer();
                    var m = Getbm();
                    return new BtcMarket
                    {
                        NewBtc = m.NewestDealPrice == 0 ? BtcPrice.Current : m.NewestDealPrice,
                        S1Price = q.SellOrders.Count == 0 ? 0 : q.SellOrders[0].Price,
                        S1Count = q.SellOrders.Count == 0 ? 0 : q.SellOrders[0].Count,
                        B1Price = q.BuyOrders.Count == 0 ? 0 : q.BuyOrders[0].Price,
                        B1Count = q.BuyOrders.Count == 0 ? 0 : q.BuyOrders[0].Count

                    };
                });
        }
        #endregion
    }
    [DataContract]
    public class OpResult<T> : OperationResult
    {
        [DataMember]
        /// <summary>
        /// 操作结果包含的数据
        /// </summary>
        public T Result { get; set; }
    }

    public class IdManager : IDisposable
    {

        IdContainer ic;
        public IdManager()
        {
            ic = new IdContainer();
        }
        public int GetTraderId(int uid)
        {
            return ic.GetTraderId(uid);
        }
        public int LogIn(string name, string pwd)
        {
            ApplicationUser u;
            using (var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
            {
                u = um.Find(name, pwd);
            }

            if (u == null) return -1;
            var t = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == u.UserName).FirstOrDefault();
            if (t == null) return -1;
            var r = ic.Create(t.Id);
            return r;

        }

        public void Dispose()
        {
            ic.Dispose();
        }
    }
    /// <summary>
    /// 用于api用户的id管理
    /// </summary>
    public class UserIdItem
    {
        public int UserId { get; private set; }
        public int TraderId { get; private set; }
        public DateTime LastAccess { get; set; }
        public bool IsValid
        {
            get { return LastAccess.AddMinutes(5) >= DateTime.Now; }
        }
        public UserIdItem(int uid, int tid)
        {
            UserId = uid; TraderId = tid; LastAccess = DateTime.Now;

        }

        /// <summary>
        /// 更新最后访问时间
        /// </summary>
        public void Refresh() { LastAccess = DateTime.Now; }
        public UserIdItem() { }

    }
    /// <summary>
    /// userId管理器:如果20分钟没有反应,则删除此Id
    /// </summary>
    public class IdContainer : IDisposable
    {
        Dictionary<int, UserIdItem> dic;
        object dicLoc;
        Timer t;
        public IdContainer()
        {
            dic = new Dictionary<int, UserIdItem>();
            dicLoc = new object();
            t = new Timer();
            t.Interval = 5 * 60 * 1000;
            t.Elapsed += t_Elapsed;
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                t.Stop();
                var ks = dic.Keys.ToList();
                foreach (var v in ks)
                {
                    var i = dic[v];
                    if (!i.IsValid)
                    {
                        lock (dicLoc)
                            dic.Remove(i.UserId);
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
        }

        static int GenerateId()
        {
            var r = Math.Abs(Guid.NewGuid().GetHashCode());
            return (int)r;
        }
        /// <summary>
        /// 由交易员Id创建userId
        /// </summary>
        /// <param name="traderId"></param>
        /// <returns></returns>
        public int Create(int traderId)
        {
            int t = GenerateId();
            while (dic.ContainsKey(t))
                t = GenerateId();
            lock (dicLoc)
                dic.Add(t, new UserIdItem(t, traderId));
            return t;
        }
        /// <summary>
        /// 由登录id得到交易id;如果没有返回-1
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public int GetTraderId(int uid)
        {
            if (dic.ContainsKey(uid))
            {

                dic[uid].Refresh();
                return dic[uid].TraderId;
            }
            return -1;
        }

        public void Dispose()
        {
            t.Dispose();
        }
    }
}