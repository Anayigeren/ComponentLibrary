using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using HongYang.Enterprise.Data.AdoNet;
using HongYang.Enterprise.Logging;
using Oracle.ManagedDataAccess.Client;
using HongYang.Enterprise.Data.DataEntity;
using Dapper;

namespace HongYang.Enterprise.Data
{
    /// <summary>
    /// 数据操作帮助类扩展
    /// </summary>
    public partial class DBHelper
    {
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

                db.ExecuteNonQuery(procudurnName, parameters, null, null, CommandType.StoredProcedure);
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
    }
}
