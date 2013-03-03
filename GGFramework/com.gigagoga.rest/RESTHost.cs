using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Collections.Specialized;

namespace com.gigagoga.rest
{
    /// <summary>
    /// Represents a REST service host.
    /// For example a WebService.
    /// </summary>
    public class RESTHost
    {
        private String _baseURI = null;
        private String _resource = null;
        private String _userAgent = null;

        private String _apiKey = null;
        private String _secret = null;


        /// <summary>
        /// Gets or sets the ServiceURL property.
        /// For example the URI to a WebService.
        /// </summary>
        public String BaseURI
        {
            get { return _baseURI; }
            set { _baseURI = value; }
        }

        /// <summary>
        /// Gets or sets the Resource property.
        /// For example the method name of a WebService
        /// or a resource location.
        /// </summary>
        public String Resource
        {
            get { return this._resource; }
            set { this._resource = value; }
        }


        /// <summary>
        /// Gets or sets the UserAgent property.
        /// </summary>
        public String UserAgent
        {
            get { return this._userAgent; }
            set { this._userAgent = value; }
        }

        /// <summary>
        /// Creates a new instance of this object.
        /// </summary>
        public RESTHost(string apiKey, string secretKey)
        {
            _apiKey = apiKey;
            _secret = secretKey;
        }

        /// <summary>
        /// Invokes the req against a remote server.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="readBytes"></param>
        /// <returns></returns>
        public RESTResult InvokeRequest(RESTRequest req, bool readBytes)
        {
            // If encrypt entity data add that header.
            if (req.EncryptEntityData)
            {
                req.Headers["X-Encrypted-Entity-Data"] = "true";
            }

            // If encrypt entity data add that header.
            if (req.EncryptQueryData)
            {
                req.Headers["X-Encrypted-Query-Data"] = "true";
            }

            string timestamp = RESTUtil.GetTimestamp();
            String signatureData = req.HttpMethod + ' ' + this.getResourceURI(req) + ' ' + timestamp;

            req.Headers["X-Timestamp"] = timestamp;
            req.Headers["X-ApiKey"] = _apiKey;
            req.Headers["X-Signature"] = RESTUtil.SignString(signatureData, _secret);


            RESTResult result = new RESTResult();
            result = getPostResponse(req, readBytes);

            return result;
        }

        /// <summary>
        /// Invokes a web service req.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public RESTResult InvokeRequest(RESTRequest req)
        {
            return InvokeRequest(req, false);
        }



        private RESTResult getPostResponse(RESTRequest req, bool readRaw)
        {
            RESTResult result = new RESTResult();

            HttpWebRequest request = WebRequest.Create(this.getResourceURI(req)) as HttpWebRequest;
            request.AllowAutoRedirect = false;
            request.Headers.Add(req.Headers);

            request.UserAgent = this.UserAgent;
            request.Method = req.HttpMethod;
            request.Timeout = 50000;

            if (req.HttpMethod == "POST" && req.EntityData == null)
            {
                req.EntityData = "";
            }

            if (req.HttpMethod == "POST" && req.EntityData != null)
            {
                string data = req.EntityData;
                if (req.EncryptEntityData)
                {
                    data = RESTUtil.EncryptString(data, _apiKey, _secret);
                }

                byte[] requestData = new UTF8Encoding().GetBytes(data);
                request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                request.ContentLength = requestData.Length;

                /* Submit the request to the body */
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(requestData, 0, requestData.Length);
                    requestStream.Close();
                }
            }

            try
            {
                using (HttpWebResponse httpResponse = request.GetResponse() as HttpWebResponse)
                {
                    result.setStatusCode(httpResponse.StatusCode);
                    result.setHeaders(httpResponse.Headers as NameValueCollection);
                    using(Stream response = httpResponse.GetResponseStream())
                    {
                        if (readRaw)
                        {
                            int len = (int)httpResponse.ContentLength;

                            byte[] respRaw = new byte[len];
                            response.Read(respRaw, 0, len);
                            result.setResponseRaw(respRaw);
                        }

                        if (!readRaw)
                        {
                            using (StreamReader reader = new StreamReader(response, Encoding.UTF8))
                            {
                                result.setResponseText(reader.ReadToEnd());
                                if (result.Headers["X-Encrypted"] == "true")
                                {
                                    result.setResponseText(RESTUtil.DecryptString(result.ResponseText, _apiKey, _secret));
                                }
                                reader.Close();
                            }
                        }
                        httpResponse.Close();
                    }
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    using (HttpWebResponse errorResponse = webEx.Response as HttpWebResponse)
                    {
                        result.setStatusCode(errorResponse.StatusCode);
                        result.setHeaders(errorResponse.Headers as NameValueCollection);
                        using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream(), Encoding.UTF8))
                        {
                            result.setResponseText(reader.ReadToEnd());
                            if (result.Headers["X-Encrypted"] == "true")
                            {
                                result.setResponseText(RESTUtil.DecryptString(result.ResponseText, _apiKey, _secret));
                            }
                            reader.Close();
                        }
                    }
                }
                else
                {
                    result.setStatusCode(HttpStatusCode.BadRequest);
                    result.setResponseText(webEx.Message);
                }
            }

            return result;
        }

        private string getResourceURI(RESTRequest req)
        {
            String resourceURI = this.BaseURI + this.Resource;
            String query = req.GetQueryString();
            if (!String.IsNullOrEmpty(query))
            {
                if (req.EncryptQueryData)
                {
                    query = HttpUtility.UrlEncode( RESTUtil.EncryptString(query, _apiKey, _secret), Encoding.UTF8);
                }
                resourceURI += '?' + query;
            }
            return resourceURI;
        }

    }
}