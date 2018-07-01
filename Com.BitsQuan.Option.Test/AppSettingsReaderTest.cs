using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Miscellaneous;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Test
{
    enum TestEnum { Value1, Value2 }
    [TestClass]
    public class AppSettingsReaderTest
    {
        [TestMethod]
        public void ReadTest()
        {
            Assert.AreEqual(AppSettings.Read<int>("CountPerMinuteLimit"), 10000);
            Assert.AreEqual(AppSettings.Read<double>("fuseSpanInMinutes"), 0.5);
            Assert.AreEqual(AppSettings.Read<string>("string"), "string_value");
            Assert.AreEqual(AppSettings.Read<TestEnum>("enum"), TestEnum.Value1);
            Assert.AreEqual(AppSettings.Read<TestEnum>("asdfas", TestEnum.Value2), TestEnum.Value2);
            Assert.AreEqual(AppSettings.Read<Guid>("guid"), Guid.Parse("2DE638A6-76C8-4443-8657-D2B97A120698"));
            Assert.AreEqual(AppSettings.Read<DateTime>("datetime"), DateTime.Parse("2015/04/23 14:17:13"));
            Assert.AreEqual(AppSettings.Read<DateTime>("datetime_fail", DateTime.Parse("2015/04/23 14:17:13")), DateTime.Parse("2015/04/23 14:17:13"));
            Assert.AreEqual(AppSettings.Read<bool>("bool_1"), true);
            Assert.AreEqual(AppSettings.Read<bool>("bool_2"), true);
            Assert.AreEqual(AppSettings.Read<bool>("bool_3", false), false);
        }
    }
}
