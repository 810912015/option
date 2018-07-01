using Com.BitsQuan.Option.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Com.BitsQuan.Boss.Imp
{
    public class Result{
        public int Code{get;set;}
        public string Desc{get;set;}
        public string Thing{get;set;}
    }
    public abstract class OrderMaker
    {
        public TextLog Log { get; set; }
        public string Name { get; protected set; }
        public string Pwd { get; protected set; }
        public abstract Result OrderIt(string who, decimal count, decimal price, int dir, params string[] append);
        public abstract Result Redo(int oid, params string[] append);
    }
    public class LoginHelper
    {
        string bs = "http://192.168.10.69:801/";
        string login = "bqapi/Trade/Login?name={0}&pwd={1}";
        public int Login(string Name,string Pwd="123456")
        {
            var str = HttpExecutor.Get(string.Format(bs + login, Name, Pwd));
            JObject j = JObject.Parse(str);
            var n = (int)j.Property("Result").Value;
            return n;
        }
    }
    public class CHelper
    {
        string bs = "http://192.168.10.69:801/";
        string contracts = "bqapi/Trade/GetContracts?uid={0}";

        public List<Tuple<string, string, string>> ContractList { get; private set; }

        public CHelper()
        {
            ContractList = new List<Tuple<string, string, string>>();
        }
        public void Refresh()
        {

            var n = new LoginHelper().Login("hello1", "123456");
            var str2 = HttpExecutor.Get(string.Format(bs + contracts, n));
            JObject cs = JObject.Parse(str2);
            var cs2 = (JArray)cs.Property("Result").Value;
            var csr = cs2.Children();
            foreach (JObject v in csr)
            {
                ContractList.Add(Tuple.Create(
                    (string)v.Property("Id").Value,
                    (string)v.Property("Code").Value,
                    (string)v.Property("Name").Value
                    ));
            }
            
        }
    }
    public class OptionOrderMaker : OrderMaker
    {
        string bs = "http://192.168.10.69:801/"; 
        string market="bqapi/Trade/QueryMarket?uid={0}&cname={1}";
        string contracts = "bqapi/Trade/GetContracts?uid={0}";
        string order = "bqapi/Trade/OrderIt?uid={0}&code={1}&policy={2}&price={3}&count={4}&direct={5}&openclose={6}&userOpId={7}";
        string redo = "bqapi/Trade/Redo?uid={0}&orderId={1}";
        CHelper ch;
        int? uid;
        public OptionOrderMaker(CHelper ch,string userName,string pwd="123456")
        {
            this.Name = userName; this.Pwd = pwd;
            this.ch = ch;
            uid = null;
        }
       
        public override Result OrderIt(string who, decimal count, decimal price, int dir, params string[] append)
        {
            if (uid == null)
            {
                uid = new LoginHelper().Login(this.Name, this.Pwd);
            }
            var r=HttpExecutor.Get(string.Format(bs+order,(int)uid, 
                who,1,price,count,dir,2,1));
            return new Result { Code = 0, Desc = "", Thing = r };
        }

        public override Result Redo(int oid, params string[] append)
        {
            return new Result();
        }
    }
    public class SpotOrderMaker : OrderMaker
    {

        public override Result OrderIt(string who, decimal count, decimal price, int dir, params string[] append)
        {
            throw new NotImplementedException();
        }

        public override Result Redo(int oid, params string[] append)
        {
            throw new NotImplementedException();
        }
    }
    public abstract class MarketUpdator
    {
        public Dictionary<string, string> market { get;protected set; }

        public abstract decimal GetS1Count(string who);
        public abstract decimal GetS1Price(string who);
        public abstract decimal GetB1Count(string who);

        public abstract decimal B1Price(string who);
        public abstract decimal CurPrice(string who);
        /// <summary>
        /// 谁,哪个指标,变动后的值
        /// </summary>
        event Action<string, string,string> OnUpdated;
    }
    public class OptionMarket:MarketUpdator
    {

        public override decimal GetS1Count(string who)
        {
            throw new NotImplementedException();
        }

        public override decimal GetS1Price(string who)
        {
            throw new NotImplementedException();
        }

        public override decimal GetB1Count(string who)
        {
            throw new NotImplementedException();
        }

        public override decimal B1Price(string who)
        {
            throw new NotImplementedException();
        }

        public override decimal CurPrice(string who)
        {
            throw new NotImplementedException();
        }
    }
    public class SpotMarket : MarketUpdator
    {

        public override decimal GetS1Count(string who)
        {
            throw new NotImplementedException();
        }

        public override decimal GetS1Price(string who)
        {
            throw new NotImplementedException();
        }

        public override decimal GetB1Count(string who)
        {
            throw new NotImplementedException();
        }

        public override decimal B1Price(string who)
        {
            throw new NotImplementedException();
        }

        public override decimal CurPrice(string who)
        {
            throw new NotImplementedException();
        }
    }
    public class MotiveRobot
    {
        public List<string> tradeObjects;
        OrderMaker om;
        MarketUpdator mu;
        TextLog Log;
        Timer t;
        public MotiveRobot(OrderMaker om, MarketUpdator mu,List<string> tradeObjects,int intervalInSeconds=10)
        {
            this.tradeObjects = tradeObjects;
            this.om = om; this.mu = mu;
            Log = new TextLog(om.Name + ".txt");
            t = new Timer();
            t.Interval = intervalInSeconds * 1000;
            t.Elapsed += t_Elapsed;
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                t.Stop();
                //如果有1单则下对手单,否则按最新价加(-10,10)的随机数报单
                foreach (var v in tradeObjects)
                {
                    decimal count = 0;
                    decimal price = 0;
                    int dir = 0;
                    if (mu.GetS1Count(v) > 0)
                    {
                        count= mu.GetS1Price(v);
                        price = mu.GetS1Price(v);
                        dir = 1;
                    }
                    else if (mu.GetB1Count(v) > 0)
                    {
                        count = mu.GetB1Count(v); price = mu.GetB1Count(v); dir = 2;
                    }
                    else
                    {
                        count = new Random(DateTime.Now.Millisecond).Next(0, 10);
                        price = new Random(DateTime.Now.Millisecond).Next(-10, 10);
                        dir = new Random(DateTime.Now.Millisecond).Next(1, 3);
                    }
                    om.OrderIt(v, count, price, dir, "");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                t.Start();
            }
        }
        
    }

    public class HttpExecutor
    {
        public static string Get(string Url)
        {
            try
            {
                CookieContainer cc = new CookieContainer();
                System.Net.HttpWebRequest wReq = (HttpWebRequest)System.Net.WebRequest.Create(Url);
                wReq.Method = "GET";
                wReq.CookieContainer = cc;
                System.Net.WebResponse wResp = wReq.GetResponse();
                System.IO.Stream respStream = wResp.GetResponseStream();
                string r = "";
                using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream, Encoding.UTF8))
                {
                    r = reader.ReadToEnd();
                }
                return r;
            }
            catch (Exception e) { throw e; }
        }
    }

    public class OptionMotiveRobot
    {
        
    }
    public class SpotMotiveRobot
    {

    }
}
