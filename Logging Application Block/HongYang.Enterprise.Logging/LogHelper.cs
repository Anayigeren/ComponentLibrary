﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using System.Xml.Linq;

namespace HongYang.Enterprise.Logging
{
    /// <summary>
    /// 日志组件帮助类
    /// 仅实现文本日志记录，待完善
    /// </summary>
    public class LogHelper
    {
        public static string LogDbName = "Log";

        /// <summary>
        /// 文本日志记录器
        /// </summary>
        public static ILog RollFileLog = null;

        /// <summary>
        /// 自定义记入日志记录器的实现
        /// </summary>
        public static ILogAppenderHelper AppenderHelper = null;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        /// <returns></returns>
        static LogHelper()
        {
        }

        /// <summary>
        /// 初始化日志组件
        /// </summary>
        public static void LogInit() => LogInit(LogDbName);

        /// <summary>
        /// 初始化日志组件
        /// </summary>
        /// <param name="dbName">写入数据的连接串名称</param>
        public static void LogInit(string dbName)
        {
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException(configFilePath + "日志配置文件未发现");
            }

            AppenderHelper = new DefaultLogAppenderHelper(dbName);
            XmlConfigurator.Configure(Valiteconfigfile(configFilePath));
            RollFileLog = LogManager.GetLogger("RollFile") ;
        }

        /// <summary>
        /// 验证配置文件
        /// </summary>
        /// <param name="configFilePath">配置文件路径</param>
        /// <returns></returns>
        private static Stream Valiteconfigfile(string configFilePath)
        {
            XElement document = XElement.Load(configFilePath);
            var log4netcfg = document.Element("log4net");
            if (log4netcfg == null)
            {
                throw new Exception("配置文件中未发现log4net节点");
            }

            var appenders = log4netcfg.Elements("appender").ToList();
            var rollfile = appenders.FindAll(p => p.Attribute("name").Value.Equals("RollFile"));
            if (rollfile.Count == 0)
            {
                throw new Exception($"配置文件中未发现RollFile节点配置");
            }

            var stream = new MemoryStream();
            document.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// 写本地文本文件日志（默认Error等级）
        /// </summary>
        /// <param name="message">记录信息</param>
        /// <param name="level">log等级</param>
        public static void Write(object message, LogLevel level = LogLevel.Error)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    RollFileLog.Debug(message);
                    break;

                case LogLevel.Info:
                    RollFileLog.Info(message);
                    break;

                case LogLevel.Warn:
                    RollFileLog.Warn(message);
                    break;

                case LogLevel.Error:
                    RollFileLog.Error(message);
                    break;

                case LogLevel.Fatal:
                    RollFileLog.Fatal(message);
                    break;

                default:
                    RollFileLog.Error(message);
                    break;
            }
        }

        /// <summary>
        /// 写本地文本文件日志（异步）
        /// </summary>
        /// <param name="message">记录信息</param>
        /// <param name="level">log等级</param>
        public static Task WriteTask(object message, LogLevel level = LogLevel.Error)
        {
            return Task.Run(() =>
            {
                Write(message, level);
            });
        }

        /// <summary>
        /// 写数据库日志（支持所有数据库类型日志的写入，因为是开放Db写入的实现给调用方）
        /// </summary>
        /// <param name="log">记录信息</param>
        /// <param name="level">log等级</param>
        public static void WriteDb<T>(T log, LogLevel level = LogLevel.Error) where T : class, new()
        {
            string errorMessage = string.Empty;
            try
            {
                var result = AppenderHelper.WriteDb<T>(log, ref errorMessage);
                if (!result)
                {
                    Write(errorMessage, level); // 写入数据库失败，写入文本日志
                }
            }
            catch (Exception ex)
            {
                Write(ex);
            }
        }

        /// <summary>
        /// 写数据库日志 - 异步
        /// </summary>
        /// <param name="log">记录信息</param>
        /// <param name="level">log等级</param>
        public static Task WriteDbTask<T>(T log, LogLevel level = LogLevel.Error) where T : class, new()
        {
            return Task.Run(() =>
            {
                WriteDb(log, level);
            });
        }

        /// <summary>
        /// 如果那些子系统不希望记录在公共的文件下或者数据表中
        /// 则可以在log4net.config中自行添加 appender，然后获取记录器
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        public static ILog GetLogger(string systemName)
        {
            return LogManager.GetLogger(systemName);
        }
    }
}
