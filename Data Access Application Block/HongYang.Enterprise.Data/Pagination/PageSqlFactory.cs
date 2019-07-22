using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HongYang.Enterprise.Data.Connenction;

namespace HongYang.Enterprise.Data
{
    public class PageSqlFactory
    {
        public static IPageSql GetPageSql(string dataBaseName)
        {
            return CreateInstance(ConnectionStringFactory.CreateInstance()[dataBaseName]);
        }

        public static IPageSql CreateInstance(ConnectionStringItem dbconfig)
        {
            string provide = dbconfig.provide;
            IPageSql pageSql;

            switch (provide)
            {
                case "HongYang.Enterprise.Data.AdoNet.Oracle":
                    pageSql = new OraclePageSql();
                    break;
                default:
                    pageSql = new OraclePageSql();
                    break; 
            }
            return pageSql;
        }
    }
}
