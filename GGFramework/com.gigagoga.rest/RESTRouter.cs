using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.ServiceModel.Web;

namespace com.gigagoga.rest
{
    /// <summary>
    /// Delegates incomming requests to matching RESTInvokeAttribute attributed methods.
    /// </summary>
    [Obfuscation()]
    public abstract class RESTRouter : IHttpHandler
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public RESTRouter()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Interface property fro iHttpHandler.
        /// </summary>
        public abstract bool IsReusable
        {
            get;
        }

        /// <summary>
        /// Interface member. Matches against all methods in the class that has invoke attrs.
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ExpiresAbsolute = DateTime.Now.AddDays(-10);

            // ARG!!! Use the FilePath to enable all extensions...
            String serviceEndPoint = context.Request.FilePath;

            String appURI = context.Request.Url.AbsoluteUri.Substring(0, context.Request.Url.AbsoluteUri.IndexOf(serviceEndPoint));

            appURI = appURI.TrimEnd('/') + "/" + HttpRuntime.AppDomainAppVirtualPath.Trim('/');

            Uri baseURI = new Uri(appURI);

            // Handle the Method:
            HttpVerbs httpMethod = HttpVerbs.GET;
            try
            {
                httpMethod = (HttpVerbs)Enum.Parse(typeof(HttpVerbs), context.Request.HttpMethod, true);
            }
            catch (Exception)
            {
            }
            

            String httpMethodOverride = context.Request.Headers["X-HTTP-Method-Override"] ?? context.Request.Params["X-HTTP-Method-Override"];
            if (!String.IsNullOrEmpty(httpMethodOverride))
            {
                // Match valid overrides:
                switch (httpMethod + "-" + httpMethodOverride.ToUpper())
                {
                    // Implement common 7 (plus PATCH?)
                    case "POST-PUT":
                    case "POST-DELETE":
                    case "POST-TRACE":
                    case "POST-PATCH":
                    case "GET-HEAD":
                    case "GET-OPTIONS":
                        try
                        {
                            httpMethod = (HttpVerbs)Enum.Parse(typeof(HttpVerbs), httpMethodOverride.ToUpper(), true);
                        }
                        catch (Exception)
                        {
                        }

                        break;

                    default:
                        // This will currently be hit by GET-GET and POST-POST
                        break;
                }
            }

            context.Response.AppendHeader("X-HTTP-Method", httpMethod.ToString());

            bool foundMatch = false;
            int routeMatch = 0;

            // Reflect this type and check for WebInvoke attributes which UriTemplate matches URI.
            MethodInfo[] methods = this.GetType().GetMethods();// (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Public);

            for (int methodIndex = 0; methodIndex < methods.Length && foundMatch == false; methodIndex++)
            {
                // Get attributes
                RESTInvokeAttribute[] attrs = methods[methodIndex].GetCustomAttributes(typeof(RESTInvokeAttribute), false) as RESTInvokeAttribute[];

                List<RESTInvokeAttribute> attrsList = new List<RESTInvokeAttribute>(attrs);

                foreach (RESTInvokeAttribute invoke in attrsList)
                {
                    if (invoke.HttpVerb == httpMethod)
                    {
                        UriTemplate uriTemplate = new UriTemplate(invoke.UriTemplate);
                        UriTemplateMatch match = uriTemplate.Match(baseURI, context.Request.Url);
                        if (match != null && !foundMatch)
                        {
                            context.Response.AppendHeader("X-ROUTE-MATCH-" + routeMatch, methods[methodIndex].Name);
                            methods[methodIndex].Invoke(this, new object[] { context, match });
                            foundMatch = true;
                        }
                    }
                    routeMatch++;
                }
            }

            if (!foundMatch)
            {
                DefaultMethod(context);
            }
        }


        /// <summary>
        /// Default virtual that outputs No match...
        /// </summary>
        /// <param name="context"></param>
        public virtual void DefaultMethod(HttpContext context)
        {
            // Default when no other methiod matches...
            context.Response.Write( this.GetType().FullName + " No match and no DefaultMethod for " + context.Request.RawUrl);
        }

        /// <summary>
        /// Outputs a HTML or JSON representation of all methods that are REST:y
        /// 
        /// </summary>
        /// <param name="context"></param>
        protected void ServiceDiscovery(HttpContext context)
        {
            IDictionary<String, String> resources = new SortedDictionary<String, String>();

            // Reflect this type and check for WebInvoke attributes which UriTemplate matches URI.
            MethodInfo[] methods = this.GetType().GetMethods(); // (BindingFlags.Instance | BindingFlags.DeclaredOnly); //  | BindingFlags.NonPublic

            for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++)
            {
                // Get attributes
                RESTInvokeAttribute[] attrs = methods[methodIndex].GetCustomAttributes(typeof(RESTInvokeAttribute), false) as RESTInvokeAttribute[];

                for (int attrIndex = 0; attrIndex < attrs.Length; attrIndex++)
                {
                    context.Response.Write(
                        String.Format("<div><code>{0}</code> <code>{1}</code><p>{2}</p></div>\r"
                        , attrs[attrIndex].UriTemplate
                        , attrs[attrIndex].HttpVerb.ToString()
                        , attrs[attrIndex].Description)
                        );
                }
            }
            /*
            using (IEnumerator<KeyValuePair<String, String>> resource = resources.GetEnumerator())
            {
                while (resource.MoveNext())
                {
                    context.Response.Write(
                        String.Format("<div><code>{0}</code> <code>{1}</code></div>\r", resource.Current.Key, resource.Current.Value)
                        );
                }
            }
            */
        }
    }
}