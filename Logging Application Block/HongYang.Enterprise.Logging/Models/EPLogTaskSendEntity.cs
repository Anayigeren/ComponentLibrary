using System;

namespace HongYang.Enterprise.Logging.Models
{
    /// <summary>
    /// 任务调度日志
    /// ep_log_tasksend
    /// </summary>
    public class EPLogTaskSendEntity : LogEntity
    {


        /// <summary>
        /// 任务id
        /// </summary>
        public string TaskId { get; set; }
        /// <summary>
        /// 任务(Job名称)
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 执行job结果 0-成功 1-失败
        /// </summary>
        public string Results { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime ExecutionStartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime ExecutionEndTime { get; set; }

        /// <summary>
        /// 持续时间（秒）
        /// </summary>
        public string Executionduration { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string Content { get; set; }
    }
}
