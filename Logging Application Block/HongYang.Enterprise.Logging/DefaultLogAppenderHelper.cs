using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using HongYang.Enterprise.Logging.AdoNet;

namespace HongYang.Enterprise.Logging
{
    public class DefaultLogAppenderHelper : ILogAppenderHelper
    {
        private string _dataBaseName = string.Empty;

        public DefaultLogAppenderHelper(string dataBaseName)
        { 
            _dataBaseName = dataBaseName;
        }

        public bool WriteDb<T>(T message, ref string errorMessage) where T : class, new()
        {
            string sqlText = string.Empty;
            try
            {
                Database db = DatabaseFactory.GetDatabase(_dataBaseName);
                sqlText = db.InsertSQLByParameter(message);
                return db.ExecuteNonQuery(sqlText, message) > 0;
            }
            catch (Exception ex)
            {
                LogHelper.Write("sqlText:" + sqlText + "\n" + ex.ToString());
            }

            return false;
        }
    }
}
