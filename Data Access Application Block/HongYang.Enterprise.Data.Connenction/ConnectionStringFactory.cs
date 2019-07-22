using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
namespace HongYang.Enterprise.Data.Connenction
{
    /// <summary>
    /// 维护数据库连接工厂
    /// 加解密，同时使用单例模式
    /// 只允许生成一个实例
    /// </summary>
    public class ConnectionStringFactory
    {
        private static BaseConnectionString _instance;

        public static BaseConnectionString CreateInstance(ConnectionConfigType configType = ConnectionConfigType.xml)
        {
            if (_instance != null)
            {
                return _instance;
            }

            switch (configType)
            {
                case ConnectionConfigType.xml:
                    _instance = new ConnectionString();
                    break;

                case ConnectionConfigType.json:
                    _instance = new ConnectionStringJson();
                    break;

                default:
                    _instance = new ConnectionString();
                    break;
            }

            return _instance;
        }
    }
}
