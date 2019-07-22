using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using HongYang.Enterprise.Data.AdoNet;

namespace HongYang.Enterprise.Data.DataEntity.T4
{
    /// <summary>
    /// T4模板生成帮助类
    /// </summary>
    public class T4Helper
    {
        public DataTable table = null;
        public string TableName = "";
        public string TableComment = "";
        public string DbName = "";
        public T4Table Entitys = null;
        OracleDatabase o = null;
        public T4Helper(string connstr, string tableName)
        {
            Msg msg = new Msg();
            o = new OracleDatabase(connstr);
            DataSet dataset = null;
            try{
                dataset = o.ExecuteDataSet("select * from " + tableName + " where 1=2");
            }
            catch(Exception ex){
                throw new Exception("读取表失败" + tableName + ex.ToString());
            }            
            TableName = tableName;
            table = dataset.Tables[0];
            DataSet comment = GetComment();
            Entitys = new T4Table();
            Entitys.TableName = GetFistrUpper(tableName);
            Entitys.TableComment = GetTableComment(tableName);
            TableComment = Entitys.TableComment;
            Entitys.Columns = new List<T4Columns>();
            var dbColInfo=GetColDbInfo();
            Entitys.Primary = GetPKKey();
            foreach (DataColumn dc in table.Columns)
            {
                Entitys.Columns.Add(new T4Columns()
                {
                    Name = GetFistrUpper(dc.ColumnName),
                    strDBType = dc.DataType.Name,
                    Comment = GetComment(comment, dc.ColumnName.ToUpper()),
                    IsPrimary = dc.ColumnName.ToLower() == Entitys.Primary.ToLower()           ,
                    IsNoNull = dbColInfo.Select("column_name='"+dc.ColumnName.ToUpper()+"'")[0]["NULLABLE"].ToString ()=="N"                     
                });
            }
        }

        public DataTable GetColDbInfo( )
        {
            string sqlstr = @"  select * from cols where table_name='" + TableName.ToUpper()+"'";
       
            DataSet d = o.ExecuteDataSet(sqlstr);
            return d.Tables[0];
        }

        public string GetComment(DataSet d, string colnum)
        {
            return d.Tables[0].Select("column_name='" + colnum + "'")[0]["comments"].ToString(); ;
        }

        /// <summary>
        /// 获取字段说明
        /// </summary>
        /// <returns></returns>
        public DataSet GetComment()
        {
            string sqlstr = @"   select * 
from user_COL_comments 
where Table_Name='{0}'
 
order by Table_Name";
            sqlstr = string.Format(sqlstr, TableName.ToUpper());
            DataSet d = o.ExecuteDataSet(sqlstr);

            return d;
        }

        /// <summary>
        /// 获取表说明
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetTableComment(string tableName)
        {
            string sqlstr = "select * from user_tab_comments where table_name='" + tableName.ToUpper() + "'";
            DataSet d = o.ExecuteDataSet(sqlstr);
            if (null != d && d.Tables[0].Rows.Count > 0)
                return d.Tables[0].Rows[0]["comments"].ToString();
            return "";
        }

        /// <summary>
        /// 获取指定数据表的主键
        /// </summary>
        /// <returns></returns>
        public string GetPKKey()
        {
            string sqlstr = @"select a.* from USER_cons_columns a, USER_constraints b
 where a.table_name ='{0}'
 and a.table_name = b.table_name
 and a.constraint_name = b.constraint_name
 and b.constraint_type ='P'";
            sqlstr = string.Format(sqlstr, TableName.ToUpper());
            DataSet d = o.ExecuteDataSet(sqlstr);
            if (d.Tables[0].Rows.Count == 0)
                throw new Exception(TableName + "没有设置主键");
            return GetFistrUpper(d.Tables[0].Rows[0]["column_name"].ToString());
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string GetFistrUpper(string text)
        {
            return text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();

        }

        /// <summary>
        /// 将需要的数据类型，修改为可空类型
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public string GetCSharpVariable(string typename)
        {
            switch (typename)
            {
                case "DateTime":
                case "Decimal":
                case "UInt32":
                case "UInt16":
                case "UInt64":
                case "Double":
                case "Boolean":
                case "SByte":
                    return typename + "?";
                    break;
                default:
                    return typename;
            }
            return typename;
        }


    }


}
