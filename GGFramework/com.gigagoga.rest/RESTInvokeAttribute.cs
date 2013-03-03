using System;
using System.Reflection;

namespace com.gigagoga.rest
{
    /// <summary>
    /// Summary description for RESTInvokeAttribute
    /// </summary>
    [Obfuscation()]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RESTInvokeAttribute : Attribute
    {
        private HttpVerbs httpVerb;
        private string uriTemplate;
        private string description;

        /// <summary>
        /// The verb that should be matched.
        /// First check when finding delegate method.
        /// </summary>
        public HttpVerbs HttpVerb
        {
            get { return this.httpVerb; }
            set { this.httpVerb = value; }
        }

        /// <summary>
        /// Pattern to match against.
        /// </summary>
        public string UriTemplate
        {
            get { return this.uriTemplate; }
            set { this.uriTemplate = value; }
        }

        /// <summary>
        /// Description of the method.
        /// </summary>
        public string Description
        {
            get { return this.description ?? ""; }
            set { this.description = value; }
        }

        /// <summary>
        /// CTor
        /// </summary>
        public RESTInvokeAttribute()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="httpVerb"></param>
        /// <param name="uriTemplate"></param>
        public RESTInvokeAttribute(HttpVerbs httpVerb, String uriTemplate)
        {
            this.httpVerb = httpVerb;
            this.uriTemplate = uriTemplate;
        }

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="httpVerb"></param>
        /// <param name="uriTemplate"></param>
        /// <param name="description"></param>
        public RESTInvokeAttribute(HttpVerbs httpVerb, String uriTemplate, String description)
        {
            this.httpVerb = httpVerb;
            this.uriTemplate = uriTemplate;
            this.description = description;
        }

    }
}