using System;

namespace HongYang.Enterprise.Data
{
    /// <summary>
    /// Msg 的摘要说明。
    /// 消息类。用于底层向前端传递消，消息分为六个级别，Trace,Debug,Info,Warn,Error,Fatal
    /// 分别为小问题、查错信息、一般信息、警告信息、异常信息、重大错误六个级别。当消息的级别
    /// 大于要显示的信息时前端对该信息进行显示。
    /// 作者：许德俊
    /// 日期：2006-1-6
    /// 
    /// </summary>
    public enum MsgLevel  // 定义枚举类
    {
        /// <summary>
        /// 小问题
        /// </summary>
        Trace,
        /// <summary>
        /// 查错信息
        /// </summary>
        Debug,
        /// <summary>
        /// 一般信息
        /// </summary>
        Info,
        /// <summary>
        /// 警告信息
        /// </summary>
        Warn,
        /// <summary>
        /// 异常信息
        /// </summary>
        Error,
        /// <summary>
        /// 重大错误
        /// </summary>
        Fatal
    }
    /// <summary>
    /// 定义信息类
    /// </summary>
    [Serializable]
    public class Msg
    {

        public Msg()
        {
            Result = true;
            // msg = System.Guid.NewGuid().ToString();
        }

        #region 公共属性
        //过度设计了，貌似暂时没这需求
        //private string msg = "";
        ///// <summary>
        ///// 日志唯一ID
        ///// </summary>
        //public string MsgID
        //{
        //    get
        //    {
        //        return msg;
        //    }
        //    private set
        //    {
        //    }
        //}

        /// <summary>
        /// 结果
        /// </summary>
        public bool Result
        {
            get;
            set;
        }

        /// <summary>
        /// 消息实体，存放非基础类型数据
        /// </summary>
        public object MsgObjectContent
        {
            get;
            set;
        }


        private string msgcontent = "";
        /// <summary>
        /// 累加消息
        /// </summary>
        /// <param name="msg"></param>
        public void AddMsg(string msg)
        {
            this.msgcontent += msg;

        }

        #endregion


        /// <summary>
        /// 清除消息
        /// </summary>
        /// 
        public void ClearMsg()
        {
            msgcontent = "";

        }

        /// <summary>
        /// 重写ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return msgcontent;
        }
    }

    /// <summary>
    /// 定义信息类
    /// </summary>
    [Serializable]
    public class Msg<T>
    {

        public Msg()
        {
            Result = true;
        }

        #region 公共属性


        /// <summary>
        /// 结果
        /// </summary>
        public bool Result
        {
            get;
            set;
        }

        /// <summary>
        /// 消息实体，存放非基础类型数据
        /// </summary>
        public T MsgObjectContent
        {
            get;
            set;
        }


        private string msgcontent = "";
        /// <summary>
        /// 累加消息
        /// </summary>
        /// <param name="msg"></param>
        public void AddMsg(string msg)
        {
            this.msgcontent += msg;

        }

        #endregion


        /// <summary>
        /// 清除消息
        /// </summary>
        /// 
        public void ClearMsg()
        {
            msgcontent = "";

        }

        /// <summary>
        /// 重写ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return msgcontent;
        }
    }
}
