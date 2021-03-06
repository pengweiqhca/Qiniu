﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Qiniu.Util;

namespace Qiniu.Common
{
    /// <summary>
    /// 签名/加密
    /// 特别注意，不同平台使用的Cryptography可能略有不同，使用中如有遇到问题，请反馈
    /// 提交您的issue到 https://github.com/qiniu/csharp-sdk
    /// </summary>
    public class Signature
    {
        private Mac mac;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="mac">账户访问控制(密钥)</param>
        public Signature(Mac mac)
        {
            this.mac = mac;
        }

        private string encodedSign(byte[] data)
        {
            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(mac.SecretKey));
            byte[] digest = hmac.ComputeHash(data);
            return StringHelper.urlSafeBase64Encode(digest);
        }

        private string encodedSign(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            return encodedSign(data);
        }

        /// <summary>
        /// 签名-字节数据
        /// </summary>
        /// <param name="data">待签名的数据</param>
        /// <returns></returns>
        public string sign(byte[] data)
        {
            return string.Format("{0}:{1}", mac.AccessKey,encodedSign(data));
        }

        /// <summary>
        /// 签名-字符串数据
        /// </summary>
        /// <param name="str">待签名的数据</param>
        /// <returns></returns>
        public string sign(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            return sign(data);
        }

        /// <summary>
        /// 附带数据的签名
        /// </summary>
        /// <param name="data">待签名的数据</param>
        /// <returns></returns>
        public string signWithData(byte[] data)
        {
            string sstr = StringHelper.urlSafeBase64Encode(data);
            return string.Format("{0}:{1}:{2}", mac.AccessKey, encodedSign(sstr), sstr);
        }

        /// <summary>
        /// 附带数据的签名
        /// </summary>
        /// <param name="str">待签名的数据</param>
        /// <returns>签名结果</returns>
        public string signWithData(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            return signWithData(data);
        }

        /// <summary>
        /// HTTP请求签名
        /// </summary>
        /// <param name="url">请求目标的URL</param>
        /// <param name="body">请求的主体数据</param>
        /// <returns></returns>
        public string signRequest(string url, byte[] body)
        {
            Uri u = new Uri(url);
            using (HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(mac.SecretKey)))
            {
                string pathAndQuery = u.PathAndQuery;
                byte[] pathAndQueryBytes = Encoding.UTF8.GetBytes(pathAndQuery);
                using (MemoryStream buffer = new MemoryStream())
                {
                    buffer.Write(pathAndQueryBytes, 0, pathAndQueryBytes.Length);
                    buffer.WriteByte((byte)'\n');
                    if (body != null && body.Length > 0)
                    {
                        buffer.Write(body, 0, body.Length);
                    }
                    byte[] digest = hmac.ComputeHash(buffer.ToArray());
                    string digestBase64 = StringHelper.urlSafeBase64Encode(digest);
                    return string.Format("{0}:{1}", mac.AccessKey, digestBase64);
                }
            }
        }

        /// <summary>
        /// HTTP请求签名
        /// </summary>
        /// <param name="url">请求目标的URL</param>
        /// <param name="body">请求的主体数据</param>
        /// <returns></returns>
        public string signRequest(string url, string body)
        {
            byte[] data = Encoding.UTF8.GetBytes(body);
            return signRequest(url, data);
        }
    }
}
