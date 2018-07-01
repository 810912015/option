using Google.Authenticator;
using MessagingToolkit.QRCode.Codec;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Com.BitsQuan.Miscellaneous
{
    /// <summary>
    /// 比特币支付接口
    /// </summary>
    public interface IBtcPay
    {
        /// <summary>
        /// 支付完成事件:金额,地址
        /// </summary>
        event Action<decimal, string> OnSuccess;
        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="to">地址</param>
        /// <param name="amount">金额</param>
        /// <returns>支付操作是否成功</returns>
        bool Pay(string to, decimal amount);

        string GenerateAddress(string label);
    }

    public class FakeBtcPay:IBtcPay
    {

        public event Action<decimal, string> OnSuccess;

        public bool Pay(string to, decimal amount)
        {
            Task.Factory.StartNew(() => {
                Thread.Sleep(10 * 1000);
                if (OnSuccess != null)
                {
                    OnSuccess(amount, to);
                }
            });
            return true;
        }


        public string GenerateAddress(string label)
        {
            return "1HsuNyJ6QUnepAgZavznDM5PfAPQMGMTUt";
        }
    }

    public class QrImageMaker
    {
        public System.Drawing.Bitmap Make(string label)
        {
            QRCodeEncoder coder = new QRCodeEncoder();
            var r = coder.Encode("1HsuNyJ6QUnepAgZavznDM5PfAPQMGMTUt");
            return r;
        }

        public void A2()
        {
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            
        }
    }
}
