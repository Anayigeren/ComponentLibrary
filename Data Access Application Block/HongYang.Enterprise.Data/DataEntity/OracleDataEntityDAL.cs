using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using HongYang.Enterprise.Logging;
using HongYang.Enterprise.Data.Connenction;
using HongYang.Enterprise.Data.AdoNet;

namespace HongYang.Enterprise.Data.DataEntity
{
    /// <summary>
    /// 创 建 人：林志斌
    /// 创建时间：2019/07/22
    /// 功    能：Oracle实体对象的数据访问层，内部可创建数据库连接对象 来扩展Dapper的支持
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class OracleDataEntityDAL<T> : BaseDAL where T : class, new()
    {
        /// <summary>
        /// 错误消息
        /// </summary>
        public Msg error = new Msg();

        /// <summary>
        /// 数据库连接名称
        /// </summary>
        protected string _dbName = string.Empty;

        /// <summary>
        /// 带年份表的年份
        /// </summary>
        protected string _year = string.Empty;

        /// <summary>
        /// 是否开启日志跟踪（默认开启）
        /// 调用DBHelper的方法不在DBHelper中写日志而交于本层来写，避免重复写日志，对数据库操作写日志
        /// </summary>
        protected DBTrack _track = DBTrack.Open;

        /// <summary>
        /// 实体类型
        /// </summary>
        readonly Type t = typeof(T);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbName">数据库连接名称</param>
        /// <returns></returns>
        public OracleDataEntityDAL(string dbName)
        {
            this._dbName = dbName;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbName">数据库连接名称</param>
        /// <param name="year">带年份表的年份</param>
        public OracleDataEntityDAL(string dbName, string year)
        {
            this._year = year;
            this._dbName = dbName;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbName">数据库连接名称</param>
        /// <param name="track">是否开启日志跟踪</param>
        /// <param name="year">带年份表的年份</param>
        public OracleDataEntityDAL(string dbName, DBTrack track, string year = "")
        {
            _year = year;
            _track = track;
            _dbName = dbName;
        }

        /// <summary>
        /// 创建连接对象，开放一些Dapper提供的数据操作
        /// </summary>
        /// <returns></returns>
        protected override IDbConnection CreateConnection()
        {
            var connectionStringItem = DatabaseFactory.GetConnectionStringItem(_dbName);
            IDbConnection connection = new OracleConnection(connectionStringItem.ConnStr);
            return connection;
        }


        #region  CRUD函数
        /// <summary>
        /// 插入 entity 记录.
        /// </summary>
        /// <param name="entity">entity 类对象</param>
        /// <param name="commandTimeout">执行超时时间</param>
        /// <returns></returns>
        public virtual int Insert(T entity, IDbTransaction transaction = null, int commandTimeout = DEFAULTTIMEOUT)
        {
            try
            {
                return DBHelper.ExecuteSQLHelper(
                    InsertSQLByParameter(entity), entity, _dbName, error, DBTrack.Close, transaction, commandTimeout);
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.Insert({t.Name})");
            }

            return 0;
        }

        /// <summary>
        /// 批量插入 entity 记录.
        /// </summary>
        /// <param name="entityList">一组 entity 类对象</param>
        /// <param name="commandTimeout">执行超时时间</param>
        /// </summary>
        public virtual bool InsertList(List<T> entityList, int commandTimeout = DEFAULTTIMEOUT)
        {
            try
            {
                return DBHelper.TranscationExecute((db) =>
                {
                    foreach (T entity in entityList)
                    {
                        int _t = db.Execute(InsertSQLByParameter(entity), entity, null, commandTimeout, CommandType.Text);
                        if (_t == 0)
                        {
                            return false;
                        }
                    }

                    return true;
                }, _dbName, error, DBTrack.Close);
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.InsertList({t.Name})");
            }

            return false;
        }

        /// <summary>
        /// 更新 entity 类对象
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="customSQL">自定义条件sql</param>
        /// <param name="noUpdateFileds">排除更新字段</param>
        /// <param name="updateFileds">只更新的字段</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">执行超时时间（默认1min）</param>
        /// <returns></returns>
        public virtual int Update(T entity, string customSQL = "", List<string> noUpdateFileds = null, List<string> updateFileds = null, IDbTransaction transaction = null, int commandTimeout = DEFAULTTIMEOUT)
        {
            int ret = 0;
            try
            {
                ret = DBHelper.ExecuteSQLHelper(
                    UpdateSQLByParameter(entity, customSQL, noUpdateFileds, updateFileds), entity,
                    _dbName, error, DBTrack.Close, transaction, commandTimeout);
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.Update({t.Name})");
            }

            return ret;
        }

        /// <summary>
        /// 批量插入 entity 记录.
        /// </summary>
        /// <param name="entityList">一组 entity 类对象</param>
        /// <param name="commandTimeout">执行超时时间</param>
        /// </summary>
        public virtual bool UpdateList(List<T> entityList, int commandTimeout = DEFAULTTIMEOUT)
        {
            try
            {
                return DBHelper.TranscationExecute((db) =>
                {
                    foreach (T entity in entityList)
                    {
                        int _t = db.Execute(UpdateSQLByParameter(entity), entity, null, commandTimeout, CommandType.Text);
                        if (_t == 0)
                        {
                            return false;
                        }
                    }

                    return true;
                }, _dbName, error, DBTrack.Close);
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.UpdateList({t.Name})");
            }

            return false;
        }

        /// <summary>
        /// 根据主键值删除记录.
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="commandTimeout">执行超时时间</param>
        /// <returns></returns>
        public virtual int Delete(string id, int commandTimeout = DEFAULTTIMEOUT)
        {
            int ret = 0;
            try
            {
                string retMessage = string.Empty;
                ret = DBHelper.ExecuteSQLHelper(
                    DeleteSQLByParameter(id, out DynamicParameters param),
                    param, _dbName, ref retMessage, DBTrack.Close, null, commandTimeout);
                if (ret == 0)
                {
                    error.Result = false;
                    error.AddMsg($"0行被删除。\n{retMessage}");
                }
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.Delete({t.Name})");
            }

            return ret;
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="idList">主键列表</param>
        /// <param name="commandTimeout">执行超时时间</param>
        /// <returns></returns>
        public virtual bool DeleteList(List<string> idList, int commandTimeout = DEFAULTTIMEOUT)
        {
            try
            {
                return DBHelper.TranscationExecute((db) =>
                {
                    foreach (string id in idList)
                    {
                        int _t = db.Execute(DeleteSQLByParameter(id, out DynamicParameters param), param, null, commandTimeout, CommandType.Text);
                        if (_t == 0) // 如果受影响的行数为0则事务提交失败
                        {
                            return false;
                        }
                    }

                    return true;
                }, _dbName, error, DBTrack.Close);
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.DeleteList({t.Name})");
            }

            return false;
        }

        /// <summary>
        /// 批量执行与entity相关的SQL，未使用参数化（待完善）
        /// </summary>
        /// <param name="InsertList">插入实体列表</param>
        /// <param name="UpdateList">更新实体列表</param>
        /// <param name="DeleteList">删除实体列表</param>
        /// <param name="SqlList">sql列表</param>
        /// <param name="allowNull">是否允许0行影响</param>
        /// <returns></returns>
        public virtual bool TranscationExecute(
            List<T> InsertList,
            List<T> UpdateList,
            List<T> DeleteList,
            List<string> SqlList,
            bool allowNull = false)
        {
            bool ret = false;
            List<string> sqlArr = new List<string>();
            try
            {
                if (null != InsertList)
                {
                    foreach (T entity in InsertList)
                    {
                        string _SqlText = InsertSQL(entity);
                        sqlArr.Add(_SqlText);
                    }
                }
                if (null != UpdateList)
                {
                    foreach (T entity in UpdateList)
                    {
                        string _SqlText = UpdateSQL(entity);
                        sqlArr.Add(_SqlText);
                    }
                }
                if (null != DeleteList)
                {
                    foreach (T entity in DeleteList)
                    {
                        string _SqlText = DeleteSQL(GetValue(PrimaryKey(entity), entity).ToString());
                        sqlArr.Add(_SqlText);
                    }
                }

                if (null != SqlList)
                {
                    sqlArr.AddRange(SqlList);
                }

                ret = DBHelper.TranscationExecute(sqlArr, _dbName, error, allowNull, DBTrack.Close);
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.TranscationExecute()");
            }
            return ret;
        }

        /// <summary>
        /// 获取 entity 类对象.
        /// </summary>
        /// <param name="id">主键id，实体上标注[key]的字段值</param>
        /// <param name="commandTimeout">执行超时时间</param>
        /// <returns></returns>
        public virtual T Load(string id, int commandTimeout = DEFAULTTIMEOUT)
        {
            T entity = default(T);
            var conn = CreateConnectionAndOpen();
            try
            {
                DynamicParameters Parameters = new DynamicParameters();
                string sqlText = LoadSQLByParameter(id, out DynamicParameters param);
                return conn.QueryFirst<T>(sqlText, param, null, commandTimeout);
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.Load({t.Name})");
            }
            finally
            {
                conn.CloseIfOpen();
            }

            return entity;
        }

        /// <summary>
        /// 查询返回 DataTable 全部数据
        /// </summary>
        public virtual DataTable GetDataTable()
        {
            T entity = default(T);
            return this.GetDataTable(entity);
        }

        /// <summary>
        /// 查询返回 DataTable
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="orderBy">排序 eg: id desc</param>
        /// <param name="dataPage">分页数据</param>
        /// <param name="commandTimeout">操作超时时间（秒）</param>
        /// <returns>DataTable</returns>
        public virtual DataTable GetDataTable(T entity, string orderBy = "", DataPage dataPage = null, int commandTimeout = DEFAULTTIMEOUT)
        {
            try
            {
                string sqlstr = ListSQLByParameter(entity, out DynamicParameters param, orderBy, dataPage);
                return DBHelper.SqlHelperGetDataTable(sqlstr, param, _dbName, error, DBTrack.Close, commandTimeout);
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.GetDataTable({t.Name})");
            }

            return null;

        }

        /// <summary>
        /// 查询返回 IDataReader
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="orderBy">排序 eg: id desc</param>
        /// <param name="dataPage">分页数据</param>
        /// <param name="commandTimeout">操作超时时间（秒）</param>
        /// <returns>IDataReader</returns>
        public virtual IDataReader GetDataReader(T entity, string orderBy = "", DataPage dataPage = null, int commandTimeout = DEFAULTTIMEOUT)
        {
            var conn = CreateConnectionAndOpen();
            try
            {
                string sqlstr = ListSQLByParameter(entity, out DynamicParameters param, orderBy, dataPage);
                return conn.ExecuteReader(sqlstr, param, null, commandTimeout);
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.GetDataTable({t.Name})");
            }
            finally
            {
                conn.CloseIfOpen();
            }

            return null;
        }

        /// <summary>
        /// 返回一组 entity 类对象.
        /// </summary>
        /// <param name="entity">entity 类对象</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="dataPage">分页数据</param>
        /// <param name="commandTimeout">超时时间（秒）</param>
        /// <param name="buffered">默认值改为true，一次性取完断开连接。如果想自行一笔一笔取，可设置为false。</param>
        /// <returns></returns>
        public virtual List<T> List(T entity, string orderBy = "", DataPage dataPage = null, int commandTimeout = DEFAULTTIMEOUT, bool buffered = true)
        {
            var conn = CreateConnectionAndOpen();
            try
            {
                string sqlstr = ListSQLByParameter(entity, out DynamicParameters param, orderBy, dataPage);
                return conn.Query<T>(sqlstr, param, null, buffered, commandTimeout).AsList();
            }
            catch (Exception ex)
            {
                ex.DALExceptionHandler(error, _track, $"OracleDataEntityDAL.List({t.Name})");
            }
            finally
            {
                conn.CloseIfOpen();
            }

            return null;
        }

        /// <summary>
        /// 返回一组 entity 类对象(重载).
        /// </summary>
        public virtual List<T> List()
        {
            T entity = default(T);
            return this.List(entity);
        }
        #endregion


        #region  SQL语句生成，异常直接抛
        /// <summary>
        /// 返回实体插入的SQL，生成参数化sql
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual string InsertSQL(T entity)
        {
            try
            {
                StringBuilder strColumns = new StringBuilder();
                StringBuilder strValues = new StringBuilder();
                PropertyInfo[] propertys = t.GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    if (pi.IsNoMapKey()
                        || pi.Name.ToUpper().Contains("_MAX")
                        || pi.Name.ToUpper().Contains("_MIN"))
                    {
                        // 未映射字段、_MAX和_MIN做为查询关键字不做为插入字段，因此过滤掉
                        continue;
                    }

                    object value = GetValue(pi.Name, entity);
                    if (value == null
                        || string.IsNullOrEmpty(value.ToString()))
                    {
                        // 字段值为空
                        continue;
                    }

                    strColumns.Append($"{pi.Name},");
                    if (pi.PropertyType == typeof(Nullable<DateTime>)
                        && value != null
                        && !string.IsNullOrEmpty(value.ToString()))
                    {
                        // 不为空的时间类型不做绑定变量
                        strValues.Append($"to_date('{value.ToString()}','yyyy-mm-dd hh24:mi:ss'),");
                    }
                    else
                    {
                        strValues.Append($"'{value.ToString().FilterChar()}',");
                    }
                }

                return string.Format(
                    "insert into {0}{1}({2}) values({3})",
                    t.Name,
                    _year,
                    strColumns.ToString().TrimEnd(','),
                    strValues.ToString().TrimEnd(','));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 参数化新增SQL
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual string InsertSQLByParameter(T entity)
        {
            try
            {
                StringBuilder strColumns = new StringBuilder();
                StringBuilder strValues = new StringBuilder();
                PropertyInfo[] propertys = t.GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    if (pi.IsNoMapKey()
                        || pi.Name.ToUpper().Contains("_MAX")
                        || pi.Name.ToUpper().Contains("_MIN"))
                    {
                        // 未映射字段、_MAX和_MIN做为查询关键字不做为插入字段，因此过滤掉
                        continue;
                    }

                    object value = GetValue(pi.Name, entity);
                    if (value == null
                        || string.IsNullOrEmpty(value.ToString()))
                    {
                        // 字段值为空
                        continue;
                    }

                    strColumns.Append($"{pi.Name},");
                    if (pi.PropertyType == typeof(Nullable<DateTime>)
                        && value != null
                        && !string.IsNullOrEmpty(value.ToString()))
                    {
                        // 不为空的时间类型不做绑定变量
                        strValues.Append($"to_date('{value.ToString()}','yyyy-mm-dd hh24:mi:ss'),");
                    }
                    else
                    {
                        strValues.Append($":{pi.Name},");
                    }
                }

                return string.Format(
                    "insert into {0}{1}({2}) values({3})", t.Name, _year,
                    strColumns.ToString().TrimEnd(','),
                    strValues.ToString().TrimEnd(','));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新SQL
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="customSQL">自定义条件</param>
        /// <param name="noUpdateFileds">不需要更新字段, 字段区分大小写，要严格按照实体字段名</param>
        /// <param name="updateFileds">只需要更新字段</param>
        /// <returns>sql</returns>
        public virtual string UpdateSQL(T entity, string customSQL = "", List<string> noUpdateFileds = null, List<string> updateFileds = null)
        {
            string primaryKey = PrimaryKey(entity);
            object pkvalue = GetValue(primaryKey, entity);
            if (pkvalue == null)
                throw new Exception($"主键{primaryKey}不能为空");
            StringBuilder strValues = new StringBuilder();
            PropertyInfo[] propertys = t.GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                if (pi.IsNoMapKey()
                    || primaryKey.ToLower().Equals(pi.Name.ToLower())
                    || pi.Name.ToUpper().Contains("_MAX")
                    || pi.Name.ToUpper().Contains("_MIN"))
                {
                    // 未映射字段、主键、_MAX和_MIN做为查询关键字不做为更新字段，因此过滤掉
                    continue;
                }

                if ((noUpdateFileds != null && noUpdateFileds.Contains(pi.Name))
                    || (updateFileds != null && !updateFileds.Contains(pi.Name)))
                {
                    // 排除不更新字段 或 不在指定更新字段
                    continue;
                }

                object value = GetValue(pi.Name, entity);
                value = value == null || string.IsNullOrEmpty(value.ToString()) ? "" : value;
                if (pi.PropertyType == typeof(Nullable<DateTime>)
                    && value != null
                    && !string.IsNullOrEmpty(value.ToString()))
                {
                    // 不为空的时间类型不做绑定变量
                    strValues.Append($"{pi.Name}=to_date('{value.ToString()}','yyyy-mm-dd hh24:mi:ss'),");
                }
                else
                {
                    strValues.Append($"{pi.Name}='{value.ToString().FilterChar()}',");
                }
            }

            if (strValues.Length == 0)
            {
                throw new Exception("生成更新语句出错,请检查noUpdateFileds或updateFileds参数的大小写正确性");
            }

            return string.Format(
                    "update {0} set {1} where {2}='{3}' {4}",
                    t.Name + _year,
                    strValues.ToString().TrimEnd(','),
                    primaryKey,
                    pkvalue.ToString(),
                    customSQL);
        }

        /// <summary>
        /// 参数化更新SQL
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="customSQL">自定义条件</param>
        /// <param name="noUpdateFileds">不需要更新字段</param>
        /// <param name="updateFileds">只需要更新字段</param>
        /// <returns>sql</returns>
        public virtual string UpdateSQLByParameter(T entity, string customSQL = "", List<string> noUpdateFileds = null, List<string> updateFileds = null)
        {
            try
            {
                string primaryKey = PrimaryKey(entity);
                object pkvalue = GetValue(primaryKey, entity);
                if (pkvalue == null)
                    throw new Exception($"主键{primaryKey}不能为空");
                StringBuilder strValues = new StringBuilder();
                PropertyInfo[] propertys = t.GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    if (pi.IsNoMapKey()
                        || primaryKey.ToLower().Equals(pi.Name.ToLower())
                        || pi.Name.ToUpper().Contains("_MAX")
                        || pi.Name.ToUpper().Contains("_MIN"))
                    {
                        // 未映射字段、主键、_MAX和_MIN做为查询关键字不做为更新字段，因此过滤掉
                        continue;
                    }

                    if ((noUpdateFileds != null && noUpdateFileds.Contains(pi.Name))
                        || (updateFileds != null && !updateFileds.Contains(pi.Name)))
                    {
                        // 排除不更新字段 或 不在指定更新字段
                        continue;
                    }

                    object value = GetValue(pi.Name, entity);
                    value = value == null || string.IsNullOrEmpty(value.ToString()) ? "" : value;
                    if (pi.PropertyType == typeof(Nullable<DateTime>)
                        && value != null
                        && !string.IsNullOrEmpty(value.ToString()))
                    {
                        // 不为空的时间类型不做绑定变量
                        strValues.Append($"{pi.Name}=to_date('{value.ToString()}','yyyy-mm-dd hh24:mi:ss'),");
                    }
                    else
                    {
                        strValues.Append($"{pi.Name}=:{pi.Name},");
                    }
                }

                if (strValues.Length == 0)
                {
                    throw new Exception("生成更新语句出错,请检查noUpdateFileds或updateFileds参数的大小写正确性");
                }
                return string.Format(
                        "update {0} set {1} where {2}=: {3} {4}",
                        t.Name + _year,
                        strValues.ToString().TrimEnd(','),
                        primaryKey,
                        primaryKey,
                        customSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除SQL
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns>SQL</returns>
        public virtual string DeleteSQL(string id)
        {
            T entity = new T();
            string primaryKey = PrimaryKey(entity);
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                throw new Exception("主键字段不存在");
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("传入主键值不能为空");
            }

            return $"delete from {t.Name}{_year} where 1=1 and {primaryKey} = '{id}'";
        }

        /// <summary>
        /// 参数化删除SQL
        /// </summary>
        /// <param name="entity">实体，只要主键不为空</param>
        /// <returns>SQL</returns>
        public virtual string DeleteSQLByParameter(string id, out DynamicParameters param)
        {
            var entity = new T();
            string primaryKey = PrimaryKey(entity);
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                throw new Exception("主键字段不存在");
            }

            if (id == null || string.IsNullOrEmpty(id.ToString()))
            {
                throw new Exception("传入主键值不能为空");
            }

            param = new DynamicParameters();
            param.Add(primaryKey, id);
            return $"delete from {t.Name}{_year} where 1=1 and {primaryKey} =:{primaryKey}";
        }

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual string LoadSQL(string id)
        {
            var entity = new T();
            return $"select * from {t.Name}{_year} where 1=1 and {PrimaryKey(entity)}='{id.FilterChar()}'";
        }

        /// <summary>
        /// 参数化主键查询SQL
        /// </summary>
        /// <param name="id">主键</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public virtual string LoadSQLByParameter(string id, out DynamicParameters param)
        {
            var entity = new T();
            string primaryKey = PrimaryKey(entity);
            param = new DynamicParameters();
            param.Add(primaryKey, id);
            return $"select * from {t.Name}{_year} where 1=1 and {PrimaryKey(entity)}=:{PrimaryKey(entity)}";
        }

        /// <summary>
        /// 查询集合SQL
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="orderBy">排序 eg: id desc</param>
        /// <param name="dataPage">分页参数</param>
        /// <returns>SQL</returns>
        public virtual string ListSQL(T entity, string orderBy = "", DataPage dataPage = null)
        {
            try
            {
                Query query = new Query()
                {
                    TableName = t.Name + _year,
                    Order = orderBy
                };
                StringBuilder sqlBuilder = new StringBuilder();
                if (entity == null)
                {
                    // 全表查询
                    return PageHelper.PageSQL(query);
                }

                PropertyInfo[] propertys = t.GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    object value = GetValue(pi.Name, entity);
                    if (pi.IsNoMapKey()
                        || value == null
                        || string.IsNullOrEmpty(value.ToString()))
                    {
                        // 过滤非数据表字段和空值
                        continue;
                    }
                    if (pi.PropertyType == typeof(Nullable<DateTime>))
                    {
                        // 可空时间类型字段处理
                        if (pi.Name.ToUpper().Contains("_MAX"))
                        {
                            // 最大时间查询
                            sqlBuilder.AppendFormat(
                                " and {0} <= to_date('{1}','yyyy-mm-dd hh24:mi:ss')",
                                pi.Name.ToUpper().Replace("_MAX", ""),
                                GetValue(pi.Name, entity).ToString().FilterChar());
                        }
                        else if (pi.Name.ToUpper().Contains("_MIN"))
                        {
                            // 最小时间查询
                            sqlBuilder.AppendFormat(
                                " and {0} >= to_date('{1}','yyyy-mm-dd hh24:mi:ss')",
                                pi.Name.ToUpper().Replace("_MIN", ""),
                                GetValue(pi.Name, entity).ToString().FilterChar());
                        }
                        else
                        {
                            sqlBuilder.AppendFormat(
                                " and {0} = to_date('{1}','yyyy-mm-dd hh24:mi:ss')",
                                pi.Name,
                                GetValue(pi.Name, entity).ToString().FilterChar());
                        }
                    }
                    else
                    {
                        if (pi.Name.ToUpper().Contains("_MAX"))
                        {
                            // 最大值查询
                            sqlBuilder.AppendFormat(" and {0}<='{1}'", pi.Name.ToUpper().Replace("_MAX", ""), value.ToString().FilterChar());
                        }
                        else if (pi.Name.ToUpper().Contains("_MIN"))
                        {
                            // 最小值查询
                            sqlBuilder.AppendFormat(" and {0}>='{1}'", pi.Name.ToUpper().Replace("_MIN", ""), value.ToString().FilterChar());
                        }
                        else
                        {
                            // 普通字段
                            sqlBuilder.AppendFormat(" and {0}='{1}'", pi.Name, value.ToString().FilterChar());
                        }
                    }
                }

                query.Filter = sqlBuilder.ToString();
                if (dataPage != null)
                {
                    //分页处理
                    IPageSql pageSql = PageSqlFactory.GetPageSql(_dbName);
                    return pageSql.PageSql(dataPage, query);
                }

                return PageHelper.PageSQL(query);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 参数化查询集合SQL
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="parameters">动态参数</param>
        /// <param name="orderBy">排序 eg: id desc</param>
        /// <param name="dataPage">分页参数</param>
        /// <returns>SQL</returns>
        public virtual string ListSQLByParameter(T entity, out DynamicParameters parameters, string orderBy = "", DataPage dataPage = null)
        {
            try
            {
                parameters = new DynamicParameters();
                Query query = new Query()
                {
                    TableName = t.Name + _year,
                    Order = orderBy
                };
                StringBuilder sqlBuilder = new StringBuilder();
                if (entity == null)
                {
                    // 全表查询
                    return PageHelper.PageSQL(query);
                }

                PropertyInfo[] propertys = t.GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    object value = GetValue(pi.Name, entity);
                    if (pi.IsNoMapKey()
                        || value == null
                        || string.IsNullOrEmpty(value.ToString()))
                    {
                        // 过滤非数据表字段和空值
                        continue;
                    }
                    if (pi.PropertyType == typeof(Nullable<DateTime>))
                    {
                        // 可空时间类型字段处理
                        if (pi.Name.ToUpper().Contains("_MAX"))
                        {
                            // 最大时间查询
                            sqlBuilder.AppendFormat(
                                " and {0} <= to_date('{1}','yyyy-mm-dd hh24:mi:ss')",
                                pi.Name.ToUpper().Replace("_MAX", ""),
                                GetValue(pi.Name, entity).ToString().FilterChar());
                        }
                        else if (pi.Name.ToUpper().Contains("_MIN"))
                        {
                            // 最小时间查询
                            sqlBuilder.AppendFormat(
                                " and {0} >= to_date('{1}','yyyy-mm-dd hh24:mi:ss')",
                                pi.Name.ToUpper().Replace("_MIN", ""),
                                GetValue(pi.Name, entity).ToString().FilterChar());
                        }
                        else
                        {
                            sqlBuilder.AppendFormat(
                                " and {0} = to_date('{1}','yyyy-mm-dd hh24:mi:ss')",
                                pi.Name,
                                GetValue(pi.Name, entity).ToString().FilterChar());
                        }
                    }
                    else
                    {
                        string piName = pi.Name.ToUpper();
                        if (piName.Contains("_MAX"))
                        {
                            // 最大值查询
                            piName = piName.Replace("_MAX", "");
                            sqlBuilder.AppendFormat(" and {0}<=:{0}", piName);
                        }
                        else if (pi.Name.ToUpper().Contains("_MIN"))
                        {
                            // 最小值查询
                            piName = piName.Replace("_MIN", "");
                            sqlBuilder.AppendFormat(" and {0}>=:{0}", piName);
                        }
                        else
                        {
                            // 普通字段
                            sqlBuilder.AppendFormat(" and {0}=:{0}", pi.Name);
                        }

                        parameters.Add(piName, value.ToString().FilterChar());
                    }
                }

                query.Filter = sqlBuilder.ToString();
                if (dataPage != null)
                {
                    //分页处理
                    IPageSql pageSql = PageSqlFactory.GetPageSql(_dbName);
                    return pageSql.PageSql(dataPage, query);
                }

                return PageHelper.PageSQL(query);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 创建参数化的动态参数列表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual DynamicParameters CreateDataDynamicParameters(T entity)
        {
            return DBHelper.CreateDataDynamicParameters<T>(entity, _track);
        }
        #endregion

        #region 公共函数
        /// <summary>
        /// 获取固定属性的值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public object GetValue(string propertyName, T entity)
        {
            Type type = entity.GetType();
            PropertyInfo pi = type.GetProperty(propertyName);
            if (pi != null && pi.CanRead)
            {
                return pi.GetValue(entity, null);
            }

            FieldInfo fi = type.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null)
            {
                return fi.GetValue(entity);
            }

            return null;
        }

        /// <summary>
        /// 设置指定属性的值
        /// </summary>
        /// <param name="properName"></param>
        /// <param name="value"></param>
        /// <param name="entity"></param>
        public void SetValue(string properName, string value, T entity)
        {
            PropertyInfo pro = entity.GetType().GetProperty(properName);
            pro.SetValue(entity, value.ChangeType(pro.PropertyType), null);
        }

        /// <summary>
        /// 主键字段名称
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public virtual string PrimaryKey(T entity)
        {
            try
            {
                Type type = entity.GetType();
                PropertyInfo[] propertys = t.GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    object[] attrs = pi.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        //判断KeyAttribute属性，获取主键字
                        if ("System.ComponentModel.DataAnnotations.KeyAttribute" == attr.GetType().FullName)
                        {
                            return pi.Name;
                        }
                    }
                }

                return "";
            }
            catch
            {
                return "";
            }
        }
        #endregion
    }
}
