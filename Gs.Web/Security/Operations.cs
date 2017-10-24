using GalleryServer.Business;

namespace GalleryServer.Web.Security
{
    public static class Operations
    {
        public static OperationAuthorizationRequirement AddChildAlbum = new OperationAuthorizationRequirement { RequestedPermission = SecurityActions.AddChildAlbum };
        public static OperationAuthorizationRequirement EditAlbum = new OperationAuthorizationRequirement { RequestedPermission = SecurityActions.EditAlbum };
        public static OperationAuthorizationRequirement DeleteAlbum = new OperationAuthorizationRequirement { RequestedPermission = SecurityActions.DeleteAlbum };
        public static OperationAuthorizationRequirement DeleteChildAlbum = new OperationAuthorizationRequirement { RequestedPermission = SecurityActions.DeleteChildAlbum };
        public static OperationAuthorizationRequirement AddMediaObject = new OperationAuthorizationRequirement { RequestedPermission = SecurityActions.AddMediaObject };
        public static OperationAuthorizationRequirement EditMediaObject = new OperationAuthorizationRequirement { RequestedPermission = SecurityActions.EditMediaObject };
        public static OperationAuthorizationRequirement DeleteMediaObject = new OperationAuthorizationRequirement { RequestedPermission = SecurityActions.DeleteMediaObject };
        public static OperationAuthorizationRequirement HideWatermark = new OperationAuthorizationRequirement { RequestedPermission = SecurityActions.HideWatermark };
        public static OperationAuthorizationRequirement Synchronize = new OperationAuthorizationRequirement { RequestedPermission = SecurityActions.Synchronize };
    }
}
