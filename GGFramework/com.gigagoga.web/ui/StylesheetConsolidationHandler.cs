using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.IO;
using System.IO.Compression;

namespace com.gigagoga.web.ui
{
    /// <summary>
    /// A class that handles consolidation of css files.
    /// Vary by parameters v and path(s).
    /// Example: /style/css.ashx?v=125&path=companyglobal.css&path=module.css
    /// </summary>
    public class StylesheetConsolidationHandler : System.Web.IHttpHandler
    {

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/css";

            NameValueCollection requestParameters = context.Request.QueryString;
            // Check if the qs contains invalid chars ie &amp;
            if (context.Request.RawUrl.IndexOf("&amp;") > 0)
            {
                string query = context.Request.QueryString.ToString().Replace("&amp%3b", "&");
                requestParameters = HttpUtility.ParseQueryString(query);
            }


            // return if no path-params are present
            if (String.IsNullOrEmpty(requestParameters["path"]))
            {
                // Print message
                context.Response.Write("/* Missing parameter path */");
                return;
            }


            List<string> cssFiles = new List<string>();

            string[] paths = requestParameters.GetValues("path");
            foreach (string pa in paths)
            {
                context.Response.Write("\n/* " + pa + " */");
                string physPath = context.Server.MapPath((pa.StartsWith("/") ? "~" : "") + pa);
                if (!cssFiles.Contains(physPath) && File.Exists(physPath))
                {
                    cssFiles.Add(physPath);
                }
            }

            setCaching(context, cssFiles.ToArray());

            string line;
            foreach (string cssFile in cssFiles)
            {

                string[] lines = File.ReadAllLines(cssFile);
                for (int j = 0; j < lines.Length; j++)
                {
                    line = lines[j].Trim(' ', '\t');

                    // Check for comment
                    if (line.StartsWith("/*"))
                    {
                        // Skip comments
                        while (!line.EndsWith("*/"))
                        {
                            j++;
                            line = lines[j].Trim(' ', '\t');
                        }
                    }

                    if (!line.StartsWith("/*") && !line.EndsWith("*/") && line.Length > 0)
                    {
                        // context.Response.Write("\r\n" + line);
                        context.Response.Write(line);
                        if (line.EndsWith("}"))
                        {
                            context.Response.Write("\n");
                        }
                    }
                }
            }

            tryGZipEncode(context);
        }


        /// <summary>
        /// Sets the response cache object.
        /// </summary>
        void setCaching(HttpContext context, string[] cacheFiles)
        {
            context.Response.AddFileDependencies(cacheFiles);

            // Set up the cache
            HttpCachePolicy cache = context.Response.Cache;
            cache.SetCacheability(HttpCacheability.Public);

            context.Response.Cache.VaryByParams["v"] = true;
            context.Response.Cache.VaryByParams["path"] = true;

            cache.SetOmitVaryStar(true);
            cache.SetExpires(DateTime.Now.AddYears(1));
            cache.SetValidUntilExpires(false);

            cache.SetLastModifiedFromFileDependencies();
            cache.SetETagFromFileDependencies();
        }

        /// <summary>
        /// Check for GZIP cababilities
        /// </summary>
        /// <param name="context"></param>
        void tryGZipEncode(HttpContext context)
        {
            String acceptEncoding = context.Request.Headers["Accept-Encoding"];
            if (!String.IsNullOrEmpty(acceptEncoding) && acceptEncoding.Contains("gzip"))
            {
                context.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
                context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                context.Response.AddHeader("Content-Encoding", "gzip");
            }
        }
    }
}
