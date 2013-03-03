using System;
using System.Collections.Generic;
using System.Text;

namespace com.gigagoga.rest
{
    /// <summary>
    /// Basic props for a web service
    /// </summary>
    public static class RESTConfig
    {
        private static string _apiKey, _secretKey, _apiKeyDirectory;

        /// <summary>
        /// Api key, consumer key...
        /// </summary>
        public static string ApiKey
        {
            get { return _apiKey; }
        }

        /// <summary>
        /// Secret key
        /// </summary>
        public static string SecretKey
        {
            get { return _secretKey; }
        }

        /// <summary>
        /// Place for storing key values.
        /// </summary>
        public static string ApiKeyDirectory
        {
            get { return _apiKeyDirectory; }
        }

        /// <summary>
        /// Initiates the static class.
        /// </summary>
        /// <param name="apikey"></param>
        /// <param name="secretKey"></param>
        /// <param name="apiKeyDirectory"></param>
        public static void Init(string apikey, string secretKey, string apiKeyDirectory)
        {
            _apiKey = apikey;
            _secretKey = secretKey;
            _apiKeyDirectory = apiKeyDirectory;
        }


    }
}
