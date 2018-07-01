using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Infra;
using Com.BitsQuan.Option.Provider;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Com.BitsQuan.Option.Imp
{
    /// <summary>
    /// 系统级参数服务:启动时从数据库读取,终止时更新数据库
    /// </summary>
    public class GlobalPrmService : IInitialbe
    {
        public GlobalPrmService() { Prms = new Dictionary<string, GlobalPrm>(); }
        Dictionary<string, GlobalPrm> Prms { get; set; }

        public int? GetInt(string key)
        {
            if (key == null) return null;
            if (!Prms.ContainsKey(key)) return null;
            var p = Prms[key];
            int i;
            if (int.TryParse(p.Value, out i)) return i;
            else return null;
        }
        public decimal? GetDecimal(string key)
        {
            if (key == null) return null;
            if (!Prms.ContainsKey(key)) return null;
            var p = Prms[key];
            decimal i;
            if (decimal.TryParse(p.Value, out i)) return i;
            else return null;
        }
        public string GetString(string key)
        {
            if (key == null) return null;
            if (Prms.ContainsKey(key))
            {
                return Prms[key].Value;
            }
            else return null;
        }
        public void Set(string key, string value)
        {
            if (key == null) return;
            if (Prms.ContainsKey(key)) Prms[key].Value = value;
            else
            {
                GlobalPrm p = new GlobalPrm { Name = key, Value = value };
                Prms.Add(key, p);
            }
        }


        public void Init()
        {
            using (var db = new OptionDbCtx())
            {
                foreach (var v in db.GlobalPrms)
                {
                    Prms.Add(v.Name, v);
                }
            }
        }
        public void Flush()
        {
            using (var db = new OptionDbCtx())
            {
                db.GlobalPrms.AddOrUpdate(Prms.Values.ToArray());
                db.SaveChanges();
            }
        }
    }
}
