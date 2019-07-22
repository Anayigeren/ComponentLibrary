using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HongYang.Enterprise.Data.DataEntity
{
    /// <summary>
    /// ORACLE数据分页帮助类
    /// </summary>
    public class PageHelper
    {
        /// <summary>
        /// 返回查询语句
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string PageSQL(Query query)
        {
            if (!string.IsNullOrWhiteSpace(query.QuerySql))
            {
                return query.QuerySql;
            }

            return PageSQL(query.TableName, query.Field, query.Filter, query.Order);
        }

        /// <summary>
        /// 返回查询语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="field"></param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public static string PageSQL(string tableName, string field = "*", string filter = "", string orderBy = "")
        {
            orderBy = string.IsNullOrWhiteSpace(orderBy) ? string.Empty : $"order by {orderBy}";
            return $"select {field} from {tableName} where 1=1 {filter} {orderBy}";
        }

        /// <summary>
        /// 分页公共方法
        /// </summary>
        /// <param name="Sqlstr">SQL语句</param>
        /// <param name="Order">排序</param>
        /// <param name="CurrentPage">当前页数</param>
        /// <param name="PageSize">每页记录数</param>
        /// <returns>返回SQL语句</returns>
        public static string PageSQL(string Sqlstr, string Order, int CurrentPage, int PageSize)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("select * from (");
            sb.Append("select A.* ");
            sb.Append(",rownum rn from(" + Sqlstr + Order + ") A ");
            sb.Append(")" + "where rn>" + (CurrentPage - 1) * PageSize + " and rn<=" + CurrentPage * PageSize + "");

            return sb.ToString();
        }

        /// <summary>
        /// 分页公共方法
        /// </summary>
        /// <param name="Field">字段列表</param>
        /// <param name="TableName">数据表名</param>
        /// <param name="Where">条件</param>
        /// <param name="Order">排序</param>
        /// <param name="CurrentPage">当前页数</param>
        /// <param name="PageSize">每页记录数</param>
        /// <returns>返回SQL语句</returns>
        public static string PageSQL(string Field, string TableName, string Where, string Order, int CurrentPage, int PageSize)
        {
            StringBuilder sb = new StringBuilder();
            Field = Field == "" ? "A.*" : Field;
            Where = Where == "" ? "1=1" : Where;

            sb.Append("select * from (");
            sb.Append("select " + Field);
            sb.Append(",rownum rn from(select * from " + TableName + " where " + Where + Order + ") A ");
            sb.Append(" where rownum<=" + CurrentPage * PageSize + ")" + "where rn>" + (CurrentPage - 1) * PageSize + " ");

            return sb.ToString();
        }

        /// <summary>
        /// 分页公共方法(跨年)
        /// </summary>
        /// <param name="Field">字段列表</param>
        /// <param name="TableName">数据表名</param>
        /// <param name="Where">条件</param>
        /// <param name="minyear">最小年份</param>
        /// <param name="yearAdd">总年数</param>
        /// <param name="Order">排序</param>
        /// <param name="CurrentPage">当前页数</param>
        /// <param name="PageSize">每页记录数</param>
        /// <returns>返回SQL语句</returns>
        public static string PageSQLByYear(string Field, string TableName, string Where, string minyear, int yearAdd, string Order, int CurrentPage, int PageSize)
        {
            StringBuilder sb = new StringBuilder();
            yearAdd = yearAdd < 1 ? 1 : yearAdd;
            Field = Field == "" ? "A.*" : Field;
            Where = Where == "" ? "1=1" : Where;

            sb.Append("select * from (");
            sb.Append("select " + Field);
            sb.Append(",rownum rn from(select * from (");
            for (int i = 0; i < yearAdd; i++)
            {
                string year = (int.Parse(minyear) + i).ToString();
                sb.Append("select * from " + TableName + year + " where " + Where);
                if (i < yearAdd - 1)
                    sb.Append(" union all ");
            }

            sb.Append(")" + Order + ") A ");
            sb.Append("where rownum<=" + CurrentPage * PageSize + ")" + "where rn>" + (CurrentPage - 1) * PageSize + " ");

            return sb.ToString();
        }

        /// <summary>
        /// 分页公共方法(跨年)
        /// </summary>
        /// <param name="Field">字段列表</param>
        /// <param name="TableName">数据表名</param>
        /// <param name="bieMing">数据表别名，用于SQL语句</param>
        /// <param name="r_bieMing">关联表别名，用于SQL语句</param>
        /// <param name="Where">条件</param>
        /// <param name="minyear">最小年份</param>
        /// <param name="yearAdd">总年数</param>
        /// <param name="Order">排序</param>
        /// <param name="CurrentPage">当前页数</param>
        /// <param name="PageSize">每页记录数</param>
        /// <param name="R_TableName">关联表</param>
        /// <returns>返回SQL语句</returns>
        public static string PageSQLByYear(string Field, string TableName, string bieMing, string r_bieMing, string Where, string minyear, int yearAdd, string Order, int CurrentPage, int PageSize, params string[] R_TableName)
        {
            StringBuilder sb = new StringBuilder();
            yearAdd = yearAdd < 1 ? 1 : yearAdd;
            string _field = Field == "" ? "A.*" : Field == bieMing + ".*" ? "A.*" : Field;
            Where = Where == "" ? "1=1" : Where;

            sb.Append("select * from (");
            sb.Append("select " + _field);
            sb.Append(",rownum rn from(select * from (");
            for (int i = 0; i < yearAdd; i++)
            {
                string year = (int.Parse(minyear) + i).ToString();
                if (Field == bieMing + ".*")
                    sb.Append("select " + Field + " from " + TableName + year + " " + bieMing);
                else
                    sb.Append("select * from " + TableName + minyear + " " + bieMing);
                foreach (string r_name in R_TableName)
                {
                    sb.Append("," + r_name + minyear + " " + r_bieMing);
                }
                sb.Append(" where " + Where);
                if (i < yearAdd - 1)
                    sb.Append(" union all ");
            }

            sb.Append(")" + Order + ") A ");
            sb.Append("where rownum<=" + CurrentPage * PageSize + ")" + "where rn>" + (CurrentPage - 1) * PageSize + " ");

            return sb.ToString();
        }
    }
}
