using Microsoft.VisualStudio.TestTools.UnitTesting;
using HongYang.Enterprise.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HongYang.Enterprise.Data.Connenction;
using HongYang.Enterprise.Data;

namespace HongYang.Enterprise.Logging.Tests
{
    [TestClass()]
    public class LogHelperTests
    {
        [TestMethod()]
        public void WriteTest()
        {
            LogHelper.LogInit(new CustomAppenderHelper());
            LogHelper.Write("本地文本日志记录");
            LogHelper.Write("本地文本日志记录-失败", LogLevel.Fatal);
            LogHelper.WriteDb(new Models.EPLogLoginEntity()
            {
                LogID = Guid.NewGuid().ToString(),
                DateTime = DateTime.Now,
                DepID = "cs"
            });
            var dbHelper = new DBHelper();
        }
    }
}