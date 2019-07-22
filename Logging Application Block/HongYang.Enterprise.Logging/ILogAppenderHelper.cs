using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using HongYang.Enterprise.Logging.Helpers;

namespace HongYang.Enterprise.Logging
{
    public interface ILogAppenderHelper
    {
        /// <summary>
        /// 写入数据库的方法，由具体调用系统实现
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="errorMessage">写数据失败提示信息</param>
        /// <returns></returns>
        bool WriteDb<T>(T message, ref string errorMessage);

    }

    public class CustomAppenderHelper : ILogAppenderHelper
    {
        public bool WriteDb<T>(T message, ref string errorMessage)
        {
            errorMessage = "写数据库内部错误";
            return false;
        }
    }

    public class CustomAppenderHelper1 : ILogAppenderHelper
    {
        public bool WriteDb<T>(T message, ref string errorMessage)
        {
            errorMessage = "";
            return true;
        }
    }
}
