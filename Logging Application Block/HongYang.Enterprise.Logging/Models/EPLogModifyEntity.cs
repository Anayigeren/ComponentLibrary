using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Logging.Models
{
    /// <summary>
    /// 表示数据表记录更新、删除日志的实体类型
    /// </summary>
    public class EPLogModifyEntity:LogEntity
    {
        /// <summary>
        /// 获取或设置业务表主键
        /// </summary>
        public string BusGuid { get; set; }

        /// <summary>
        /// 获取或设置表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 获取或设置原记录内容
        /// </summary>
        public string OldContent { get; set; }

        /// <summary>
        /// 获取或设置新记录内容
        /// </summary>
        public string NewContent { get; set; }
        
        /// <summary>
        /// 获取或设置修改类型 0修改，1删除，1位字符
        /// </summary>
        public string ModifyType { get; set; }

        /// <summary>
        /// 返回EPLogModifyEntity实例
        /// </summary>
        public EPLogModifyEntity()
        {
            
        }
    }
}
