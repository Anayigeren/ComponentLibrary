using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Logging.Models
{
    /// <summary>
    /// 用于记录两个业务系统之间交互的日志
    /// 可以记录 发送时间发送内容，反馈时间反馈内容
    /// </summary>
    public class EPLogMessageEntity : LogEntity
    {
        /// <summary>
        ///  消息唯一键
        /// </summary>
        public string Messageid { get; set; }

        /// <summary>
        ///  发送系统
        /// </summary>
        public string Sendfrom { get; set; }

        /// <summary>
        ///  接受模块
        /// </summary>
        public string Sendto { get; set; }

        /// <summary>
        ///  发送时间
        /// </summary>
        public DateTime Sendtime { get; set; }

        /// <summary>
        ///  发送内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        ///  响应内容
        /// </summary>
        public string Resultcontent { get; set; }

        /// <summary>
        ///  发送模块
        ///  或用于标明发送模式
        /// </summary>
        public string Msgid { get; set; }

        /// <summary>
        ///  发送结果
        ///  1-成功 0-失败
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 响应时间
        /// </summary>
        public DateTime Resulttime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Groupid { get; set; }

        /// <summary>
        /// 返回EPLogLoginEntity
        /// </summary>
        public EPLogMessageEntity()
        {

        }
    }
}
