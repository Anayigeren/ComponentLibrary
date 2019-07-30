using System;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using HongYang.Enterprise.Data.AdoNet;
using HongYang.Enterprise.Logging;
using HongYang.Enterprise.Data.DataEntity;
using Dapper;

/// <summary>
/// 创 建 人：林志斌
/// 创建时间：2019/07/22
/// 功    能：数据库操作帮组类，基于Dapper封装
/// </summary>
namespace HongYang.Enterprise.Data
{
    /// <summary>
    /// 数据操作帮助类
    /// 可以封装使用部分类允许其他一些类扩展DBHelper
    /// </summary>
    [Serializable]
    public partial class DBHelper
    {
        /// <summary>
        /// 默认超时时间(秒数)
        /// </summary>
        public const int DEFAULTTIMEOUT = 60;

        /// <summary>
        /// 当超过这个监控阈值，记录日志。
        /// 将所有堆栈函数，全部记录下来
        /// </summary>
        public static int _traceDBRowLength = 5000;

        #region SqlHelper
        /// <summary>
        /// 查询处理(Sql语句)
        /// </summary>
        /// <param name="sql">查询SQL语句</param>
        /// <param name="dataBaseName">连接数据名</param>
        /// <param name="returnMessage">返回异常信息</param>
        /// <returns>返回DataSet类型</returns>
        public static DataSet SqlHelper(string sql, string dataBaseName, ref string returnMessage)
        {
            return SqlHelper(sql, null, dataBaseName, ref returnMessage);
        }

        /// <summary>
        /// 参数化查询处理（Sql语句）
        /// eg: DBHelper.SqlHelper("select name from student where id =: ID", new{ ID = "1" }, "admin", ref message);
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="param">动态替换参数</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="track">是否记录本地日志</param>
        /// <returns></returns>
        public static DataSet SqlHelper(
            string sql,
            dynamic param,
            string dataBaseName,
            Msg msg = null,
            DBTrack track = DBTrack.Open)
        {
            try
            {
                msg = msg ?? new Msg();
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                string returnMessage = string.Empty;
                var data = SqlHelper(sql, param, dataBaseName, ref returnMessage, track);
                msg.AddMsg(returnMessage);
                msg.Result = data != null;
                return data;
            }
            catch (Exception ex)
            {
                msg.Result = false;
                msg.AddMsg(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// 参数化查询处理（Sql语句）
        /// eg: DBHelper.SqlHelper("select name from student where id =: ID", new{ ID = "1" }, "admin", ref message);
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="param">动态替换参数</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="cmdTimeout">超时时间（秒）</param>
        /// <param name="tran"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public static DataSet SqlHelper(
            string sql,
            dynamic param,
            string dataBaseName,
            ref string returnMessage,
            DBTrack track = DBTrack.Open,
            int? cmdTimeout = DEFAULTTIMEOUT,
            IDbTransaction tran = null,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.ExecuteDataSet(sql, param, tran, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                if (track == DBTrack.Open)
                {
                    LogHelper.Write("DBHelper.SqlHelper方法异常：\n" + ex.Message
                        + $"{StackTraceLog.GetStackTraceLog().ToString()}");
                }
            }

            return null;
        }

        /// <summary>
        /// 查询处理(Sql语句)
        /// </summary>
        /// <param name="query">查询SQL语句</param>
        /// <param name="dataBaseName">连接数据名</param>
        /// <param name="returnMessage">返回信息</param>
        /// <returns>返回DataSet类型</returns>
        public static DataSet SqlHelper(string query, string dataBaseName, ref string returnMessage, DataPage dataPage)
        {
            try
            {
                Msg msg = new Msg();
                DataSet ds = null;
                ds = SqlHelper(query, null, dataBaseName, dataPage, msg);
                returnMessage = msg.ToString();
                return ds;
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }

            return null;
        }

        /// <summary>
        /// 查询处理(Sql语句)
        /// </summary>
        /// <param name="sqlText">查询SQL语句</param>
        /// <param name="param">参数列表</param>
        /// <param name="dataBaseName">连接数据名</param>
        /// <param name="msg">返回信息</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="track">开启日志跟踪，系统会默认根据设定的阈值来进记录超过返回数据函数调用堆栈， 已防止不正常调用或代码书写错误，导致返回大数据集回来</param>
        /// <param name="dataPage">分页数据</param>
        /// <returns>返回DataSet类型</returns>
        public static DataSet SqlHelper(
            string sqlText,
            dynamic param,
            string dataBaseName,
            DataPage dataPage = null,
            Msg msg = null,
            DBTrack track = DBTrack.Open,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                msg = msg ?? new Msg();
                Query query = new Query()
                {
                    QuerySql = sqlText
                };
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                IPageSql pageSql = PageSqlFactory.GetPageSql(dataBaseName);
                sqlText = dataPage != null ? pageSql.PageSql(dataPage, query) : sqlText; // 分页查询
                DataSet data = db.ExecuteDataSet(sqlText, param, null, cmdTimeout, cmdType);
                if (data != null)
                {
                    string countSqlText = pageSql.CountSql(query); // 查询总数
                    object count = ExecuteScalar(countSqlText, dataBaseName, msg, track);
                    dataPage.TotalCount = count != null ? int.Parse(count.ToString()) : 0;
                }

                //开启日志跟踪，记录数据超过设定的阈值记录日志和调用堆栈，
                if (track == DBTrack.Open && data.Tables[0].Rows.Count > _traceDBRowLength)
                {
                    LogHelper.Write($"查询大数据量超过{_traceDBRowLength}条，实际值为{data.Tables[0].Rows.Count}。\n" +
                        $"{StackTraceLog.GetStackTraceLog().ToString()}");
                }

                return data;
            }
            catch (Exception ex)
            {
                msg.Result = false;
                msg.AddMsg("执行DBHelper.SqlHelper()方法异常。sqlText:" + sqlText + "\n"
                    + ex.Message + StackTraceLog.GetStackTraceLog().ToString());
                if (track == DBTrack.Open)
                    LogHelper.Write("sqlText:" + sqlText + "\n" + ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
            }
            return null;
        }

        /// <summary>
        /// 分页查询 未使用参数化（后续补充）
        /// </summary>
        /// <param name="dataPage">分页数据</param>
        /// <param name="query">查询数据</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="returnMessage">返回错误消息</param>
        /// <returns></returns>
        public static DataSet PageHelper(DataPage dataPage, Query query, string dataBaseName, ref string returnMessage)
        {
            try
            {
                Msg msg = new Msg();
                IPageSql pageSql = PageSqlFactory.GetPageSql(dataBaseName);
                DataSet ds = SqlHelper(pageSql.PageSql(dataPage, query), dataBaseName, ref returnMessage);
                if (ds != null)
                {
                    object countObj = ExecuteScalar(pageSql.CountSql(query), dataBaseName);
                    dataPage.TotalCount = countObj != null ? int.Parse(countObj.ToString()) : 0;
                }
                return ds;
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }

            return null;
        }

        /// <summary>
        /// 查询返回 DataTable 数据集
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="param"></param>
        /// <param name="dataBaseName"></param>
        /// <param name="msg"></param>
        /// <param name="track"></param>
        /// <param name="cmdTimeout"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public static DataTable SqlHelperGetDataTable(
            string sqlText,
            dynamic param,
            string dataBaseName,
            Msg msg = null,
            DBTrack track = DBTrack.Open,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                msg = msg ?? new Msg();
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.ExecuteDataTable(sqlText, param, null, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                msg.Result = false;
                msg.AddMsg(ex.Message);
                if (track == DBTrack.Open)
                    LogHelper.Write("执行SqlHelperGetDataTable方法异常:" + sqlText + "\n" + ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
            }

            return null;
        }
        #endregion


        /// <summary>
        /// DML处理(Sql语句)
        /// </summary>
        /// <param name="query">执行SQL语句</param>
        /// <param name="dataBaseName">连接数据名</param>
        /// <param name="returnMessage">返回信息</param>
        /// <returns>受影响行数</returns>
        public static int ExecuteSQLHelper(string query, string dataBaseName, ref string returnMessage)
        {
            return ExecuteSQLHelper(query, null, dataBaseName, ref returnMessage);
        }

        /// <summary>
        /// DML处理(Sql语句)
        /// </summary>
        /// <param name="sqlText">执行SQL语句</param>
        /// <param name="parameter">动态参数</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="msg">返回消息</param>
        /// <param name="track">是否日志跟踪</param>
        /// <param name="tran"></param>
        /// <param name="cmdTimeout">超时时间（秒）</param>
        /// <param name="cmdType">命令方式</param>
        /// <returns>受影响行数</returns>
        public static int ExecuteSQLHelper(
            string sqlText,
            dynamic parameter,
            string dataBaseName,
            Msg msg = null,
            DBTrack track = DBTrack.Open,
            IDbTransaction tran = null,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                msg = msg ?? new Msg();
                string returnMessage = string.Empty;
                int count = ExecuteSQLHelper(sqlText, parameter, dataBaseName, ref returnMessage, track, tran, cmdTimeout, cmdType);
                msg.Result = count > 0;
                msg.AddMsg(returnMessage);
                return count;
            }
            catch (Exception ex)
            {
                msg.Result = false;
                msg.AddMsg(ex.Message);
            }

            return 0;
        }

        /// <summary>
        /// 执行sql语句,返回受影响行数 eg: ExecuteSQLHelper("delete student where id=:ID", new {ID="0"}, "admin");
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="param">匿名对象的参数，可以简单写，比如 new {Name="user1"} 即可，会统一处理参数和参数的size</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="returnMessage">错误信息</param>
        /// <param name="track">是否日志跟踪</param>
        /// <param name="tran">事务</param>
        /// <param name="cmdTimeout">超时时间</param>
        /// <param name="cmdType">命令方式</param>
        /// <returns>受影响行数</returns>
        public static int ExecuteSQLHelper(
            string sqlText,
            dynamic param,
            string dataBaseName,
            ref string returnMessage,
            DBTrack track = DBTrack.Open,
            IDbTransaction tran = null,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.ExecuteNonQuery(sqlText, param, tran, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                if (track == DBTrack.Open)
                {
                    LogHelper.Write("sqlText:" + sqlText + "\n" + ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
                }
            }

            return 0;
        }

        /// <summary>
        /// ExecuteScalar 返回结果集第一行第一列
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="dataBaseName">数据连接</param>
        /// <param name="msg">Msg</param>
        /// <param name="track">是否日志跟踪</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlText, string dataBaseName, Msg msg = null, DBTrack track = DBTrack.Open)
        {
            return ExecuteReader(sqlText, null, dataBaseName, msg, track);
        }

        /// <summary>
        /// ExecuteScalar 返回结果集第一行第一列
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="dataBaseName">数据连接</param>
        /// <param name="msg">msg</param>
        /// <param name="track">是否日志跟踪</param>
        /// <param name="tran">事务</param>
        /// <param name="cmdTimeout">超时时间</param>
        /// <param name="cmdType">命令方式</param>
        /// <returns></returns>
        public static object ExecuteReader(
            string sqlText,
            dynamic param,
            string dataBaseName,
            Msg msg = null,
            DBTrack track = DBTrack.Open,
            IDbTransaction tran = null,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                msg = msg ?? new Msg();
                string returnMessage = string.Empty;
                object ret = ExecuteScalar(sqlText, param, dataBaseName, ref returnMessage, track, tran, cmdTimeout, cmdType);
                msg.Result = ret != null;
                msg.AddMsg(returnMessage);
                return ret;
            }
            catch (Exception ex)
            {
                msg.Result = false;
                msg.AddMsg(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// 执行sql语句，并返回单个数值
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="param">匿名对象的参数，可以简单写，比如 new {Name="user1"} 即可，会统一处理参数和参数的size</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="returnMessage">返回错误消息</param>
        /// <param name="track">是否日志跟踪</param>
        /// <param name="tran">事务</param>
        /// <param name="cmdTimeout">超时时间</param>
        /// <param name="cmdType">命令方式</param>
        public static object ExecuteScalar(
            string sqlText,
            dynamic param,
            string dataBaseName,
            ref string returnMessage,
            DBTrack track = DBTrack.Open,
            IDbTransaction tran = null,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.ExecuteScalar(sqlText, (object)param, tran, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                if (track == DBTrack.Open)
                {
                    LogHelper.Write("sqlText:" + sqlText + "\n" + ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
                }
            }

            return null;
        }

        /// <summary>
        /// 返回单个值
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="param">匿名对象的参数，可以简单写，比如 new {Name="user1"} 即可，会统一处理参数和参数的size</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="returnMessage">返回错误消息</param>
        /// <param name="track">是否日志跟踪</param>
        /// <param name="tran">事务</param>
        /// <param name="cmdTimeout">超时时间</param>
        /// <param name="cmdType">命令方式</param>
        public static IDataReader ExecuteReader(
            string sqlText,
            dynamic param,
            string dataBaseName,
            ref string returnMessage,
            DBTrack track = DBTrack.Open,
            IDbTransaction tran = null,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.ExecuteReader(sqlText, (object)param, tran, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                if (track == DBTrack.Open)
                {
                    LogHelper.Write("sqlText:" + sqlText + "\n" + ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
                }
            }

            return null;
        }

        /// <summary>
        /// 事务执行,如果事物执行失败，则在msg的MsgObjectContent中会记录失败的语句
        /// </summary>
        /// <param name="query">SQL语句列表</param>
        /// <param name="dataBaseName">数据库名</param>
        /// <param name="msg">返回信息</param>
        /// <param name="allowNull">是否允许0行更新</param>
        /// <param name="track">异常日志跟踪</param>
        /// <returns>返回bool类型，成功，失败。</returns>
        public static bool TranscationExecute(
            List<string> query,
            string dataBaseName,
            Msg msg = null,
            bool allowNull = false,
            DBTrack track = DBTrack.Open)
        {
            try
            {
                msg = msg ?? new Msg();
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                bool jieGuo = db.TranscationExecute(query, allowNull, null, (p) =>
                {
                    // 将错误的语句压入堆栈中
                    List<string> list = new List<string>() { p };
                    msg.MsgObjectContent = list;
                }) > 0;
                return jieGuo;
            }
            catch (Exception ex)
            {
                msg.Result = false;
                msg.AddMsg(ex.Message);
                if (track == DBTrack.Open)
                    LogHelper.Write("执行TranscationExecute方法异常：" + ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
            }
            return false;
        }

        /// <summary>
        /// 事务执行
        /// </summary>
        /// <param name="dataBaseName">数据库名</param>
        /// <param name="msg">返回信息</param>
        /// <param name="track">异常日志跟踪</param>
        /// <returns>返回bool类型，成功，失败。</returns>
        public static bool TranscationExecute(
            Func<IDbConnection, bool> tran,
            string dataBaseName,
            Msg msg = null,
            DBTrack track = DBTrack.Open)
        {
            try
            {
                msg = msg ?? new Msg();
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.TranscationExecute(tran);
            }
            catch (Exception ex)
            {
                msg.Result = false;
                msg.AddMsg(ex.Message);
                if (track == DBTrack.Open)
                    LogHelper.Write("执行TranscationExecute方法异常：" + ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
            }

            return false;
        }

        /// <summary>
        /// 获取指定表名的主键字段名称
        /// </summary>
        /// <param name="tableName">数据表名</param>
        /// <param name="dataBaseName">数据库链接名称</param>
        /// <param name="msg">消息对象</param>
        /// <returns>返回主键字段名称</returns>
        public static string GetTablePrimaryKey(string tableName, string dataBaseName, Msg msg = null, DBTrack track = DBTrack.Open)
        {
            try
            {
                msg = msg ?? new Msg();
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.GetTablePrimaryKey(tableName);
            }
            catch (Exception ex)
            {
                msg.Result = false;
                msg.AddMsg(ex.Message);
                if (track == DBTrack.Open)
                    LogHelper.Write("执行GetTablePrimaryKey方法异常：" + ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
            }
            return null;
        }

        /// <summary>
        /// 更新CLOB
        /// </summary>
        /// <param name="tablename">数据表</param>
        /// <param name="filed"></param>
        /// <param name="where"></param>
        /// <param name="value"></param>
        /// <param name="dataBaseName"></param>
        /// <returns></returns>
        public static int Updateclob(string tablename, string filed, string where, object value, string dataBaseName, Msg msg = null, DBTrack track = DBTrack.Open)
        {
            try
            {
                msg = msg ?? new Msg();
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.UpdateClob(tablename, filed, where, value);
            }
            catch (Exception ex)
            {
                msg.Result = false;
                msg.AddMsg(ex.Message);
                if (track == DBTrack.Open)
                    LogHelper.Write("执行Updateclob方法异常：" + ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
            }

            return 0;
        }

        #region 开放Dapper的一些操作
        /// <summary>
        /// 获取dynamic列表，常用在只查询部分字段
        /// eg: QueryDynamic(("select Id,Name from TbUser where Id = : Id", new { Id = 10 });
        /// </summary>
        /// <param name="sqlText">sql语句 eg: select Id,Name from TbUser where Id = : Id</param>
        /// <param name="param">匿名对象的参数，可以简单写，比如 new {Name="user1"} 即可，会统一处理参数和参数的size</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="returnMessage">返回错误消息</param>
        /// <param name="track">是否日志跟踪</param>
        /// <param name="buffered">默认值改为true，一次性取完断开连接,如果想自行一笔一笔取，可设置为false</param>
        /// <param name="cmdTimeout">超时时间(默认60s)</param>
        /// <param name="cmdType">命令方式</param>
        public static IEnumerable<dynamic> QueryDynamics(
            string sqlText,
            dynamic param,
            string dataBaseName,
            ref string returnMessage,
            DBTrack track = DBTrack.Open,
            bool buffered = true,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.QueryDynamics(sqlText, (object)param, buffered, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                if (track == DBTrack.Open)
                {
                    LogHelper.Write("执行QueryDynamics异常，sqlText:" + sqlText + "\n" + ex.ToString()
                        + StackTraceLog.GetStackTraceLog().ToString());
                }
            }

            return null;
        }

        /// <summary>
        /// 查询多个dynamic列表结果集， 方便查询分页列表数据，同时统计总数
        /// eg: QueryMultipleDynamic(("select Id,Name from User where Id =:Id; select * from Order;", new { Id = 10 });
        /// </summary>
        /// <param name="sqlText">多个sql语句,语句以分号结束 eg: select Id,Name from User where Id =: Id; select * from Order;</param>
        /// <param name="param">匿名对象的参数，可以简单写，比如 new {Name="user1"} 即可，会统一处理参数和参数的size</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="returnMessage">返回错误消息</param>
        /// <param name="track">是否日志跟踪</param>
        /// <param name="tran"></param>
        /// <param name="cmdTimeout">超时时间(默认60s)</param>
        /// <param name="cmdType">命令方式</param>
        public static List<List<dynamic>> QueryMultipleDynamic(
            string sqlText,
            dynamic param,
            string dataBaseName,
            ref string returnMessage,
            DBTrack track = DBTrack.Open,
            IDbTransaction tran = null,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.QueryMultipleDynamic(sqlText, (object)param, tran, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                if (track == DBTrack.Open)
                {
                    LogHelper.Write("执行QueryMultipleDynamic异常，sqlText:" + sqlText + "\n" + ex.ToString()
                        + StackTraceLog.GetStackTraceLog().ToString());
                }
            }

            return null;
        }

        /// <summary>
        /// 获取Hashtable列表，可以方便转为json对象
        /// (注意：类似count(*)要给字段名，比如 select count(*) as recordCount from TbUser。)
        /// eg: QueryHashtables(("select Id,Name from TbUser where Id = : Id", new { Id = 10 });
        /// </summary>
        /// <param name="sqlText">多个sql语句以分号相隔 eg: select Id,Name from TbUser where Id =: Id; select * from TbOrder</param>
        /// <param name="param">匿名对象的参数，可以简单写，比如 new {Name="user1"} 即可，会统一处理参数和参数的size</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="returnMessage">返回错误消息</param>
        /// <param name="track">是否日志跟踪</param>
        /// <param name="buffered">默认值改为true，一次性取完断开连接,如果想自行一笔一笔取，可设置为false</param>
        /// <param name="cmdTimeout">超时时间(默认60s)</param>
        /// <param name="cmdType">命令方式</param>
        public static List<Hashtable> QueryHashtables(
            string sqlText,
            dynamic param,
            string dataBaseName,
            ref string returnMessage,
            DBTrack track = DBTrack.Open,
            bool buffered = true,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.QueryHashtables(sqlText, (object)param, buffered, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                if (track == DBTrack.Open)
                {
                    LogHelper.Write("执行QueryHashtables异常，sqlText:" + sqlText + "\n" + ex.ToString()
                        + StackTraceLog.GetStackTraceLog().ToString());
                }
            }

            return null;
        }


        /// <summary>
        /// 获取多个Hashtable列表结果集， 可以方便转为json对象 (注意：类似count(*)要给字段名，比如 select count(*) as recordCount from TbUser。)
        /// eg: QueryMultipleHashtables(("select Id,Name from TbUser where Id =: Id;select count(*) as recordCount from TbUser", new { Id = 10 });
        /// </summary>
        /// <param name="sqlText">多个sql语句以分号相隔 eg: select Id,Name from TbUser where Id =: Id;select * from TbOrder</param>
        /// <param name="param">匿名对象的参数，可以简单写，比如 new {Name="user1"} 即可，会统一处理参数和参数的size</param>
        /// <param name="dataBaseName">连接名称</param>
        /// <param name="returnMessage">返回错误消息</param>
        /// <param name="track">是否日志跟踪</param>
        /// <param name="tran">默认值改为true，一次性取完断开连接,如果想自行一笔一笔取，可设置为false</param>
        /// <param name="cmdTimeout">超时时间(默认60s)</param>
        /// <param name="cmdType">命令方式</param>
        public static List<List<Hashtable>> QueryMultipleHashtables(
            string sqlText,
            dynamic param,
            string dataBaseName,
            ref string returnMessage,
            DBTrack track = DBTrack.Open,
            IDbTransaction tran = null,
            int? cmdTimeout = DEFAULTTIMEOUT,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                return db.QueryMultipleHashtables(sqlText, (object)param, tran, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                if (track == DBTrack.Open)
                {
                    LogHelper.Write("执行QueryHashtables异常，sqlText:" + sqlText + "\n" + ex.ToString()
                        + StackTraceLog.GetStackTraceLog().ToString());
                }
            }

            return null;
        }
        #endregion

        #region 参数化相关
        /// <summary>
        /// 创建参数化的动态参数列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static DynamicParameters CreateDataDynamicParameters<T>(T entity, DBTrack track = DBTrack.Open)
        {
            DynamicParameters parameters = new DynamicParameters();
            try
            {
                PropertyInfo[] propertys = typeof(T).GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    if (pi.IsNoMapKey()
                        || pi.Name.ToUpper().Contains("_MAX")
                        || pi.Name.ToUpper().Contains("_MIN"))
                    {
                        // 过滤掉未映射字段、_MAX和_MIN字段
                        continue;
                    }

                    object value = GetValue(pi.Name, entity);
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        parameters.Add(pi.Name, DBNull.Value);
                    }
                    else
                    {
                        parameters.Add(pi.Name, value.ToString().FilterChar());
                    }
                }
                return parameters;
            }
            catch (Exception ex)
            {
                if (track == DBTrack.Open)
                    LogHelper.Write(ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
            }

            return parameters;
        }

        private static object GetValue(string propertyName, object entity)
        {
            Type type = entity.GetType();
            PropertyInfo pi = type.GetProperty(propertyName);
            if (pi != null && pi.CanRead)
                return pi.GetValue(entity, null);
            FieldInfo fi = type.GetField(propertyName, BindingFlags.Instance
              | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null)
                return fi.GetValue(entity);
            return null;
        }
        #endregion
    }
}
