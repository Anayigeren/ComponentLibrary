using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
namespace HongYang.Enterprise.Data.Connenction
{
    /// <summary>
    /// 维护数据库连接
    /// 加解密，同时使用单例模式
    /// 只允许生成一个实例
    /// </summary>
    public abstract class BaseConnectionString
    {
        /// <summary>
        /// 单例模式实例
        /// </summary>
        public static BaseConnectionString _instance = null;

        /// <summary>
        /// 配置文件的路径
        /// </summary>
        public static string CONFIG_FILE_PATH = "";

        /// <summary>
        /// 缓存所有数据库连接信息
        /// </summary>
        public static Dictionary<string, ConnectionStringItem> _parser = new Dictionary<string, ConnectionStringItem>();

        /// <summary>
        /// 不同的配置文件的初始化方式由子类实现
        /// </summary>
        public abstract void ValidationAndInitConfig();

        /// <summary>
        /// 构造函数
        /// </summary>
        public BaseConnectionString()
        {
        }

        /// <summary>
        /// 返回链接字符串
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        public ConnectionStringItem this[string systemName]
        {
            get
            {
                if (_parser.Count == 0)
                {
                    ValidationAndInitConfig();
                }

                if (!_parser.ContainsKey(systemName))
                {
                    throw new Exception(ConnectionConst.CONFIG_SYSTEM_NOFOUND);
                }

                return _parser[systemName];
            }
        }

        /// <summary>
        /// 强制重新刷新配置
        /// </summary>
        public void Refresh()
        {
            ValidationAndInitConfig();
        }
    }
}
