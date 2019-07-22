using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
namespace HongYang.Enterprise.Data.Connenction
{

    /// <summary>
    /// 每个配置项的实例化节点
    /// </summary>
    public class ConnectionStringItem
    {
        private const string _enrypt_key = "www.HongYangSoftWare.comHongYang.HIS3";
 
        private string _encrypt_str = "";

        /// <summary>
        /// 链接字符串
        /// 如果Encrypt==true,则为加密字符串
        /// 如果Encrypt==false,则为普通字符串
        /// </summary>
       public string ConnStr
        {
            get
            {

                //加密未完成
                if (Encrypt)
                {
                    DES_ des = new DES_(_enrypt_key);
                    try
                    {
                        return des.Decrypt(_encrypt_str);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("解密数据库连接加密串时失败", ex);
                    }
                    
                }
                return _encrypt_str;
            }
            
        }

        public string GetConnStr(string key)
        {
            //加密未完成
            if (Encrypt)
            {
                DES_ des = new DES_(key);
                try
                {
                    return des.Decrypt(_encrypt_str);
                }
                catch (Exception ex)
                {
                    throw new Exception("解密数据库连接加密串时失败", ex);
                }

            }
            return _encrypt_str;

        }

        /// <summary>
        /// 加密串
        /// </summary>
        public string EncryptConnStr
       {
           get
           {

               return _encrypt_str;
           }

           set
           {
               _encrypt_str = value;
           }
       }

        /// <summary>
        /// 系统名
        /// </summary>
        public string SystemName
        {
            get;
            set;
        }

        /// <summary>
        /// 是否加密
        /// </summary>
        public bool Encrypt
        {
            get;
            set;

        }

        /// <summary>
        /// 数据类型提供器
        /// </summary>
        public string provide
        {
            get;
            set;
        }

        /// <summary>
        /// 生成数据库加密串
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static string CreateEncryptString(string conn)
        {
            DES_ des = new DES_(_enrypt_key);
            return des.Encrypt(conn);
        }
    }
}
