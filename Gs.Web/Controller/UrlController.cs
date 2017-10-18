using System;
using System.Linq;
using GalleryServer.Business;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace GalleryServer.Web.Controller
{
    public class UrlController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the URL to the current web application. Does not include the containing page or the trailing slash. 
        /// Guaranteed to not return null. Example: If the gallery is installed in a virtual directory 'gallery'
        /// on domain 'www.site.com', this returns 'http://www.site.com/gallery'.
        /// </summary>
        /// <returns>Returns the URL to the current web application.</returns>
        public string GetAppUrl()
        {
            return $"{GetScheme()}://{GetHost()}{GetPathBase()}";
        }

        /// <summary>
        /// Get the URI scheme, DNS host name or IP address, and port number for the current application. 
        /// Examples: http://www.site.com, http://localhost, http://127.0.0.1, http://godzilla
        /// </summary>
        /// <returns>Returns the URI scheme, DNS host name or IP address, and port number for the current application, 
        /// or null.</returns>
        public string GetHostUrl()
        {
            return $"{GetScheme()}://{GetHost()}";
        }

        /// <summary>
        /// Get the path, relative to the web site root, to the current web application. Does not include the containing page 
        /// or the trailing slash. Example: If GS is installed at C:\inetpub\wwwroot\dev\gallery, and C:\inetpub\wwwroot\ is 
        /// the parent web site, this property returns /dev/gallery. Guaranteed to not return null.
        /// </summary>
        /// <returns>Get the path, relative to the web site root, to the current web application.</returns>
        public string GetAppRoot()
        {
            return $"{GetHost()}{GetPathBase()}";
        }

        /// <summary>
        /// Get the path, relative to the web site root, to the directory containing the Gallery Server 
        /// resources. Does not include the containing page or the trailing slash. Example: If GS is installed at
        /// C:\inetpub\wwwroot\dev\gallery, where C:\inetpub\wwwroot\ is the parent web site, and the gallery support files are in
        /// the gsp directory, this property returns /dev/gallery/gsp. Guaranteed to not return null.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetGalleryRoot()
        {
            return GetPathBase();
        }

        /// <summary>
        /// Gets the URL, relative to the website root and optionally including any query string parameters, to the current page.
        /// This method is a wrapper for a call to HttpContext.Current.Request.Url. If the current URL is an API call (i.e. it starts
        /// with "~/api", the referrer is used instead. Returns null if <see cref="HttpContext.Current" /> is null.
        /// Examples: "/dev/gs/gallery.aspx", "/dev/gs/gallery.aspx?g=admin_email&amp;aid=2389" 
        /// </summary>
        /// <param name="includeQueryString">When <c>true</c> the query string is included.</param>
        /// <returns>Returns the URL, relative to the website root and including any query string parameters, to the current page,
        /// or null if <see cref="HttpContext.Current" /> is null.</returns>
        public string GetCurrentPageUrl(bool includeQueryString = false)
        {
            var urlPath = _httpContextAccessor.HttpContext.Request.GetUri().AbsolutePath;
            var query = _httpContextAccessor.HttpContext.Request.GetUri().Query;

            if (IsWebApiRequest())
            {
                if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Referer"))
                {
                    var uri = new Uri(_httpContextAccessor.HttpContext.Request.Headers["Referer"]);
                    urlPath = uri.AbsolutePath;
                    query = uri.Query;
                }
                else
                {
                    // We don't know the current web page, so just return an empty string. This should not typically occur.
                    urlPath = query = string.Empty;
                }
            }

            if (includeQueryString)
                return string.Concat(urlPath, query);
            else
                return urlPath;
        }

        /// <summary>
        /// Determines whether the current request is a Web.API request.
        /// </summary>
        /// <returns><c>true</c> if it is web API request; otherwise, <c>false</c>.</returns>
        public bool IsWebApiRequest()
        {
            return GetPathBase().Contains("/api/");
        }

        /// <summary>
        /// Retrieves the specified query string parameter value from the query string. Returns string.Empty 
        /// if the parameter is not found.
        /// </summary>
        /// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
        /// <returns>Returns the value of the specified query string parameter.</returns>
        /// <remarks>Do not call UrlDecode on the string, as it appears that .NET already does this.</remarks>
        public string GetQueryStringParameterString(string parameterName)
        {
            if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue(parameterName, out var parmValues))
            {
                return parmValues.FirstOrDefault() ?? string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieves the specified query string parameter value from the query string. Returns int.MinValue if
        /// the parameter is not found, it is not a valid integer, or it is &lt;= 0.
        /// </summary>
        /// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
        /// <returns>Returns the value of the specified query string parameter.</returns>
        public int GetQueryStringParameterInt32(string parameterName)
        {
            if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue(parameterName, out var parmValues))
            {
                if (int.TryParse(parmValues.FirstOrDefault(), out var qsValue) && (qsValue >= 0))
                {
                    return qsValue;
                }
            }

            return int.MinValue;
        }

        /// <overloads>UrlEncodes a string using System.Uri.EscapeDataString().</overloads>
        /// <summary>
        /// UrlEncodes a string using System.Uri.EscapeDataString().
        /// </summary>
        /// <param name="text">The text to URL encode.</param>
        /// <returns>Returns <paramref name="text"/> as an URL-encoded string.</returns>
        public string UrlEncode(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            return Uri.EscapeDataString(text);
        }

        /// <summary>
        /// UrlEncodes a string using System.Uri.EscapeDataString(), excluding the character specified in <paramref name="charNotToEncode"/>.
        /// This overload is useful for encoding URLs or file paths where the forward or backward slash is not to be encoded.
        /// </summary>
        /// <param name="text">The text to URL encode</param>
        /// <param name="charNotToEncode">The character that, if present in <paramref name="text"/>, is not encoded.</param>
        /// <returns>Returns <paramref name="text"/> as an URL-encoded string.</returns>
        public string UrlEncode(string text, char charNotToEncode)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            string[] tokens = text.Split(new char[] { charNotToEncode });
            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = UrlEncode(tokens[i]);
            }

            return String.Join(charNotToEncode.ToString(), tokens);
        }

        /// <summary>
        /// UrlDecodes a string using System.Uri.UnescapeDataString().
        /// </summary>
        /// <param name="text">The text to URL decode.</param>
        /// <returns>Returns text as an URL-decoded string.</returns>
        public string UrlDecode(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            // Pre-process for + sign space formatting since System.Uri doesn't handle it
            // plus literals are encoded as %2b normally so this should be safe.
            text = text.Replace("+", " ");
            return Uri.UnescapeDataString(text);
        }

        /// <summary>
        /// Gets the scheme. Ex: "http", "https"
        /// </summary>
        /// <returns>A <see cref="string" /> containing the scheme.</returns>
        private string GetScheme()
        {
            return _httpContextAccessor.HttpContext.Request.Scheme;
        }

        /// <summary>
        /// Gets the Domain Name System (DNS) host name or IP address and the port number for the current web application. Includes the
        /// port number if it differs from the default port. Ex: "www.site.com", "www.site.com:8080", "godzilla", "192.168.0.50", "75.135.92.12:8080"
        /// </summary>
        /// <returns>A <see cref="string" /> containing the authority component of the URI for the current web application.</returns>
        private string GetHost()
        {
            return _httpContextAccessor.HttpContext.Request.Host.ToUriComponent();
        }

        /// <summary>
        /// Get the path, relative to the web site root, to the current web application. Does not include the containing page 
        /// or the trailing slash. Example: If GS is installed at C:\inetpub\wwwroot\dev\gallery, and C:\inetpub\wwwroot\ is 
        /// the parent web site, this property returns /dev/gallery. Guaranteed to not return null.
        /// </summary>
        /// <returns>Get the path, relative to the web site root, to the current web application.</returns>
        private string GetPathBase()
        {
            return _httpContextAccessor.HttpContext.Request.PathBase.ToUriComponent();
        }

    }
}
