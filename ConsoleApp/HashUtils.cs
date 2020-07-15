using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EnsureThat;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ConsoleApp
{
    public interface IHashUtils
    {
        bool CheckHash(IEnumerable<KeyValuePair<string, StringValues>> collection);
        string CalcHMAC(string val);
            
    }
    
    public class AvangateSettings
    {
        public string HmacKey { get; set; }
        
        public string MerchantCode { get; set; }
        
        public string SetupAndTrainingPackageProductId { get; set; }
    }
    
    internal sealed class HashUtils : IHashUtils
    {
        private readonly string HmacKey;

        public HashUtils(AvangateSettings avangateSettings)
        {
            EnsureArg.IsNotNullOrWhiteSpace(avangateSettings.HmacKey, nameof(avangateSettings.HmacKey));
            EnsureArg.IsNotNullOrWhiteSpace(avangateSettings.HmacKey,
                $"{nameof(avangateSettings)}.{nameof(avangateSettings)}.{nameof(avangateSettings.HmacKey)}");

            this.HmacKey = avangateSettings.HmacKey;
        }

        static string StripSplashes(string str)
        {
            return str.Replace("\\\\", "\\").Replace("\\'", "'");
        }

        static int StrLength(string str)
        {
            return Encoding.UTF8.GetByteCount(str);
        }

        public bool CheckHash(IEnumerable<KeyValuePair<string, StringValues>> collection)
        {
            EnsureArg.IsNotNull(collection, nameof(collection));

            string baseStringForHash = "";
            string hashValue = "";
            foreach (var kv in collection)
            {
                var value = kv.Value;
                if (value.Count == 0)
                {
                    baseStringForHash += "0";
                    continue;
                }
                if (kv.Key == "HASH")
                {
                    hashValue = value[0];
                }
                else
                {
                    baseStringForHash += string.Concat(value.Select(StripSplashes).Select(v => StrLength(v) + v));
                }
            }
            if (string.IsNullOrWhiteSpace(hashValue) || string.IsNullOrWhiteSpace(baseStringForHash))
            {
                return false;
            }
            var hmac = CalcHMAC(baseStringForHash);
            return hmac.Equals(hashValue.ToLowerInvariant());
        }

        public string CalcHMAC(string val) => CalcHmac(val, this.HmacKey);

        public static string CalcHmac(string val, string hmacKey)
        {
            var keyInBytes = Encoding.UTF8.GetBytes(hmacKey);
            var payloadInBytes = Encoding.UTF8.GetBytes(val);

            var md5 = new HMACMD5(keyInBytes);
            var hash = md5.ComputeHash(payloadInBytes);

            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}