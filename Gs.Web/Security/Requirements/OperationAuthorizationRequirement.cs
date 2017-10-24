using GalleryServer.Business;
using Microsoft.AspNetCore.Authorization;

namespace GalleryServer.Web.Security
{
    public class OperationAuthorizationRequirement : IAuthorizationRequirement
    {
        public SecurityActions RequestedPermission { get; set; }
    }
}
