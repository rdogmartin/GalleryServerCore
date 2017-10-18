using Microsoft.Extensions.Caching.Memory;

namespace GalleryServer.Data
{
    public class DiHelper
    {
        private static IMemoryCache _cache;
        //private static IHttpContextAccessor _httpContextAccessor;
        //private static SignInManager<GalleryUser> _signInManager;
        //private static GalleryRoleManager _roleManager;
        //private static IHostingEnvironment _env;

        public static void Configure(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            //_httpContextAccessor = httpContextAccessor;
            //_signInManager = signInManager;
            //_roleManager = roleManager;
            //_env = env;
        }

        //public static HttpContext HttpContext => _httpContextAccessor.HttpContext;

        //public static string GetRemoteIP => HttpContext.Connection.RemoteIpAddress.ToString();

        //public static string GetUserAgent => HttpContext.Request.Headers["User-Agent"].ToString();

        //public static string GetScheme => HttpContext.Request.Scheme;

        public static IMemoryCache GetCache()
        {
            return _cache;
        }

    }
}
