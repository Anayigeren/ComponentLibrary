using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Data.DataEntity.T4
{

    public class T4Table
    {
        /// <summary>
        /// 列集合
        /// </summary>

        public List<T4Columns> Columns
        {
            get;
            set;
        }

        /// <summary>
        /// 主键
        /// </summary>
        public string Primary
        {
            get;
            set;
        }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName
        {
            get;
            set;
        }
        /// <summary>
        /// 表说明
        /// </summary>
        public string TableComment
        {
            get; 
            set; 
        }
    }
}
