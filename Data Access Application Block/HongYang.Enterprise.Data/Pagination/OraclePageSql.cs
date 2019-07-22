using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HongYang.Enterprise.Data.DataEntity;

namespace HongYang.Enterprise.Data
{
    public class OraclePageSql : IPageSql
    {
        /// <summary>
        /// ORACLE分页sql，QuerySql不为空以QuerySql为准
        /// </summary>
        /// <param name="page"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public string PageSql(DataPage page, Query query)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select * from (");
            sb.Append("select * from (");
            sb.AppendFormat("select A.* ,rownum rn from({0}) A )", PageHelper.PageSQL(query));
            sb.AppendFormat(" where rownum <= {0})", page.EndPageIndex);
            sb.AppendFormat(" where rn > {0}", page.StartPageIndex);
            return sb.ToString();
        }

        /// <summary>
        /// 查询总数，避免在函数中使用 order by 这样无意义的行为
        /// </summary>
        /// <param name="page"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public string CountSql(Query query)
        {
            return string.Format("select count(*) count from ({0})", PageHelper.PageSQL(query.TableName, query.Field, query.Filter));
        }
    }
}
