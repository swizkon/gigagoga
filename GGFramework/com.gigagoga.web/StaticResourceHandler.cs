using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Web;

namespace com.gigagoga.web
{
    /// <summary>
    /// A handler that acts as a resource manager for static files, such as js, css and images.
    /// Sets the response headers correctly.
    /// All mapped configs must end with .ashx.
    /// Example path *.js.ashx and *.css.ashx maps to this handler
    /// </summary>
    public class StaticResourceHandler : IHttpHandler
    {
        public StaticResourceHandler()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string filePath = null;
            string physPath = context.Request.PhysicalPath;
            if (physPath.LastIndexOf(".ashx") > 0)
            {
                filePath = physPath.Substring(0, physPath.LastIndexOf(".ashx"));
            }

            if (filePath == null)
            {
                context.Response.StatusCode = System.Net.HttpStatusCode.BadRequest.GetHashCode();
                return;
            }

            bool fileExists = File.Exists(filePath);

            if (!fileExists)
            {
                context.Response.StatusCode = System.Net.HttpStatusCode.NotFound.GetHashCode();
                return;
            }

            // Check if the file exists, if so set the cache and check for cache headers
            if (fileExists)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                context.Response.AddFileDependency(filePath);
                context.Response.Cache.SetETagFromFileDependencies();
                context.Response.Cache.SetLastModifiedFromFileDependencies();
                context.Response.Cache.SetExpires(DateTime.Now.AddMonths(3));
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Cache.SetValidUntilExpires(true);

                // Check for cache headers for ETag and If-Modified-Since
                /*
                if (context.Response.Headers["Etag"] != null)
                {
                    context.Response.Headers["X-ETag"] = context.Response.Headers["Etag"];
                }
                */

                if (!String.IsNullOrEmpty(context.Request.Headers["If-Modified-Since"]))
                {
                    DateTime ifModifiedSince = DateTime.Now;
                    if (DateTime.TryParse(context.Request.Headers["If-Modified-Since"], out ifModifiedSince))
                    {
                        if (ifModifiedSince.ToString() == fileInfo.LastWriteTime.ToString())
                        {
                            context.Response.StatusCode = 304;
                            return;
                        }
                    }
                }
            }


            String fileExt = Path.GetExtension(filePath);

            if (filePath != null && fileExt == ".js")
            {
                outputJsFile(context, filePath);
            }
            else if (filePath != null && fileExt == ".css")
            {
                outputCssFile(context, filePath);
            }
            else if (filePath != null && fileExt == ".gif")
            {
                outputGifImage(context, filePath);
            }
            else if (filePath != null && fileExt == ".png")
            {
                outputPngImage(context, filePath);
            }
            else if (filePath != null && File.Exists(filePath))
            {
                context.Response.WriteFile(filePath);
            }
            else
            {
                context.Response.Write(filePath);
            }
        }



        void tryGZipEncoding(HttpContext context)
        {
            String acceptEncoding = context.Request.Headers["Accept-Encoding"];
            if (!String.IsNullOrEmpty(acceptEncoding) && acceptEncoding.Contains("gzip"))
            {
                context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                context.Response.AddHeader("Content-Encoding", "gzip");
                context.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
            }
        }

        void outputJsFile(HttpContext context, string file)
        {
            if (File.Exists(file))
            {
                context.Response.ContentType = "text/javascript";
                tryGZipEncoding(context);

                string[] lines = File.ReadAllLines(file);
                for (int j = 0; j < lines.Length; j++)
                {
                    String line = lines[j].Trim(' ', '\t');
                    if (line.StartsWith("//"))
                    {
                        continue;
                    }
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
                        // TODO Check for comment token = " // ".
                        if (line.IndexOf(" // ") > 0)
                        {
                            line = line.Substring(0, line.IndexOf(" // "));
                        }
                        context.Response.Write("\n" + line);
                    }
                }
                /*
                context.Response.WriteFile(file);
                */
            }
        }

        void outputCssFile(HttpContext context, string file)
        {
            if (File.Exists(file))
            {
                context.Response.ContentType = "text/css";
                tryGZipEncoding(context);

                string[] lines = File.ReadAllLines(file);
                for (int j = 0; j < lines.Length; j++)
                {
                   String line = lines[j].Trim(' ', '\t');

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
                        context.Response.Write(line);
                        if (line.EndsWith("}"))
                        {
                            context.Response.Write("\n");
                        }
                    }
                }
            }
        }

        void outputGifImage(HttpContext context, string file)
        {
            if (File.Exists(file))
            {
                context.Response.ContentType = "image/gif";
                // TODO Check for crop, box resize params...
                context.Response.WriteFile(file);
            }
        }

        void outputPngImage(HttpContext context, string file)
        {
            if (File.Exists(file))
            {
                context.Response.ContentType = "image/png";
                // tryGZipEncoding(context);
                // TODO Check for crop, box resize params...
                context.Response.WriteFile(file);
            }
        }

        #endregion
    }
}
