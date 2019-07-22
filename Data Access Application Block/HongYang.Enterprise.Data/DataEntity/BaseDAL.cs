using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using HongYang.Enterprise.Logging;

namespace HongYang.Enterprise.Data.DataEntity
{
    /// <summary>
    /// 数据库操作抽象基类
    /// </summary>
    public abstract class BaseDAL
    {
        /// <summary>
        /// 默认超时时间(秒数)
        /// </summary>
        public const int DEFAULTTIMEOUT = 60;

        /// <summary>
        /// 创建连接对象，并打开连接        
        /// </summary>
        /// <returns></returns>
        public virtual IDbConnection CreateConnectionAndOpen()
        {
            try
            {
                IDbConnection connection = CreateConnection();
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                LogHelper.Write("执行BaseDAL.CreateConnectionAndOpen())失败。\n" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 创建连接对象，子类需要重写，根据自己的数据库类型实例化自己的数据库连接对象(支持IDbConnection接口)
        /// Dapper可以在所有Ado.net Providers下工作，包括sqlite, sqlce, firebird, oracle, MySQL, PostgreSQL and SQL Server。
        /// </summary>
        /// <returns></returns>
        protected abstract IDbConnection CreateConnection();
    }
}
