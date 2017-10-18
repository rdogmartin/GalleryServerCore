using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace GalleryServer.Web.Controller
{
    public class UserController
    {
        #region Private Fields

        private readonly UserManager<GalleryUser> _userManager;
        private readonly RoleController _roleController;
        //private readonly GalleryRoleManager _roleManager;
        private readonly EmailController _emailController;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Constructor

        public UserController(UserManager<GalleryUser> userManager, RoleController roleController, EmailController emailController, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleController = roleController;
            _emailController = emailController;
            _httpContextAccessor = httpContextAccessor;

            _roleController.SetUserController(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the current user. Returns an empty string for anonymous users.
        /// </summary>
        /// <value>The name of the current user.</value>
        public string UserName
        {
            get
            {
                return Utils.ParseUserName(_httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated.
        /// </summary>
        public bool IsAuthenticated => _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;

        #endregion

        #region Public Methods

        public IQueryable<GalleryUser> GetGalleryUsers()
        {
            return _userManager.Users;
        }

        /// <summary>
        /// Gets an unsorted collection of all the users in the database. The users may be returned from a cache.
        /// </summary>
        /// <returns>Returns a collection of all the users in the database.</returns>
        public IUserAccountCollection GetAllUsers()
        {
            IUserAccountCollection usersCache = CacheController.GetUsersCache();

            if (usersCache == null)
            {
                usersCache = new UserAccountCollection();

                foreach (var user in _userManager.Users)
                {
                    usersCache.Add(ToUserAccount(user));
                }

                CacheController.SetCache(CacheItem.Users, usersCache);
            }

            return usersCache;
        }

        /// <summary>
        /// Populates the properties of <paramref name="userToLoad" /> with information about the user. Requires that the
        /// <see cref="IUserAccount.UserName" /> property of the <paramref name="userToLoad" /> parameter be assigned a value.
        /// If no user with the specified username exists, no action is taken.
        /// </summary>
        /// <param name="userToLoad">The user account whose properties should be populated.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="userToLoad" /> is null.</exception>
        public async Task LoadUser(IUserAccount userToLoad)
        {
            if (userToLoad == null)
                throw new ArgumentNullException(nameof(userToLoad));

            if (String.IsNullOrEmpty(userToLoad.UserName))
            {
                throw new ArgumentException("The UserName property of the userToLoad parameter must have a valid value. Instead, it was null or empty.");
            }

            IUserAccount user = await GetUser(userToLoad.UserName);

            if (user != null)
            {
                user.CopyTo(userToLoad);
            }
        }

        /// <overloads>
        /// Gets information from the data source for a user.
        /// </overloads>
        /// <summary>
        /// Gets information from the data source for the current logged-on user.
        /// </summary>
        /// <returns>A <see cref="IUserAccount"/> representing the current logged-on user.</returns>
        public async Task<IUserAccount> GetUser()
        {
            return await GetUser(UserName);
        }

        /// <summary>
        /// Gets information from the data source for the current logged-on user.
        /// </summary>
        /// <returns>A <see cref="IUserAccount"/> representing the current logged-on user.</returns>
        public async Task<IUserAccount> GetUser(string userName)
        {
            return string.IsNullOrEmpty(userName) ? null : ToUserAccount(await _userManager.FindByNameAsync(userName));
        }

        /// <summary>
        /// Gets information from the data source for the current logged-on user.
        /// </summary>
        /// <returns>A <see cref="IUserAccount"/> representing the current logged-on user.</returns>
        public async Task<GalleryUser> GetGalleryUser(string userName)
        {
            return string.IsNullOrEmpty(userName) ? null : await _userManager.FindByNameAsync(userName);
        }

        /// <summary>
        /// Gets an unsorted collection of users the current user has permission to view. Users who have administer site permission can view all users,
        /// as can gallery administrators when the application setting <see cref="IAppSetting.AllowGalleryAdminToViewAllUsersAndRoles"/> is true. When
        /// the setting is false, gallery admins can only view users in galleries they have gallery admin permission in. Note that
        /// a user may be able to view a user but not update it. This can happen when the user belongs to roles that are associated with
        /// galleries the current user is not an admin for. The users may be returned from a cache. Guaranteed to not return null.
        /// This overload is slower than <see cref="GetUsersCurrentUserCanView(bool, bool)"/>, so use that one when possible.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>
        /// Returns an <see cref="IUserAccountCollection"/> containing a list of roles the user has permission to view.
        /// </returns>
        /// <overloads>
        /// Gets a collection of users the current user has permission to view.
        /// </overloads>
        public async Task<IUserAccountCollection> GetUsersCurrentUserCanView(int galleryId)
        {
            return await GetUsersCurrentUserCanView(await _roleController.IsCurrentUserSiteAdministrator(), await _roleController.IsCurrentUserGalleryAdministrator(galleryId));
        }

        /// <summary>
        /// Gets an unsorted collection of users the current user has permission to view. Users who have administer site permission can view all users,
        /// as can gallery administrators when the application setting <see cref="IAppSetting.AllowGalleryAdminToViewAllUsersAndRoles" /> is true. When 
        /// the setting is false, gallery admins can only view users in galleries they have gallery admin permission in. Note that
        /// a user may be able to view a user but not update it. This can happen when the user belongs to roles that are associated with
        /// galleries the current user is not an admin for. The users may be returned from a cache. Guaranteed to not return null.
        /// This overload is faster than <see cref="GetUsersCurrentUserCanView(int)" />, so use this one when possible.
        /// </summary>
        /// <param name="userIsSiteAdmin">If set to <c>true</c>, the currently logged on user is a site administrator.</param>
        /// <param name="userIsGalleryAdmin">If set to <c>true</c>, the currently logged on user is a gallery administrator for the current gallery.</param>
        /// <returns>
        /// Returns an <see cref="IUserAccountCollection"/> containing a list of roles the user has permission to view.
        /// </returns>
        public async Task<IUserAccountCollection> GetUsersCurrentUserCanView(bool userIsSiteAdmin, bool userIsGalleryAdmin)
        {
            if (userIsSiteAdmin)
            {
                return GetAllUsers();
            }
            else if (userIsGalleryAdmin)
            {
                // See if we have a list in the cache. If not, generate it and add to cache.
                var usersCache = CacheController.GetUsersCurrentUserCanViewCache();

                IUserAccountCollection users;
                string cacheKeyName = String.Empty;

                if (_httpContextAccessor.HttpContext.Session != null)
                {
                    cacheKeyName = GetCacheKeyNameForUsersCurrentUserCanView(UserName);

                    if ((usersCache != null) && (usersCache.TryGetValue(cacheKeyName, out users)))
                    {
                        return users;
                    }
                }

                // Nothing in the cache. Calculate it - this is processor intensive when there are many users and/or roles.
                users = await DetermineUsersCurrentUserCanView(userIsSiteAdmin, userIsGalleryAdmin);

                // Add to the cache before returning.
                if (usersCache == null)
                {
                    usersCache = new ConcurrentDictionary<string, IUserAccountCollection>();
                }

                // Add to the cache, but only if we have access to the session ID.
                if (_httpContextAccessor.HttpContext.Session != null)
                {
                    lock (usersCache)
                    {
                        usersCache.AddOrUpdate(cacheKeyName, users, (key, existingUsers) =>
                        {
                            existingUsers.Clear();
                            existingUsers.AddRange(users);
                            return existingUsers;
                        });
                    }

                    CacheController.SetCache(CacheItem.UsersCurrentUserCanView, usersCache);
                }

                return users;
            }

            return new UserAccountCollection();
        }

        /// <summary>
        /// Gets a data entity containing information about the specified <paramref name="userName" /> or the current user
        /// if <paramref name="userName" /> is null or empty. A <see cref="GallerySecurityException" /> is thrown if the 
        /// current user does not have view and edit permission to the requested user. The instance can be JSON-parsed and sent to the
        /// browser.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="galleryId">The gallery ID. Optional parameter - But note that when not specified, the <see cref="User.UserAlbumId" />
        /// property is assigned to zero, regardless of its actual value.</param>
        /// <returns>Returns <see cref="Entity.User" /> object containing information about the current user.</returns>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have permission to view and edit the user.</exception>
        /// <exception cref="InvalidUserException">Thrown when the requested user does not exist.</exception>
        /// <exception cref="InvalidGalleryException">Thrown when the gallery ID does not represent an existing gallery.</exception>
        public async Task<User> GetUserEntity(string userName, int galleryId)
        {
            Factory.LoadGallery(galleryId); // Throws ex if gallery ID is not valid

            if (String.IsNullOrWhiteSpace(userName))
                return new User() { IsNew = true, GalleryId = galleryId, Roles = _roleController.GetDefaultRolesForUser() };

            var user = await GetUser(userName);

            if (user == null)
            {
                if (await _roleController.IsCurrentUserSiteAdministrator() || await _roleController.IsCurrentUserGalleryAdministrator(galleryId))
                    throw new InvalidUserException(String.Format("User '{0}' does not exist", userName));
                else
                    throw new GallerySecurityException("Insufficient permission to view the user."); // Throw to avoid giving non-admin clues about existence of user
            }
            else if (!await UserCanViewAndEditUser(user))
                throw new GallerySecurityException("Insufficient permission to view user.");

            var userPerms = SecurityManager.GetUserObjectPermissions(await _roleController.GetGalleryServerRolesForUser(), galleryId);

            return new User()
            {
                UserName = user.UserName,
                Comment = user.Comment,
                Email = user.Email,
                IsApproved = user.IsApproved,
                IsAuthenticated = IsAuthenticated,
                CanAddAlbumToAtLeastOneAlbum = userPerms.UserCanAddAlbumToAtLeastOneAlbum,
                CanAddMediaToAtLeastOneAlbum = userPerms.UserCanAddMediaAssetToAtLeastOneAlbum,
                CanEditAtLeastOneAlbum = userPerms.UserCanEditAtLeastOneAlbum,
                CanEditAtLeastOneMediaAsset = userPerms.UserCanEditAtLeastOneMediaAsset,
                EnableUserAlbum = ProfileController.GetProfile(user.UserName).GetGalleryProfile(galleryId).EnableUserAlbum,
                UserAlbumId = Math.Max((galleryId > Int32.MinValue ? ProfileController.GetUserAlbumId(user.UserName, galleryId) : 0), 0), // Returns 0 for no user album
                GalleryId = galleryId,
                CreationDate = user.CreationDate,
                IsLockedOut = user.IsLockedOut,
                LastActivityDate = user.LastActivityDate,
                LastLoginDate = user.LastLoginDate,
                LastPasswordChangedDate = user.LastPasswordChangedDate,
                Roles = (await _roleController.GetGalleryServerRolesForUser(userName)).Select(r => r.RoleName).ToArray(),
                Password = null,
                PasswordResetRequested = null,
                PasswordChangeRequested = null,
                NotifyUserOnPasswordChange = null
            };
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="userName">The membership user whose lock status you want to clear.</param>
        /// <returns><c>true</c> if the membership user was successfully unlocked; otherwise, <c>false</c>.</returns>
        public async Task<bool> UnlockUser(string userName)
        {
            var result = await _userManager.SetLockoutEnabledAsync(await GetGalleryUser(userName), true);
            return result.Succeeded;
        }

        /// <summary>
        /// Get a list of galleries the current user can administer. Site administrators can view all galleries, while gallery
        /// administrators may have access to zero or more galleries.
        /// </summary>
        /// <returns>Returns an <see cref="IGalleryCollection" /> containing the galleries the current user can administer.</returns>
        public async Task<IGalleryCollection> GetGalleriesCurrentUserCanAdminister()
        {
            return await GetGalleriesUserCanAdminister(UserName);
        }

        /// <summary>
        /// Get a list of galleries the specified <paramref name="userName"/> can administer. Site administrators can view all
        /// galleries, while gallery administrators may have access to zero or more galleries.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// Returns an <see cref="IGalleryCollection"/> containing the galleries the current user can administer.
        /// </returns>
        public async Task<IGalleryCollection> GetGalleriesUserCanAdminister(string userName)
        {
            IGalleryCollection adminGalleries = new GalleryCollection();
            foreach (IGalleryServerRole role in await _roleController.GetGalleryServerRolesForUser(userName))
            {
                if (role.AllowAdministerSite)
                {
                    return Factory.LoadGalleries();
                }
                else if (role.AllowAdministerGallery)
                {
                    foreach (IGallery gallery in role.Galleries)
                    {
                        if (!adminGalleries.Contains(gallery))
                        {
                            adminGalleries.Add(gallery);
                        }
                    }
                }
            }

            return adminGalleries;
        }

        /// <summary>
        /// Gets a collection of all the galleries the specified <paramref name="userName" /> has access to.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>Returns an <see cref="IGalleryCollection" /> of all the galleries the specified <paramref name="userName" /> has access to.</returns>
        public async Task<IGalleryCollection> GetGalleriesForUser(string userName)
        {
            IGalleryCollection galleries = new GalleryCollection();

            foreach (IGalleryServerRole role in await _roleController.GetGalleryServerRolesForUser(userName))
            {
                foreach (IGallery gallery in role.Galleries)
                {
                    if (!galleries.Contains(gallery))
                    {
                        galleries.Add(gallery);
                    }
                }
            }

            return galleries;
        }

        /// <summary>
        /// Validates the logged on user has permission to save the specified <paramref name="userToSave"/> and to add/remove the user 
        /// to/from the specified <paramref name="roles"/>. Throw a <see cref="GallerySecurityException"/> if user is not authorized.
        /// This method assumes the logged on user is a site administrator or gallery administrator but does not verify it.
        /// </summary>
        /// <param name="userToSave">The user to save. The only property that must be specified is <see cref="IUserAccount.UserName" />.</param>
        /// <param name="roles">The roles to be associated with the user.</param>
        /// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
        public async Task ValidateLoggedOnUserHasPermissionToSaveUser(IUserAccount userToSave, string[] roles)
        {
            if (roles == null)
                throw new ArgumentNullException(nameof(roles));

            var rolesForUser = await GetRolesAsync(userToSave.UserName);
            var rolesToAdd = roles.Where(r => !rolesForUser.Contains(r)).ToArray();
            var rolesToRemove = rolesForUser.Where(r => !roles.Contains(r)).ToArray();

            // Enforces the following rules:
            // 1. A user with site administration permission has no restrictions. Subsequent rules do not apply.
            // 2. Gallery admin is not allowed to add admin site permission to any user or update any user that has site admin permission.
            // 3. Gallery admin cannot add or remove a user to/from a role associated with other galleries, UNLESS he is also a gallery admin
            //    to those galleries.
            // 4. NOT ENFORCED: If user to be updated is a member of roles that apply to other galleries, Gallery admin must be a gallery admin 
            //    in every one of those galleries. Not enforced because this is considered acceptable behavior.

            if (await _roleController.IsCurrentUserSiteAdministrator())
                return;

            VerifyGalleryAdminIsNotUpdatingUserWithAdminSitePermission(userToSave, rolesToAdd);

            VerifyGalleryAdminCanAddOrRemoveRolesForUser(rolesToAdd, rolesToRemove);

            #region RULE 4 (Not enforced)
            // RULE 4: Gallery admin can update user only when he is a gallery admin in every gallery the user to be updated is a member of.

            //// Step 1: Get a list of galleries the user to be updated is associated with.
            //IGalleryCollection userGalleries = new GalleryCollection();
            //foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser(userToSave.UserName))
            //{
            //  foreach (IGallery gallery in role.Galleries)
            //  {
            //    if (!userGalleries.Contains(gallery))
            //    {
            //      userGalleries.Add(gallery);
            //    }
            //  }
            //}

            //// Step 2: Validate that the current user is a gallery admin for every gallery the user to be updated is a member of.
            //foreach (IGallery userGallery in userGalleries)
            //{
            //  if (!adminGalleries.Contains(userGallery))
            //  {
            //    throw new GallerySecurityException("You are attempting to save changes to a user that affects multiple galleries, including at least one gallery you do not have permission to administer. To edit this user, you must be a gallery administrator in every gallery this user is a member of.");
            //  }
            //}
            #endregion
        }

        /// <summary>
        /// Automatically logs on the user specified in the query string parameter 'user' and then reloads the page with the 
        /// parameter removed. If the parameter does not exist or the user is already logged on, do nothing. Additionally, if the 
        /// specified user is not specified in the web.config application setting named GalleryServerAutoLogonUsers, no action is taken.
        /// Note: All requests come through here, even those for resources like CSS and js files.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        public void AutoLogonUser(HttpContext context)
        {
            throw new NotImplementedException();
            //string username;
            //if (!ValidateAutoLogonUserRequest(context, out username))
            //    return;

            //// If we get here, either no one is logged in or the currently logged on user is different than the requested one.
            //// Log out the current user, then log in new user.
            //FormsAuthentication.SignOut();
            //FormsAuthentication.SetAuthCookie(username, false);

            //var newUser = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity(username, "Forms"), RoleController.GetRolesForUser(username));
            //context.User = newUser;

            //// Reload the current page with the 'user' query string parameter removed.
            //context.Response.Redirect(Utils.RemoveQueryStringParameter(Utils.GetCurrentPageUrl(true), "user"), true);
        }

        /// <summary>
        /// Determine whether user has permission to perform at least one of the specified security actions. Un-authenticated users
        /// (anonymous users) are always considered NOT authorized (that is, this method returns false) except when the requested
        /// security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalMediaObject" />,
        /// since Gallery Server is configured by default to allow anonymous viewing access
        /// but it does not allow anonymous editing of any kind. This method will continue to work correctly if the webmaster configures
        /// Gallery Server to require users to log in in order to view objects, since at that point there will be no such thing as
        /// un-authenticated users, and the standard gallery server role functionality applies.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: SecurityActions.AdministerSite | SecurityActions.AdministerGallery). If multiple actions are
        /// specified, the method is successful if the user has permission for at least one of the actions. If you require that all actions
        /// be satisfied to be successful, call one of the overloads that accept a SecurityActionsOption and
        /// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
        /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. This parameter is ignored
        /// for anonymous users. The parameter may be null.</param>
        /// <param name="albumId">The album ID to which the security action applies.</param>
        /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in
        /// this gallery. This parameter is not required <paramref name="securityActions" /> is SecurityActions.AdministerSite (you can specify
        /// <see cref="int.MinValue" />).</param>
        /// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
        /// is ignored for logged on users.</param>
        /// <param name="isVirtualAlbum">if set to <c>true</c> the album is virtual album.</param>
        /// <returns>
        /// Returns true when the user is authorized to perform the specified security action against the specified album;
        /// otherwise returns false.
        /// </returns>
        public bool IsUserAuthorized(SecurityActions securityActions, IGalleryServerRoleCollection roles, int albumId, int galleryId, bool isPrivate, bool isVirtualAlbum)
        {
            return IsUserAuthorized(securityActions, roles, albumId, galleryId, isPrivate, SecurityActionsOption.RequireOne, isVirtualAlbum);
        }

        /// <summary>
        /// Determine whether user has permission to perform the specified security actions. When multiple security actions are passed, use
        /// <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied to be successful or only one item
        /// must be satisfied. Un-authenticated users (anonymous users) are always considered NOT authorized (that is, this method returns
        /// false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or
        /// <see cref="SecurityActions.ViewOriginalMediaObject" />, since Gallery Server is configured by default to allow anonymous viewing access
        /// but it does not allow anonymous editing of any kind. This method will continue to work correctly if the webmaster configures
        /// Gallery Server to require users to log in in order to view objects, since at that point there will be no such thing as
        /// un-authenticated users, and the standard gallery server role functionality applies.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: SecurityActions.AdministerSite | SecurityActions.AdministerGallery). If multiple actions are
        /// specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied to be successful or
        /// only one item must be satisfied.</param>
        /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. This parameter is ignored
        /// for anonymous users. The parameter may be null.</param>
        /// <param name="albumId">The album ID to which the security action applies.</param>
        /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in
        /// this gallery. This parameter is not required <paramref name="securityActions" /> is SecurityActions.AdministerSite (you can specify
        /// <see cref="int.MinValue" />).</param>
        /// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
        /// is ignored for logged on users.</param>
        /// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
        /// to be successful or just one.</param>
        /// <param name="isVirtualAlbum">if set to <c>true</c> the album is a virtual album.</param>
        /// <returns>
        /// Returns true when the user is authorized to perform the specified security action against the specified album;
        /// otherwise returns false.
        /// </returns>
        public bool IsUserAuthorized(SecurityActions securityActions, IGalleryServerRoleCollection roles, int albumId, int galleryId, bool isPrivate, SecurityActionsOption secActionsOption, bool isVirtualAlbum)
        {
            return SecurityManager.IsUserAuthorized(securityActions, roles, albumId, galleryId, IsAuthenticated, isPrivate, secActionsOption, isVirtualAlbum);
        }

        /// <summary>
        /// Determine whether the user belonging to the specified <paramref name="roles" /> is a site administrator. The user is considered a site
        /// administrator if at least one role has Allow Administer Site permission.
        /// </summary>
        /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. The parameter may be null.</param>
        /// <returns>
        /// 	<c>true</c> if the user is a site administrator; otherwise, <c>false</c>.
        /// </returns>
        public bool IsUserSiteAdministrator(IGalleryServerRoleCollection roles)
        {
            return SecurityManager.IsUserSiteAdministrator(roles);
        }

        /// <summary>
        /// Determine whether the user belonging to the specified <paramref name="roles"/> is a gallery administrator for the specified
        /// <paramref name="galleryId"/>. The user is considered a gallery administrator if at least one role has Allow Administer Gallery permission.
        /// </summary>
        /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. The parameter may be null.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>
        /// 	<c>true</c> if the user is a gallery administrator; otherwise, <c>false</c>.
        /// </returns>
        public bool IsUserGalleryAdministrator(IGalleryServerRoleCollection roles, int galleryId)
        {
            return SecurityManager.IsUserGalleryAdministrator(roles, galleryId);
        }

        public Task AddToRoleAsync(GalleryUser user, string roleName)
        {
            return _userManager.AddToRoleAsync(user, roleName);
        }

        public Task AddToRolesAsync(GalleryUser user, string[] roleNames)
        {
            return _userManager.AddToRolesAsync(user, roleNames);
        }

        public Task RemoveFromRoleAsync(GalleryUser user, string roleName)
        {
            return _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public Task RemoveFromRolesAsync(GalleryUser user, string[] roleNames)
        {
            return _userManager.RemoveFromRolesAsync(user, roleNames);
        }

        public async Task<IList<string>> GetRolesAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return new string[] { };

            var user = await GetUserByUserName(userName);
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<GalleryUser> GetUserByUserName(string userName)
        {
            if (userName == null)
                throw new ArgumentNullException(nameof(userName));

            return await _userManager.FindByNameAsync(userName.Trim());
        }

        /// <summary>
        /// Gets a list of users in the specified role for the current application.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>A list of users in the specified role for the current application.</returns>
        public async Task<IList<GalleryUser>> GetUsersInRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return new GalleryUser[] { };

            return await _userManager.GetUsersInRoleAsync(roleName.Trim());
            //return RoleGsp.GetUsersInRole(roleName.Trim());
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the current application.
        /// </summary>
        /// <param name="userName">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// 	<c>true</c> if the specified user is in the specified role for the configured applicationName; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userName" /> or <paramref name="roleName" /> is null
        /// or an empty string.</exception>
        public async Task<bool> IsUserInRole(string userName, string roleName)
        {
            return await _roleController.IsUserInRole(userName, roleName);
        }

        /// <summary>
        /// Gets a <see cref="User" /> instance having the properties specified in <see cref="Utils.InstallFilePath" />.
        /// Supports these properties: UserName, Password, Email. Returns null if none of these exist in the text file.
        /// </summary>
        /// <returns>An instance of <see cref="User" />, or null.</returns>
        public User GetAdminUserFromInstallTextFile()
        {
            User user = null;

            try
            {
                using (var sr = new StreamReader(Utils.InstallFilePath))
                {
                    var lineText = sr.ReadLine();
                    while (lineText != null)
                    {
                        var kvp = lineText.Split(new[] { '=' });

                        if (kvp.Length == 2)
                        {
                            if (kvp[0].Equals("UserName", StringComparison.OrdinalIgnoreCase))
                            {
                                if (user == null)
                                    user = new User();

                                user.UserName = kvp[1].Trim(); // Found username row
                            }

                            if (kvp[0].Equals("Password", StringComparison.OrdinalIgnoreCase))
                            {
                                if (user == null)
                                    user = new User();

                                user.Password = kvp[1].Trim(); // Found password row
                            }

                            if (kvp[0].Equals("Email", StringComparison.OrdinalIgnoreCase))
                            {
                                if (user == null)
                                    user = new User();

                                user.Email = kvp[1].Trim(); // Found email row
                            }
                        }

                        lineText = sr.ReadLine();
                    }
                }
            }
            catch (FileNotFoundException) { }

            return user;
        }

        /// <overloads>
        /// Gets a collection of Gallery Server roles.
        /// </overloads>
        /// <summary>
        /// Gets Gallery Server roles representing the roles for the currently logged-on user. Returns an empty collection if 
        /// no user is logged in or the user is logged in but not assigned to any roles (Count = 0).
        /// The roles may be returned from a cache. Guaranteed to not return null.
        /// </summary>
        /// <returns>
        /// Returns an <see cref="IGalleryServerRoleCollection" /> representing the roles for the currently logged-on user.
        /// </returns>
        public async Task<IGalleryServerRoleCollection> GetGalleryServerRolesForUser()
        {
            return await _roleController.GetGalleryServerRolesForUser();
        }

        /// <summary>
        /// Gets Gallery Server roles representing the roles for the specified <paramref name="userName"/>. Returns an empty collection if 
        /// the user is not assigned to any roles (Count = 0). The roles may be returned from a cache. Guaranteed to not return null.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// Returns an <see cref="IGalleryServerRoleCollection"/> representing the roles for the specified <paramref name="userName" />.
        /// </returns>
        /// <remarks>This method may run on a background thread and is therefore tolerant of the inability to access HTTP context 
        /// or the current user's session.</remarks>
        public async Task<IGalleryServerRoleCollection> GetGalleryServerRolesForUser(string userName)
        {
            return await _roleController.GetGalleryServerRolesForUser(userName);
        }

        /// <summary>
        /// Verify that any role needed for album ownership exists and is properly configured. If an album owner
        /// is specified and the album is new (IsNew == true), the album is persisted to the data store. This is 
        /// required because the ID is not assigned until it is saved, and a valid ID is required to configure the
        /// role.
        /// </summary>
        /// <param name="album">The album to validate for album ownership. If a null value is passed, the function
        /// returns without error or taking any action.</param>
        internal async Task ValidateRoleExistsForAlbumOwner(IAlbum album)
        {
            await _roleController.ValidateRoleExistsForAlbumOwner(album);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a new user with the specified e-mail address to the data store.
        /// </summary>
        /// <param name="userName">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The email for the new user.</param>
        /// <returns>Returns a new user with the specified e-mail address to the data store.</returns>
        private IUserAccount CreateUser(string userName, string password, string email)
        {
            throw new NotImplementedException();
            //// This function is a re-implementation of the System.Web.Security.Membership.CreateUser method. We can't call it directly
            //// because it uses the default provider, and we might be using a named provider.
            //MembershipCreateStatus status;
            //MembershipUser user = MembershipGsp.CreateUser(userName, password, email, null, null, true, null, out status);
            //if (user == null)
            //{
            //    throw new MembershipCreateUserException(status);
            //}

            //return ToUserAccount(user);
        }

        /// <summary>
        /// Send an e-mail to the users that are subscribed to new account notifications. These are specified in the
        /// <see cref="IGallerySettings.UsersToNotifyWhenAccountIsCreated" /> configuration setting. If 
        /// <see cref="IGallerySettings.RequireEmailValidationForSelfRegisteredUser" /> is enabled, do not send an e-mail at this time. 
        /// Instead, it is sent when the user clicks the confirmation link in the e-mail.
        /// </summary>
        /// <param name="user">An instance of <see cref="IUserAccount"/> that represents the newly created account.</param>
        /// <param name="isSelfRegistration">Indicates when the user is creating his or her own account. Set to false when an
        /// administrator creates an account.</param>
        /// <param name="isEmailVerified">If set to <c>true</c> the e-mail has been verified to be a valid, active e-mail address.</param>
        /// <param name="galleryId">The gallery ID storing the e-mail configuration information and the list of users to notify.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
        private void NotifyAdminsOfNewlyCreatedAccount(IUserAccount user, bool isSelfRegistration, bool isEmailVerified, int galleryId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            IGallerySettings gallerySettings = Factory.LoadGallerySetting(galleryId);

            if (isSelfRegistration && !isEmailVerified && gallerySettings.RequireEmailValidationForSelfRegisteredUser)
            {
                return;
            }

            EmailTemplate emailTemplate;
            if (isSelfRegistration && gallerySettings.RequireApprovalForSelfRegisteredUser)
            {
                emailTemplate = _emailController.GetEmailTemplate(EmailTemplateForm.AdminNotificationAccountCreatedRequiresApproval, user);
            }
            else
            {
                emailTemplate = _emailController.GetEmailTemplate(EmailTemplateForm.AdminNotificationAccountCreated, user);
            }

            foreach (IUserAccount userToNotify in gallerySettings.UsersToNotifyWhenAccountIsCreated)
            {
                if (!String.IsNullOrEmpty(userToNotify.Email))
                {
                    MailAddress admin = new MailAddress(userToNotify.Email, userToNotify.UserName);
                    try
                    {
                        _emailController.SendEmail(admin, emailTemplate.Subject, emailTemplate.Body, galleryId);
                    }
                    catch (WebException ex)
                    {
                        AppEventController.LogError(ex);
                    }
                    catch (SmtpException ex)
                    {
                        AppEventController.LogError(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Send an e-mail to the user associated with the new account. This will be a verification e-mail if e-mail verification
        /// is enabled; otherwise it is a welcome message. The calling method should ensure that the <paramref name="user"/>
        /// has a valid e-mail configured before invoking this function.
        /// </summary>
        /// <param name="user">An instance of <see cref="IUserAccount"/> that represents the newly created account.</param>
        /// <param name="galleryId">The gallery ID. This specifies which gallery to use to look up configuration settings.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
        private void NotifyUserOfNewlyCreatedAccount(IUserAccount user, int galleryId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

            bool enableEmailVerification = gallerySetting.RequireEmailValidationForSelfRegisteredUser;
            bool requireAdminApproval = gallerySetting.RequireApprovalForSelfRegisteredUser;

            if (enableEmailVerification)
            {
                _emailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationAccountCreatedNeedsVerification);
            }
            else if (requireAdminApproval)
            {
                _emailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationAccountCreatedNeedsApproval);
            }
            else
            {
                _emailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationAccountCreated);
            }
        }

        ///// <summary>
        ///// Throws an exception if the user cannot be deleted, such as when trying to delete his or her own account, or when deleting
        ///// the only account with admin permission.
        ///// </summary>
        ///// <param name="userName">Name of the user to delete.</param>
        ///// <param name="preventDeletingLoggedOnUser">If set to <c>true</c>, throw a <see cref="GallerySecurityException"/> if attempting
        ///// to delete the currently logged on user.</param>
        ///// <param name="preventDeletingLastAdminAccount">If set to <c>true</c> throw a <see cref="GallerySecurityException"/> if attempting
        ///// to delete the last user with <see cref="SecurityActions.AdministerSite" /> permission. When false, do not perform this check. It does not matter
        ///// whether the user to delete is actually an administrator.</param>
        ///// <exception cref="GallerySecurityException">Thrown when the user cannot be deleted because doing so violates one of the business rules.</exception>
        //private async Task ValidateDeleteUser(string userName, bool preventDeletingLoggedOnUser, bool preventDeletingLastAdminAccount)
        //{
        //    if (preventDeletingLoggedOnUser)
        //    {
        //        // Don't let user delete their own account.
        //        if (userName.Equals(UserName, StringComparison.OrdinalIgnoreCase))
        //        {
        //            throw new GallerySecurityException("You are not allowed to delete the account you are logged on as. If you want to delete this account, first log on as another user.");
        //        }
        //    }

        //    if (preventDeletingLastAdminAccount)
        //    {
        //        if (!await DoesAtLeastOneOtherSiteAdminExist(userName))
        //        {
        //            if (!await DoesAtLeastOneOtherGalleryAdminExist(userName))
        //            {
        //                throw new GallerySecurityException("You are attempting to delete the only user with permission to administer a gallery or site. If you want to delete this account, first assign another account to a role with administrative permission.");
        //            }
        //        }
        //    }

        //    // User can delete account only if he is a site admin or a gallery admin in every gallery this user can access.
        //    IGalleryCollection adminGalleries = await GetGalleriesCurrentUserCanAdminister();

        //    if (adminGalleries.Count > 0) // Only continue when user is an admin for at least one gallery. This allows regular users to delete their own account.
        //    {
        //        foreach (IGallery gallery in await GetGalleriesForUser(userName))
        //        {
        //            if (!adminGalleries.Contains(gallery))
        //            {
        //                throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "The user '{0}' has access to a gallery (Gallery ID = {1}) that you are not an administrator for. To delete a user, one of the following must be true: (1) you are a site administrator, or (2) you are a gallery administrator in every gallery the user has access to.", userName, gallery.GalleryId));
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// If user is a gallery admin, verify at least one other user is a gallery admin for each gallery. If user is not a gallery 
        /// admin for any gallery, return <c>true</c> without actually verifying that each that each gallery has an admin, since it
        /// is reasonable to assume it does (and even if it didn't, that shouldn't prevent us from deleting this user).
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns><c>true</c> if at least one user besides <paramref name="userName" /> is a gallery admin for each gallery;
        /// otherwise <c>false</c>.</returns>
        private async Task<bool> DoesAtLeastOneOtherGalleryAdminExist(string userName)
        {
            bool atLeastOneOtherAdminExists = false;

            IGalleryCollection galleriesUserCanAdminister = await GetGalleriesUserCanAdminister(userName);

            if (galleriesUserCanAdminister.Count == 0)
            {
                // User is not a gallery administrator, so we don't have to make sure there is another gallery administrator.
                // Besides, we can assume there is another one anyway.
                return true;
            }

            foreach (IGallery gallery in galleriesUserCanAdminister)
            {
                // Get all the roles that have gallery admin permission to this gallery
                foreach (IGalleryServerRole role in _roleController.GetGalleryServerRolesForGallery(gallery).GetRolesWithGalleryAdminPermission())
                {
                    // Make sure at least one user besides the user specified in userName is in these roles.
                    foreach (var userNameInRole in await GetUsersInRole(role.RoleName))
                    {
                        if (!userNameInRole.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        {
                            atLeastOneOtherAdminExists = true;
                            break;
                        }
                    }

                    if (atLeastOneOtherAdminExists)
                        break;
                }

                if (atLeastOneOtherAdminExists)
                    break;
            }

            return atLeastOneOtherAdminExists;
        }

        /// <summary>
        /// Determine if at least one other user beside <paramref name="userName" /> is a site administrator.
        /// </summary>
        /// <param name="userName">A user name.</param>
        /// <returns><c>true</c> if at least one other user beside <paramref name="userName" /> is a site administrator; otherwise <c>false</c>.</returns>
        private async Task<bool> DoesAtLeastOneOtherSiteAdminExist(string userName)
        {
            bool atLeastOneOtherAdminExists = false;

            foreach (IGalleryServerRole role in _roleController.GetGalleryServerRoles())
            {
                if (!role.AllowAdministerSite)
                    continue;

                foreach (var userInAdminRole in await GetUsersInRole(role.RoleName))
                {
                    if (!userInAdminRole.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    {
                        atLeastOneOtherAdminExists = true;
                        break;
                    }
                }
            }
            return atLeastOneOtherAdminExists;
        }

        //private void DeleteUserAlbum(string userName, int galleryId)
        //{
        //    IAlbum album = GetUserAlbum(userName, galleryId);

        //    if (album != null)
        //        AlbumController.DeleteAlbum(album);
        //}

        /// <summary>
        /// Remove the user from any roles. If a role is an ownership role, then delete it if the user is the only member.
        /// Remove the user from ownership of any albums.
        /// </summary>
        /// <param name="userName">Name of the user to be deleted.</param>
        /// <remarks>The user will be specified as an owner only for those albums that belong in ownership roles, so
        /// to find all albums the user owns, we need only to loop through the user's roles and inspect the ones
        /// where the names begin with the album owner role name prefix variable.</remarks>
        private async Task UpdateRolesAndOwnershipBeforeDeletingUser(string userName)
        {
            List<string> rolesToDelete = new List<string>();

            var userRoles = await GetRolesAsync(userName);
            foreach (var roleName in userRoles)
            {
                if (_roleController.IsRoleAnAlbumOwnerRole(roleName))
                {
                    if (!(await GetUsersInRole(roleName)).Any())
                    {
                        // The user we are deleting is the only user in the owner role. Mark for deletion.
                        rolesToDelete.Add(roleName);
                    }
                }
            }

            if (userRoles.Any())
            {
                foreach (string role in userRoles)
                {
                    _roleController.RemoveUserFromRole(userName, role);
                }
            }

            foreach (string roleName in rolesToDelete)
            {
                _roleController.DeleteGalleryServerProRole(roleName);
            }
        }

        private static IUserAccount ToUserAccount(GalleryUser u)
        {
            if (u == null)
                return null;

            return new UserAccount(null, DateTime.MinValue, u.Email, true, false, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, null, null, null, u.UserName, false, string.Empty, string.Empty, string.Empty);
        }

        /// <summary>
        /// Make sure the logged-on person has authority to save the user info and that h/she isn't doing anything stupid,
        /// like removing admin permission from his or her own account. Throws a <see cref="GallerySecurityException"/> when
        /// the action is not allowed.
        /// </summary>
        /// <param name="userToSave">The user to save.</param>
        /// <param name="roles">The roles to associate with the user.</param>
        /// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
        /// <exception cref="InvalidUserException">Thrown when the e-mail address is not valid.</exception>
        private async Task ValidateSaveUser(IUserAccount userToSave, string[] roles)
        {
            if (AppSetting.Instance.InstallationRequested && (GetAdminUserFromInstallTextFile().UserName == userToSave.UserName))
            {
                // We are creating the user specified in install.txt. Don't continue validation because it will fail 
                // if no one is logged in to the gallery or the logged on user doesn't have permission to create/edit a user.
                // This is not a security vulnerability because if the user has the ability to write a file to App_Data
                // the server is already compromised.
                return;
            }

            if (!await UserCanViewAndEditUser(userToSave))
            {
                throw new GallerySecurityException("You must be a gallery or site administrator to save changes to this user.");
            }

            if (userToSave.UserName.Equals(UserName, StringComparison.OrdinalIgnoreCase))
            {
                ValidateUserCanSaveOwnAccount(userToSave, roles);
            }

            ValidateLoggedOnUserHasPermissionToSaveUser(userToSave, roles);

            ValidateEmail(userToSave);
        }

        /// <summary>
        /// Validates the user can save his own account. Throws a <see cref="GallerySecurityException" /> when the action is not allowed.
        /// </summary>
        /// <param name="userToSave">The user to save.</param>
        /// <param name="roles">The roles to associate with the user.</param>
        /// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
        private async Task ValidateUserCanSaveOwnAccount(IUserAccount userToSave, IEnumerable<string> roles)
        {
            // This function should be called only when the logged on person is updating their own account. They are not allowed to 
            // revoke approval and they must remain in at least one role that has Administer Site or Administer Gallery permission.
            if (!userToSave.IsApproved)
            {
                throw new GallerySecurityException("Cannot revoke approval. You are editing the user account you are logged on as, and are trying to revoke approval, which would disable this account. You must log on as another user to revoke approval for this account.");
            }

            if (!(await _roleController.GetGalleryServerRoles(roles)).Any(role => role.AllowAdministerSite || role.AllowAdministerGallery))
            {
                throw new GallerySecurityException("Cannot remove requested roles. You are editing the user account you are logged on as, and you are attempting to remove your ability to administer this gallery. If you really want to do this, log on as another user and make the changes from that account.");
            }
        }

        /// <summary>
        /// Verifies that the specified <paramref name="userToSave" /> is not a site administrator or is being added to a site administrator
        /// role. Calling methods should invoke this function ONLY when the current user is a gallery administrator.
        /// </summary>
        /// <param name="userToSave">The user to save. The only property that must be specified is <see cref="IUserAccount.UserName" />.</param>
        /// <param name="rolesToAdd">The roles to be associated with the user. Must not be null. The roles should not already be assigned to the
        /// user, although no harm is done if they are.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="userToSave" /> or <paramref name="rolesToAdd" /> is null.</exception>
        private async Task VerifyGalleryAdminIsNotUpdatingUserWithAdminSitePermission(IUserAccount userToSave, IEnumerable<string> rolesToAdd)
        {
            if (userToSave == null)
                throw new ArgumentNullException(nameof(userToSave));

            if (rolesToAdd == null)
                throw new ArgumentNullException(nameof(rolesToAdd));

            IGalleryServerRoleCollection rolesAssignedOrBeingAssignedToUser = (await _roleController.GetGalleryServerRolesForUser(userToSave.UserName)).Copy();

            foreach (string roleToAdd in rolesToAdd)
            {
                if (rolesAssignedOrBeingAssignedToUser.GetRole(roleToAdd) == null)
                {
                    IGalleryServerRole role = Factory.LoadGalleryServerRole(roleToAdd);

                    if (role != null)
                    {
                        rolesAssignedOrBeingAssignedToUser.Add(role);
                    }
                }
            }

            foreach (IGalleryServerRole role in rolesAssignedOrBeingAssignedToUser)
            {
                if (role.AllowAdministerSite)
                {
                    throw new GallerySecurityException("You must be a site administrator to add a user to a role with Administer site permission or update an existing user who has Administer site permission. Sadly, you are just a gallery administrator.");
                }
            }
        }

        /// <summary>
        /// Verifies the current user can add or remove the specified roles to or from a user. Specifically, the user must be a gallery
        /// administrator in every gallery each role is associated with. Calling methods should invoke this function ONLY when the current 
        /// user is a gallery administrator.
        /// </summary>
        /// <param name="rolesToAdd">The roles to be associated with the user. Must not be null. The roles should not already be assigned to the
        /// user, although no harm is done if they are.</param>
        /// <param name="rolesToRemove">The roles to remove from user.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rolesToAdd" /> or <paramref name="rolesToRemove" /> is null.</exception>
        private async Task VerifyGalleryAdminCanAddOrRemoveRolesForUser(IEnumerable<string> rolesToAdd, IEnumerable<string> rolesToRemove)
        {
            if (rolesToAdd == null)
                throw new ArgumentNullException(nameof(rolesToAdd));

            if (rolesToRemove == null)
                throw new ArgumentNullException(nameof(rolesToRemove));

            IGalleryCollection adminGalleries = await GetGalleriesCurrentUserCanAdminister();

            List<string> rolesBeingAddedOrRemoved = new List<string>(rolesToAdd);
            rolesBeingAddedOrRemoved.AddRange(rolesToRemove);

            foreach (string roleName in rolesBeingAddedOrRemoved)
            {
                // Gallery admin cannot add or remove a user to/from a role associated with other galleries, UNLESS he is also a gallery admin
                // to those galleries.
                IGalleryServerRole roleToAddOrRemove = Factory.LoadGalleryServerRole(roleName);

                if (roleToAddOrRemove != null)
                {
                    foreach (IGallery gallery in roleToAddOrRemove.Galleries)
                    {
                        if (!adminGalleries.Contains(gallery))
                        {
                            throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "You are attempting to save changes to a user that will affect multiple galleries, including at least one gallery you do not have permission to administer. Specifically, the role '{0}' applies to gallery {1}, which you are not an administrator for.", roleToAddOrRemove.RoleName, gallery.GalleryId));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets an unsorted list of users the currently logged on user can view.
        /// </summary>
        /// <param name="userIsSiteAdmin">If set to <c>true</c>, the currently logged on user is a site administrator.</param>
        /// <param name="userIsGalleryAdmin">If set to <c>true</c>, the currently logged on user is a gallery administrator for the current gallery.</param>
        /// <returns>Returns an <see cref="IUserAccountCollection"/> containing a list of roles the user has permission to view.</returns>
        private async Task<IUserAccountCollection> DetermineUsersCurrentUserCanView(bool userIsSiteAdmin, bool userIsGalleryAdmin)
        {
            if (userIsSiteAdmin || (userIsGalleryAdmin && AppSetting.Instance.AllowGalleryAdminToViewAllUsersAndRoles))
            {
                return GetAllUsers();
            }

            // Filter the accounts so that only users in galleries where
            // the current user is a gallery admin are shown.
            IGalleryCollection adminGalleries = await GetGalleriesCurrentUserCanAdminister();

            IUserAccountCollection users = new UserAccountCollection();

            foreach (IUserAccount user in GetAllUsers())
            {
                foreach (IGalleryServerRole role in await _roleController.GetGalleryServerRolesForUser(user.UserName))
                {
                    bool userHasBeenAdded = false;
                    foreach (IGallery gallery in role.Galleries)
                    {
                        if (adminGalleries.Contains(gallery))
                        {
                            // User belongs to a gallery that the current user is a gallery admin for. Include the account.
                            users.Add(user);
                            userHasBeenAdded = true;
                            break;
                        }
                    }
                    if (userHasBeenAdded) break;
                }
            }
            return users;
        }

        private string GetCacheKeyNameForUsersCurrentUserCanView(string userName)
        {
            return String.Concat(_httpContextAccessor.HttpContext.Session.Id, "_", userName, "_Users");
        }

        /// <summary>
        /// Determines whether the user has permission to view and edit the specified user. Determines this by checking
        /// whether the logged on user is a site administrator, the same as the user being viewed, or a gallery 
        /// administrator for at least one gallery associated with the user, or a gallery admin for ANY gallery and the 
        /// option AllowGalleryAdminToViewAllUsersAndRoles is enabled. NOTE: This function assumes the current
        /// user is a site or gallery admin, so be sure this rule is enforced at some point before persisting to
        /// the data store.
        /// </summary>
        /// <param name="user">The user to evaluate.</param>
        /// <returns><c>true</c> if the user has permission to view and edit the specified user; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
        private async Task<bool> UserCanViewAndEditUser(IUserAccount user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (await _roleController.IsCurrentUserSiteAdministrator())
            {
                return true;
            }

            if (UserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase))
            {
                return true; // User can edit their own account
            }

            // Return true if any of the galleries the current user can administer is also one of the galleries the specified
            // user is associated with.
            var galleriesCurUserCanAdmin = await GetGalleriesCurrentUserCanAdminister();
            var userIsInGalleryCurrentUserHasAdminRightsFor = (await _roleController.GetGalleryServerRolesForUser()).Any(r => r.Galleries.Any(galleriesCurUserCanAdmin.Contains));

            return userIsInGalleryCurrentUserHasAdminRightsFor || (AppSetting.Instance.AllowGalleryAdminToViewAllUsersAndRoles && (await GetGalleriesCurrentUserCanAdminister()).Any());
        }

        /// <summary>
        /// Verifies that the e-mail address for the <paramref name="user" /> conforms to the expected format. No action is
        /// taken if <see cref="Entity.User.Email" /> is null or empty.
        /// </summary>
        /// <param name="user">The user to validate.</param>
        /// <exception cref="InvalidUserException">Thrown when the e-mail address is not valid.</exception>
        private void ValidateEmail(IUserAccount user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!String.IsNullOrEmpty(user.Email) && !HelperFunctions.IsValidEmail(user.Email))
            {
                throw new InvalidUserException("E-mail is not valid.");
            }
        }

        #endregion

        public Task<bool> RoleExists(string roleName)
        {
            return _roleController.RoleExists(roleName);
        }

        public Task AddUserToRole(string userName, string roleName)
        {
            return _roleController.AddUserToRole(userName, roleName);
        }

        public Task RemoveUserFromRole(GalleryUser user, string roleName)
        {
            return _roleController.RemoveUserFromRole(user, roleName);
        }

        /// <summary>
        /// Retrieve Gallery Server roles, optionally excluding roles that were programmatically
        /// created to assist with the album ownership and user album functions. Excluding the owner roles may be useful
        /// in reducing the clutter when an administrator is viewing the list of roles, as it hides those not specifically created
        /// by the administrator. The roles may be returned from a cache.
        /// </summary>
        /// <param name="includeOwnerRoles">If set to <c>true</c> include all roles that serve as an album owner role.
        /// When <c>false</c>, exclude owner roles from the result set.</param>
        /// <returns>
        /// Returns the Gallery Server roles, optionally excluding owner roles.
        /// </returns>
        public IGalleryServerRoleCollection GetGalleryServerRoles()
        {
            return _roleController.GetGalleryServerRoles();
        }

        /// <summary>
        /// Delete the specified role. Both components of the role are deleted: the IGalleryServerRole and ASP.NET role.
        /// </summary>
        /// <param name="roleName">Name of the role. Must match an existing <see cref="IGalleryServerRole.RoleName"/>. If no match
        /// if found, no action is taken.</param>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
        public void DeleteGalleryServerProRole(string roleName)
        {
            _roleController.DeleteGalleryServerProRole(roleName);
        }
    }
}
