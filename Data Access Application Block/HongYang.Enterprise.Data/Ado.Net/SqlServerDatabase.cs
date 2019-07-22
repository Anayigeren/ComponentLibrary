using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace HongYang.Enterprise.Data.AdoNet
{
    /// <summary>
    /// OracleDBFactory产生Oracle相关的数据库操作对象。
    /// </summary>
    [Serializable]
    public sealed class SqlServerDatabase : Database
    {
        #region 受保护的构造函数
        /// <summary>
        /// 私有的无参数构造函数。
        /// </summary>
        public SqlServerDatabase(   )
            : base( )
        {
        }


        /// <summary>
        /// 私有的无参数构造函数。
        /// </summary>
        public SqlServerDatabase(string connstr)
            : base(connstr)
        {
        }

        #endregion

        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <param name="connString">数据库连接字符串</param>
        /// <returns>指定数据源的连接对象</returns>
        public override IDbConnection GetDBConnection(string connString)
        {      
            SqlConnection dbConn = new SqlConnection(connString);
            return (IDbConnection)dbConn;
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
                return dbConn.CreateCommand();
            }
            catch (SqlException ex)
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
            return new SqlDataAdapter((SqlCommand)dbCommand);
        }


        public override IDataParameter GetDataParameter(object value)
        {
            SqlParameter op = new SqlParameter();
            op.Value = value;
            return op;
        }

        public override IDataParameter GetDataParameter(string param_name, object value, DbType type, ParameterDirection direction)
        {
            SqlParameter op = new SqlParameter();
            op.ParameterName = param_name;
            op.Value = value;
            op.DbType = type;
            op.Direction = direction;

            return op;
        }

        public override string GetTablePrimaryKey(string tableName)
        {
            throw new NotImplementedException("SqlServerDatabase未实现GetTablePrimaryKey方法");
        }

        public override int UpdateClob(string tablename, string filed, string where, object value)
        {
            throw new NotImplementedException("SqlServerDatabase未实现UpdateClob方法");
        }
    }
}
