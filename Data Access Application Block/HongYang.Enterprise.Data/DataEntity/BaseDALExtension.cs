using Dapper;
using HongYang.Enterprise.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HongYang.Enterprise.Data.DataEntity
{
    /// <summary>
    /// 数据库操作基类用到静态辅助函数的扩展
    /// </summary>
    public static class BaseDALExtension
    {
        /// <summary>
        /// 如果连接是Open状态，则关闭连接
        /// </summary>
        /// <param name="connection"></param>
        public static void CloseIfOpen(this IDbConnection connection)
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="value"></param>
        /// <param name="convertsionType"></param>
        /// <returns></returns>
        public static object ChangeType(this object value, Type convertsionType)
        {
            // 判断convertsionType类型是否为泛型，因为nullable是泛型类
            if (convertsionType.IsGenericType
                // 判断convertsionType是否为nullable泛型类
                && convertsionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null || value.ToString().Length == 0)
                {
                    return null;
                }

                // 如果convertsionType为nullable类，声明一个NullableConverter类，该类提供从Nullable类到基础基元类型的转换
                NullableConverter nullableConverter = new NullableConverter(convertsionType);
                // 将convertsionType转换为nullable对的基础基元类型
                convertsionType = nullableConverter.UnderlyingType;
            }
            return Convert.ChangeType(value, convertsionType);
        }

        /// <summary>
        /// 转义' 为 '' 否则数据库会插入不成功
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FilterChar(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Replace("'", "''");
        }

        /// <summary>
        /// 是否是非数据表字段 通过实体字段通过[NotMapped]标记
        /// </summary>
        /// <param name="pi">PropertyInfo</param>
        /// <returns></returns>
        public static bool IsNoMapKey(this PropertyInfo pi)
        {
            object[] attrs = pi.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                //判断NotMapped属性，看是否非数据库字段
                if ("System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute" == attr.GetType().FullName)
                {
                    if (!((Attribute)(attr)).IsDefaultAttribute())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 根据DataReader生成DataTable，轻量级。      
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static DataTable DataReaderToDataTable(this IDataReader reader)
        {
            DataTable dataTable = new DataTable();
            int fieldCount = reader.FieldCount;
            for (int i = 0; i <= fieldCount - 1; i++)
            {
                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }
            //populate datatable
            dataTable.BeginLoadData();
            object[] fieldValues = new object[fieldCount];
            while (reader.Read())
            {
                reader.GetValues(fieldValues);
                dataTable.LoadDataRow(fieldValues, true);
            }
            dataTable.EndLoadData();
            return dataTable;
        }

        /// <summary>
        /// DAL中异常处理
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="errorMsg"></param>
        /// <param name="track"></param>
        /// <param name="methodName"></param>
        public static void DALExceptionHandler(this Exception exception, Msg errorMsg, DBTrack track, string methodName)
        {
            errorMsg.Result = false;
            errorMsg.AddMsg(exception.Message);
            if (track == DBTrack.Open)
            {
                LogHelper.Write($"执行{methodName}方法异常。\n" + exception.Message);
            }

        }
    }
}
