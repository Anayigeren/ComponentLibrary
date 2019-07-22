using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
 
using System.Reflection;
using System.ComponentModel;

namespace HongYang.Enterprise.Data.DataEntity
{
    /// <summary>
    ///  弘扬基础平台业务ORACL映射类
    /// </summary>
    [Serializable]
    public class OracleDataEntity : Entity
    {


        public OracleDataEntity() : base()
        {

        }

        /// <summary>
        /// 返回主键
        /// </summary>
        /// <returns></returns>
        public virtual string PrimaryKey()
        {
            throw new NotImplementedException();
        }
    }
}



