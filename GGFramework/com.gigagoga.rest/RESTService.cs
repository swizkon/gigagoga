using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Services;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Xml;

namespace com.gigagoga.rest
{
    /// <summary>
    /// Class that contains methods to validate an incoming request.
    /// </summary>
    public class RESTService : WebService
    {
        // private members for faster access.
        private string _apiKey, _secretKey;

        private String _rawEntityBody = null;

        private NameValueCollection _queryData = null;
        private NameValueCollection _formData = null;

        /// <summary>
        /// Gets the API key associated with the request.
        /// The value maybe null.
        /// </summary>
        public String ApiKey
        {
            get
            {
                return _apiKey ?? (_apiKey = Context.Request.Headers["X-ApiKey"]);
            }
        }

        /// <summary>
        /// Gets the Secret key associated with the API key.
        /// The value maybe null.
        /// </summary>
        public String SecretKey
        {
            get
            {
                return _secretKey ?? (_secretKey = RESTAuthenticator.GetSecretKey(ApiKey) );
            }
        }

        
        /// <summary>
        /// Gets the query part as a NameValueCollection.
        /// </summary>
        public NameValueCollection QueryString
        {
            get
            {
                if (_queryData == null && Context.Request.Headers["X-Encrypted-Query-Data"] == "true")
                {
                    try
                    {
                        string qData = HttpUtility.UrlDecode( Context.Request.Url.Query.TrimStart('?'), Encoding.UTF8);
                        _queryData = HttpUtility.ParseQueryString(RESTUtil.DecryptString(qData, ApiKey, SecretKey), Encoding.UTF8);
                    }
                    catch (Exception)
                    {
                        _queryData = null;
                    }
                }
                else if (_queryData == null)
                {
                    _queryData = Context.Request.QueryString;
                }

                return _queryData ?? (_queryData = new NameValueCollection());
            }
        }

        /// <summary>
        /// Gets the body without considering encryption.
        /// </summary>
        public String RawEntityBody
        {
            get
            {
                if (_rawEntityBody == null)
                {
                    using (StreamReader reader = new StreamReader(Context.Request.InputStream, Encoding.UTF8))
                    {
                        _rawEntityBody = reader.ReadToEnd();
                        reader.Close();
                    }
                }
                return _rawEntityBody;
            }
        }

        /// <summary>
        /// Gets the body and decrypts it if header "X-Encrypted-Entity-Data" is true.
        /// </summary>
        public String EntityBody
        {
            get
            {
                String entityBody = RawEntityBody;
                if (Context.Request.Headers["X-Encrypted-Entity-Data"] == "true")
                {
                    entityBody = RESTUtil.DecryptString(entityBody, ApiKey, SecretKey);
                }
                return entityBody;
            }
        }

        /// <summary>
        /// Gets the entity body as a NameValueCollection.
        /// </summary>
        public NameValueCollection Form
        {
            get
            {
                if (_formData == null && Context.Request.Headers["X-Encrypted-Entity-Data"] == "true")
                {
                    try
                    {
                        _formData = HttpUtility.ParseQueryString(EntityBody, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        _formData = null;
                        WriteToResponse(ex.GetType().Name + " " + ex.Message);
                    }
                }
                else if (_formData == null)
                {
                    _formData = Context.Request.Form;
                }

                return _formData ?? (_formData = new NameValueCollection());
            }
        }


        /// <summary>
        /// CTor
        /// </summary>
        public RESTService()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Uses the headers to authenticate an incoming request.
        /// Allowed methods are GET and POST.
        /// </summary>
        /// <returns></returns>
        public RESTAuthenticator Authenticate()
        {
            return RESTAuthenticator.AuthenticateByHeaders(Context.Request, new string[]{"GET", "POST"});
        }


        /// <summary>
        /// Writes the string to the Response output stream.
        /// </summary>
        /// <param name="s"></param>
        public void WriteToResponse(string s)
        {
            if (String.IsNullOrEmpty(ApiKey) || String.IsNullOrEmpty(SecretKey))
            {
                Context.Response.Write(s);
            }
            else if (Context.Request.Headers["X-Encrypt-Response"] == "true")
            {
                Context.Response.AddHeader("X-Encrypted", "true");
                Context.Response.Write(RESTUtil.EncryptString(s, ApiKey, SecretKey));
            }
            else
            {
                Context.Response.Write(s);
            }
        }

        /// <summary>
        /// Saves a XML document to the output.
        /// Also sets the response content type.
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void WriteToResponse(XmlDocument xmlDoc)
        {
            Context.Response.AddHeader("X-Content-Type", "application/xml");
            WriteToResponse(xmlDoc.OuterXml);
        }
    }
}
