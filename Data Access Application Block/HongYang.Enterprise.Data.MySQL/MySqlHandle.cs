using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Data.MySQL
{
    public class MySqlHandle : DbHandleBase
    {
        #region 初始化必须
        /// <summary>
        ///初始化，需要传入连接字符串
        /// </summary>
        /// <param name="conStr">连接字符串</param>
        /// <returns></returns>
        public MySqlHandle(string conStr) : base(conStr)
        {
            //DapperExtensions默认是用SqlServer的标记，这边使用MySql的特殊语法标记，需要在初始化时给予指定。
            //DapperExtensions是静态的，全局只能支持一种数据库 
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.MySqlDialect();
        }

        /// <summary>
        /// 创建连接对象。子类需要重写，根据自己的数据库类型实例化自己的数据库连接对象(支持IDbConnection接口)。
        /// Dapper可以在所有Ado.net Providers下工作，包括sqlite, sqlce, firebird, oracle, MySQL, PostgreSQL and SQL Server。
        /// </summary>
        /// <returns></returns>
        protected override IDbConnection CreateConnection()
        {
            //throw new Exception("//todo:使用前请先下载支持Mysql的程序包MySql.Data");
            IDbConnection connection = new MySqlConnection(ConnectionString);
            return connection;
        }
        #endregion

        #region 如果你的系统会使用不同数据库类型，或者今后会更改数据库类型，则建议重写的以下相关方法。否则，忽视之。

        /// <summary>
        /// mysql版。获取分页查询sql，支持多语句查询，详见otherSql参数
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
            return null;//todo 留给志同道合的朋友写吧
        }

        /// <summary>
        /// mysql版。获取前几笔数据的sql
        /// </summary>
        /// <param name="selectSql">主体sql. e.g. select * from Ta A Inner Join Tb B On A.Id=B.UserId where A.Id=100 order by A.Id</param>
        /// <param name="top"></param>
        /// <returns></returns>
        public override string GetTopSql(string selectSql, int top)
        {
            return null;//todo 留给志同道合的朋友写吧
        }

        /// <summary>
        /// mysql版。执行sql前对sql的处理事件。可以检查sql安全性，也可以按不同数据库sql写法做调整。
        /// </summary>
        public override string CheckSql(string strSql)
        {
            //sqlserver、mysql是用@，oracle的参数是用冒号
            return strSql.Replace(':', '@');//如果你们项目确定是用一种数据库写法，以后不会变，则直接return strSql;

            //if (如果有可疑sql(mysql版)) 则记录日志并抛异常
        }

        #endregion
    }
}
