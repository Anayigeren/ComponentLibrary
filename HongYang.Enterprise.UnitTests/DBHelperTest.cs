using System;
using System.Collections.Generic;
using System.Linq;
using HongYang.Enterprise.Data;
using HongYang.Enterprise.Data.DataEntity;
using HongYang.Enterprise.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace HongYang.Enterprise.UnitTests
{
    [TestClass]
    public class DBHelperTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            //log4net.config和appsettings.json拷贝到bin/Debug下
            LogHelper.LogInit(new DefaultLogAppenderHelper());
            string dbName = "Admin";

            OracleDataEntityDAL<Ep_log_message> dal = new OracleDataEntityDAL<Ep_log_message>(dbName);
             
            //ExecuteSQLHelper---------------------------------------------------
            string message = string.Empty;
            int resultCount =  DBHelper.ExecuteSQLHelper(dal.InsertSQL(new Ep_log_message()
                {
                    Logid = Guid.NewGuid().ToString(),
                    Sendfrom = "192.168.101.233",
                    Sendtime = DateTime.Now
                }), dbName, ref message);
            LogHelper.Write("DBHelper.ExecuteSQLHelper----\r\n影响行数：" + resultCount + message + "\r\n----");


            //参数化 ExecuteSQLHelper----------------------------------------------
            Ep_log_message log = new Ep_log_message()
            {
                Logid = Guid.NewGuid().ToString(),
                Sendfrom = "192.168.101.233",
                Sendtime = DateTime.Now
            };
            resultCount = DBHelper.ExecuteSQLHelper(dal.InsertSQLByParameter(new Ep_log_message()
                {
                    Logid = Guid.NewGuid().ToString(),
                    Sendfrom = "192.168.101.233",
                    Sendtime = DateTime.Now
                }), log, dbName, ref message);
            LogHelper.Write("参数化DBHelper.ExecuteSQLHelper----\r\n影响行数：" + resultCount + message + "\r\n----");


            //SqlHelper-----------------------------------------------------------
            var dataSet = JsonConvert.SerializeObject(DBHelper.SqlHelper(
                "select * from Ep_log_message where Sendto = '192.168.101.233'", dbName, ref message));
            LogHelper.Write("DBHelper.SqlHelper----\r\n" + dataSet + "\r\n----");


            //参数化 SqlHelper------------------------------------------------------
            dataSet = JsonConvert.SerializeObject(DBHelper.SqlHelper(
                "select * from Ep_log_message where Sendto =:Sendto",
                new { Sendto = "192.168.101.233" }, dbName, ref message));
            LogHelper.Write("DBHelper.SqlHelper----\r\n" + dataSet + "\r\n----");

            //Updateclob------------------------------------------------------------
            string clob = "为进一步贯彻落实我市扶持民营企业健康发展“四个千亿”有关政策，推进实施民营企业增债“一千亿”计划，深化民营企业知识产权保护体系建设，6月4日下午，市工商联会同市检察院、市高新投、市委统战部经济处举办推动“四个千亿”政策落实深圳市重点民营企业座谈会。会议邀请专家解读我市扶持民营企业健康发展“四个千亿”有关政策和作民营企业知识产权保护专题讲座。座谈会由市工商联党组成员、专职副主席谢振文主持，70家重点民营企业代表及有关部门同志参加会议，随手科技作为唯一一家金融科技类企业受邀参加了会议，公司法务总监吴昊和办公室主任姚勇作为公司代表参会并发言。";
            resultCount = DBHelper.Updateclob("Ep_log_message", "Content", "Sendto='192.168.101.233'", clob, dbName);
            LogHelper.Write("DBHelper.Updateclob----\r\n影响行数：" + resultCount + "\r\n----");

            //TranscationExecute-----------------------------------------------------
            List<string> list = new List<string>();
            list.Add(dal.InsertSQL(new Ep_log_message()
            {
                Logid = Guid.NewGuid().ToString(),
                Sendfrom = "192.168.101.233",
                Sendtime = DateTime.Now
            }));
            list.Add(dal.InsertSQL(new Ep_log_message()
            {
                Logid = Guid.NewGuid().ToString(),
                Sendfrom = "192.168.101.233",
                Sendtime = DateTime.Now
            }));
            bool ret = DBHelper.TranscationExecute(list, dbName);
            LogHelper.Write("DBHelper.TranscationExecute----\r\n" + ( ret ? "成功" : "失败" ) + "\r\n----");

            //PageHelper-----------------------------------------------------
            var data = DBHelper.PageHelper(
                new DataPage()
                {
                    PageSize = 5,
                    CurrentPage = 1
                },
                new Query()
                {
                    TableName = "Ep_log_message",
                    Field = "logid,sendfrom,sendtime",
                    Order = "sendtime desc",
                }, dbName, ref message);
            LogHelper.Write("DBHelper.PageHelper----\r\n" + JsonConvert.SerializeObject(data) + "\r\n----");

            //Query
            List<dynamic> dyList = DBHelper.QueryDynamics("select logid,sendfrom,sendtime from Ep_log_message where Sendfrom =:Sendfrom",
                new { Sendfrom = "192.168.101.233" }, dbName, ref message).ToList();
            LogHelper.Write("DBHelper.QueryDynamics----\r\n" + JsonConvert.SerializeObject(dyList) + "\r\n----");
        }
    }
}
