using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HongYang.Enterprise.Data
{
    public class SqliteHandle : DbHandleBase
    {
        #region 初始化必须
        /// <summary>
        /// 初始化，需要传入连接字符串
        /// </summary>
        /// <param name="conStr">连接字符串</param>
        /// <returns></returns>
        public SqliteHandle(string conStr) : base(conStr)
        {
            //DapperExtensions默认是用SqlServer的标记，这边使用Sqlite的特殊语法标记，需要在初始化时给予指定。
            //DapperExtensions是静态的，全局只能支持一种数据库 
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.SqliteDialect();
        }

        /// <summary>
        /// 创建连接对象
        /// </summary>
        /// <returns></returns>
        protected override IDbConnection CreateConnection()
        {
            //todo 可用Nuget下载支持Sqlite的程序包，然后就可以了。
            //IDbConnection connection = new SQLiteConnection(ConnectionString);// System.Data.SQLite
            //return connection;
            throw new Exception("//todo:使用前请先下载支持SQLite的程序包System.Data.SQLite");
        }
        #endregion
    }
}
