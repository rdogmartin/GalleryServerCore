using Microsoft.AspNetCore.Http;

namespace GalleryServer.Data
{
    public class WebHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static HttpContext HttpContext => _httpContextAccessor.HttpContext;

        public static string GetRemoteIP => HttpContext.Connection.RemoteIpAddress.ToString();

        public static string GetUserAgent => HttpContext.Request.Headers["User-Agent"].ToString();

        public static string GetScheme => HttpContext.Request.Scheme;
    }
}
