using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.Collections.Specialized;


namespace com.gigagoga.rest
{
    /// <summary>
    /// Summary description for RESTResponse
    /// </summary>
    public class RESTResult
    {
        // Headers
        private NameValueCollection _headers = new NameValueCollection();

        // The response in plain or encrypted text.
        private string _responseText;

        private byte[] _responseRaw;

        // HTTP Status code
        private HttpStatusCode _statusCode;


        /// <summary>
        /// Gets the response text.
        /// </summary>
        public String ResponseText
        {
            get { return _responseText; }
        }


        /// <summary>
        /// Gets the raw bytes in the response.
        /// </summary>
        public byte[] ResponseRaw
        {
            get
            {
                return _responseRaw;
            }
        }


        /// <summary>
        /// Gets the headers.
        /// </summary>
        public NameValueCollection Headers
        {
            get { return _headers; }
        }


        /// <summary>
        /// Gets the status code.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
            /*
            set { _statusCode = value; }
            */
        }

        /// <summary>
        /// CTor.
        /// Sets the default values.
        /// </summary>
        public RESTResult()
        {
            _statusCode = HttpStatusCode.OK;
            _headers = new NameValueCollection();
            _responseText = String.Empty;
        }

        /// <summary>
        /// Internal method for setting the response text
        /// </summary>
        /// <param name="headers"></param>
        internal void setHeaders(NameValueCollection headers)
        {
            _headers = headers;
        }

        /// <summary>
        /// Internal method for setting the response text
        /// </summary>
        /// <param name="responseText"></param>
        internal void setResponseText(string responseText)
        {
            _responseText = responseText;
        }


        /// <summary>
        /// Internal method for setting the status code.
        /// </summary>
        /// <param name="statusCode"></param>
        internal void setStatusCode(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }


        internal void setResponseRaw(byte[] respRaw)
        {
            _responseRaw = respRaw;
        }
    }
}