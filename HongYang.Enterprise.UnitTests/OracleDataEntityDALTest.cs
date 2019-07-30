using System;
using System.Collections.Generic;
using HongYang.Enterprise.Data;
using HongYang.Enterprise.Data.DataEntity;
using HongYang.Enterprise.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace HongYang.Enterprise.UnitTests
{
    [TestClass]
    public class OracleDataEntityDALTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            /*
             * log4net.config和appsettings.json拷贝到bin/Debug下
             * 
             * */
            LogHelper.LogInit(new DefaultLogAppenderHelper());

            //连接名称
            string dbName = "Admin";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            OracleDataEntityDAL<Ep_log_message> dal = new OracleDataEntityDAL<Ep_log_message>(dbName);

            #region CRUD
            //Delete
            int deleteCount = dal.Delete("0123456789");
            LogHelper.Write($"DAL.Delete：{deleteCount}条记录");

            //Insert
            int insertCount = dal.Insert(new Ep_log_message()
            {
                Logid = "0123456789",
                Content = "Hello",
                Msgid = "00001",
                Sendfrom = "127.0.0.1",
                Sendto = "127.0.0.1",
                Sendtime = DateTime.Now,
                Result = "0",
                Groupid = "123",
                Resulttime = DateTime.Now,
                Resultcontent = "resultMessage",
            });
            LogHelper.Write($"DAL.Insert：{insertCount}条记录");


            //Update
            int updateCount = dal.Update(new Ep_log_message()
            {
                Logid = "0123456789",
                Content = "Hello1111",
                Msgid = "00001",
                Sendfrom = "127.0.0.1",
                Sendto = "192.168.101.233",
                Sendtime = DateTime.Now,
                Result = "0",
                Groupid = "123",
                Resulttime = DateTime.Now,
                Resultcontent = "resultMessage",
            });
            LogHelper.Write($"DAL.Update：{updateCount}条记录");

            //Load
            LogHelper.Write(dal.Load("0123456789"));
            sb.AppendLine("------------DAL.Load:");
            sb.AppendLine(JsonConvert.SerializeObject(dal.Load("0123456789")));
            sb.AppendLine("------------\r\n");

            //List
            sb.AppendLine("------------DAL.List():");
            sb.AppendLine(JsonConvert.SerializeObject(dal.List()));
            sb.AppendLine("------------DAL.List(T):");
            sb.AppendLine(JsonConvert.SerializeObject(
                dal.List(new Ep_log_message()
                {
                    Sendto = "192.168.101.233"
                })));
            sb.AppendLine("------------\r\n");
            #endregion

            #region TranscationList
            //InsertList
            List<Ep_log_message> logs = new List<Ep_log_message>();
            for (int i = 0; i < 100; i++)
            {
                logs.Add(new Ep_log_message() {
                    Logid = Guid.NewGuid().ToString(),
                    Content = "Hello",
                    Msgid = "00001",
                    Sendfrom = "127.0.0.1",
                    Sendto = "127.0.0.1",
                    Sendtime = DateTime.Now,
                    Result = "0",
                    Groupid = "123",
                    Resulttime = DateTime.Now,
                    Resultcontent = "resultMessage",
                });
            }
            bool ret = dal.InsertList(logs);

            LogHelper.Write("DAL.InsertList：" + (ret ? "成功": "失败"));

            //UpdateList
            ret = dal.UpdateList(logs);
            LogHelper.Write("DAL.UpdateList：" + (ret ? "成功" : "失败"));
            #endregion

            #region GetDataTable
            //GetDataTable
            sb.AppendLine("------------DAL.GetDataTable(T):");
            sb.AppendLine(JsonConvert.SerializeObject(dal.GetDataTable(
                new Ep_log_message()
                {
                    Sendto = "192.168.101.233"
                })));
            sb.AppendLine("------------\r\n");
            #endregion

            #region SQL
            //SQL
            sb.AppendLine("------------SQL:");
            var log = new Ep_log_message()
            {
                Logid = "0123456789",
                Content = "Hello1111",
                Msgid = "00001",
                Sendfrom = "127.0.0.1",
                Sendto = "192.168.101.233",
                Sendtime = DateTime.Now,
                Result = "0",
                Groupid = "123",
                Resulttime = DateTime.Now,
                Resultcontent = "resultMessage",
            };
            sb.AppendLine("LoadSQL：" + dal.LoadSQL("0123456789"));
            sb.AppendLine("DeleteSQL:" + dal.DeleteSQL("0123456789"));
            sb.AppendLine("UpdateSQL：" + dal.UpdateSQL(log));
            sb.AppendLine("InsertSQL：" + dal.InsertSQL(log));
            sb.AppendLine("UpdateSQLByParameter：" + dal.UpdateSQLByParameter(log));
            sb.AppendLine("InsertSQLByParameter：" + dal.InsertSQLByParameter(log));
            sb.AppendLine("------------\r\n");
            LogHelper.Write(sb.ToString());
            #endregion
        }
    }
}
