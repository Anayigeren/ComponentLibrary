using System;
using System.Web;
using System.Data;
using System.Collections;
using System.Reflection;
using HongYang.Enterprise.Data;
using HongYang.Enterprise.Data.Connenction;
using System.Collections.Generic;
namespace HongYang.Enterprise.Data.AdoNet
{
    /// <summary>
    /// 数据库抽象工厂摘要说明。
    /// </summary>
    [Serializable]
    public class DatabaseFactory
    {

        /// <summary>
        /// 用于存储非dll实例
        /// </summary>
        static Dictionary<string, Type> asslist = new Dictionary<string, Type>();

        /// <summary>
        /// 创建指定的子类的实例。
        /// </summary>
        /// <param name="assemblyQualifiedTypeName">子类的完全限定名。</param>
        /// <remarks>
        /// 应用程序不直接创建子类的实例，必须调用CreateInstance方法。
        /// </remarks>
        static Database CreateInstance(ConnectionStringItem dbConfig)
        {
            Type t = null;// Type.GetType(dbConfig.provide, false);
            if (dbConfig.provide.IndexOf(',') != -1)
            {
                if (!asslist.ContainsKey(dbConfig.provide))
                {
                    string dllName = dbConfig.provide.Split(',')[1];
                    string typeName = dbConfig.provide.Split(',')[0];
                    var ass = Assembly.Load(dllName);
                    asslist.Add(dbConfig.provide, ass.GetType(typeName));
                }
                t = asslist[dbConfig.provide];
            }
            else
            {
                t = Type.GetType(dbConfig.provide, false);
            }

            //if(t == null || dbConfig.provide == "HongYang.Enterprise.Data.AdoNet.MySQL")
            //{
            //    var path = "";
            //    //if (HttpContext.Current != null)
            //    //{
            //    //    path = HttpRuntime.AppDomainAppPath + "bin\\HongYang.Enterprise.Data.MySQL.dll";
            //    //}
            //    //else
            //    //{
            //    //    path = AppDomain.CurrentDomain.BaseDirectory + "HongYang.Enterprise.Data.MySQL.dll";
            //    //}

            //    Assembly assembly = Assembly.LoadFile(path);
            //    if(null != assembly)
            //    {
            //        t = assembly.GetType(dbConfig.provide);
            //    }
            //}

            if (t == null)
                throw new Exception("无法实例化" + dbConfig.provide);
            Database db = Activator.CreateInstance(t, dbConfig.ConnStr) as Database;
            return db;
        }

        /// <summary>
        /// 根据数据库名，生成数据库实例
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static Database GetDatabase(string database)
        {
            return CreateInstance(ConnectionStringFactory.CreateInstance(ConnectionConfigType.json)[database]);
        }

        /// <summary>
        /// 根据连接串，生成数据库实例
        /// </summary>
        /// <param name="connStr"></param>
        /// <param name="provide"></param>
        /// <returns></returns>
        public static Database GetDatabase(string connStr, string provide)
        {
            Type t = Type.GetType(provide, false);
            Database db = Activator.CreateInstance(t, connStr) as Database;
            return db;
        }

        /// <summary>
        /// 根据数据库名，返回连接串信息
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static ConnectionStringItem GetConnectionStringItem(string database)
        {
            return ConnectionStringFactory.CreateInstance(ConnectionConfigType.json)[database];
        }
    }
}
