using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Text;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{ 
    /// <summary>
    /// 熔断器
    /// </summary>
    public class ContractFuse:IContractFuse
    {
        public static double FuseSpanInMin = MatchParams.FuseSpanInMinutes;
        public static readonly TextLog Log = new TextLog("fuse.txt");
        public FuseExcutor Excutor { get; private set; }
        FuseBoundary boundary; 
        public ContractFuse(Contract c)
        {
            
            this.Contract = c;
            startTime = null;
            boundary = new FuseBoundary(c,FuseSpanInMin);
            boundary.Extrem.OnExtremChanged += extrem_OnExtremChanged;
            MaxPrice = boundary.CalMaxPrice();
            MinPrice = boundary.CalMinPrice(); 
            Excutor = new FuseExcutor(
                () => { return (decimal)MaxPrice; },
                () => { return (decimal)MinPrice; },
                (a) => { Log.Info(string.Format("熔断结束无成交,改动最大值,旧{0},新{1}",MaxPrice,a)); 
                    MaxPrice = a; },
                (a) => { Log.Info(string.Format("熔断结束无成交,改动最小值,旧{0},新{1}",MinPrice,a)); MinPrice = a; },
                (a) => { startTime = a; },
                () =>
                {
                    var emax = boundary.Extrem.MaxIn5Min;
                    var emin = boundary.Extrem.MinIn5Min;
                    var nmax=boundary.CalMaxPrice();
                    var nmin=boundary.CalMinPrice();
                    Log.Info(string.Format("熔断结束有成交,大新{0}-旧{1},小新{2}-旧{3},5分钟最大值{4}-最小值{5}",MaxPrice,nmax,MinPrice,nmin,emax,emin));
                    MaxPrice = nmax;                  
                    MinPrice = nmin;
                }
                );
        }

        public ContractFuse(Contract c ,decimal max,decimal min)
            : this(c)
        {
            this.MaxPrice = max; this.MinPrice = min;
        }

        void extrem_OnExtremChanged(decimal arg1, decimal arg2)
        {
            if (IsFusing)
            {
                Log.Info(string.Format("熔断中边界值变化,不改变边界.5分钟大{0}-小{1},边界大{2}-小{3},熔断结束还有{4}秒", arg1, arg2,maxp,minp,RemainInSeconds));
                return;
            }

            MaxPrice = boundary.CalMaxPrice();
            MinPrice = boundary.CalMinPrice();
            Log.Info(string.Format("5分钟边界值变化:{0}-大{1}-小{2}-计算后-大{3}-小{4},5分钟大{5}-小{6}", this.Contract.Code, arg1, arg2,MaxPrice,MinPrice));
        }

        public bool IsFusing
        {
            get
            {
                if (Excutor.isExecuting) return true;
                return RemainInSeconds > 0;
            }
        }
        public FuseType FuseType { get; private set; }
        /// <summary>
        /// 熔断开始时间
        ///     如果没有熔断则为null
        /// </summary>
        DateTime? startTime; 
        public int RemainInSeconds
        {
            get
            {
                if (startTime == null) return 0;
                return (int)((DateTime) startTime).AddMinutes(FuseSpanInMin)
                    .Subtract(DateTime.Now).TotalSeconds;
            }
        }

        
        public event Action<Contract,decimal?> OnMaxChanged;
        public event Action<Contract, decimal?> OnMinChanged;
        decimal? maxp;
        public decimal? MaxPrice
        {
            get
            {
                return maxp;
            }
            private set
            {
                var rv = Math.Round((decimal)value, 2);
                
                if (maxp != rv)
                {
                    //Log.Info(string.Format("最大值变化:{0}-原{1}-新{2}", Contract.Code, maxp, rv));
                    maxp = rv;
                    if (OnMaxChanged != null)
                    {
                        foreach (var v in OnMaxChanged.GetInvocationList())
                        {
                            try
                            {
                                ((Action<Contract, decimal?>)v)(this.Contract, maxp);
                            }
                            catch(Exception e)
                            {
                                Singleton<TextLog>.Instance.Error(e, "fuse setmax");
                                continue;
                            }
                        }
                            
                    }
                }
            }
        }
        decimal? minp;
        public decimal? MinPrice { get { return minp; } 
            private set {
            var rv = Math.Round((decimal)value, 2);
            if (minp != rv)
            {
                //Log.Info(string.Format("最小值变化:{0}-原{1}-新{2}", Contract.Code, maxp, rv));
                minp = rv;
                if (OnMinChanged == null) return;
                foreach (var v in OnMinChanged.GetInvocationList())
                {
                    try
                    {
                        ((Action<Contract, decimal?>)v)(this.Contract, minp);
                    }
                    catch (Exception e)
                    {
                        Singleton<TextLog>.Instance.Error(e, "fuse set min");
                        continue;
                    }
                }
            }
        } }

        public Contract Contract { get; private set; }
        //    2.11熔断机制
        //为应对价格剧烈波动，期权价格发生剧烈波动时启动熔断机制。
        //熔断时间为5分钟，5分钟后恢复正常交易。
        //上涨熔断机制启动时，系统不接受大于熔断价格的报价，只接受小于等于熔断价格的买平委托和卖开委托，不接受买开委托和卖平委托，
        //已进入系统的买开委托和卖平委托继续驻留委托队列。
        //下跌熔断机制启动时，系统不接受小于熔断价格的报价，只接受大于等于熔断价格的卖平委托和买开委托，不接受买平委托和卖开委托，
        //已进入系统的买平委托和卖开委托继续驻留委托队列。
        //超出上涨范围:只接受卖单
        //超出下跌范围:只接受买单
        //认购期权，当盘面价格5分钟内触及max（5分钟内最低价的2倍，5分钟内最低价+10%X5分钟内比特币最低价），启动上涨熔断机制。
        //认购期权，当盘面价格5分钟内触及max（5分钟内最高价的1/2，5分钟内最高价-10%X5分钟内比特币最高价），启动下跌熔断机制。
        //认沽期权，当盘面价格5分钟内触及max（5分钟内最低价的2倍，5分钟内的最低价+10%X行权价），启动上涨熔断机制。
        //认沽期权，当盘面价格5分钟内触及max（5分钟内最高价的1/2，5分钟内最高价-10%X行权价），启动下跌熔断机制。
        //认购期权价格在比特币价格5%以上时启动熔断机制跟踪。
        //期权价格在行权价的5%以下时不启动熔断机制跟踪。比如，比特币的认购期权行权价为2000，则认购期权价格在100元以下不启动熔断跟踪机制，
        //即使认购期权的价格在10分钟以内由0.01涨到100元也不启动熔断机制。
      
        public bool ShouldAccept(Order o)
        {
            if (o.IsMarketPrice()) return true;
            if (!IsFusing)
            {
                if (o.Price >= MaxPrice)
                {
                    if (o.Direction == TradeDirectType.买)
                    {
                        if (MaxPrice != null)
                        {
                            Log.Info(string.Format("报价大于上限熔断:{0}-上限{1}-{2}", Contract.Code, MaxPrice, o.ToShortString()));
                            o.Price = (decimal)MaxPrice;
                            FuseType = FuseType.上涨熔断;

                            StartFuse();
                            RecordIt(o.Price);
                        }
                            
                    }
                    
                }
                if (o.Price <= MinPrice)
                {
                    if (o.Direction == TradeDirectType.卖)
                    {
                        if (MinPrice != null)
                        {
                            Log.Info(string.Format("报价小于下限熔断:{0}-限{1}-{2}", Contract.Code, MinPrice, o.ToShortString()));
                            o.Price = (decimal)MinPrice;
                            FuseType = FuseType.下跌熔断;
                            StartFuse();
                            RecordIt(o.Price);
                        }
                    }
                    
                }
                return true;
            }
            bool isAllow = true;
            if (o.Price > MaxPrice)
            {
                if (o.Direction == TradeDirectType.卖)
                {
                    isAllow = true;
                }
                else isAllow = false;
            }
            if (o.Price < MinPrice)
            {
                if (o.Direction == TradeDirectType.买)
                {
                    isAllow = true;
                }
                else isAllow = false;
            }
           
            return isAllow;
        }

        static FuseSaver fs = new FuseSaver();
        void RecordIt(decimal price)
        {
            lock (fs)
            {
                var fr = new FuseRecord
                {
                    Id = IdService<FuseRecord>.Instance.NewId(),
                    Contract = this.Contract,
                    ContractId = this.Contract.Id,
                    FuseType = this.FuseType,
                    MaxPrice = this.MaxPrice ?? 0,
                    MinPrice = this.MinPrice ?? 0,
                    Price = price,
                    StartTime = startTime ?? DateTime.Now
                };
                fs.Save(fr);
            }
        }
        public static decimal MinFuseRatioOfBtcPrice = 0.03m;
        public void Handle(Deal d)
        {
            if (!IsFusing)
            {
                if (d.Price >= MaxPrice)
                {
                    Log.Info(string.Format("成交价大于上限熔断:{0}-上限{1}-{2}", Contract.Code, MaxPrice, d.Price));
                    FuseType = FuseType.上涨熔断;

                    StartFuse();
                    RecordIt(d.Price);
                }
                else if (d.Price <= MinPrice)
                {
                    Log.Info(string.Format("成交价小于下限熔断:{0}-上限{1}-{2}", Contract.Code, MinPrice, d.Price));
                    FuseType = FuseType.下跌熔断;
                    StartFuse();
                    RecordIt(d.Price);
                }
            }
            if (Excutor != null) Excutor.HasDealsWhenFusing = true;
            boundary.Extrem.Put(d.Price);
        }

        void StartFuse()
        {

            Excutor.Start((decimal)MaxPrice, (decimal)MinPrice,this.FuseType);
        }
        /// <summary>
        /// 是否允许交易进行:任何时候不能超出价格范围
        ///     返回:0 表示在范围内
        ///          1 表示超出上涨范围
        ///          2 表示超出下跌范围
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>

        public int ShouldAllowDeal(decimal price)
        {
            if(MinPrice==null||MaxPrice==null) return 0;
            if (price > MaxPrice) return 1;
            if (price < MinPrice) return 2;
            return 0; 
        }
    }
}
