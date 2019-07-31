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
    public class ConnectionStringJson : BaseConnectionString
    {
        /// <summary>
        /// 初始化时的配置链接
        /// </summary>
        public ConnectionStringJson(): base()
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory;
            CONFIG_FILE_PATH = System.IO.Path.Combine(dirPath, ConnectionConst.CONFIG_JSON_FILE_NAME);
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
            try
            {
                _parser.Clear();
                JObject jo = ReadJsonConfigFile(CONFIG_FILE_PATH);
                var dataBases = (JObject)(jo[ConnectionConst.ROOT]);
                if (dataBases == null)
                {
                    throw new Exception(string.Format(ConnectionConst.CONFIG_LOST_ITEM, ConnectionConst.ROOT));
                }

                IEnumerable<JProperty> properties = dataBases.Properties();
                foreach (JProperty jp in properties)
                {
                    var item = (JObject)jp.Value;
                    ConnectionStringItem connectionItem = new ConnectionStringItem();
                    if (string.IsNullOrWhiteSpace(jp.Name)
                        || !Convert.ToBoolean(item.GetNodeValue(ConnectionConst.ENABLED)))
                    {
                        // 没有名称 || 不启用
                        continue;
                    }

                    connectionItem.SystemName = jp.Name;
                    connectionItem.EncryptConnStr = item.GetNodeValue(ConnectionConst.CONNSTR);
                    connectionItem.provide = item.GetNodeValue(ConnectionConst.PROVIDE);
                    connectionItem.Encrypt = Convert.ToBoolean(item.GetNodeValue(ConnectionConst.ENCRYPT));
                    if (_parser.ContainsKey(jp.Name))
                    {
                        throw new Exception(ConnectionConst.CONFIG_REPETITION_ITEM + "[" + jp.Name + "]");
                    }

                    _parser.Add(connectionItem.SystemName, connectionItem);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ConnectionConst.CONFIG_FILE_EXCEPTION + ex.Message);
            }
        }

        /// <summary>
        /// 读取JSON文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>JSON文件中的value值</returns>
        public static JObject ReadJsonConfigFile(string path)
        {
            using (System.IO.StreamReader file = System.IO.File.OpenText(path))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    return (JObject)JToken.ReadFrom(reader);
                }
            }
        }
    }

    public static class JObjectExtension
    {
        /// <summary>
        /// 读取JObject的节点值
        /// </summary>
        /// <param name="name">节点名称</param>
        /// <returns>JSON文件中的value值</returns>
        public static string GetNodeValue(this JObject jObject, string name, bool isAllowEmpty = false)
        {
            var value = jObject[name];
            if (value == null)
            {
                throw new Exception(string.Format(ConnectionConst.CONFIG_LOST_ITEM, name));
            }

            if (!isAllowEmpty && string.IsNullOrWhiteSpace(value.ToString()))
            {
                throw new Exception(string.Format(ConnectionConst.CONFIG_LOST_VALUE_ITEM, name));
            }

            return value.ToString();
        }
    }
}
