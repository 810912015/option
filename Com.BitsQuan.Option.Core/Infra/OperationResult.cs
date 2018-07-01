
using Com.BitsQuan.Option.Core;
using System.Runtime.Serialization;
namespace Com.BitsQuan.Option.Imp
{
    [DataContract]
    /// <summary>
    /// 操作结果
    /// </summary>
    public class OperationResult
    {
        
        public OperationResult() { }
      
        public OperationResult(int code, string desc) { this.ResultCode = code; this.Desc = desc; }
        [DataMember]
        public bool IsSuccess { get { return ResultCode == 0; } set { } }
        /// <summary>
        /// 操作代码
        /// </summary>
        [DataMember]
        public int ResultCode { get; set; }
        /// <summary>
        /// 结果描述
        /// </summary>
        [DataMember]
        public string Desc { get; set; }
        public static readonly OperationResult SuccessResult = new OperationResult { ResultCode = 0, Desc = "成功" };

        public override string ToString()
        {
            return string.Format("操作结果-代码:{0},描述:{1}", ResultCode, Desc);
        }
    }
    public class OrderResult : OperationResult
    {
        public string UserOpId { get; set; }
        public Order Order { get; set; }

        public override string ToString()
        {
            return string.Format("{0},用户操作id:{1},当前:{2}", base.ToString(), UserOpId, Order.ToShortString());
        }
    } 
}
