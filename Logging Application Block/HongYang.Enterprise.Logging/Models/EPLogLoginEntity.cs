using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Logging.Models
{
    /// <summary>
    /// 表示用户登录日志实体类型
    /// </summary>
    public class EPLogLoginEntity : LogEntity
    {
        /// <summary>
        /// 获取或设置登录者IP地址
        /// </summary>
        public string UserIP { get; set; }

        /// <summary>
        /// 获取或设置登录的WEB服务器IP
        /// </summary>
        public string WebServer { get; set; }

        /// <summary>
        /// 获取或设置登录行为：0 上线，1 注销
        /// </summary>
        public string LogOut { get; set; }

        /// <summary>
        /// 登出时间
        /// </summary>
        public DateTime OutTime { get; set; }

        /// <summary>
        /// 返回EPLogLoginEntity
        /// </summary>
        public EPLogLoginEntity()
        { 
        
        }
    }
}
