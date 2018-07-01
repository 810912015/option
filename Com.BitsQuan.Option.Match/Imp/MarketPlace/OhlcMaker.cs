using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Linq;

namespace Com.BitsQuan.Option.Match.Imp
{

    /// <summary>
    /// 根据每个成交产生ohlc数据,当时间界限达到时引发ohlc生成事件     
    /// </summary>
    public class OhlcMaker
    {
        public Ohlc ohlc { get; private set; }
        int IntervalInMin;
        Timer t;
        bool isTimerFirstExecute;
        public OhlcMaker(int cid, OhlcType ot)
        {
            IntervalInMin = (int)ot;
            CreateOhlc(cid, ot);
            isTimerFirstExecute = true;
            t = new Timer();
            //首次启动时先找到最近的界限
            t.Interval = ohlc.WhenInDt.AddMinutes(IntervalInMin).Subtract(DateTime.Now).TotalMilliseconds;
            t.Elapsed += t_Elapsed;
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                t.Stop();
                if (OnOhlcMaked != null)
                {
                    Ohlc o = new Ohlc
                    {
                        WhatId = ohlc.WhatId,
                        WhenInDt = ohlc.WhenInDt,
                         OhlcType =ohlc.OhlcType,
                          Open =ohlc.Open, Close =ohlc.Close, High =ohlc.High, Low=ohlc.Low ,
                           Volume =ohlc.Volume, When=ohlc.When
                    };

                    OnOhlcMaked(o);
                }
                ohlc.SetTime(ohlc.WhenInDt.AddMinutes(IntervalInMin));
                ohlc.Open = ohlc.High = ohlc.Low = ohlc.Close;
                ohlc.Volume = 0;
                if (isTimerFirstExecute)
                {
                    //当找到界限后可以按固定时间引发事件了
                    t.Interval = IntervalInMin * 60 * 1000;
                    isTimerFirstExecute = false;
                }
                
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
            finally
            {
                t.Start();
            }
        }
        void CreateOhlc(int cid, OhlcType ot)
        {
            ohlc = new Ohlc(GetPreBoundForTime(ot, DateTime.Now),
               ohlc == null ? 0 : ohlc.Close, ohlc == null ? 0 : ohlc.Close, ohlc == null ? 0 : ohlc.Close, ohlc == null ? 0 : ohlc.Close, 0);
            ohlc.OhlcType = ot;
            ohlc.WhatId = cid;
        }
        /// <summary>
        /// ohlc产生事件:周期性产生,如每5分钟产生一次M5型ohlc
        /// </summary>
        public static event Action<Ohlc> OnOhlcMaked;
        /// <summary>
        /// 获取时间所在区间的上限:比如5分钟线,现在是7分,则返回5分时的时间
        /// </summary>
        /// <param name="ot">ohlc类型,值是区间值,如5分钟线的5分钟</param>
        /// <param name="dtn"></param>
        /// <returns></returns>
        public DateTime GetPreBoundForTime(OhlcType ot,DateTime dtn)
        {
            DateTime dtr;
            var m = (int)dtn.Subtract(dtn.Date).TotalMinutes % (int)ot;
            var t = dtn.AddMinutes(m * (-1));
            dtr = dtn.Date.AddHours(t.Hour).AddMinutes(t.Minute);
            return dtr;
        }
        public void Handle(IDeal d)
        {
            
            if (ohlc.Open == 0||d.When<ohlc.WhenInDt) ohlc.Open = (double)d.Price;
            if (ohlc.High == 0||(double)d.Price>ohlc.High) ohlc.High = (double)d.Price;
            if (ohlc.Low == 0||(double)d.Price<ohlc.Low) ohlc.Low = (double)d.Price;
            if (ohlc.Close == 0||d.When>=ohlc.WhenInDt) ohlc.Close = (double)d.Price;
            ohlc.Volume += (double)d.DealCount;
        }
    }

    public class OhlcGenarator
    {
        Dictionary<int, Dictionary<OhlcType,OhlcMaker>> dic; 
        public OhlcGenarator()
        {
            dic = new Dictionary<int, Dictionary<OhlcType, OhlcMaker>>(); 
        }
        public void Handle(IDeal d)
        {
            if (!dic.ContainsKey(d.WhatById))
            {

                if (!dic.ContainsKey(d.WhatById))
                {
                    var td=new  Dictionary<OhlcType, OhlcMaker>();
                            td.Add(OhlcType.M5, new OhlcMaker(d.WhatById,OhlcType.M5));
                            td.Add(OhlcType.M15, new OhlcMaker(d.WhatById, OhlcType.M15));
                            td.Add(OhlcType.M30, new OhlcMaker(d.WhatById, OhlcType.M30));
                            td.Add(OhlcType.M60, new OhlcMaker(d.WhatById, OhlcType.M60));
                            td.Add(OhlcType.M480, new OhlcMaker(d.WhatById, OhlcType.M480));
                            td.Add(OhlcType.M1440, new OhlcMaker(d.WhatById, OhlcType.M1440));
                         dic.Add(d.WhatById,td); 
                }
            }
            foreach (var v in dic[d.WhatById].Values)
                v.Handle(d);
        }

        public List<double> GetCurrent(int whatId, OhlcType type)
        {
            if (!dic.ContainsKey(whatId)) return null;
            var d = dic[whatId][type].ohlc;
            return d.List;
        }
    }
}
