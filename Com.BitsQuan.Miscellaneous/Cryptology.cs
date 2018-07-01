using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Com.BitsQuan.Miscellaneous
{
    public class CryptologyUtil
    {
        /// <summary>
        /// 对称加密算法
        /// </summary>
        /// <typeparam name="T">对称加密算法类型</typeparam>
        /// <param name="encryptor">加密算法实例对象</param>
        /// <param name="text">待加密的字符</param>
        /// <param name="encd">待加密的字符编码</param>
        /// <param name="key">加密算法密钥</param>
        /// <param name="iv">加密算法初始化向量</param>
        /// <returns></returns>
        public static string SymmetryEncrypt<T>(T encryptor, string text, Encoding encd, string key, string iv)
            where T : SymmetricAlgorithm
        {
            if (string.IsNullOrEmpty(text)
                || string.IsNullOrEmpty(key)
                || string.IsNullOrEmpty(iv))
                throw new ArgumentNullException("参数不正确！");
            CryptoStream cs = null;
            Encoding ec = encd ?? Encoding.Default;
            Byte[] StrBytes = ec.GetBytes(text);
            Byte[] KeyBytes = Encoding.ASCII.GetBytes(key);
            Byte[] IVBytes = Encoding.ASCII.GetBytes(iv);
            try
            {
                MemoryStream ms = new MemoryStream();
                cs = new CryptoStream(ms, encryptor.CreateEncryptor(KeyBytes, IVBytes), CryptoStreamMode.Write);
                cs.Write(StrBytes, 0, StrBytes.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (CryptographicException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            finally
            {
                if (cs != null)
                    cs.Close();
            }
        }

        /// <summary>
        /// 对称解密算法
        /// </summary>
        /// <typeparam name="T">对称解密算法类型</typeparam>
        /// <param name="decryptor">对称解密算法实例对象</param>
        /// <param name="ciphertext">代解密的字符串</param>
        /// <param name="encd">输出的解密字符串编码</param>
        /// <param name="key">解密密钥</param>
        /// <param name="iv">解密初始化向量</param>
        /// <returns></returns>
        public static string SymmetryDecrypt<T>(T decryptor, string ciphertext, Encoding encd, string key, string iv)
            where T : SymmetricAlgorithm
        {
            if (string.IsNullOrEmpty(ciphertext)
                || string.IsNullOrEmpty(key)
                || string.IsNullOrEmpty(iv))
                throw new ArgumentNullException("参数不正确！");
            CryptoStream cs = null;
            Encoding ec = encd ?? Encoding.Default;
            Byte[] StrBytes = Convert.FromBase64String(ciphertext);
            Byte[] KeyBytes = Encoding.ASCII.GetBytes(key);
            Byte[] IVBytes = Encoding.ASCII.GetBytes(iv);
            try
            {
                MemoryStream ms = new MemoryStream();
                cs = new CryptoStream(ms, decryptor.CreateDecryptor(KeyBytes, IVBytes), CryptoStreamMode.Write);
                cs.Write(StrBytes, 0, StrBytes.Length);
                cs.FlushFinalBlock();
                return ec.GetString(ms.ToArray());
            }
            catch (CryptographicException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            finally
            {
                if (cs != null)
                    cs.Close();
            }
        }
    }
}
