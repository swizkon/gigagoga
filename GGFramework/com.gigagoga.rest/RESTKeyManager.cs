using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace com.gigagoga.rest
{
    /// <summary>
    /// A static class containing methods to create and get
    /// ApiKey and SecretKey data.
    /// </summary>
    public static class RESTKeyManager
    {
        private static IDictionary<char, byte> ___keys = null;
        private static IDictionary<byte, char> ___values = null;

        private static string ___apiKeyDirectory = null;


        /// <summary>
        /// Gets or sets the bytes to use for
        /// writing the api keys and secret keys.
        /// The length must be 256.
        /// </summary>
        private static byte[] ___Index
        {
            get
            {
                List<byte> result = new List<byte>();
                using (IEnumerator<KeyValuePair<char, byte>> iter = ___keys.GetEnumerator())
                {
                    while (iter.MoveNext())
                    {
                        result.Add(iter.Current.Value);
                    }
                }
                return result.ToArray();
            }
            set
            {
                if (value.Length != 256)
                {
                    throw new ArgumentException("The length of the byte array doesnt match.");
                }
                ___keys = new Dictionary<char, byte>(256);
                ___values = new Dictionary<byte, char>(256);
                for (int i = 0; i < 256; i++)
                {
                    ___keys.Add((char)i, value[i]);
                    ___values.Add(value[i], (char)i);
                }
            }
        }


        /// <summary>
        /// Creates a new api key with a secret key.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="secretkey"></param>
        /// <returns></returns>
        public static bool Create(string apiKey, string secretkey)
        {
            string path = ___getApiKeyFilePath(apiKey);
            if (File.Exists(path))
            {
                return false;
            }
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, ___encrypt(secretkey), Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



        /// <summary>
        /// Returns the secret key value for the apiKey,
        /// or null if apikey doesnt match.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static string GetSecretKey(string apiKey)
        {
            string path = ___getApiKeyFilePath(apiKey);
            if (!File.Exists(path))
            {
                return null;
            }
            try
            {
                string fileData = File.ReadAllText(path, Encoding.UTF8);
                return ___decrypt(fileData);
            }
            catch (Exception)
            {
                return null;
            }
        }


        private static string ___getApiKeyFilePath(string apiKey)
        {
            if (___apiKeyDirectory == null)
            {
                throw new InvalidOperationException("The ApikeyDirectory is not set. Call Initialize.");
            }
            if (___keys == null)
            {
                throw new InvalidOperationException("The index is not set. Call Initialize.");
            }

            string result = ___apiKeyDirectory.TrimEnd('\\');


            // Single folder name
            string keyFolderName = "\\";
            // Two folders depth
            string folder1 = "\\", folder2 = "\\";

            // Create a single folder for the key
            char[] chars = apiKey.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                byte b = ___keys[chars[i]];
                keyFolderName += b.ToString("x2");

                // Check which folder to append to:
                if (i % 2 == 0)
                {
                    folder2 += b.ToString("x2");
                }
                else
                {
                    folder1 += b.ToString("x2");
                }
            }

            result += keyFolderName;

            /*
            char[] chars = apiKey.PadLeft(apiKey.Length + (3 - apiKey.Length % 3), '0').ToCharArray();
            for (int i = 2; i < chars.Length; i = i + 3)
            {
                byte b0 = ___keys[chars[i - 2]];
                byte b1 = ___keys[chars[i - 1]];
                byte b2 = ___keys[chars[i]];

                result += "\\" + b0.ToString("x2")
                               + b1.ToString("x2")
                               + b2.ToString("x2");
            }
            */
            return result + '\\' + ___getFileName(apiKey);
        }

        private static string ___getFileName(string fileKey)
        {
            return fileKey.GetHashCode().ToString("x2") + ".gkx";
        }


        /// <summary>
        /// Indicating if this class is initiated and ready for use.
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return ( ___apiKeyDirectory != null && ___keys != null );
            }
        }

        /// <summary>
        /// Create a new index if it is not already set.
        /// </summary>
        /// <param name="storeKey">A name for storing the index keys for this </param>
        /// <param name="apiKeyFolder">The directory where keys are stored</param>
        public static void Initialize(string storeKey, string apiKeyFolder)
        {
            ___apiKeyDirectory = apiKeyFolder;

            if (___keys == null)
            {
                // Try load and decrypt file with this key, else create
                // random index and store.
                string path = ___apiKeyDirectory.TrimEnd('\\') + '\\' + ___getFileName(storeKey);
                //Does the file exist, load it, else create random index and store it.
                if (File.Exists(path))
                {
                    ___Index = File.ReadAllBytes(path);
                }
                else
                {
                    ___generateNewIndex();
                    File.WriteAllBytes(path, ___Index);
                }
            }
        }


        private static void ___generateNewIndex()
        {
            if (___keys == null)
            {
                List<byte> tempChars = new List<byte>(256);
                for (int i = 0; i < 256; i++)
                {
                    tempChars.Add((byte)i);
                }
                List<byte> valueChars = new List<byte>(256);
                Random rnd = new Random(Environment.TickCount);
                while (tempChars.Count > 0)
                {
                    int index = rnd.Next(0, tempChars.Count);
                    valueChars.Add(tempChars[index]);
                    tempChars.RemoveAt(index);
                }
                ___keys = new Dictionary<char, byte>(256);
                ___values = new Dictionary<byte, char>(256);
                for (int i = 0; i < 256; i++)
                {
                    ___keys.Add((char)i, valueChars[i]);
                    ___values.Add(valueChars[i], (char)i);
                }
            }
        }



        private static string ___encrypt(string input)
        {
            string result = "";
            char[] chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                result += (char)___keys[chars[i]];
            }
            return result;
        }

        private static string ___decrypt(string input)
        {
            string result = "";
            char[] chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                result += (char)___values[(byte)chars[i]];
            }
            return result;
        }
    }
}
