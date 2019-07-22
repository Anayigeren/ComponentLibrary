namespace HongYang.Enterprise.Logging.Models
{
    /// <summary>
    /// 任务调度业务日志
    /// 用来记录任务调度执行具体业务的日志
    /// 业务分为2种 执行内部执行 或则跳转到外部执行
    /// ep_log_taskbusiness
    /// </summary>
    public class EPLogTaskBusinessEntity : LogEntity
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
        /// 业务表主键
        /// 业务主要操作那张表的主键 如用户表便是用户id等
        /// </summary>
        public string BusId { get; set; }
        /// <summary>
        /// 业务表名
        /// 业务主要操作的表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 0-内部操作（内部增删改查） 1-外部操作（通常是指调用API等）
        /// </summary>
        public string BusType { get; set; }

        /// <summary>
        /// 发送内容
        /// 执行业务的具体内容 新增，修改 ，删除便存实体JSON
        /// 执行外部操作，便存URL，GET/POST，XML JSON等参数
        /// </summary>
        public string SendContent { get; set; }

        /// <summary>
        /// 返回内容
        /// 执行逻辑完返回的具体内容 新增，修改 ，删除便存错误信息
        /// 执行外部操作 便存返回的结果Response
        /// </summary>
        public string ReturnContent { get; set; }

        /// <summary>
        /// 任务调度日志ID
        /// </summary>
        public string TaskSendLogId { get; set; }

    }
}
