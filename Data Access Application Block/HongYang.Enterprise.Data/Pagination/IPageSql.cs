using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HongYang.Enterprise.Data
{
    public interface IPageSql
    {
        /// <summary>
        /// 普通分页
        /// </summary>
        /// <param name="dataPage"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        string PageSql(DataPage dataPage, Query query);

        /// <summary>
        /// 总数
        /// </summary>
        /// <param name="page"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        string CountSql(Query query);
    }
}
