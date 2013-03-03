using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace com.gigagoga.core.security
{
    /// <summary>
    /// Contains static methods for encryption.
    /// </summary>
    public static class Cryptographer
    {

        private static byte[] ___getKey(string key)
        {
            byte[] Salt = Encoding.ASCII.GetBytes(key);
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(key, Salt);
            byte[] _key = SecretKey.GetBytes(32);
            return _key;
        }

        private static byte[] ___getIV(string key)
        {
            MD5 md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(key));
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

        private static Rijndael ___getRijndael(string cryptKey, string cryptIV)
        {
            Rijndael result = Rijndael.Create();
            result.KeySize = 256;
            result.BlockSize = 256;
            result.Key = ___getKey(cryptKey);
            result.IV = ___getIV(cryptIV);
            return result;
        }

        /// <summary>
        /// Encrypts a string using two keys.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="secret"></param>
        /// <param name="cryptValue"></param>
        /// <returns></returns>
        public static string Encrypt(string userName, string secret, string cryptValue)
        {
            string result = null;
            using (Rijndael crypto = ___getRijndael(userName, secret))
            {
                using (ICryptoTransform Encryptor = crypto.CreateEncryptor(crypto.Key, crypto.IV))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write))
                        {
                            byte[] cryptBytes = Encoding.Unicode.GetBytes(cryptValue);
                            cryptoStream.Write(cryptBytes, 0, cryptBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            byte[] CipherBytes = memoryStream.ToArray();
                            cryptoStream.Close();
                            result = Convert.ToBase64String(CipherBytes);
                        }
                        memoryStream.Close();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Decrypts a string using two keys.
        /// </summary>
        public static string Decrypt(string userName, string secret, string cryptValue)
        {
            string result = null;
            // string id = key;

            using (Rijndael RijndaelCipher = ___getRijndael(userName, secret))
            {
                using (ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(RijndaelCipher.Key, RijndaelCipher.IV))
                {
                    byte[] EncryptedData = Convert.FromBase64String(cryptValue);

                    using (MemoryStream memoryStream = new MemoryStream(EncryptedData))
                    {
                        // Create a CryptoStream. (always use Read mode for decryption).
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read))
                        {
                            // Since at this point we don't know what the size of decrypted data
                            // will be, allocate the buffer long enough to hold EncryptedData
                            // DecryptedData is never longer than EncryptedData.
                            byte[] PlainText = new byte[EncryptedData.Length];

                            // Start decrypting.
                            int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);
                            cryptoStream.Close();

                            // Decryptor.Dispose();
                            // Convert decrypted data into a string.
                            result = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);
                        }
                        memoryStream.Close();
                    }
                }
            }
            // Return decrypted string.  
            return result;
        }


        /// <summary>
        /// Returns a hashed hex value from input
        /// using the specified hash algorithm.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="hashAlgorithmType"></param>
        /// <returns></returns>
        /// <see cref="System.Security.Cryptography.HashAlgorithm"/>
        public static string HexHash(string input, HashAlgorithmType hashAlgorithmType)
        {
            string result;
            using (HashAlgorithm hashAlg = HashAlgorithm.Create(hashAlgorithmType.ToString().ToUpper()))
            {
                result = ___computeHexHash(hashAlg, input);
            }
            return result;
        }

        /// <summary>
        /// Returns the Base64String calculated by using a KeyedHashAlgorithm.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="algorithmType"></param>
        /// <returns></returns>
        public static string Base64Hash(string input, string key, KeyedHashAlgorithmType algorithmType)
        {
            return Base64Hash(input, new UTF8Encoding().GetBytes(key), algorithmType);
        }

        /// <summary>
        /// Returns the Base64String calculated by using a KeyedHashAlgorithm.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="algorithmType"></param>
        /// <returns></returns>
        public static string Base64Hash(string input, byte[] key, KeyedHashAlgorithmType algorithmType)
        {
            string result;
            using (KeyedHashAlgorithm keyedHashAlgorithm = KeyedHashAlgorithm.Create(algorithmType.ToString().ToUpper()))
            {
                keyedHashAlgorithm.Key = key;
                result = ___computeBase64Hash(keyedHashAlgorithm, input);
            }
            return result;
        }


        private static string ___computeHexHash(HashAlgorithm hashAlg, string input)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            StringBuilder result = new StringBuilder();
            byte[] hashedBytes = hashAlg.ComputeHash(encoder.GetBytes(input));
            for (int i = 0; i < hashedBytes.Length; i++)
            {
                result.Append(hashedBytes[i].ToString("x2"));
            }
            return result.ToString();
        }

        private static string ___computeBase64Hash(KeyedHashAlgorithm hashAlg, string input)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            StringBuilder result = new StringBuilder();
            byte[] hashedBytes = hashAlg.ComputeHash(encoder.GetBytes(input));
            return Convert.ToBase64String(hashedBytes);
        }


    }
}
