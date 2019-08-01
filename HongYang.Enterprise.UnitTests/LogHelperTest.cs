using System;
using System.Collections.Generic;
using System.Linq;
using HongYang.Enterprise.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace HongYang.Enterprise.UnitTests
{
    [TestClass]
    public class LogHelperTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            /*
             * log4net.config和appsettings.json拷贝到bin/Debug下
             * */

            LogHelper.LogInit();

            LogHelper.Write("123");
            LogHelper.Write("失败日志", LogLevel.Fatal);
            // 写数据库
            LogHelper.WriteDb(new Ep_log_message()
            {
                Logid = Guid.NewGuid().ToString(),
                Msgid = "m001",
                Groupid = "g001",
                Sendfrom = "192.168.101.233",
                Sendto = "127.0.0.1",
                Sendtime = DateTime.Now,
                Resulttime = DateTime.Now,
                Result = "1",
                Content = "消息内容",
                Resultcontent = "Resultcontent",
            });

            // 异步写入数据库
            LogHelper.WriteDbTask(new Ep_log_message()
            {
                Logid = Guid.NewGuid().ToString(),
                Msgid = "m001",
                Groupid = "111111111111",
                Sendfrom = "192.168.101.233",
                Sendto = "127.0.0.1",
                Sendtime = DateTime.Now,
                Resulttime = DateTime.Now,
                Result = "1",
                Content = "消息内容",
                Resultcontent = "Resultcontent",
            });
        }
    }
}
