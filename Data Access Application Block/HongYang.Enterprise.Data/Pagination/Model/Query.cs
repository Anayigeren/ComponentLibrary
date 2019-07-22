using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HongYang.Enterprise.Data
{

    public class Query
    {
        //查询字段
        private string field;
        public string Field
        {
            get
            {
                return string.IsNullOrEmpty(field) ? "*" : field;
            }
            set
            {
                field = value;
            }
        }

        /// <summary>
        /// 表名
        /// </summary>
        private string tableName;
        public string TableName
        {
            get
            {
                return string.IsNullOrEmpty(tableName)?"dual":tableName;
            }
            set
            {
                tableName = value;
            }
        }

        /// <summary>
        /// 过滤条件
        /// </summary>
        private string filter;
        public string Filter
        {
            get
            {
                return string.IsNullOrEmpty(filter)? "1=1" : filter;
            }
            set
            {
                filter = value;
            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        private string order;
        public string Order
        {
            get
            {
                return order;
            }
            set
            {
                order = value;
            }
        }

        /// <summary>
        /// 分组
        /// </summary>
        private string group;
        public string Group
        {
            get
            {
                return group;
            }
            set
            {
                group = value;
            }
        }

        /// <summary>
        /// 查询sql
        /// </summary>
        private string querySql;
        public string QuerySql
        {
            get
            {
                if (string.IsNullOrEmpty(querySql))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("select {0} from {1} where {2}", Field, TableName, Filter);
                    if (!string.IsNullOrEmpty(Group))
                        sb.AppendFormat(" group by {0}", Group);
                    return sb.ToString();
                }
                return querySql;
            }
            set
            {
                querySql = value;
            }
        }
    }
}
