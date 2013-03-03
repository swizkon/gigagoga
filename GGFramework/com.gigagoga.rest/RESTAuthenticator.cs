using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;

namespace com.gigagoga.rest
{
    /// <summary>
    /// Summary description for RESTAuthenticator
    /// </summary>
    public class RESTAuthenticator
    {

        #region Private fields

        // Message to wrap.
        private string _message = "Not Authenticated";

        // Http status code
        private HttpStatusCode _statusCode = HttpStatusCode.OK;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the status message of the request.
        /// </summary>
        public string StatusMessage
        {
            get { return _message; }
            set { _message = value; }
        }

        /// <summary>
        /// Gets or sets the StatusCode for the request.
        /// The HashCode of this enum corresponds to HTTP Status codes.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
            set { _statusCode = value; }
        }

        #endregion



        /// <summary>
        /// Perform validation using all QS or form params and load 
        /// APIKEY etc from the headers collection.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="allowedHttpMethods"></param>
        /// <returns></returns>
        public static RESTAuthenticator AuthenticateByHeaders(HttpRequest request, string[] allowedHttpMethods)
        {
            RESTAuthenticator auth = new RESTAuthenticator();

            string xApiKey = request.Headers["X-ApiKey"];
            string xTimestamp = request.Headers["X-Timestamp"];
            string xSignature = request.Headers["X-Signature"];


            if (String.IsNullOrEmpty(xSignature) || String.IsNullOrEmpty(xApiKey) || String.IsNullOrEmpty(xTimestamp))
            {
                auth._message = "Bad Request";
                auth._statusCode = HttpStatusCode.BadRequest;
                return auth;
            }


            if (!isAllowedHttpMethod(request.HttpMethod, allowedHttpMethods))
            {
                auth._message = "Method Not Allowed";
                auth._statusCode = HttpStatusCode.MethodNotAllowed;
                return auth;
            }


            #region Validate Timestamp
            // Get timestamp as DateTime and validate:
            DateTime remoteTimestamp = new DateTime(1970, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            DateTime.TryParse(xTimestamp, out remoteTimestamp);
            if (!isValidTimestamp(remoteTimestamp.ToUniversalTime(), 60 * 15))
            {
                // Expired timestamp parameter.
                auth._message = "Timestamp invalid or expired";
                auth._statusCode = HttpStatusCode.BadRequest;
                return auth;
            }
            #endregion


            // Check if the API KEY and secret are OK
            String secret = GetSecretKey(xApiKey);
            if (String.IsNullOrEmpty(secret))
            {
                auth._message = "Invalid API key: " + xApiKey;
                auth._statusCode = HttpStatusCode.BadRequest;
                return auth;
            }

            if (! isValidResourceSignature(request, secret, xSignature))
            {
                auth._message = "Invalid signature";
                auth._statusCode = HttpStatusCode.BadRequest;
                return auth;
            }

            // Passed all tests
            auth._message = "OK";
            auth._statusCode = HttpStatusCode.OK;
            return auth;
        }

        /// <summary>
        /// Authenticates a Request and returns the status object.
        /// </summary>
        /// <param name="request">The request from which to get parameters.</param>
        /// <param name="allowedHttpMethods">The valid methods for this operation. Array can contain DELETE, GET, HEAD, PUT, POST</param>
        /// <param name="reqParams">An array of parameter names that must exist in the request.</param>
        /// <param name="apiKeyParamName">The name of the parameter that contains the Account identifier.</param>
        /// <param name="signatureParamName">The name of the parameter that contains the signed request.</param>
        /// <param name="timestampParamName">The name of the parameter that contains the timestamp in UTC format</param>
        /// <returns></returns>
        public static RESTAuthenticator AuthenticateRequest(HttpRequest request, string[] allowedHttpMethods, string[] reqParams, string apiKeyParamName, string signatureParamName, string timestampParamName)
        {
            RESTAuthenticator auth = new RESTAuthenticator();


            #region Validate Timestamp
            // Validate Timestamp
            // TODO Refactor this check to validate against Pragmatic paradigm
            // Exit if request missing timestamp param
            if (String.IsNullOrEmpty(request[timestampParamName]))
            {
                // Missing timestamp parameter.
                auth._message = "Missing Timestamp parameter";
                auth._statusCode = HttpStatusCode.BadRequest;
                return auth;
            }
            // Validate Timestamp
            // Has timestamp parameter.
            // Get timestamp as DateTime and validate:
            DateTime remoteTimestamp = new DateTime(1970, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            DateTime.TryParse(request[timestampParamName], out remoteTimestamp);
            if (!isValidTimestamp(remoteTimestamp.ToUniversalTime(), 60 * 15))
            {
                // Expired timestamp parameter.
                auth._message = "Timestamp invalid or expired";
                auth._statusCode = HttpStatusCode.BadRequest;
                return auth;
            }
            #endregion


            #region Validate HTTPMethod
            // Validate HTTPMethod
            if ( !isAllowedHttpMethod(request.HttpMethod, allowedHttpMethods) )
            {
                auth._message = "Method Not Allowed";
                auth._statusCode = HttpStatusCode.MethodNotAllowed;
                return auth;
            }
            #endregion


            #region Validate parameters
            if (missingRequiredParams(request, reqParams))
            {
                // Missing required parameter.
                auth._message = "Missing required parameters";
                auth._statusCode = HttpStatusCode.BadRequest;
                return auth;
            }
            #endregion



            #region Validate signature
            string secretKey = GetSecretKey(request[apiKeyParamName]);
            string signature = request[signatureParamName];
            if (!isValidSignature(request, secretKey, signature, reqParams))
            {
                auth._message = "Invalid signature";
                auth._statusCode = HttpStatusCode.BadRequest;
                return auth;
            }
            #endregion


            // If all tests passed set and return OK response
            auth._message = "OK";
            auth._statusCode = HttpStatusCode.OK;
            return auth;
        }

        /// <summary>
        /// Checks if the httpMethod exists in the allowedHttpMethods-array.
        /// Returns true if httpMethod is found, else false.
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="allowedHttpMethods"></param>
        /// <returns>Returns true if httpMethod is found, else false.</returns>
        private static bool isAllowedHttpMethod(string httpMethod, string[] allowedHttpMethods)
        {
            for (int i = 0; i < allowedHttpMethods.Length; i++)
                if (allowedHttpMethods[i].Equals(httpMethod))
                    return true;
            return false;
        }

        /// <summary>
        /// Checks if a DateTime is within a range of seconds.
        /// </summary>
        /// <param name="remoteTimestamp">An UTC DateTime</param>
        /// <param name="secondsRange">The valid offset remote server.</param>
        /// <returns></returns>
        private static bool isValidTimestamp(DateTime remoteTimestamp, int secondsRange)
        {
            return ((int)Math.Abs(DateTime.UtcNow.Subtract(remoteTimestamp).TotalSeconds) < secondsRange);
        }

        /// <summary>
        /// Checks if a required parameter
        /// is missing or is an empty string.
        /// Returns true if missing required parameter, else false.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="reqParams"></param>
        /// <returns>Returns true if all required parameters are found, else false.</returns>
        private static bool missingRequiredParams(HttpRequest request, string[] reqParams)
        {
            // Iterate throught the required params:
            for (int i = 0; i < reqParams.Length; i++)
                // If is null or empty return true
                if( String.IsNullOrEmpty( request[ reqParams[i] ]) )
                    return true;
            // All exists, return true
            return false;
        }


        /// <summary>
        /// Has OK secret create a signature using the Method + URI + Timestamp
        /// </summary>
        /// <param name="request"></param>
        /// <param name="secretKey"></param>
        /// <param name="remoteSignature"></param>
        /// <returns></returns>
        private static bool isValidResourceSignature(HttpRequest request, string secretKey, string remoteSignature)
        {
            string signatureData = request.HttpMethod + ' ' + request.Url.AbsoluteUri + ' ' + request.Headers["X-Timestamp"];
            string localSignature = RESTUtil.SignString(signatureData, secretKey);
            return localSignature.Equals(remoteSignature);
        }


        private static bool isValidSignature(HttpRequest request, string secretKey, string signature, string[] signatureParams)
        {
            // Prevent from executing if missing params:

            if( String.IsNullOrEmpty(secretKey) || String.IsNullOrEmpty(signature) )
            {
                return false;
            }

            // string secretKey = GetSecretKey(request[apiKeyParamName]); 

            IDictionary<string, string> signatureKeys = new SortedDictionary<string, string>();
            for (int i = 0; i < signatureParams.Length; i++)
            {
                signatureKeys.Add(signatureParams[i], request[signatureParams[i]]);
            }
            // signatureKeys.Remove(signatureParamName);

            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<String, String> pair in signatureKeys)
            {
                if (pair.Value != null && pair.Value.Length > 0)
                {
                    data.Append(pair.Key);
                    data.Append(pair.Value);
                }
            }

            string localSignature = RESTUtil.GetSignature(signatureKeys, secretKey);

            data = null;
            signatureKeys = null;

            return (localSignature.Equals(signature));
        }


        /// <summary>
        /// Returns the secret key for an apiKey,
        /// or null if not found.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static string GetSecretKey(string apiKey)
        {
            // TODO Add a static Dictionary to speed up lookup 
            // TODO Move this method to the RESTUtil-class.

            string secretKey = null;

            // Break early if not correct input.
            if (String.IsNullOrEmpty(apiKey))
            {
                return secretKey;
            }

            if (RESTKeyManager.IsInitialized)
            {
                secretKey = RESTKeyManager.GetSecretKey(apiKey);
            }
            else if (!String.IsNullOrEmpty(apiKey))
            {
                string apiKeyFile = HttpRuntime.AppDomainAppPath.TrimEnd('\\') + "\\App_Data\\AUTHKEY_DIRECTORY\\" + apiKey;

                if (System.IO.File.Exists(apiKeyFile))
                {
                    secretKey = System.IO.File.ReadAllText(apiKeyFile);
                }
            }
            return secretKey;
        }


    }
}