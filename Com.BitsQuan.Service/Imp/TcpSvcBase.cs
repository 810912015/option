using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match;
using System;
using System.ServiceModel;

namespace Com.BitsQuan.Service
{
    public abstract class TcpSvcBase<T>
    {
        protected TextLog log { get; set; }
        protected MatchService srv { get; set; }
        protected IConnectionMgr<T> connMgr { get; set; }
        public void Execute(Action a, string logTag = "")
        {
            try
            {
                a();
            }
            catch (Exception e)
            {
                log.Error(e, logTag);
            }
        }
        protected T CurCallBack
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<T>();
            }
        }
    }
}
