using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Imp
{
    public class CoinRepo:SingletonWithInit<CoinRepo>,IInitialbe
    {
       
        public Coin CNY { get; private set; }
        public Coin BTC { get;private  set; }
        public Coin LTC { get;private  set; }

        Dictionary<string, Coin> dic;
        public Coin GetByName(string name)
        {
            if (string.IsNullOrEmpty(name) || !dic.ContainsKey(name.ToUpper())) return null;
            return dic[name.ToUpper()];
        }
        public CoinRepo() { this.dic = new Dictionary<string, Coin>(); }
        public void Init()
        {
            using (var db = new OptionDbCtx())
            {
                foreach (var v in db.Set<Coin>().ToList())
                {
                    if (dic.ContainsKey(v.Name))
                    {
                        dic[v.Name] = v;
                    }
                    else dic.Add(v.Name, v);
                }
            }
            CNY =dic .ContainsKey ("CNY")? dic["CNY"]:null;
            BTC = dic.ContainsKey("BTC") ? dic["BTC"] : null;  
            LTC = dic.ContainsKey("LTC") ? dic["LTC"] : null;  
        }
    }
}
