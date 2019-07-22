using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

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
        bool WriteDb<T>(T message, ref string errorMessage) where T : class, new();

    }

    public class DefaultLogAppenderHelper : ILogAppenderHelper
    {
        public bool WriteDb<T>(T message, ref string errorMessage) where T : class, new()
        {
            errorMessage = "未实现日志写入数据的方法，请注入ILogAppenderHelper的实现";
            return false;
        }
    }
}
