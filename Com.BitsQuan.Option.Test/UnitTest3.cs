using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Miscellaneous;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class CExeRecHandlerTest : TradeBase
    {
        [TestMethod]
        public void TestMethod1()
        {
            RestoreDb();
            ms = new MatchService();
            CExeRecHandler cerh = new CExeRecHandler();
            var t = t1; 
            var r = cerh.CreateRecord(t, c, PositionType.义务仓, 10, 2000, true, 130);
            Assert.AreEqual(r.Id, 1);

            cerh.SaveRecord(t, c, PositionType.义务仓, 10, 2000, true, 130);
            cerh.Flush();
        }

        [TestMethod]
        public void Mail()
        {
            QQExMailSender qes = new QQExMailSender();
            qes.SendTo("810912015@qq.com", "helll", "it is a test");
            Assert.IsNotNull(qes);
        }
        [TestMethod]
        public void Sms()
        {
            var s = string.Format("尊敬的{0}，您好！{1}【比权网】", DateTime.Now.ToString("yyMMddHHmmss"), "金额错误");
            QQExMailSender qes = new QQExMailSender();
            qes.SendMassage2("15921462689", s);
            Assert.IsNotNull(qes);
        }
    }
}
