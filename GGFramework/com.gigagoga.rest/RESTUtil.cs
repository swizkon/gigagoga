using System;
using System.Collections.Generic;
using System.Web;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace com.gigagoga.rest
{
    /// <summary>
    /// Summary description for RESTUtil
    /// </summary>
    public static class RESTUtil
    {
        /// <summary>
        /// Returns current date as ISO 8601 timestamp
        /// </summary>
        /// <returns>Returns current date as ISO 8601 timestamp</returns>
        public static string GetTimestamp()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd\\THH:mm:ss.fff\\Z");
        }

        /// <summary>
        /// Static for generating a signature.
        /// </summary>
        /// <param name="sortedDictionary"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static string GetSignature(IDictionary<string, string> sortedDictionary, string secretKey)
        {
            StringBuilder data = new StringBuilder();

            foreach (KeyValuePair<String, String> pair in sortedDictionary)
            {
                if (pair.Value != null && pair.Value.Length > 0)
                {
                    data.Append(pair.Key);
                    data.Append(pair.Value);
                }
            }
            return SignString(data.ToString(), secretKey);
        }


        /// <summary>
        /// Computes RFC 2104-compliant HMAC signature.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static string SignString(string data, string secretKey)
        {
            Encoding encoding = new UTF8Encoding();
            HMACSHA1 signature = new HMACSHA1(encoding.GetBytes(secretKey));
            return Convert.ToBase64String(signature.ComputeHash(encoding.GetBytes(data.ToCharArray())));
        }


        private static Rijndael _GetRijndael(string apiKey, string secretKey)
        {
            Rijndael result = Rijndael.Create();
            result.KeySize = 256;
            result.BlockSize = 256;
            result.Key = _getKey(apiKey);
            result.IV = _getIV(secretKey);
            return result;
        }

        /// <summary>
        /// Returns a byte-array for encryption based on machine IdKey.
        /// </summary>
        /// <returns>Returns a byte-array for encryption based on machine IdKey.</returns>
        private static byte[] _getKey(string apiKey)
        {
            byte[] Salt = Encoding.ASCII.GetBytes(apiKey);
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(apiKey, Salt);
            byte[] _key = SecretKey.GetBytes(32);
            return _key;
        }

        /// <summary>
        /// Returns a byte-array for encryption based on machine IdKey.
        /// </summary>
        /// <returns>Returns a byte-array for encryption based on machine IdKey.</returns>
        private static byte[] _getIV(string secretKey)
        {
            MD5 md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(secretKey));
            // Create a new Stringbuilder to collect the bytes and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            byte[] result = new byte[32];
            char[] chars = sBuilder.ToString().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                result[i] = (byte)chars[i];
            }
            return result;
        }

        /// <summary>
        /// EncryptString
        /// </summary>
        /// <param name="input"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static string EncryptString(string input, string apiKey, string secretKey)
        {
            string result = "";
            byte[] PlainText = Encoding.Unicode.GetBytes(input);
            using (Rijndael RijndaelCipher = _GetRijndael(apiKey, secretKey))
            {
                ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(RijndaelCipher.Key, RijndaelCipher.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(PlainText, 0, PlainText.Length);
                        cryptoStream.FlushFinalBlock();
                        byte[] CipherBytes = memoryStream.ToArray();
                        cryptoStream.Close();
                        result = Convert.ToBase64String(CipherBytes);
                    }
                    memoryStream.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// DecryptString
        /// </summary>
        /// <param name="input"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static string DecryptString(string input, string apiKey, string secretKey)
        {
            // string id = key;
            byte[] EncryptedData = Convert.FromBase64String(input);
            byte[] PlainText = new byte[EncryptedData.Length];

            Rijndael RijndaelCipher = _GetRijndael(apiKey, secretKey);

            ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(RijndaelCipher.Key, RijndaelCipher.IV);
            MemoryStream memoryStream = new MemoryStream(EncryptedData);
            // Create a CryptoStream. (always use Read mode for decryption).
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);
            // Since at this point we don't know what the size of decrypted data
            // will be, allocate the buffer long enough to hold EncryptedData
            // DecryptedData is never longer than EncryptedData.

            // Start decrypting.
            int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);
            memoryStream.Close();
            cryptoStream.Close();
            // Decryptor.Dispose();
            // Convert decrypted data into a string.

            string DecryptedData = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);
            // Return decrypted string.  
            return DecryptedData;
        }

    }
}