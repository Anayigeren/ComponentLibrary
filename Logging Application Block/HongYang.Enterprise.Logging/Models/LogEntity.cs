using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Logging.Models
{
    /// <summary>
    /// 表示自定义日志实体的基类
    /// </summary>
    [Serializable]
    public class LogEntity
    {
        /// <summary>
        /// 获取或设置日志ID（36位）
        /// </summary>
        public string LogID { get; set; }

        /// <summary>
        /// 获取或设置日志时间
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// 获取或设置用户ID 最长36位
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 获取或设置机构/单位ID 最长36位
        /// </summary>
        public string OrgID { get; set; }

        /// <summary>
        /// 获取或设置用户科室ID 最长36位
        /// </summary>
        public string DepID { get; set; }

        /// <summary>
        /// 返回LogEntity实例
        /// </summary>
        public LogEntity()
        { 
            
        }
    }
}
