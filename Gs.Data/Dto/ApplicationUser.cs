using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace GalleryServer.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class GalleryUser : IdentityUser
    {
        
    }

    public class GalleryRole : IdentityRole
    {
        public GalleryRole() : base() { }

        public GalleryRole(string name) : base(name) { }

        public string Description { get; set; }
    }

    public class GalleryRoleManager : RoleManager<GalleryRole>
    {
        public GalleryRoleManager(IRoleStore<GalleryRole> store, IEnumerable<IRoleValidator<GalleryRole>> roleValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<GalleryRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
        {
        }
    }
}
