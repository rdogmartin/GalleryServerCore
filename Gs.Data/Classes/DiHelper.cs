using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace GalleryServer.Data
{
    public class DiHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;
        private static IMemoryCache _cache;
        private static SignInManager<GalleryUser> _signInManager;
        private static GalleryRoleManager _roleManager;
        private static IHostingEnvironment _env;

        public static void Configure(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, SignInManager<GalleryUser> signInManager, GalleryRoleManager roleManager, IHostingEnvironment env)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _env = env;
        }

        public static HttpContext HttpContext => _httpContextAccessor.HttpContext;

        public static string GetRemoteIP => HttpContext.Connection.RemoteIpAddress.ToString();

        public static string GetUserAgent => HttpContext.Request.Headers["User-Agent"].ToString();

        public static string GetScheme => HttpContext.Request.Scheme;

        public static IMemoryCache GetCache()
        {
            return _cache;
        }

        public static SignInManager<GalleryUser> GetSignInManager()
        {
            return _signInManager;
        }

        public static UserManager<GalleryUser> GetUserManager()
        {
            return _signInManager.UserManager;
        }

        public static GalleryRoleManager GetRoleManager()
        {
            return _roleManager;
        }

        public static IHostingEnvironment GetHostingEnvironment()
        {
            return _env;
        }
    }
}
