using System;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace TiebaSignIn.Util
{
    public static class Encryption
    {

        public static string EncodeMD5(string value)
        {
            using var loggerFactory = LoggerFactory.Create(conf => { conf.AddConsole(); });
            ILogger logger = loggerFactory.CreateLogger(nameof(Encryption));

            try
            {
                var md5 = MD5.Create();
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                var sb = new StringBuilder();
                foreach (var item in hash)
                {
                    sb.Append(item.ToString("X2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                logger.LogError("字符串进行MD5加密错误 -- {0}", ex);
                throw;
            }
        }
    }
}