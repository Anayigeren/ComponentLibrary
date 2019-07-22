using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class ConnectionString : BaseConnectionString
    {
        /// <summary>
        /// 初始化时的配置链接
        /// 
        /// </summary>
        public ConnectionString(): base()
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory;
            CONFIG_FILE_PATH = System.IO.Path.Combine(dirPath, ConnectionConst.CONFIG_XML_FILE_NAME);
            if (!System.IO.File.Exists(CONFIG_FILE_PATH))
            {
                throw new Exception(ConnectionConst.CONFIG_FILE_NOFOUND + CONFIG_FILE_PATH);
            }

            ValidationAndInitConfig();
        }

        /// <summary>
        /// 验证并初始化连接项
        /// </summary>
        public override void ValidationAndInitConfig()
        {
            _parser.Clear();
            var doc = XElement.Load(CONFIG_FILE_PATH);
            var _items = doc.Elements(ConnectionConst.CHILD);
            foreach (XElement child in _items)
            {
                XAttribute sysname = child.Attribute(ConnectionConst.SYSNAME);
                XAttribute connstr = child.Attribute(ConnectionConst.CONNSTR);
                XAttribute encrypt = child.Attribute(ConnectionConst.ENCRYPT);
                XAttribute dbtype = child.Attribute(ConnectionConst.DBTYPE);
                XAttribute provide = child.Attribute(ConnectionConst.PROVIDE);

                //数据库配置重复
                if (sysname == null)
                {
                    throw new Exception(ConnectionConst.CONFIG_NONAME_NODE);
                }

                if (_parser.ContainsKey(sysname.Value))
                {
                    throw new Exception(ConnectionConst.CONFIG_REPETITION_ITEM + "[" + sysname.Value + "]");
                }

                if (connstr == null)
                {
                    throw new Exception(string.Format(ConnectionConst.CONFIG_LOST_ITEM, "connectionstring"));
                }
                if (sysname == null)
                {
                    throw new Exception(string.Format(ConnectionConst.CONFIG_LOST_ITEM, "name"));
                }
                if (encrypt == null)
                {
                    throw new Exception(string.Format(ConnectionConst.CONFIG_LOST_ITEM, "encrypt"));
                }
                if (provide == null)
                {
                    throw new Exception(string.Format(ConnectionConst.CONFIG_LOST_ITEM, "provide"));
                }
                //有了provide 就不需要databasetype
                if (string.IsNullOrEmpty(provide.Value))
                {
                    throw new Exception(ConnectionConst.CONFIG_PROVIDE_ERROR + "[" + sysname.Value + "]");
                }

                _parser.Add(sysname.Value, new ConnectionStringItem()
                {
                    SystemName = sysname.Value,
                    EncryptConnStr = connstr.Value,
                    Encrypt = encrypt.Value == "True",
                    provide = provide.Value
                });
            }
        }
    }
}
