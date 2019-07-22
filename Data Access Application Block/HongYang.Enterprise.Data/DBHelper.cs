using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using HongYang.Enterprise.Data.AdoNet;
using HongYang.Enterprise.Logging;
using System.Reflection;
using Oracle.ManagedDataAccess.Client;
using HongYang.Enterprise.Data.DataEntity;
using Dapper;
using System.Collections;

namespace HongYang.Enterprise.Data
{
    /// <summary>
    /// 数据操作帮助类
    /// 可以封装
    /// 使用部分类允许其他DLL 来扩展DBHelper
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
        /// <returns></returns>
        public static DataSet SqlHelper(
            string sql,
            dynamic param,
            string dataBaseName,
            Msg msg = null)
        {
            try
            {
                msg = msg ?? new Msg();
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                string returnMessage = string.Empty;
                var data = SqlHelper(sql, param, dataBaseName, ref returnMessage);
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
                    LogHelper.Write("SqlHelper方法异常：\n" + ex.Message + $"{StackTraceLog.GetStackTraceLog().ToString()}");
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
                msg.AddMsg("sqlText:" + sqlText + "\n" + ex.Message + StackTraceLog.GetStackTraceLog().ToString());
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
                msg =  msg ?? new Msg();
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
        /// <param name="dataBaseName">连接数据名</param>
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
                db.ExecuteScalar(sqlText, (object)param, tran, cmdTimeout, cmdType);
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

        /// <summary>
        /// 给SQL命令绑定参数
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="parameters"></param>
        private static void AttachParameters(IDbCommand dbCommand, IDataParameter[] parameters)
        {
            if (dbCommand == null) throw new ArgumentNullException("dbCommand", "无效的SQL命令");

            if (parameters != null)
            {
                foreach (IDataParameter param in parameters)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(dbCommand.CommandText, string.Format(@"(:\b{0}\b)+", param.ParameterName), System.Text.RegularExpressions.RegexOptions.Multiline))
                    {
                        continue;
                    }

                    if (param.Value == null
                        && (param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.InputOutput))
                        param.Value = DBNull.Value;

                    dbCommand.Parameters.Add(param);
                }
            }
        }

        /// <summary>
        /// 给SQL命令绑定参数
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="parameters"></param>
        private static void BindParameters(IDbCommand dbCommand, object[] parameters)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand", "无效的SQL命令");

            if (parameters != null)
            {
                IDataParameter[] cmdParameters = AssignParameterValues(parameters);
                foreach (IDataParameter param in cmdParameters)
                {
                    if (param.Value == null &&
                        (param.Direction == ParameterDirection.Input ||
                         param.Direction == ParameterDirection.InputOutput))
                        param.Value = DBNull.Value;

                    dbCommand.Parameters.Add(param);
                }
            }
        }

        /// <summary>
        /// 按SQL命令的参数顺序和参数值类型指定其参数值
        /// </summary>
        /// <param name="paramValues"></param>
        private static IDataParameter[] AssignParameterValues(object[] paramValues)
        {
            if (paramValues == null)
                return null;
            IDataParameter[] cmdParameters = new IDataParameter[paramValues.Length];
            for (int i = 0; i < cmdParameters.Length; i++)
            {
                if (paramValues[i] is IDataParameter)
                {
                    cmdParameters[i] = (IDataParameter)paramValues[i];
                    if (((IDataParameter)paramValues[i]).Value == null)
                    {
                        cmdParameters[i].Value = DBNull.Value;
                    }
                    else
                        cmdParameters[i].Value = ((IDataParameter)paramValues[i]).Value;
                }
                else if (paramValues[i] == null)
                {
                    cmdParameters[i].Value = DBNull.Value;
                }
                else
                {
                    cmdParameters[i].Value = paramValues[i];
                }
            }
            return cmdParameters;
        }

        #region 执行用于保存BLOB二进制数据记录的方法
        // 作者:黄兴
        // 日期:2005.08.04
        // 修订说明:初始创建。
        /// <summary>
        /// 执行用于保存BLOB二进制数据记录的方法
        /// </summary>
        /// <param name="stream">需要保存的流</param>
        /// <param name="cmdType">SQL命令类型（SQL语句或存储过程），参见<see cref="CommandType"/>。</param>
        /// <param name="sTable">数据表名</param>
        /// <param name="sBlobField">所要保存的Bolb字段名</param>
        /// <param name="sIDName">关键索引字段</param>
        /// <param name="sIDValue">关键索引字段值</param>
        /// <returns>返回bool类型，成功-True，失败-False。</returns>
        public static bool StreamToBlob(ref Stream stream, CommandType cmdType,
            string sTable, string sBlobField, string sIDName, string sIDValue, string dataBaseName, Msg msg = null)
        {
            msg = msg ?? new Msg();
            Database db = DatabaseFactory.GetDatabase(dataBaseName);

            #region 参数判断
            if (stream == null) throw new ArgumentNullException("stream", "无效的数据流对象");
            if (sTable == null) throw new ArgumentNullException("stream", "无效的数据表名");
            if (sBlobField == null) throw new ArgumentNullException("stream", "无效的二进制流字段名");
            if (sIDName == null) throw new ArgumentNullException("stream", "无效的主键名");
            if (sIDValue == null) throw new ArgumentNullException("stream", "无效的主键值");
            #endregion

            DataSet dataSet = null;

            using (IDbCommand cmd = db.GetDBCommand(db.dbConn))
            {
                cmd.CommandType = cmdType;

                IDbDataAdapter dataAdapter = db.GetDataAdapter(cmd);

                byte[] buff = null;
                DataRow row = null;
                int iRtn = 0;
                string sSQL = String.Format("SELECT * FROM {0} WHERE {1} = '{2}'", sTable, sIDName, sIDValue);

                cmd.CommandText = sSQL;
                dataAdapter.SelectCommand = cmd;
                AttachParameters(cmd, (IDataParameter[])null);
                try
                {
                    //adapter = new OdbcDataAdapter( sSQL, conn );				
                    dataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                    dataSet = new DataSet(sTable);

                    OracleCommandBuilder builder = new OracleCommandBuilder((OracleDataAdapter)dataAdapter);

                    dataAdapter.Fill(dataSet);

                    if (0 == dataSet.Tables[0].Rows.Count)
                    {
                        return false;
                    }

                    row = dataSet.Tables[0].Rows[0];
                    buff = new byte[stream.Length];
                    stream.Position = 0;
                    stream.Read(buff, 0, System.Convert.ToInt32(stream.Length));


                    row[sBlobField] = buff;
                    iRtn = dataAdapter.Update(dataSet);
                }
                catch (Exception ee)
                {
                    string err = ee.Message.ToString();
                    buff = null;
                }
                finally
                {
                    if (dataSet != null)
                        dataSet.Dispose();
                }
                if (buff == null || iRtn == 0)
                    return false;
            }
            return true;
        }
        #endregion

        #region 执行用于读取BLOB二制数据记录的方法
        // 作者:黄兴
        // 日期:2005.08.04
        // 修订说明:初始创建。
        /// <summary>
        /// 执行用于读取BLOB二制数据记录的方法
        /// </summary>
        /// <param name="stream">返回的流数据</param>
        /// <param name="cmdType">SQL命令类型（SQL语句或存储过程），参见<see cref="CommandType"/>。</param>
        /// <param name="sTable">数据表名</param>
        /// <param name="sBlobField">所要保存的Bolb字段名</param>
        /// <param name="sIDName">关键索引字段</param>
        /// <param name="sIDValue">关键索引字段值</param>
        /// <returns>返回bool类型，成功-True，失败-False。</returns>
        public static bool BlobToStream(ref Stream stream, CommandType cmdType, string sTable,
            string sBlobField, string sIDName, string sIDValue, string dataBaseName, Msg msg = null)
        {
            msg = msg ?? new Msg();
            Database db = DatabaseFactory.GetDatabase(dataBaseName);
            #region 参数判断
            if (sTable == null) throw new ArgumentNullException("stream", "无效的数据表名");
            if (sBlobField == null) throw new ArgumentNullException("stream", "无效的二进制流字段名");
            if (sIDName == null) throw new ArgumentNullException("stream", "无效的主键名");
            if (sIDValue == null) throw new ArgumentNullException("stream", "无效的主键值");
            #endregion

            string sSQL = String.Format("SELECT * FROM {0} WHERE {1} = '{2}'", sTable, sIDName, sIDValue);

            DataSet dataSet = null;

            using (IDbCommand cmd = db.GetDBCommand(db.dbConn))
            {
                cmd.CommandType = cmdType;

                IDbDataAdapter dataAdapter = db.GetDataAdapter(cmd);

                DataRow row = null;
                byte[] buff = new byte[0];
                bool bRtn = true;

                try
                {
                    cmd.CommandText = sSQL;
                    dataAdapter.SelectCommand = cmd;
                    AttachParameters(cmd, (IDataParameter[])null);

                    dataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                    dataSet = new DataSet(sTable);

                    OracleCommandBuilder builder = new OracleCommandBuilder((OracleDataAdapter)dataAdapter);

                    dataAdapter.Fill(dataSet);

                    if (0 == dataSet.Tables[0].Rows.Count)
                    {
                        return false;
                    }

                    row = dataSet.Tables[0].Rows[0];
                    if (null == row)
                        return false;

                    buff = (byte[])row[sBlobField];

                    stream.Write(buff, 0, buff.GetUpperBound(0) + 1);
                }
                catch (Exception ee)
                {
                    string err = ee.Message.ToString();
                    bRtn = false;
                }
                finally
                {
                    if (dataSet != null)
                        dataSet.Dispose();
                }
                return bRtn;
            }
        }
        #endregion

        #region 取当前序列,条件为seq.nextval或seq.currval
        /// 
        /// 取当前序列
        public static decimal GetSeq(string seqName, string dataBaseName, Msg msg = null)
        {
            string sql = "";
            try
            {
                msg = msg ?? new Msg();
                decimal seqnum = 0;
                sql = "select " + seqName + ".nextval from dual";
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                DataSet data = db.ExecuteDataSet(sql, CommandType.Text, null);
                //  CloseConn();
                if (null != data && 0 < data.Tables[0].Rows.Count)
                {
                    return Convert.ToDecimal(data.Tables[0].Rows[0][0]);
                }
                return seqnum;
            }
            catch (Exception ex)
            {
                msg.Result = false;
                msg.AddMsg("sqlText:" + sql + "\n" + ex.Message + StackTraceLog.GetStackTraceLog().ToString());
                LogHelper.Write("sqlText:" + sql + "\n" + ex.ToString() + StackTraceLog.GetStackTraceLog().ToString());
            }
            return 0;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procudurnName"></param>
        /// <param name="parameters"></param>
        /// <param name="dataBaseName"></param>
        /// <param name="returnMessage"></param>
        public static void ExecuteProcedure(string procudurnName, object[] parameters, string dataBaseName, ref string returnMessage)
        {
            try
            {
                Database db = DatabaseFactory.GetDatabase(dataBaseName);
                if (db == null)
                {
                    returnMessage = "连接数据库失败！";
                }

                if (procudurnName == null || procudurnName.Trim() == "")
                {
                    returnMessage = "传入的存储过程名为空！";
                }
                if (parameters == null)
                {
                    returnMessage = "传入的存储过程参数为空！";
                }

                db.ExecuteNonQuery(procudurnName, parameters, null, DEFAULTTIMEOUT, CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                returnMessage = "执行存储过程" + procudurnName + "失败！" + ex.ToString();
            }
        }

        /// <summary>
        /// 执行存储过程(外部Connection)
        /// </summary>
        /// <param name="procudurnName"></param>
        /// <param name="parameters"></param>
        /// <param name="dataBaseName"></param>
        /// <param name="returnMessage"></param>
        public static void ExecuteProcedure(string procudurnName, object[] parameters, IDbConnection dbconn, ref string returnMessage)
        {
            try
            {
                if (dbconn == null)
                {
                    returnMessage = "连接数据库失败！";
                }
                ConnectionState state = dbconn.State;
                if (ConnectionState.Open != state)
                    dbconn.Open();

                if (procudurnName == null || procudurnName.Trim() == "")
                {
                    returnMessage = "传入的存储过程名为空！";
                }
                if (parameters == null)
                {
                    returnMessage = "传入的存储过程参数为空！";
                }

                using (IDbCommand cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = procudurnName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    BindParameters(cmd, parameters);
                    var ret = cmd.ExecuteNonQuery() > 0;
                    if (!ret)
                    {
                        returnMessage = "执行存储过程" + procudurnName + "失败！";
                    }
                }
                //db.ExecuteNonQuery(procudurnName, CommandType.StoredProcedure, parameters);
            }
            catch (Exception ex)
            {
                returnMessage = "执行存储过程" + procudurnName + "失败！" + ex.ToString();
            }
            finally
            {
                if (null != dbconn)
                    dbconn.Close();
            }
        }
        #endregion

        #region 参数化相关
        /// <summary>
        /// 创建参数化的动态参数列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static DynamicParameters CreateDataDynamicParameters<T>(T entity)
        {
            DynamicParameters parameters = new DynamicParameters();
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
