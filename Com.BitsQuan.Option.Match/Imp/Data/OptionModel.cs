using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Com.BitsQuan.Option.Match.Imp.Share;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 期权交易系统在内存中的对象模型
    ///     运行中的期权交易系统数据交换只与此对象进行
    ///     此对象不负责数据的序列化
    /// </summary>
    public class OptionModel : Com.BitsQuan.Option.Match.Imp.IOptionModel
    {
        public IEnumerable<Coin> Coins { get { return initor.coins; } }


        public IEnumerable<Order> LegacyOrders { get { return legcy.Items; } }


        public IEnumerable<Trader> Traders { get { return initor.traders; } }
        public List<Deal> LatestDeals { get { return initor.LatestDeals; } }



        public IEnumerable<Contract> Contracts { get { return initor.contracts; } }

        AccountChangeHandler ars;
        SysAccountChangeHandler srs;
        public IDbModel DbModel { get; private set; }
        public LegacyOrders legcy { get { return initor.legcy; } }
        public ModelInitor initor;
        public OptionModel()
        {
            ars = new AccountChangeHandler();
            srs = new SysAccountChangeHandler();
            initor = new ModelInitor();

        }

        #region 初始化
        public void Init()
        {
            initor.Init();
            DbModel = new DbProvider(legcy);
           
            TraderService.OnAccountChanged += TraderService_OnAccountChanged;
            SystemAccount.Instance.OnSystemAccountChanged += Instance_OnSystemAccountChanged;

        }

        #endregion

        #region 序列化
        public void Flush()
        {
            ars.Flush();
            ars.Flush();
            DbModel.Flush();
            using (var db = new OptionDbCtx())
            {

                foreach (var v in Coins)
                {
                    var q = db.Set<Coin>().Where(a => a.Id == v.Id).FirstOrDefault();
                    if (q == null)
                    {
                        db.Set<Coin>().Add(v);
                    }
                }
                //保存合约的禁用
                foreach (var v in Contracts.Where(a => a.IsDel == true))
                {
                    var q = db.Set<Contract>().Where(a => a.Id == v.Id).FirstOrDefault();
                    if (q == null)
                    {
                        throw new ArgumentException("合约应该已保存,实际并未保存");
                    }
                    else
                    {
                        q.IsDel = true;
                    }
                }

                //保存用户资金账户的修改
                foreach (var v in Traders)
                {
                    {
                        var q = db.Set<Account>().Where(a => a.Id == v.Account.CacheAccount.BtcAccount.Id).FirstOrDefault();
                        if (q == null)
                        {
                            Singleton<TextLog>.Instance.Info(string.Format("数据保存:{0},{1}", v.Name, "用户现金账户应该已存在"));
                        }
                        else
                        {
                            q.Sum = v.Account.CacheAccount.BtcAccount.Sum;
                            q.Frozen = v.Account.CacheAccount.BtcAccount.Frozen;
                        }
                    }
                    {
                        var q = db.Set<Account>().Where(a => a.Id == v.Account.CacheAccount.CnyAccount.Id).FirstOrDefault();
                        if (q == null)
                        {
                            Singleton<TextLog>.Instance.Info(string.Format("数据保存:{0},{1}", v.Name, "用户现金账户应该已存在"));
                        }
                        else
                        {
                            q.Sum = v.Account.CacheAccount.CnyAccount.Sum;
                            q.Frozen = v.Account.CacheAccount.CnyAccount.Frozen;
                        }
                    }
                    {
                        var q = db.Set<BailAccount>().Where(a => a.Id == v.Account.BailAccount.Id).FirstOrDefault();
                        if (q == null)
                        {
                            Singleton<TextLog>.Instance.Info(string.Format("数据保存:{0},{1}", v.Name, "用户现金账户应该已存在"));
                        }
                        else
                        {
                            q.Sum = v.Account.BailAccount.Sum;
                            q.Frozen = v.Account.BailAccount.Frozen;
                            //q.MaintainCount = v.Account.BailAccount.MaintainCount;
                        }
                    }
                    var trader = db.Set<Trader>().FirstOrDefault(i => i.Id == v.Id);
                    if (trader != null)
                    {
                        trader.IsAutoAddBailFromCache = v.IsAutoAddBailFromCache;
                        trader.IsAutoSellRight = v.IsAutoSellRight;
                        trader.IsFrozen = v.IsFrozen;
                    }
                }
                db.SaveChanges();
            }

        }
        #endregion

        #region 接口实现

        #region create order trader contract coin ,remove contract
        /// <summary>
        /// 增加币种
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contractCode"></param>
        /// <param name="mainBailRatio"></param>
        /// <param name="mainBailTimes"></param>
        /// <returns></returns>
        public Coin AddCoin(string name, string contractCode, decimal mainBailRatio, decimal mainBailTimes)
        {
            Coin c = EntityFactory.AddCoin(name, contractCode, mainBailRatio, mainBailTimes);
            initor.coins.Add(c);
            return c;
        }


        public void RemoveContract(int c, decimal d)
        {
            try
            {
                var q = Contracts.Where(a => a.Id == c).FirstOrDefault();
                if (q == null) return;
                if (Contracts.Contains(q))
                {
                    initor.contracts.Remove(q);
                    using (OptionDbCtx db = new OptionDbCtx())
                    {
                        var con = db.Contracts.Find(c);
                        con.IsNotInUse = true;
                        con.ExcuteBasePrice = d;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "OptionModel.RemoveContract");
            }

        }
        /// <summary>
        /// 创建合约
        /// </summary>
        /// <param name="coinName"></param>
        /// <param name="exeTime"></param>
        /// <param name="exePrice"></param>
        /// <param name="positionType"></param>
        /// <param name="optionType"></param>
        /// <param name="target"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public Contract CreateContract(string coinName, DateTime exeTime, decimal exePrice, OptionType optionType,
            string target, decimal coinCount)
        {
            return EntityFactory.CreateContract(coinName, exeTime, exePrice, optionType, target, coinCount);
        }

        /// <summary>
        /// 创建委托
        /// </summary>
        /// <param name="who"></param>
        /// <param name="contract"></param>
        /// <param name="dir"></param>
        /// <param name="pos"></param>
        /// <param name="orderType"></param>
        /// <param name="policy"></param>
        /// <param name="count"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public Order CreateOrder(int who, int contract, TradeDirectType dir, OrderType orderType, OrderPolicy policy, int count, decimal price)
        {
            var w = Traders.Where(a => a.Id == who).FirstOrDefault();
            if (w == null) return null;
            var c = Contracts.Where(a => a.Id == contract).FirstOrDefault();
            if (c == null) return null;

            return EntityFactory.CreateOrder(w, c, dir, orderType, policy, count, price);
        }
        /// <summary>
        /// 创建交易用户
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Trader CreateTrader(string name)
        {
            return EntityFactory.CreateTrader(name);
        }
        #endregion
        #region sysaccoutchanged,accountchanged
        /// <summary>
        /// 系统资金账户操作流水记录
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        void Instance_OnSystemAccountChanged(decimal arg1, Trader arg2, Order arg3, decimal publicSum, decimal privateSum, SysAccountChangeType arg5, decimal bsum)
        {
            srs.Save(arg1, arg2, arg3, publicSum, privateSum, arg5, bsum);
            //如果是借款还款,则向用户的流水添加记录
            if (arg5 == SysAccountChangeType.还款 || arg5 == SysAccountChangeType.借款)
            {
                //记录个人借款记录,用于还款
                if (arg5 == SysAccountChangeType.借款)
                    arg2.RecordBorrow(arg1);
                //向个人账户添加向系统借款记录
                ars.Save(arg1, arg2, arg3, arg5, bsum);
            }
            if (arg5 == SysAccountChangeType.亏损分摊)
            {
                ars.Save(arg1, arg2, arg3, arg5, bsum);
            }
        }

        /// <summary>
        /// 保持用户资金账户的更新
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        void TraderService_OnAccountChanged(Trader arg1, decimal arg2, AccountChangeType arg3, bool arg4, string byWho, string orderDesc, decimal current, Account ac)
        {
            ars.Save(arg1, arg2, arg3, arg4, byWho, orderDesc, current, ac);
        }
        #endregion
        #region addorder,updateorder,addcontract,addtrader,updatetraderposition
        public void AddOrder(Order o)
        {
            DbModel.AddOrder(o);
        }
        public void UpdateOrder(Order o)
        {
            DbModel.UpdateOrder(o);
        }
        public void AddContract(Contract c)
        {
            initor.contracts.Add(c);
            DbModel.AddContract(c);
            SingletonWithInit<ContractService>.Instance.Flush();
        }
        public void AddTrader(Trader t)
        {
            initor.traders.Add(t);
            DbModel.AddTrader(t);
        }
        public void UpdateTraderPosition(Trader t, List<UserPosition> ups, bool isAdd)
        {
            DbModel.UpdateTraderPosition(t, ups, isAdd);
        }
        #endregion
        #endregion
        #region dispose,updatepartialorder
        public void Dispose()
        {
            initor.coins.Clear();
            initor.traders.Clear();
            initor.contracts.Clear();
            if (ars != null)
            {
                ars.Flush();
                ars.Dispose();
                ars = null;
            }
            if (srs != null)
            {
                srs.Flush();
                srs.Dispose();
                srs = null;
            }
            if (DbModel != null)
            {
                DbModel.Dispose(); DbModel = null;
            }
        }


        public void UpdatePartialOrder(Order o)
        {
            DbModel.UpdatePartialOrder(o);
        }
        #endregion
    }
}
