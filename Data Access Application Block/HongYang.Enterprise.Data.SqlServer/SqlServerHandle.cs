using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Data
{
    public class SqlServerHandle : DbHandleBase
    {
        /// <summary>
        ///初始化，需要传入连接字符串
        /// </summary>
        /// <param name="conStr">连接字符串</param>
        /// <returns></returns>
        public SqlServerHandle(string conStr) : base(conStr)
        {
        }

        /// <summary>
        /// 创建连接对象
        /// </summary>
        /// <returns></returns>
        protected override IDbConnection CreateConnection()
        {
            IDbConnection connection = new SqlConnection(ConnectionString);// System.Data.SqlClient
            return connection;
        }


        //根据sql分页
        const string PAGESQL = @"select top {0} * from(select (row_number() over({1})) as rowId,{2}) as Pt where Pt.rowId >{3} order by Pt.rowId";
        /// <summary>
        /// sqlserver版。获取分页查询sql，支持多语句查询，详见otherSql参数
        /// (这边只负责组合分页sql)
        /// </summary>
        /// <param name="selectSql">待分页的主体sql，不可包含order by。e.g. select * from Ta A Inner Join Tb B On A.Id=B.UserId where A.Id=100 </param>
        /// <param name="orderSql">分页依据的排序，order by在这边传入。e.g. order by A.Id</param>
        /// <param name="startPageIndex">起始页，第1页从0开始</param>
        /// <param name="resultsPerPage">每页记录数</param>
        /// <param name="otherSql">同时要执行的其它sql语句。比如需要同时返回表记录总数，可在这边传入" select count(1) from TbUser"。
        /// 其实就是支持多条sql执行，比如也可以传:select count(1) from TbUser;select * from TbOrder。即多条sql用分号隔开</param>
        /// <returns></returns>
        public override string GetPageSql(string selectSql, string orderSql, int startPageIndex, int resultsPerPage, string otherSql = "")
        {
            selectSql = CheckSql(selectSql);
            //selectSql开头包含select要去掉
            var sql = selectSql.TrimStart().Substring(6);
            sql = string.Format(PAGESQL, resultsPerPage, orderSql, sql, startPageIndex * resultsPerPage);
            if (!string.IsNullOrWhiteSpace(otherSql))
            {
                //其实就是支持多条sql执行，比如也可以传:select count(1) from TbUser;select * from TbOrder。即多条sql用分号隔开
                sql = sql + ";" + otherSql;
            }
            return sql;
        }

        const string TOPSQL = @"select top {0} {1}";
        /// <summary>
        /// sqlserver版。获取前几笔数据的sql
        /// </summary>
        /// <param name="selectSql">主体sql. e.g. select * from Ta A Inner Join Tb B On A.Id=B.UserId where A.Id=100 order by A.Id</param>
        /// <param name="top"></param>
        /// <returns></returns>
        public override string GetTopSql(string selectSql, int top)
        {
            selectSql = CheckSql(selectSql);
            //selectSql开头包含select要去掉
            var sql = selectSql.TrimStart().Substring(6);
            sql = string.Format(TOPSQL, top, sql);
            return sql;
        }

        /// <summary>
        /// sqlserver版。执行sql前对sql的处理事件。可以检查sql安全性，也可以按不同数据库sql写法做调整。
        /// </summary>
        public override string CheckSql(string strSql)
        {
            //sqlserver是用@，oracle的参数是用冒号
            return strSql.Replace(':', '@');//如果你们项目确定是用一种数据库写法，以后不会变，则直接return strSql;

            //if (如果有可疑sql(sqlserver版)) 则记录日志并抛异常
        }
    }
}
