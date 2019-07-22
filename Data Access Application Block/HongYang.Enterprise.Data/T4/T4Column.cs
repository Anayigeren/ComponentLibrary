using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Data.DataEntity.T4
{
    /// <summary>
    /// 生成T4模板列
    /// </summary>
    public class T4Columns
    {
        public System.Data.DbType dbType
        {
            get;
            set;
        }

        /// <summary>
        /// C#DataSet数据类型
        /// </summary>
        public string strDBType
        {

            get;
            set;
        }

        /// <summary>
        /// 列名
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 注释
        /// </summary>
        public string Comment
        {
            get;
            set;
        }

        /// <summary>
        /// 是否是主键
        /// </summary>
        public bool IsPrimary
        {
            get;
            set;
        }

        /// <summary>
        /// 是否不能为空
        /// </summary>
        public bool IsNoNull
        {
            get;
            set;
        }

        /// <summary>
        /// 文本长度
        /// </summary>
        public string StringLength
        {
            get;
            set;
        }
    }
}
