using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Text;
using System.Reflection;

namespace HongYang.Enterprise.Logging.AdoNet
{
    /// <summary>
    /// OracleDBFactory产生Oracle相关的数据库操作对象。
    /// </summary>
    [Serializable]
    internal sealed class OracleDatabase : Database
    {
        public OracleDatabase() : base()
        {
        }

        /// <summary>
        /// 私有的无参数构造函数。
        /// </summary>
        public OracleDatabase(string connstr) : base(connstr)
        {

        }

        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <param name="connString">数据库连接字符串</param>
        /// <returns>指定数据源的连接对象</returns>
        public override IDbConnection GetDBConnection(string connString)
        {
            return new OracleConnection(connString);
        }

        public override IDataParameter GetDataParameter(object value)
        {
            return new OracleParameter() { Value = value };
        }


        public override IDataParameter GetDataParameter(string param_name, object value, DbType type, ParameterDirection direction)
        {
            OracleParameter op = new OracleParameter();
            op.ParameterName = param_name;
            op.Value = value;
            op.Direction = direction;

            return op;
        }

        /// <summary>
        /// 创建与指定数据源连接相关联的SQL命令对象。
        /// </summary>
        /// <param name="dbConn">数据库连接</param>
        /// <returns></returns>
        public override IDbCommand GetDBCommand(IDbConnection dbConn)
        {
            try
            {
                return (dbConn as OracleConnection).CreateCommand();
            }
            catch (OracleException ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        /// <summary>
        /// 数据库适配器
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        public override IDbDataAdapter GetDataAdapter(IDbCommand dbCommand)
        {
            return new OracleDataAdapter((OracleCommand)dbCommand);
        }

        /// <summary>
        /// 获取指定表名的主键字段名称
        /// </summary>
        /// <param name="tableName">数据表名</param>
        /// <returns></returns>
        public override string GetTablePrimaryKey(string tableName)
        {
            try
            {
                string sqlText = @"select column_name
	                                 from user_cons_columns
                                    where constraint_name = (
                                      select constraint_name
		                                from user_constraints
		                               where table_name = :TableName
			                             and constraint_type = 'P')";
                object value = ExecuteScalar(sqlText, new { TableName = tableName});
                return value != null ? value.ToString() : string.Empty;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        /// <summary>
        /// 更新CLOB
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <param name="filed">字段名称</param>
        /// <param name="where">条件</param>
        /// <param name="value">字段值</param>
        /// <returns></returns>
        public override int UpdateClob(string tablename, string filed, string where, object value)
        {
            try
            {
                string sqlText = $"update {tablename} set {filed} = :Output where {where}";
                return ExecuteNonQuery(sqlText, new { Output = value });
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        /// <summary>
        /// 参数化新增SQL
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override string InsertSQLByParameter<T>(T entity)
        {
            try
            {
                Type t = typeof(T);
                StringBuilder strColumns = new StringBuilder();
                StringBuilder strValues = new StringBuilder();
                PropertyInfo[] propertys = t.GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    if (IsNoMapKey(pi)
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
                    "insert into {0}({1}) values({2})", t.Name,
                    strColumns.ToString().TrimEnd(','),
                    strValues.ToString().TrimEnd(','));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取固定属性的值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public object GetValue<T>(string propertyName, T entity)
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
        /// 是否是非数据表字段 通过实体字段通过[NotMapped]标记
        /// </summary>
        /// <param name="pi">PropertyInfo</param>
        /// <returns></returns>
        public bool IsNoMapKey(PropertyInfo pi)
        {
            object[] attrs = pi.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                //判断NotMapped属性，看是否非数据库字段
                if ("System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute" == attr.GetType().FullName)
                {
                    if (!((Attribute)(attr)).IsDefaultAttribute())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
