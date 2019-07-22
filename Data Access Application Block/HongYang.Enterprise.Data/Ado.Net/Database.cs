using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using HongYang.Enterprise.Logging;

namespace HongYang.Enterprise.Data.AdoNet
{
    /// <summary>
    /// 该级，都不记录日志，如果有异常
    /// 则自动抛出
    /// </summary>
    public abstract class Database
    {

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public IDbConnection dbConn;


        public Database()
        {

        }

        public Database(string connstr)
        {
            InitConnection(connstr);
        }

        public void InitConnection(string connstr)
        {
            if (dbConn == null)
                dbConn = GetDBConnection(connstr);
        }

        #region abstract method
        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <param name="connString">数据库连接字符串</param>
        /// </remarks>
        /// <returns>指定数据源的连接对象</returns>
        public abstract IDbConnection GetDBConnection(string connString);

        /// <summary>
        /// 创建与指定数据源连接相关联的SQL命令对象。
        /// </summary>
        /// <param name="dbConn"></param>
        /// <returns></returns>
        public abstract IDbCommand GetDBCommand(IDbConnection dbConn);

        /// <summary>
        /// 数据库适配器
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        public abstract IDbDataAdapter GetDataAdapter(IDbCommand dbCommand);

        /// <summary>
        /// 封装成IDataParamter
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract IDataParameter GetDataParameter(object value);

        /// <summary>
        /// 构造封装成IDataParamter
        /// </summary>
        /// <param name="param_name"></param>
        /// <param name="value"></param>
        /// <param name="t"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public abstract IDataParameter GetDataParameter(string param_name, object value, DbType type, ParameterDirection direction);

        /// <summary>
        /// 获取指定表名的主键字段名称
        /// </summary>
        /// <param name="tableName">数据表名</param>
        /// <returns></returns>
        public abstract string GetTablePrimaryKey(string tableName);

        /// <summary>
        /// 更新CLOB
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <param name="filed">字段名称</param>
        /// <param name="where">条件，where 之后的语句</param>
        /// <param name="value">字段值</param>
        /// <returns></returns>
        public abstract int UpdateClob(string tablename, string filed, string where, object value);
        
        #endregion

        #region ConnectionManage
        /// <summary>
        /// 打开数据库
        /// </summary>
        public void Open()
        {
            if (dbConn.State != ConnectionState.Open)
            {
                dbConn.Open();
            }
        }

        /// <summary>
        /// 关闭数据库
        /// </summary>
        public void Close()
        {
            if (dbConn.State == ConnectionState.Open)
            {
                dbConn.Close();
            }

        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            dbConn.Dispose();
        }
        #endregion

        #region  ExecuteMethod
        /// <summary>
        /// 执行sql前对sql字符串的处理事件
        /// 可以检查sql安全性，也可以按不同数据库sql写法做调整(sqlserver是用@，oracle的参数是用冒号)
        /// </summary>
        /// <param name="sqlText"></param>
        public virtual void CheckSql(string sqlText)
        {
            if (this.dbConn == null)
            {
                throw new InvalidOperationException("没有可用的数据库连接");
            }
            if (sqlText == null || sqlText.Length == 0)
            {
                throw new ArgumentNullException("sqlStr", "无效的SQL命令");
            }
        }

        /// <summary>
        /// 执行有返回数据集 DataSet
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="param">动态参数 eg: new {Id = "0"}</param>
        /// <param name="tran"></param>
        /// <param name="cmdTimeout">超时时间（秒）</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns>返回DataSet类型</returns>
        public DataSet ExecuteDataSet(
            string sqlText,
            dynamic param = null,
            IDbTransaction tran = null,
            int? commandTimeout = null,
            CommandType? cmdType = null)
        {
            try
            {
                Open();
                CheckSql(sqlText);
                DataSet dataSet = new DataSet();
                using (var reader = dbConn.ExecuteReader(sqlText, (object)param, tran, commandTimeout, cmdType))
                {
                    do
                    {
                        var dataTable = DataReaderToDataTable(reader);
                        dataSet.Tables.Add(dataTable);
                    }
                    while (reader.NextResult());
                    return dataSet;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// 执行有返回数据集 DataTable
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="param">动态参数 eg: new {Id = "0"}</param>
        /// <param name="tran"></param>
        /// <param name="cmdTimeout">超时时间（秒）</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns>返回DataTable类型</returns>
        public DataTable ExecuteDataTable(
            string sqlText,
            dynamic param = null,
            IDbTransaction tran = null,
            int? cmdTimeout = null,
            CommandType? cmdType = null)
        {
            try
            {
                Open();
                CheckSql(sqlText);
                using (var reader = dbConn.ExecuteReader(sqlText, (object)param, tran, cmdTimeout, cmdType))
                {
                    var dataTable = DataReaderToDataTable(reader);
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// 执行带参数的SQL命令
        /// </summary>
        /// <param name="sqlText">sql语句</param>
        /// <param name="param">动态参数</param>
        /// <param name="tran">事务</param>
        /// <param name="cmdTimeout">超时时间（默认null秒）</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        public virtual int ExecuteNonQuery(
            string sqlText,
            dynamic param = null,
            IDbTransaction tran = null,
            int? cmdTimeout = null,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                Open();
                CheckSql(sqlText);
                return dbConn.Execute(sqlText, (object)param, tran, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// ExecuteScalar 返回结果集第一行第一列
        /// </summary>
        /// <param name="sqlText">SQL语句</param>
        /// <param name="param">动态参数</param>
        /// <param name="tran">事务</param>
        /// <param name="cmdTimeout">超时时间（默认null秒）</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns>object</returns>
        public virtual object ExecuteScalar(
            string sqlText,
            dynamic param,
            IDbTransaction tran = null,
            int? cmdTimeout = null, 
            CommandType? cmdType = null)
        {
            try
            {
                Open();
                CheckSql(sqlText);
                return dbConn.ExecuteScalar(sqlText, (object)param, tran, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// IDataReader 返回结果集第一行第一列
        /// </summary>
        /// <param name="sqlText">SQL语句</param>
        /// <param name="param">动态参数</param>
        /// <param name="tran">事务</param>
        /// <param name="cmdTimeout">超时时间（默认null秒）</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns>IDataReader</returns>
        public virtual IDataReader ExecuteReader(
            string sqlText,
            dynamic param = null,
            IDbTransaction tran = null,
            int? cmdTimeout = null,
            CommandType cmdType = CommandType.Text)
        {
            try
            {
                Open();
                using (var reader = dbConn.ExecuteReader(sqlText, (object)param, tran, cmdTimeout, cmdType))
                {
                    return reader;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }
        #endregion

        #region Transcation
        /// <summary>
        /// 事务执行
        /// </summary>
        /// <param name="sqlList">SQL语句列表</param>
        /// <param name="allowNull">是否允许执行时受影响行数为0，true-允许,false-不允许</param>
        /// <param name="successCallback">事务提交前处理函数，返回false则回滚不执行本次操作</param>
        /// <param name="falseCallback">当事物执行失败时，将失败语句会返回上级调用</param>
        /// <param name="cmdTimeout">每一条任务执行超时时长（秒）</param>
        /// <returns>返回bool类型，成功，失败。</returns>
        public virtual int TranscationExecute(
            List<string> sqlList,
            bool allowNull = false,
            Func<int, bool> successCallback = null,
            Action<string> falseCallback = null,
            int? cmdTimeout = null)
        {
            int count = 0;

            dbConn.Open();
            IDbTransaction transaction = dbConn.BeginTransaction();
            try
            {
                foreach (string sql in sqlList)
                {
                    int _t = dbConn.Execute(sql, null, null, cmdTimeout);
                    if (0 == _t && !allowNull) //如果受影响的行数为0则事务提交失败
                    {
                        if (falseCallback != null)
                        {
                            falseCallback(sql);
                        }

                        transaction.Rollback();
                        return 0;
                    }

                    count += _t;
                }
                if (successCallback != null)
                {
                    if (successCallback(count))
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                else
                {
                    transaction.Commit();
                }

                return count;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="tran">执行数据库操作，返回操作结果</param>
        /// <returns></returns>
        public virtual bool TranscationExecute(Func<IDbConnection, bool> tran)
        {
            Open();
            IDbTransaction transaction = dbConn.BeginTransaction();
            try
            {
                if (tran != null)
                {
                    if (tran(dbConn))
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            finally
            {
                Close();
            }
        }
        #endregion

        #region Dapper extend
        /// <summary>
        /// 获取dynamic列表
        /// eg: QueryDynamic(("select Id,Name from TbUser where Id = : Id", new { Id = 10 });
        /// </summary>
        /// <param name="sqlText">sql eg: select Id,Name from TbUser where Id = : Id</param>
        /// <param name="param">参数 eg: new { Id = 10 }</param>
        /// <param name="buffered">默认值改为true，一次性取完断开连接。如果想自行一笔一笔取，可设置为false。</param>
        /// <param name="cmdTimeout">超时时间</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns></returns>
        public IEnumerable<dynamic> QueryDynamics(string sqlText, dynamic param = null, bool buffered = true, int? cmdTimeout = null, CommandType? cmdType = null)
        {
            try
            {
                Open();
                CheckSql(sqlText);
                return dbConn.Query(sqlText, (object)param, null, buffered, cmdTimeout, cmdType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// 查询多个dynamic列表结果集。
        /// eg: QueryMultipleDynamic(("select Id,Name from TbUser where Id =:Id; select * from TbOrder", new { Id = 10 });
        /// </summary>
        /// <param name="sqlText">多个sql语句，一个sql以分号结束 eg: select Id,Name from TbUser where Id =: Id; select * from TbOrder</param>
        /// <param name="param">eg: new { Id = 10 }</param>
        /// <param name="tran"></param>
        /// <param name="cmdTimeout">超时时间</param>
        /// <param name="cmdType">命令类型</param>
        public List<List<dynamic>> QueryMultipleDynamic(
            string sqlText,
            dynamic param = null,
            IDbTransaction tran = null,
            int ? cmdTimeout = null,
            CommandType? cmdType = null)
        {
            try
            {
                Open();
                CheckSql(sqlText);
                var gridReader = dbConn.QueryMultiple(sqlText, (object)param, tran, cmdTimeout, cmdType);
                var result = new List<List<dynamic>>();
                while (!gridReader.IsConsumed)
                {
                    var items = gridReader.Read();
                    result.Add(items.ToList());
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// 获取Hashtable列表。 可以方便转为json对象。
        /// (注意：类似count(*)要给字段名，比如 select count(*) as recordCount from TbUser。)
        /// eg: QueryHashtables(("select Id,Name from TbUser where Id = : Id", new { Id = 10 });
        /// </summary>
        /// <param name="sqlText">eg: select Id,Name from TbUser where Id = : Id</param>
        /// <param name="param">eg: new { Id = 10 }</param>
        /// <param name="buffered">注意，这边默认值为false。因为它在此函数内的DapperRowsToHashList(result)中才真正取数遍历。</param>
        /// <param name="cmdTimeout">执行超时时间（秒）</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns></returns>
        public List<Hashtable> QueryHashtables(
            string sqlText,
            dynamic param = null,
            bool buffered = false,
            int? cmdTimeout = null,
            CommandType? cmdType = null)
        {
            try
            {
                Open();
                CheckSql(sqlText);
                var result = dbConn.Query(sqlText, (object)param, null, buffered, cmdTimeout, cmdType);
                return DapperRowsToHashList(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// 获取多个Hashtable列表结果集。 可以方便转为json对象
        /// (注意：类似count(*)要给字段名，比如 select count(*) as recordCount from TbUser。)
        /// eg: dbHandleObj.QueryMultipleHashtables(("select Id,Name from TbUser where Id =: Id;select count(*) as recordCount from TbUser", new { Id = 10 });
        /// </summary>
        /// <param name="sqlText">eg: select Id,Name from TbUser where Id =: Id;select * from TbOrder</param>
        /// <param name="param">eg: new { Id = 10 }</param>
        /// <param name="tran"></param>
        /// <param name="cmdTimeout">执行超时时间（秒）</param>
        /// <param name="cmdType">命令类型</param>
        /// <returns></returns>
        public List<List<Hashtable>> QueryMultipleHashtables(
            string sqlText,
            dynamic param = null,
            IDbTransaction tran = null,
            int? cmdTimeout = null,
            CommandType? cmdType = null)
        {
            try
            {
                Open();
                CheckSql(sqlText);
                var gridReader = dbConn.QueryMultiple(sqlText, (object)param, tran, cmdTimeout, cmdType);
                var result = new List<List<Hashtable>>();
                while (!gridReader.IsConsumed)
                {
                    var items = gridReader.Read();
                    result.Add(DapperRowsToHashList(items));
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// 辅助Dapper返回的dynamic结果集转为Hashtable列表，方便转为json。
        /// (没有实体类对应的Query时，Dapper是返回dynamic类型对象，这个dynamic对象其实是它自己内部Private级别的DapperRow列表对象。)
        /// </summary>
        /// <param name="rows">必须是Dapper返回的IEnumerable<dynamic>对象</param>
        /// <returns></returns>
        protected List<Hashtable> DapperRowsToHashList(IEnumerable<dynamic> rows)
        {
            var list = new List<Hashtable>();
            foreach (var row in rows)
            {
                var obj = new Hashtable();
                foreach (var prop in row)
                {
                    obj.Add(prop.Key, prop.Value); // 含有Key,Value的dynamic对象（其实就是DapperRow，但它的访问级别是Private，因此用dynamic访问）
                }
                list.Add(obj);
            }
            return list;
        }
        #endregion

        #region private method
        /// <summary>
        /// 给SQL命令绑定参数
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="parameters"></param>
        private void AttachParameters(IDbCommand dbCommand, object[] parameters)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand", "无效的SQL命令");

            if (parameters != null)
            {
                IDataParameter[] cmdParameters = AssignParameterValues(parameters);
                foreach (IDataParameter param in cmdParameters)
                {
                    //需要判断使用了多少个 然后增加多少个
                    var testName = param.ParameterName;
                    if (param.ParameterName.Contains(':'))
                    {
                        testName = param.ParameterName.Substring(param.ParameterName.IndexOf(':')+1);
                    }
                    if (!Regex.IsMatch(dbCommand.CommandText, string.Format(@"(:\b{0}\b)+", testName),RegexOptions.Multiline)) { 
                        continue;
                    }
                    if (param.Value == null
                        && (param.Direction == ParameterDirection.Input ||param.Direction == ParameterDirection.InputOutput))
                        param.Value = DBNull.Value;

                    dbCommand.Parameters.Add(param);
                }
            }
        }

        /// <summary>
        /// 按SQL命令的参数顺序和参数值类型指定其参数值
        /// </summary>
        /// <param name="cmdParameters"></param>
        /// <param name="paramValues"></param>
        private IDataParameter[] AssignParameterValues(object[] paramValues)
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

        /// <summary>
        /// 根据DataReader生成DataTable，轻量级。      
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static DataTable DataReaderToDataTable(IDataReader reader)
        {
            DataTable dataTable = new DataTable();
            int fieldCount = reader.FieldCount;
            for (int i = 0; i <= fieldCount - 1; i++)
            {
                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }
            //populate datatable
            dataTable.BeginLoadData();
            object[] fieldValues = new object[fieldCount];
            while (reader.Read())
            {
                reader.GetValues(fieldValues);
                dataTable.LoadDataRow(fieldValues, true);
            }
            dataTable.EndLoadData();
            return dataTable;
        }
        #endregion
    }
}
