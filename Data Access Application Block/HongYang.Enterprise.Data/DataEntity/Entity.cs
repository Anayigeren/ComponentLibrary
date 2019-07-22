

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HongYang.Enterprise.Data.DataEntity
{
    public class Entity : ICloneable
    {
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// 获取固定属性的值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public object GetValue(string propertyName)
        {
            Type type = this.GetType();
            PropertyInfo pi = type.GetProperty(propertyName);
            if (pi != null && pi.CanRead)
            {
                return pi.GetValue(this, null);
            }

            FieldInfo fi = type.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null)
            {
                return fi.GetValue(this);
            }

            return null;
        }
    }
}
