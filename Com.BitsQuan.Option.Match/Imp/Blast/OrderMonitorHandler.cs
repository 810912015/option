using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 账户监视处理器:负责保存相关数据通知用户
    /// </summary>
    public class OrderMonitorHandler:IDisposable
    {
        OrderMonitor om;
        TraderMsgSaver tms;
      
        Action<string, string> sendToUserAct;
        static object tmSync=new object (); 
        public OrderMonitorHandler(OrderMonitor o,Action<string,string> sendToUserAct)
        {
            tms = new TraderMsgSaver();
          
            this.sendToUserAct = sendToUserAct;
            this.om = o;
            om.OnBailWarning += om_OnBailWarning;
            om.prepare.OnBailAutoCollected += preBlaster_OnBailAutoCollected;
            om.prepare.OnRedoed += preBlaster_OnRedoed;
            om.prepare.OnRedoing += preBlaster_OnRedoing;
           
        }
 

        TraderMsg CreateMsg(string traderName, string msg,string msgType)
        {
            lock (tmSync)
            {
                return new TraderMsg
                {
                    When = DateTime.Now,
                    MsgType = msgType,
                    Msg = msg,
                    Name = traderName,
                    Id = IdService<TraderMsg>.Instance.NewId()
                };
            }
        }

        void preBlaster_OnRedoing(string arg1, string arg2)
        {
            var tm = CreateMsg(arg1, arg2, "将要撤单");
            tms.Save(tm);
        }

        void preBlaster_OnRedoed(string arg1, string arg2)
        {
            var tm = CreateMsg(arg1, arg2, "已撤单");
            tms.Save(tm);
      
        }

        void preBlaster_OnBailAutoCollected(string arg1, decimal arg2)
        {
            var tm = CreateMsg(arg1, "自动转入金额:" + arg2, "保证金自动转入");
            tms.Save(tm);
      
        }

        void om_OnBailWarning(Trader arg1, decimal arg2)
        {

            var tm = CreateMsg(arg1.Name, "维持保证金率" + Math.Round(arg2, 2), "维持保证金率过低");
            tms.Save(tm);
            string msg=string.Format("{0}|维持保证金率过低,维持保证金率:{1}", DateTime.Now.ToString("HH:mm:ss.fff"),Math.Round(arg2,2)); 
            arg1.SendEmail(msg);
            arg1.SendMsg(msg);
        }

        public void Dispose()
        {
            if (om != null)
            {
                om.OnBailWarning -= om_OnBailWarning;
                om.prepare.OnBailAutoCollected -= preBlaster_OnBailAutoCollected;
                om.prepare.OnRedoed -= preBlaster_OnRedoed;
                om.prepare.OnRedoing -= preBlaster_OnRedoing;
               
                om.Dispose(); om = null;
            }
            if (tms != null)
            {
                tms.Dispose(); tms = null;
            }
          
            if (sendToUserAct != null)
            {
                sendToUserAct = null;

            }
        }
    }
}
