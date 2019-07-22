using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Logging.Models
{
    /// <summary>
    /// 表示用户操作日志的实体类型
    /// </summary>
    public class EPLogUserOPEntity:LogEntity
    {
        /// <summary>
        /// 获取或设置当前产品版本
        /// </summary>
        public string ProductVersion { get; set; }

        /// <summary>
        /// 获取或设置操作名称（例：建档）
        /// </summary>
        public string OPName { get; set; }

        /// <summary>
        /// 获取或设置操作结果（例：建档成功，档ID xxxxxx）
        /// </summary>
        public string OPResults { get; set; }

        /// <summary>
        /// 获取或设置操作界面路径（例：门诊收费处/档案管理）
        /// </summary>
        public string UIPath { get; set; }

        /// <summary>
        /// 获取或设置用户角色名称，多个可用半角逗号串联
        /// </summary>
        public string UserRoleName { get; set; }

        /// <summary>
        /// 获取或设置用户IP
        /// </summary>
        public string UserIP { get; set; }

        /// <summary>
        /// 获取或设置当前服务器IP
        /// </summary>
        public string WebServer { get; set; }

        /// <summary>
        /// 返回EPLogUserOPEntity
        /// </summary>
        public EPLogUserOPEntity()
        { 
            
        }
    }
}
