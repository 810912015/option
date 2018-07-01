using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Miscellaneous;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class EmailSenderTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            QQExMailSender qes = new QQExMailSender();
            qes.SendTo("18073314832@163.com", "hello" + DateTime.Now.ToString(), "content" + DateTime.Now.ToString());
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void Sms_test()
        {
            //SmsSender ss = new SmsSender();
            //ss.Send(new System.Collections.Generic.List<string> { "15921462689" }, "hello");
            SmsSender1 ss1 = new SmsSender1();
            ss1.Test();
        }

        //短信测试
        [TestMethod]
        public void Mas_test()
        {
            QQExMailSender qes = new QQExMailSender();
          //  qes.SendMassage();
        }
    }
}
