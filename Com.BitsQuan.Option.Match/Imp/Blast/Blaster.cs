using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    
    /// <summary>
    /// 爆仓:系统强行平仓
    /// 爆仓器基类
    ///     含爆仓操作:权利仓爆仓,义务仓爆仓
    /// </summary>
    public abstract class Blaster
    {
        public static readonly TextLog Log = new TextLog("blaster.txt");
        protected Market Market;
        protected IMatch matcher;
        //爆仓维持保证金比率执行阀值
        protected decimal threshold { get { return SysPrm.Instance.MonitorParams.BlastMaintainRatio;} }
        /// <summary>
        /// 重新报价器:当熔断边界发生变化时重新报价
        /// </summary>
        ReorderCollection fehc;
        BlastSellor sellor;

        BlastExecutor executor;
        protected IOptionModel model { get; private set; }
        public Blaster(IMatch match,Market m,IOptionModel model)
        {
          
            this.Market = m;
            this.matcher = match;
            this.model = model;
            this.fehc = new ReorderCollection(this.matcher);

            this.sellor = new BlastSellor(this.positionType, Log, CreateSellOrder, RaiseSell, match);

            this.executor = new BlastExecutor(this.Market,this.CreateRecord,
                this.OnBlasting, this.OnBlasted, this.positionType, Log, sellor, CalRatio,fehc,(s)=>model.Contracts.Where(a=>a.Code ==s).FirstOrDefault());
        }
      
        protected abstract PositionType positionType { get; }
        #region events
        /// <summary>
        /// 爆仓开始事件:用户id,用户名,总金额, 需要的金额,类型,时间
        /// </summary>
        public event Action<BlastRecord> OnBlasting;
         /// <summary>
        /// 爆仓结束事件:用户id,用户名,总金额, 需要的金额,类型,时间
        /// </summary>
        public event Action<BlastRecord> OnBlasted;
        static BlastSaver bs = new BlastSaver();
        /// <summary>
        /// 生成爆仓记录
        /// </summary>
        /// <param name="t"></param>
        /// <param name="needed"></param>
        /// <returns></returns>
        BlastRecord CreateRecord(Trader t, decimal needed,bool IsStart)
        {
            var br = new BlastRecord
            {
                Id = IdService<BlastRecord>.Instance.NewId(),
                Trader = t,
                TraderId = t.Id,
                BailTotal = t.GetMaintain(Market),
                BlastType = this.positionType == PositionType.权利仓 ? (IsStart ? BlastType.开始强平权利仓 : BlastType.强平权利仓结束) : (IsStart ? BlastType.开始强平义务仓 : BlastType.强平义务仓结束),
                NeededBail = needed,
                StartTime = DateTime.Now
            };
            bs.Save(br);
            return br;
        }

        void RaiseBlasting(BlastRecord br) {

            foreach (var v in OnBlasting.GetInvocationList())
            {
                try
                {
                    var va = (Action<BlastRecord>)v;
                    va.BeginInvoke(br, null, null);
                }
                catch (Exception e)
                {
                    Singleton<TextLog>.Instance.Error(e, "OnBlasting");
                }
            }
        }
        void RaiseBlasted(BlastRecord br)
        {
            foreach (var v in OnBlasted.GetInvocationList())
            {
                try
                {

                    var va = (Action<BlastRecord>)v;
                    va.BeginInvoke(br, null, null);
                }
                catch (Exception e)
                {
                    Singleton<TextLog>.Instance.Error(e);
                }
            }
        }

        static BlastOperationSaver bos = new BlastOperationSaver();
        /// <summary>
        /// 平仓操作事件
        /// </summary>
        public event Action<BlasterOperaton> OnBlastSell;
        void RaiseSell(BlasterOperaton bo)
        {
            bos.Save(bo);
            if (OnBlastSell != null)
            {
                foreach (var v in OnBlastSell.GetInvocationList())
                {
                    try
                    {
                        ((Action<BlasterOperaton>)v).BeginInvoke(bo, null, null);
                    }
                    catch (Exception e)
                    {
                        Singleton<TextLog>.Instance.Error(e);
                    }
                }
            }
        }

        #endregion
        /// <summary>
        /// 爆仓结束清理
        /// 如果保证率已经达到要求,要把所有挂出的单子撤掉
        /// 这里只撤已熔断价格挂出的单子;不是以熔断价格挂出的单子每次挂之前都会检查保证率
        /// </summary>
        /// <param name="t"></param>
        public void Clear(Trader t)
        {
            //如果保证率已经达到要求,要把所有挂出的单子撤掉
            this.fehc.RedoAllMyOrder(t);
        }

       /// <summary>
       /// 爆仓
       /// </summary>
        /// <param name="t">要爆仓的用户</param>
        /// <returns>true表示保证率满足要求,false表示保证率不满足要求</returns>
        public bool Blast(Trader t)
        {
            var mr = t.GetMaintainRatio(Market);
            if (mr >= threshold)
            {
                return true;
            }
                 
            return executor.Blast(t);
        }


        /// <summary>
        /// 平仓操作的交易对象
        /// </summary>
        /// <param name="up"></param>
        /// <returns></returns>
        protected abstract IEnumerable<Order> GetPossibleMatch(PositionSummary up);

        /// <summary>
        /// 计算平仓比率:需要平仓的份数
        /// 每份合约得到的资金数=释放的维持保证金-买入价格
        /// </summary>
        /// <param name="up"></param>
        /// <returns></returns>
        decimal CalRatio(Trader t, PositionSummary up, decimal needed)
        {
            try
            {
                if (up == null)
                {
                    Log.Info(string.Format("计算比例时合约为空:仓{0}-人{1}",up.CName,t.Name));
                    return 0;
                }
                var cp = Market.Get(up.CName).NewestDealPrice;
                var pp = up.GetReleasePerPos((a) => { return model.Contracts.Where(c => c.Code == a).FirstOrDefault(); }, cp);
                var r = needed / pp;
                Log.Info(string.Format("平仓份数:{0}-{1}-价{2}-释{3}-份{4}", t.Name, up.CName, cp, pp, r));
                return r;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return 0;
            }
        }
        /// <summary>
        /// 创建委托
        /// </summary>
        /// <param name="up"></param>
        /// <param name="total"></param>
        /// <returns>委托,是否是以熔断价报出</returns>
        protected abstract Tuple<Order, bool> CreateSellOrder(Trader t, PositionSummary up, int total);
        protected ContractFuse GetFuser(PositionSummary up)
        {
            var f = Market.Get(up.CName);
            return f.fuser;
        }
        /// <summary>
        /// 将委托放入重新报价队列
        /// </summary>
        /// <param name="o"></param>
        /// <param name="cf"></param>
        /// <param name="isFuseMax">是否是上边界发生变化时重新报价</param>
        protected void PutInReorder(Order o, ContractFuse cf, bool isFuseMax,PositionSummary pos)// upId)
        {
            this.fehc.Add(o, cf, isFuseMax,pos);
        }
    } 
}
