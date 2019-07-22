using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace HongYang.Enterprise.Data.AdoNet
{
    /// <summary>
    /// OracleDBFactory产生Oracle相关的数据库操作对象。
    /// </summary>
    [Serializable]
    public sealed class OracleDatabase : Database
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
            //if (type == DbType.String)
            //    op.OracleType = OracleType.VarChar;
            //else if (type == DbType.Decimal)
            //    op.OracleType = OracleType.Number;
            //else if (type == DbType.DateTime)
            //    op.OracleType = OracleType.DateTime;
            //else
            //    op.OracleType = OracleType.VarChar;     
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
                string sqlText = @"select column_name from user_cons_columns where constraint_name = 
                    (select constraint_name from user_constraints where table_name = :TableName and constraint_type = 'P')";
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
    }
}
