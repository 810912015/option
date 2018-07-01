using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;

namespace Com.BitsQuan.Option.Match
{
    public abstract class SvcImpBase
    {
        public bool IsStoped { get; set; }
        protected T HandleError<T>(T failValue, Func<T> f)
        {
            try
            {
                if (IsStoped) return failValue;
                return f();
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                return failValue;
            }
        }
        protected OperationResult Operate(Func<OperationResult> f)
        {
            try
            {
                if (IsStoped) return new OperationResult { ResultCode = 401, Desc = "系统维护中,请稍后重试" };
                return f();
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                return new OperationResult { ResultCode = 400, Desc = "系统错误" };
            }
        }
        protected void LogError(Action a)
        {
            try
            {
                a();
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
            }
        }
    }
}
