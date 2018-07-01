using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Com.BitsQuan.Option.Match
{
    public class KlineDataPoolInitor
    {
        public static void InitOption(KlineDataPool kdp, List<int> cids)
        {
            using (OptionDbCtx db = new OptionDbCtx())
            {
                var l = db.Ohlcs.Where(a => a.WhatId > 0 && cids.Contains(a.WhatId)).OrderByDescending(a => a.Id).Take(1000).ToList();
                l.Reverse();
                foreach (var v in l)
                {
                    kdp.Add(v);
                }
            }
        }

        public static void InitSpot(KlineDataPool kdp)
        {
            using (OptionDbCtx db = new OptionDbCtx())
            {
                var l = db.Ohlcs.Where(a => a.WhatId < 0).OrderByDescending(a => a.Id).Take(1000);
                foreach (var v in l)
                {
                    kdp.Add(v);
                }
            }
        }
    }

    public class KlineSvc : SvcImpBase,IDisposable
    {
        OhlcSaver ohlcSaver;
        IOptionModel Model;
        KlineDataPool kdp;
        public OhlcGenarator ohlcGen { get; private set; }

        public KlineSvc(IOptionModel Model)
        {
            this.Model = Model;
          
            ohlcGen = new OhlcGenarator();
            ohlcSaver = new OhlcSaver();
            kdp = new KlineDataPool((cid) =>
            {
                return Model.Contracts.Where(a => a.Id == cid && a.IsDel == false).FirstOrDefault();
            });
            
           
        }
        public void Init()
        {
            KlineDataPoolInitor.InitOption(kdp, Model.Contracts.Count() > 0 ? Model.Contracts.Select(a => a.Id).ToList() : new List<int>());
            OhlcMaker.OnOhlcMaked += OhlcMaker_OnOhlcMaked;
        }

        public IKlineData GetKlineDataByContractCode(string code)
        {
            var c = Model.Contracts.Where(a => a.Code == code).FirstOrDefault();
            if (c == null) return null;
            //return null;
            var r = kdp.GetByConctractCode(code);
            KlineDataDto rr;
            if (r == null)
            {
                rr = new KlineDataDto(code, c.Name);

            }
            else
            {
                rr = new KlineDataDto(r);
            }

            var m5 = ohlcGen.GetCurrent(c.Id, OhlcType.M5);
            if (m5 != null)
                rr.M5.Add(m5);

            var m15 = ohlcGen.GetCurrent(c.Id, OhlcType.M15);
            if (m15 != null)
                rr.M15.Add(m15);
            var m30 = ohlcGen.GetCurrent(c.Id, OhlcType.M30);
            if (m5 != null)
                rr.M30.Add(m30);
            var m60 = ohlcGen.GetCurrent(c.Id, OhlcType.M60);
            if (m60 != null)
                rr.M60.Add(m60);
            var m1440 = ohlcGen.GetCurrent(c.Id, OhlcType.M1440);
            if (m1440 != null)
                rr.M1440.Add(m1440);
            var m480 = ohlcGen.GetCurrent(c.Id, OhlcType.M480);
            if (m480 != null)
                rr.M480.Add(m480);
            return rr;
        }
        public List<double> GetLatestKline(string code, OhlcType type)
        {
            var c = Model.Contracts.Where(a => a.Code == code).FirstOrDefault();
            if (c == null) return new List<double>();
            var r = ohlcGen.GetCurrent(c.Id, type);
            return r;
        }
        void OhlcMaker_OnOhlcMaked(Ohlc obj)
        {
            kdp.Add(obj);
            ohlcSaver.Save(obj);
        }

        public void Dispose()
        {
            OhlcMaker.OnOhlcMaked -= OhlcMaker_OnOhlcMaked;
            if (ohlcSaver != null)
            {
                ohlcSaver.Dispose(); ohlcSaver = null;
            }
        }
    }
}
