using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Data.Connenction
{
    /// <summary>
    /// 程序集中的私有类
    /// 存储着链接数据库字符串中
    /// 所需要的错误信息，节点名称等等信息
    /// </summary>
    public class ConnectionConst
    {
        /// <summary>
        /// 根节点
        /// </summary>
        public const string ROOT = "Databases";

        /// <summary>
        /// 子系统配置节点
        /// </summary>
        public static string CHILD = "Database";

        /// <summary>
        /// 数据库名节点名称
        /// </summary>
        public static string SYSNAME = "name";
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string CONNSTR = "connectionstring";

        /// <summary>
        /// 是否加密
        /// </summary>
        public static string ENCRYPT = "encrypt";

        /// <summary>
        /// 数据库类型
        /// </summary>
        public static string DBTYPE = "dbtype";

        /// <summary>
        /// 是否启用
        /// </summary>
        public static string ENABLED = "enabled";

        /// <summary>
        /// 提供器，用于反射使用
        /// </summary>
        public static string PROVIDE = "provide";

        /// <summary>
        /// 配置文件的文件名
        /// </summary>
        public static string CONFIG_XML_FILE_NAME = "EnterpriseDbConfig.xml";

        /// <summary>
        /// 配置文件的文件名
        /// </summary>
        public static string CONFIG_JSON_FILE_NAME = "appsettings.json";

        /// <summary>
        /// 读取数据库连接时，配置文件未发现
        /// </summary>
        public static string CONFIG_FILE_NOTCONFIGURED = "配置文件字典中未发现指定类型的配置项";

        /// <summary>
        /// 读取数据库连接时，配置文件未发现
        /// </summary>
        public static string CONFIG_FILE_NOFOUND = "读取数据库连接时，配置文件未发现";

        /// <summary>
        /// 配置文件为非标准XML
        /// </summary>
        public static string CONFIG_FILE_EXCEPTION = "读取配置文件异常:";

        /// <summary>
        /// 配置文件为非标准XML
        /// </summary>
        public static string CONFIG_FILE_NO_XML_VALITE = "配置文件为非标准XML";

        /// <summary>
        /// 配置文件为非标准Json
        /// </summary>
        public static string CONFIG_FILE_NO_JSON_VALITE = "配置文件为非标准JSON";

        /// <summary>
        /// 发现名称重复的配置节
        /// </summary>
        public static string CONFIG_REPETITION_ITEM = "发现名称重复的配置节";

        /// <summary>
        /// 缺少name节点
        /// </summary>
        public static string CONFIG_NONAME_NODE= "缺少name节点";

        /// <summary>
        /// 未发现指定子系统
        /// </summary>
        public static string CONFIG_SYSTEM_NOFOUND = "配置文件中未发现指定子系统";

       /// <summary>
        /// 节点数据类型错误
       /// </summary>
        public static string CONFIG_DBTYPE_UNKOWN = DBTYPE + "节点数据类型错误,节点文本需要同DatabaseType枚举中的一致";

       /// <summary>
        /// 未知的数据提供类，所填写内容必须是继承了DBFactory的子类
       /// </summary>
        public static string CONFIG_PROVIDE_ERROR = "未知的数据提供类，所填写内容必须是继承了DBFactory的子类";

        /// <summary>
        /// 缺少节点
        /// </summary>
        public static string CONFIG_LOST_ITEM = "文件的配置节中缺少{0}节点";

        /// <summary>
        /// 缺少节点值
        /// </summary>
        public static string CONFIG_LOST_VALUE_ITEM = "配置文件中节点{0}的值不能为空";
    }
}
