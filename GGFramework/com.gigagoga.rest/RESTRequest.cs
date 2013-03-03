using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Collections.Specialized;

namespace com.gigagoga.rest
{
    /// <summary>
    /// Summary description for RESTServiceRequest
    /// </summary>
    public class RESTRequest
    {

        private bool _encryptEntityData = false;
        private bool _encryptQueryData = false;

        private string _httpMethod = "GET";

        private IDictionary<string, string> _params = new SortedDictionary<string, string>();

        // custom headers.
        private NameValueCollection _headers = new NameValueCollection();

        // Entity data for POST
        private string _entityData = null;


        /// <summary>
        /// Gets or sets the body of the request.
        /// </summary>
        public String EntityData
        {
            get { return _entityData; }
            set { _entityData = value; }
        }

        /// <summary>
        /// Gets or sets if the body should be encrypted before posted to the REST service.
        /// The default value is false.
        /// </summary>
        public bool EncryptEntityData
        {
            get { return _encryptEntityData; }
            set { _encryptEntityData = value; }
        }

        /// <summary>
        /// Gets or sets if the query portion should be encrypted before posted to the REST service.
        /// The default value is false.
        /// </summary>
        public bool EncryptQueryData
        {
            get { return _encryptQueryData; }
            set { _encryptQueryData = value; }
        }

        /// <summary>
        /// Gets or sets the HttpMethod to use.
        /// GET, POST, HEAD etc.
        /// Default value is GET.
        /// </summary>
        public string HttpMethod
        {
            get { return _httpMethod; }
            set { _httpMethod = value; }
        }

        /// <summary>
        /// A collection containing the headers to
        /// send to the resource.
        /// </summary>
        public NameValueCollection Headers
        {
            get { return _headers; }
        }

        /// <summary>
        /// A collection containing the parameters to
        /// send to the resource.
        /// </summary>
        public IDictionary<string, string> Parameters
        {
            get { return _params; }
        }


        /// <summary>
        /// CTor
        /// </summary>
        public RESTRequest()
        {
            //
            // TODO: Add constructor logic here
            //
        }



        /// <summary>
        /// Sets the EntityData from a string.
        /// </summary>
        /// <param name="entity"></param>
        public void SetEntityData(String entity)
        {
            _entityData = entity;
        }

        /// <summary>
        /// Setst the entity in qs format.
        /// IE a form string from a name value collection.
        /// </summary>
        /// <param name="formData"></param>
        public void SetEntityData(NameValueCollection formData)
        {
            if (formData.Count < 1)
            {
                _entityData = null;
                return;
            }

            StringBuilder sb = new StringBuilder();
            for(int i = 0 ; i < formData.Count ; i++)
            {
                sb.Append(formData.Keys[i])
                    .Append('=')
                    .Append(formData[i])
                    .Append('&');
            }
            _entityData = sb.Remove( sb.Length-1 , 1).ToString();
        }

        /// <summary>
        /// Get the query part.
        /// </summary>
        /// <returns></returns>
        public string GetQueryString()
        {
            StringBuilder buffer = new StringBuilder();
            foreach (String key in (IEnumerable<String>)_params.Keys)
            {
                String value = _params[key];
                if (value != null && value.Length > 0)
                {
                    buffer.Append(key);
                    buffer.Append('=');
                    buffer.Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
                    buffer.Append('&');
                }
            }
            String stringData = buffer.ToString();
            if (stringData.EndsWith("&"))
            {
                stringData = stringData.Remove(stringData.Length - 1, 1);
            }
            return stringData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public RESTResult GetResult(RESTHost host)
        {
            return host.InvokeRequest(this, false);
        }

        /// <summary>
        /// Returns the raw text as bytes, ie not decruypted from the host.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public RESTResult GetRaw(RESTHost host)
        {
            return host.InvokeRequest(this, true);
        }


    }
}