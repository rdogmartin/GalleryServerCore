﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
//using System.Web;
//using System.Web.Security;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace GalleryServer.Web.Controller
{
    /// <summary>
    /// Contains functionality for managing roles.
    /// </summary>
    public class RoleController
    {
        #region Private Fields

        //private RoleProvider _roleProvider;
        private readonly GalleryRoleManager _roleManager;

        private readonly AlbumTreeViewBuilder _tvBuilder;
        private readonly UserManager<GalleryUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
        private UserController _userController;

        // RegEx pattern to match "_{PortalId}" portion of GSP role name. Not used in stand-alone version of GSP.
        //private readonly System.Text.RegularExpressions.Regex _gspRoleNameSuffixRegEx = new System.Text.RegularExpressions.Regex(@"_\d+$", System.Text.RegularExpressions.RegexOptions.Compiled);

        // RegEx pattern to match the album owner role template name. The gallery ID is assigned the group name "galleryId".
        // Ex: Given "_Album Owner Template (Gallery ID 723: My gallery)", match will be a success and group name "galleryId" will contain "723"
        private static readonly string _gspAlbumOwnerTemplateRoleNameRegExPattern = string.Concat(GlobalConstants.AlbumOwnerRoleTemplateName, @" \(Gallery ID (?<galleryId>\d+): .*\)$");
        private static readonly System.Text.RegularExpressions.Regex _gspAlbumOwnerTemplateRoleNameRegEx = new System.Text.RegularExpressions.Regex(_gspAlbumOwnerTemplateRoleNameRegExPattern, System.Text.RegularExpressions.RegexOptions.Compiled);

        #endregion

        #region Constructors

        public RoleController(IHttpContextAccessor httpContextAccessor, UserManager<GalleryUser> userManager, GalleryRoleManager roleManager, AlbumTreeViewBuilder tvBuilder) // UserManager<GalleryUser> userManager, GalleryRoleManager roleManager, AlbumTreeViewBuilder tvBuilder, IHttpContextAccessor httpContextAccessor
        {
            //_userManager = null;
            //_roleManager = null;
            //_tvBuilder = null;
            //_httpContextAccessor = null;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _roleManager = roleManager;
            _tvBuilder = tvBuilder;
        }

        #endregion

        #region Properties

        //public UserManager<GalleryUser> UserManager => _userController.UserManager;

        ///// <summary>
        ///// Gets the role provider used by Gallery Server.
        ///// </summary>
        ///// <value>The role provider used by Gallery Server.</value>
        //internal RoleProvider RoleGsp
        //{
        //    get
        //    {
        //        if (_roleProvider == null)
        //        {
        //            _roleProvider = GetRoleProvider();
        //        }

        //        return _roleProvider;
        //    }
        //}

        /// <summary>
        /// Gets the role provider used by Gallery Server.
        /// </summary>
        /// <value>The role provider used by Gallery Server.</value>
        //internal GalleryRoleManager RoleGsp
        //{
        //    get
        //    {
        //        if (_roleManager == null)
        //        {
        //            _roleManager = GetRoleProvider();
        //        }

        //        return _roleManager;
        //    }
        //}

        //public UserManager<GalleryUser> UserManager
        //{
        //    get
        //    {
        //        if (_userManager == null)
        //        {
        //            _userManager = DiHelper.GetUserManager();
        //        }

        //        return _userManager;
        //    }
        //}

        public string UserName
        {
            get
            {
                return Utils.ParseUserName(_httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            }
        }

        public UserController UserController
        {
            get
            {
                if (_userController == null)
                {
                    throw new NullReferenceException("The UserController property must be set on the RoleController class.");
                }

                return _userController;
            }
            private set { _userController = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Assigns an instance of <see cref="Controller.UserController" /> to a property so that this class can access it's methods and properties. This must be manually assigned
        /// because using DI would cause a circular reference. This is probably a code smell but I don't have a good alternative at the moment.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        public void SetUserController(UserController userManager)
        {
            UserController = userManager;
        }

        /// <overloads>
        /// Persist the role to the data store. Prior to saving, validation is performed and a 
        /// <see cref="GallerySecurityException" /> is thrown if a business rule is violated.
        /// </overloads>
        /// <summary>
        /// Persist the <paramref name="role" /> to the data store. Prior to saving, validation is performed and a 
        /// <see cref="GallerySecurityException" /> is thrown if a business rule is violated.
        /// </summary>
        /// <param name="role">The role to save.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="role" /> is null.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
        /// <exception cref="InvalidGalleryServerRoleException">Thrown when an existing role cannot be found in the database that matches the 
        /// role name of the <paramref name="role" /> parameter, or one is found and the role to save is specifed as new.</exception>
        public void Save(Entity.Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var r = Factory.LoadGalleryServerRole(role.Name, true);

            if (r == null && !role.IsNew)
                throw new InvalidGalleryServerRoleException(string.Format("A role with the name '{0}' does not exist.", role.Name));

            if (role.IsNew)
            {
                if (r != null)
                    throw new InvalidGalleryServerRoleException("Cannot create role: A role already exists with the name you entered.");

                Entity.Permissions p = role.Permissions;
                CreateRole(role.Name, p.ViewAlbumOrMediaObject, p.ViewOriginalMediaObject, p.AddMediaObject, p.AddChildAlbum,
                  p.EditMediaObject, p.EditAlbum, p.DeleteMediaObject, p.DeleteChildAlbum, p.Synchronize, p.AdministerSite,
                  p.AdministerGallery, p.HideWatermark, new IntegerCollection(role.SelectedRootAlbumIds));
            }
            else
            {
                r.AllowAddChildAlbum = role.Permissions.AddChildAlbum;
                r.AllowAddMediaObject = role.Permissions.AddMediaObject;
                r.AllowAdministerSite = role.Permissions.AdministerSite;
                r.AllowAdministerGallery = role.Permissions.AdministerGallery;
                r.AllowDeleteChildAlbum = role.Permissions.DeleteChildAlbum;
                r.AllowDeleteMediaObject = role.Permissions.DeleteMediaObject;
                r.AllowEditAlbum = role.Permissions.EditAlbum;
                r.AllowEditMediaObject = role.Permissions.EditMediaObject;
                r.AllowSynchronize = role.Permissions.Synchronize;
                r.AllowViewOriginalImage = role.Permissions.ViewOriginalMediaObject;
                r.AllowViewAlbumOrMediaObject = role.Permissions.ViewAlbumOrMediaObject;
                r.HideWatermark = role.Permissions.HideWatermark;

                Save(r, new IntegerCollection(role.SelectedRootAlbumIds));
            }
        }

        /// <summary>
        /// Persist the <paramref name="roleToSave" /> to the data store, associating any album IDs listed in <paramref name="topLevelCheckedAlbumIds" />
        /// with it. Prior to saving, validation is performed and a <see cref="GallerySecurityException" /> is thrown if a business rule
        /// is violated.
        /// </summary>
        /// <param name="roleToSave">The role to save.</param>
        /// <param name="topLevelCheckedAlbumIds">The top level album IDs. May be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleToSave" /> is null.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
        /// <exception cref="InvalidGalleryServerRoleException">Thrown when an existing role cannot be found in the database that matches the 
        /// role name of the <paramref name="roleToSave" /> parameter.</exception>
        public void Save(IGalleryServerRole roleToSave, IIntegerCollection topLevelCheckedAlbumIds)
        {
            if (roleToSave == null)
                throw new ArgumentNullException(nameof(roleToSave));

            ValidateSaveRole(roleToSave);

            UpdateRoleAlbumRelationships(roleToSave, topLevelCheckedAlbumIds);

            roleToSave.Save();

            CacheController.RemoveCache(CacheItem.GalleryServerRoles);
            CacheController.RemoveCache(CacheItem.UsersCurrentUserCanView);
        }

        /// <summary>
        /// Add the specified user to the specified role.
        /// </summary>
        /// <param name="user">The user name to add to the specified role.</param>
        /// <param name="roleName">The role to add the specified user name to.</param>
        public async Task AddUserToRole(GalleryUser user, string roleName)
        {
            if (user != null && !string.IsNullOrWhiteSpace(roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }

        /// <summary>
        /// Add the specified user to the specified role.
        /// </summary>
        /// <param name="userName">The user name to add to the specified role.</param>
        /// <param name="roleName">The role to add the specified user name to.</param>
        public async Task AddUserToRole(string userName, string roleName)
        {
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(roleName))
            {
                var user = await _userManager.FindByNameAsync(userName.Trim());
                await _userManager.AddToRoleAsync(user, roleName);
                //AddUserToRoles(userName, new[] { roleName.Trim() });
            }
        }

        /// <summary>
        /// Add the specified user to the specified roles.
        /// </summary>
        /// <param name="userName">The user name to add to the specified role.</param>
        /// <param name="roleNames">The roles to add the specified user name to.</param>
        public async Task AddUserToRoles(string userName, string[] roleNames)
        {
            if (!String.IsNullOrWhiteSpace(userName) && (roleNames != null) && (roleNames.Length > 0))
            {
                var user = await _userManager.FindByNameAsync(userName.Trim());
                await _userManager.AddToRolesAsync(user, roleNames);
                //RoleGsp.AddUsersToRoles(new[] { userName.Trim() }, roleNames);

                CacheController.RemoveCache(CacheItem.UsersCurrentUserCanView);
            }
        }

        ///// <summary>
        ///// Add the specified users to the specified role.
        ///// </summary>
        ///// <param name="userNames">The user names to add to the specified role.</param>
        ///// <param name="roleName">The role to add the specified user names to.</param>
        //public void AddUsersToRole(string[] userNames, string roleName)
        //{
        //    if ((userNames != null) && (userNames.Length > 0) && !String.IsNullOrWhiteSpace(roleName))
        //    {
        //        RoleGsp.AddUsersToRoles(userNames, new[] { roleName.Trim() });

        //        CacheController.RemoveCache(CacheItem.UsersCurrentUserCanView);
        //    }
        //}

        /// <summary>
        /// Removes the specified user from the specified role.
        /// </summary>
        /// <param name="user">The user to remove from the specified role.</param>
        /// <param name="roleName">The role to remove the specified user from.</param>
        public async Task RemoveUserFromRole(GalleryUser user, string roleName)
        {
            if (user != null && !string.IsNullOrEmpty(roleName))
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
            }
        }

        /// <summary>
        /// Removes the specified user from the specified role.
        /// </summary>
        /// <param name="userName">The user to remove from the specified role.</param>
        /// <param name="roleName">The role to remove the specified user from.</param>
        public async Task RemoveUserFromRole(string userName, string roleName)
        {
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrEmpty(roleName))
            {
                await RemoveUserFromRoles(userName, new[] { roleName.Trim() });
            }
        }

        /// <summary>
        /// Removes the specified user from the specified roles.
        /// </summary>
        /// <param name="userName">The user to remove from the specified role.</param>
        /// <param name="roleNames">The roles to remove the specified user from.</param>
        public async Task RemoveUserFromRoles(string userName, string[] roleNames)
        {
            if (!string.IsNullOrWhiteSpace(userName) && (roleNames != null) && (roleNames.Length > 0))
            {
                var user = await _userManager.FindByNameAsync(userName.Trim());
                await _userManager.RemoveFromRolesAsync(user, roleNames);

                //RoleGsp.RemoveUsersFromRoles(new[] { userName.Trim() }, roleNames);

                ValidateRemoveUserFromRole(userName, roleNames);

                CacheController.RemoveCache(CacheItem.UsersCurrentUserCanView);
            }
        }

        ///// <summary>
        ///// Removes the specified users from the specified role.
        ///// </summary>
        ///// <param name="userNames">The users to remove from the specified role.</param>
        ///// <param name="roleName">The role to remove the specified users from.</param>
        //public void RemoveUsersFromRole(string[] userNames, string roleName)
        //{
        //    if ((userNames != null) && (userNames.Length > 0) && !String.IsNullOrWhiteSpace(roleName))
        //    {
        //        RemoveUsersFromRoles(userNames, new[] { roleName.Trim() });

        //        foreach (var userName in userNames)
        //        {
        //            ValidateRemoveUserFromRole(userName, new[] { roleName.Trim() });
        //        }
        //    }
        //}

        ///// <summary>
        ///// Removes the specified users from the specified roles.
        ///// </summary>
        ///// <param name="userNames">The users to remove from the specified roles.</param>
        ///// <param name="roleNames">The roles to remove the specified users from.</param>
        //public void RemoveUsersFromRoles(string[] userNames, string[] roleNames)
        //{
        //    if ((userNames != null) && (userNames.Length > 0) && (roleNames != null) && (roleNames.Length > 0))
        //    {
        //        RoleGsp.RemoveUsersFromRoles(userNames, roleNames);
        //    }

        //    foreach (var userName in userNames)
        //    {
        //        ValidateRemoveUserFromRole(userName, roleNames);
        //    }

        //    CacheController.RemoveCache(CacheItem.UsersCurrentUserCanView);
        //}

        /// <summary>
        /// Gets a role entity corresponding to <paramref name="roleName" />. If the role does not exist, an instance with 
        /// a set of default values is returned that can be used to create a new role. The instance can be serialized to JSON and
        /// subsequently used in the browser as a data object. A <see cref="GallerySecurityException" /> is thrown if the current
        /// user doesn't have permission to view the role.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <returns>Returns an <see cref="Entity.Role" /> instance.</returns>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have permission to view the role.</exception>
        public async Task<Role> GetRoleEntity(string roleName)
        {
            var role = Factory.LoadGalleryServerRole(roleName, true);

            // Throw exception if user can't view role. Note that GSP doesn't differentiate between permission to view and permission to
            // edit, so we use the UserCanEditRole function, even though we are just getting a role, not editing it.
            if (role != null && !await UserCanViewRole(role))
                throw new GallerySecurityException("Insufficient permission to view role.");

            Entity.Role r = new Entity.Role();
            Entity.Permissions p = new Entity.Permissions();

            if (role != null)
            {
                r.Name = role.RoleName.Trim();
                r.IsNew = false;
                r.IsOwner = (IsRoleAnAlbumOwnerRole(r.Name) || IsRoleAnAlbumOwnerTemplateRole(r.Name));
                p.ViewAlbumOrMediaObject = role.AllowViewAlbumOrMediaObject;
                p.ViewOriginalMediaObject = role.AllowViewOriginalImage;
                p.AddChildAlbum = role.AllowAddChildAlbum;
                p.AddMediaObject = role.AllowAddMediaObject;
                p.EditAlbum = role.AllowEditAlbum;
                p.EditMediaObject = role.AllowEditMediaObject;
                p.DeleteAlbum = false; // This permission exists only in the context of a particular album and not as a stand-alone permission
                p.DeleteChildAlbum = role.AllowDeleteChildAlbum;
                p.DeleteMediaObject = role.AllowDeleteMediaObject;
                p.Synchronize = role.AllowSynchronize;
                p.AdministerGallery = role.AllowAdministerGallery;
                p.AdministerSite = role.AllowAdministerSite;
                p.HideWatermark = role.HideWatermark;
            }
            else
            {
                r.IsNew = true;
            }

            r.Permissions = p;
            IIntegerCollection rootAlbumIds = (role != null ? role.RootAlbumIds : new IntegerCollection());

            var tvOptions = new Entity.TreeViewOptions(_userController)
            {
                EnableCheckboxPlugin = true,
                NumberOfLevels = 2,
                RequiredSecurityPermissions = SecurityActions.AdministerSite | SecurityActions.AdministerGallery,
                Galleries = Factory.LoadGalleries(),
                RootNodesPrefix = "Gallery '{GalleryDescription}': ",
                SelectedAlbumIds = rootAlbumIds,
                IncludeAlbum = true
            };

            Entity.TreeView tv = await _tvBuilder.GetAlbumsAsTreeView(tvOptions);

            r.AlbumTreeDataJson = tv.ToJson();
            r.SelectedRootAlbumIds = rootAlbumIds.ToArray();

            r.Members = await GetUsersInRole(roleName);

            return r;
        }

        /// <summary>
        /// Gets a list of all the ASP.NET roles for the current application.
        /// </summary>
        /// <returns>A list of all the ASP.NET roles for the current application.</returns>
        public string[] GetAllRoles()
        {
            return _roleManager.Roles.Select(r => r.Name).ToArray();
            //return RoleGsp.GetAllRoles();
        }

        /// <summary>
        /// Adds a role to the data source for the current application. If the role already exists, no action is taken.
        /// </summary>
        /// <param name="roleName">Name of the role. Any leading or trailing spaces are removed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleName" /> is null.</exception>
        public async Task CreateRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            // GS 4.X used a lock around the following chunck, but that doesn't work in an async world. Instead, we use the technique from
            // https://msdn.microsoft.com/en-us/magazine/jj991977.aspx?f=255&MSPPError=-2147217396
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!await RoleExists(roleName))
                {
                    await _roleManager.CreateAsync(new GalleryRole() { Name = roleName.Trim() });
                    //RoleGsp.CreateRole(roleName.Trim());
                }

            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Removes a role from the data source for the current application.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleName" /> is null.</exception>
        private async Task DeleteRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            var role = await _roleManager.FindByNameAsync(roleName.Trim());

            await _roleManager.DeleteAsync(role);
            //RoleGsp.DeleteRole(roleName.Trim(), false);
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the data source for the current application.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <returns><c>true</c> if the role exists; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleName" /> is null.</exception>
        public async Task<bool> RoleExists(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            return await _roleManager.RoleExistsAsync(roleName.Trim());
            //return RoleGsp.RoleExists(roleName.Trim());
        }

        /// <overloads>Retrieve Gallery Server roles.</overloads>
        /// <summary>
        /// Retrieve Gallery Server roles matching the specified role names. The roles may be  returned from a cache.
        /// </summary>
        /// <param name="roleNames">The role names.</param>
        /// <returns>Returns all roles.</returns>
        public async Task<IGalleryServerRoleCollection> GetGalleryServerRoles(IEnumerable<string> roleNames)
        {
            // No roles in the cache, so get from business layer and add to cache.
            IGalleryServerRoleCollection roles;
            try
            {
                roles = Factory.LoadGalleryServerRoles(roleNames);
            }
            catch (InvalidGalleryServerRoleException)
            {
                // We could not find one or more GSP roles for the ASP.NET roles we passed to Factory.LoadGalleryServerRoles(). Things probably
                // got out of sync. For example, this can happen if an admin adds an ASP.NET role outside of GSP (such as when using the 
                // DNN control panel). Purge the cache, then run the validation routine, and try again. If the same exception is thrown again,
                // let it bubble up - there isn't anything more we can do.
                CacheController.RemoveCache(CacheItem.GalleryServerRoles);

                await ValidateRoles();

                roles = Factory.LoadGalleryServerRoles(roleNames);
            }

            return roles;
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
        public IGalleryServerRoleCollection GetGalleryServerRoles(bool includeOwnerRoles = true)
        {
            if (includeOwnerRoles)
            {
                return Factory.LoadGalleryServerRoles();
            }
            else
            {
                IGalleryServerRoleCollection roles = new GalleryServerRoleCollection();

                foreach (IGalleryServerRole role in Factory.LoadGalleryServerRoles())
                {
                    if (!IsRoleAnAlbumOwnerRole(role.RoleName))
                    {
                        roles.Add(role);
                    }
                }

                return roles;
            }
        }

        ///// <overloads>
        ///// Gets a collection of Gallery Server roles.
        ///// </overloads>
        ///// <summary>
        ///// Gets Gallery Server roles representing the roles for the currently logged-on user. Returns an empty collection if 
        ///// no user is logged in or the user is logged in but not assigned to any roles (Count = 0).
        ///// The roles may be returned from a cache. Guaranteed to not return null.
        ///// </summary>
        ///// <returns>
        ///// Returns an <see cref="IGalleryServerRoleCollection" /> representing the roles for the currently logged-on user.
        ///// </returns>
        //public async Task<IGalleryServerRoleCollection> GetGalleryServerRolesForUser()
        //{
        //    return await GetGalleryServerRolesForUser(Utils.UserName);
        //}
        
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
        public Task<IGalleryServerRoleCollection> GetGalleryServerRolesForUser()
        {
            //var userName = DiHelper.HttpContext.User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //return await GetGalleryServerRolesForUser(userName);
            //var roleNames = _httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            return GetGalleryServerRoles(_httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value));
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
            if (string.IsNullOrEmpty(userName))
                return new GalleryServerRoleCollection();

            //var user = await _userManager.FindByNameAsync(userName);
            var roleNames = _httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            return await GetGalleryServerRoles(roleNames);

            //// Get cached dictionary entry matching logged on user. If not found, retrieve from business layer and add to cache.
            //var rolesCache = CacheController.GetGalleryServerRolesCache();

            //IGalleryServerRoleCollection roles;
            //if ((rolesCache != null) && (rolesCache.TryGetValue(GetCacheKeyNameForRoles(userName), out roles)))
            //{
            //    return roles;
            //}

            //// No roles in the cache, so get from business layer and add to cache.
            //try
            //{
            //    roles = Factory.LoadGalleryServerRoles(await GetRolesForUser(userName));
            //}
            //catch (InvalidGalleryServerRoleException)
            //{
            //    // We could not find one or more GSP roles for the ASP.NET roles we passed to Factory.LoadGalleryServerRoles(). Things probably
            //    // got out of synch. For example, this can happen if an admin adds an ASP.NET role outside of GSP (such as when using the 
            //    // DNN control panel). Purge the cache, then run the validation routine, and try again. If the same exception is thrown again,
            //    // let it bubble up - there isn't anything more we can do.
            //    CacheController.RemoveCache(CacheItem.GalleryServerRoles);

            //    await ValidateRoles();

            //    roles = Factory.LoadGalleryServerRoles(await GetRolesForUser(userName));
            //}

            //if (rolesCache == null)
            //{
            //    // The factory method should have created a cache item, so try again.
            //    rolesCache = CacheController.GetGalleryServerRolesCache();
            //    if (rolesCache == null)
            //    {
            //        if (AppSetting.Instance.EnableCache)
            //        {
            //            AppEventController.LogError(new WebException("The method Factory.LoadGalleryServerRoles() should have created a cache entry, but none was found. This is not an issue if it occurs occasionally, but should be addressed if it is frequent."));
            //        }

            //        return roles;
            //    }
            //}

            //// Add to the cache, but only if we have access to the session ID.
            ////if (GalleryServer.Data.DiHelper.HttpContext != null && GalleryServer.Data.DiHelper.HttpContext.Session != null)
            ////{
            //lock (rolesCache)
            //{
            //    if (!rolesCache.ContainsKey(GetCacheKeyNameForRoles(userName)))
            //    {
            //        rolesCache.TryAdd(GetCacheKeyNameForRoles(userName), roles);
            //    }
            //}
            //CacheController.SetCache(CacheItem.GalleryServerRoles, rolesCache);
            ////}

            //return roles;
        }

        /// <summary>
        /// Gets all the gallery server roles that apply to the specified <paramref name="gallery" />.
        /// </summary>
        /// <param name="gallery">The gallery.</param>
        /// <returns>Returns an <see cref="IGalleryServerRoleCollection"/> representing the roles that apply to the specified 
        /// <paramref name="gallery" />.</returns>
        public IGalleryServerRoleCollection GetGalleryServerRolesForGallery(IGallery gallery)
        {
            IGalleryServerRoleCollection roles = new GalleryServerRoleCollection();

            foreach (IGalleryServerRole role in GetGalleryServerRoles())
            {
                if (role.Galleries.Contains(gallery) && (!roles.Contains(role)))
                {
                    roles.Add(role);
                }
            }

            return roles;
        }

        /// <summary>
        /// Gets a sorted list of roles the user has permission to view. Users who have administer site permission can view all roles.
        /// Users with administer gallery permission can only view roles they have been associated with or roles that aren't 
        /// associated with *any* gallery, unless the application setting <see cref="IAppSetting.AllowGalleryAdminToViewAllUsersAndRoles" />
        /// is true, in which case they can see all roles.
        /// </summary>
        /// <param name="userIsSiteAdmin">If set to <c>true</c>, the currently logged on user is a site administrator.</param>
        /// <param name="userIsGalleryAdmin">If set to <c>true</c>, the currently logged on user is a gallery administrator for the current gallery.</param>
        /// <returns>Returns an <see cref="List&lt;IGalleryServerRole&gt;" /> containing a list of roles the user has permission to view.</returns>
        public async Task<List<IGalleryServerRole>> GetRolesCurrentUserCanView(bool userIsSiteAdmin, bool userIsGalleryAdmin)
        {
            if (userIsSiteAdmin || (userIsGalleryAdmin && AppSetting.Instance.AllowGalleryAdminToViewAllUsersAndRoles))
            {
                return GetGalleryServerRoles().GetSortedList();
            }
            else if (userIsGalleryAdmin)
            {
                IGalleryServerRoleCollection roles = GetGalleryServerRoles();
                IGalleryServerRoleCollection filteredRoles = new GalleryServerRoleCollection();

                // Build up a list of roles where (1) the current user is a gallery admin for at least one gallery, 
                // (2) the role is an album owner template role and the current user is a gallery admin for its associated gallery, or
                // (3) the role isn't associated with any albums/galleries.
                foreach (IGalleryServerRole role in roles)
                {
                    if (role.Galleries.Count > 0)
                    {
                        if (await IsUserGalleryAdminForRole(role))
                        {
                            // Current user has gallery admin permissions for at least one galley associated with the role.
                            filteredRoles.Add(role);
                        }
                    }
                    else if (IsRoleAnAlbumOwnerTemplateRole(role.RoleName))
                    {
                        if (await IsUserGalleryAdminForAlbumOwnerTemplateRole(role))
                        {
                            // The role is an album owner template role and the current user is a gallery admin for it's associated gallery.
                            filteredRoles.Add(role);
                        }
                    }
                    else
                    {
                        // Role isn't an album owner role and it isn't assigned to any albums. Add it.
                        filteredRoles.Add(role);
                    }
                }

                return filteredRoles.GetSortedList();
            }
            else
            {
                return new List<IGalleryServerRole>();
            }
        }

        /// <summary>
        /// Create a Gallery Server role corresponding to the specified parameters. Also creates the corresponding ASP.NET role.
        /// Throws an exception if a role with the specified name already exists in the data store. The role is persisted to the data store.
        /// </summary>
        /// <param name="roleName">A string that uniquely identifies the role.</param>
        /// <param name="allowViewAlbumOrMediaObject">A value indicating whether the user assigned to this role has permission to view albums
        /// and media objects.</param>
        /// <param name="allowViewOriginalImage">A value indicating whether the user assigned to this role has permission to view the original,
        /// high resolution version of an image. This setting applies only to images. It has no effect if there are no
        /// high resolution images in the album or albums to which this role applies.</param>
        /// <param name="allowAddMediaObject">A value indicating whether the user assigned to this role has permission to add media objects to an album.</param>
        /// <param name="allowAddChildAlbum">A value indicating whether the user assigned to this role has permission to create child albums.</param>
        /// <param name="allowEditMediaObject">A value indicating whether the user assigned to this role has permission to edit a media object.</param>
        /// <param name="allowEditAlbum">A value indicating whether the user assigned to this role has permission to edit an album.</param>
        /// <param name="allowDeleteMediaObject">A value indicating whether the user assigned to this role has permission to delete media objects within an album.</param>
        /// <param name="allowDeleteChildAlbum">A value indicating whether the user assigned to this role has permission to delete child albums.</param>
        /// <param name="allowSynchronize">A value indicating whether the user assigned to this role has permission to synchronize an album.</param>
        /// <param name="allowAdministerSite">A value indicating whether the user has administrative permission for all albums. This permission
        /// automatically applies to all albums across all galleries; it cannot be selectively applied.</param>
        /// <param name="allowAdministerGallery">A value indicating whether the user has administrative permission for all albums. This permission
        /// automatically applies to all albums in a particular gallery; it cannot be selectively applied.</param>
        /// <param name="hideWatermark">A value indicating whether the user assigned to this role has a watermark applied to images.
        /// This setting has no effect if watermarks are not used. A true value means the user does not see the watermark;
        /// a false value means the watermark is applied.</param>
        /// <param name="topLevelCheckedAlbumIds">The top level checked album ids. May be null.</param>
        /// <returns>
        /// Returns an <see cref="IGalleryServerRole"/> object corresponding to the specified parameters.
        /// </returns>
        /// <exception cref="InvalidGalleryServerRoleException">Thrown when a role with the specified role name already exists in the data store.</exception>
        public async Task<IGalleryServerRole> CreateRole(string roleName, bool allowViewAlbumOrMediaObject, bool allowViewOriginalImage, bool allowAddMediaObject, bool allowAddChildAlbum, bool allowEditMediaObject, bool allowEditAlbum, bool allowDeleteMediaObject, bool allowDeleteChildAlbum, bool allowSynchronize, bool allowAdministerSite, bool allowAdministerGallery, bool hideWatermark, IIntegerCollection topLevelCheckedAlbumIds)
        {
            // GS 4.X used a lock around the following chunck, but that doesn't work in an async world. Instead, we use the technique from
            // https://msdn.microsoft.com/en-us/magazine/jj991977.aspx?f=255&MSPPError=-2147217396
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                // Create the ASP.NET role.
                await CreateRole(roleName);

                // Create the Gallery Server role that extends the functionality of the ASP.NET role.
                IGalleryServerRole role = Factory.CreateGalleryServerRoleInstance(roleName, allowViewAlbumOrMediaObject, allowViewOriginalImage, allowAddMediaObject, allowAddChildAlbum, allowEditMediaObject, allowEditAlbum, allowDeleteMediaObject, allowDeleteChildAlbum, allowSynchronize, allowAdministerSite, allowAdministerGallery, hideWatermark);

                UpdateRoleAlbumRelationships(role, topLevelCheckedAlbumIds);

                ValidateSaveRole(role);

                role.Save();

                return role;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Delete the specified role. Both components of the role are deleted: the IGalleryServerRole and ASP.NET role.
        /// </summary>
        /// <param name="roleName">Name of the role. Must match an existing <see cref="IGalleryServerRole.RoleName"/>. If no match
        /// if found, no action is taken.</param>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
        public void DeleteGalleryServerProRole(string roleName)
        {
            ValidateDeleteRole(roleName);

            try
            {
                DeleteGalleryServerRole(roleName);
            }
            finally
            {
                try
                {
                    DeleteAspnetRole(roleName);

                    RemoveMissingRolesFromDefaultRolesForUsersSettings();
                }
                finally
                {
                    CacheController.RemoveCache(CacheItem.GalleryServerRoles);
                    CacheController.RemoveCache(CacheItem.UsersCurrentUserCanView);
                }
            }
        }

        /// <summary>
        /// Throws an exception if the role cannot be deleted, such as when deleting the only role with Administer site permission
        /// or deleting a role that would lessen the logged-on users own level of administrative access.
        /// </summary>
        /// <param name="roleName">Name of the role to be deleted.</param>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
        public void ValidateDeleteRole(string roleName)
        {
            IGalleryServerRole roleToDelete = Factory.LoadGalleryServerRole(roleName);

            if (roleToDelete == null)
                return;

            // Test 1: Don't let user delete the only role with Administer site permission.
            ValidatePreventLastSysAdminRoleDeletion(roleToDelete);

            // Test 2: Don't let user delete a role with site admin or gallery admin permissions if that means the user will 
            // lose their own administrative access.
            ValidatePreventLoggedOnUserFromLosingAdminAccess(roleToDelete);

            // Test 3: User can delete role only if he is a site admin or a gallery admin in every gallery this role is associated with.
            ValidatePreventRoleDeletionAffectingOtherGalleries(roleToDelete);
        }

        /// <summary>
        /// Make sure the list of ASP.NET roles is synchronized with the Gallery Server roles. If any are missing from 
        /// either, add it. Also verify that users assigned to roles still exist (e.g. they might have been deleted in AD).
        /// </summary>
        public async Task ValidateRoles()
        {
            List<IGalleryServerRole> validatedRoles = new List<IGalleryServerRole>();
            IGalleryServerRoleCollection galleryRoles = Factory.LoadGalleryServerRoles();
            bool needToPurgeCache = false;

            foreach (string roleName in GetAllRoles())
            {
                IGalleryServerRole galleryRole = galleryRoles.GetRole(roleName);
                if (galleryRole == null)
                {
                    // This is an ASP.NET role that doesn't exist in our list of gallery server roles. Add it with minimum permissions
                    // applied to zero albums.
                    IGalleryServerRole newRole = Factory.CreateGalleryServerRoleInstance(roleName, false, false, false, false, false, false, false, false, false, false, false, false);
                    newRole.Save();
                    needToPurgeCache = true;
                }
                validatedRoles.Add(galleryRole);

                // Check that each user in this role still exists in the membership. The only known case where they might not is when
                // using AD and a user is deleted outside of GS. But we run for all membership providers for extra robustness.
                //var verifiedUsers = new HashSet<string>();
                //foreach (var user in await GetUsersInRole(roleName))
                //{
                //    if (!verifiedUsers.Contains(user.UserName) && _userManager. UserController.GetUser(user.UserName, false) == null)
                //    {
                //        await RemoveUserFromRole(user, roleName);
                //        needToPurgeCache = true;
                //    }

                //    verifiedUsers.Add(user.UserName);
                //}
            }

            // Now check to see if there are gallery roles that are not ASP.NET roles. Add if necessary.
            foreach (IGalleryServerRole galleryRole in galleryRoles)
            {
                if (!validatedRoles.Contains(galleryRole))
                {
                    // Need to create an ASP.NET role for this gallery role.
                    CreateRole(galleryRole.RoleName);
                    needToPurgeCache = true;
                }
            }

            if (needToPurgeCache)
            {
                CacheController.RemoveCache(CacheItem.GalleryServerRoles);
            }
        }

        /// <summary>
        /// Verify a role with AllowAdministerSite permission exists, creating it if necessary. Return the role name.
        /// </summary>
        /// <returns>A <see cref="System.String" />.</returns>
        public async Task<string> ValidateSysAdminRole()
        {
            // Create the Sys Admin role if needed. If it already exists, make sure it has AllowAdministerSite permission.
            if (!await RoleExists(GlobalConstants.SysAdminRoleName))
            {
                await CreateRole(GlobalConstants.SysAdminRoleName);
            }

            var role = Factory.LoadGalleryServerRole(GlobalConstants.SysAdminRoleName);
            if (role == null)
            {
                role = Factory.CreateGalleryServerRoleInstance(GlobalConstants.SysAdminRoleName, true, true, true, true, true, true, true, true, true, true, true, false);
            }
            else if (!role.AllowAdministerSite)
            {
                // Role already exists. Make sure it has Sys Admin permission.
                role.AllowAdministerSite = true;
            }

            foreach (var gallery in Factory.LoadGalleries())
            {
                var album = Factory.LoadRootAlbumInstance(gallery.GalleryId);
                if (!role.RootAlbumIds.Contains(album.Id))
                {
                    role.RootAlbumIds.Add(album.Id);
                }
            }

            role.Save();

            return role.RoleName;
        }

        /// <summary>
        /// Create a role named Authenticated Users if it does not exist, giving it view-only permission for all albums 
        /// in all galleries. Return the role name.
        /// </summary>
        /// <returns>A <see cref="System.String" />.</returns>
        public async Task<string> CreateAuthUsersRole()
        {
            if (!await RoleExists(GlobalConstants.AuthenticatedUsersRoleName))
            {
                await CreateRole(GlobalConstants.AuthenticatedUsersRoleName);
            }

            var role = Factory.LoadGalleryServerRole(GlobalConstants.AuthenticatedUsersRoleName);
            if (role == null)
            {
                role = Factory.CreateGalleryServerRoleInstance(GlobalConstants.AuthenticatedUsersRoleName, true, true, false, false, false, false, false, false, false, false, false, false);
            }

            foreach (var gallery in Factory.LoadGalleries())
            {
                var album = Factory.LoadRootAlbumInstance(gallery.GalleryId);
                if (!role.RootAlbumIds.Contains(album.Id))
                {
                    role.RootAlbumIds.Add(album.Id);
                }
            }

            role.Save();

            return role.RoleName;
        }

        /// <summary>
        /// Verify the roles stored in the <see cref="IGallerySettings.DefaultRolesForUser" /> property of every gallery exists. If not, remove them from the setting.
        /// </summary>
        public async Task RemoveMissingRolesFromDefaultRolesForUsersSettings()
        {
            // Loop through all galleries, including the template gallery.
            foreach (var galleryId in Factory.LoadGalleries().Select(g => g.GalleryId).Concat(new[] { Factory.GetTemplateGalleryId() }))
            {
                var gallerySettings = Factory.LoadGallerySetting(galleryId, true);

                // GS 4.X: var rolesToRemove = gallerySettings.DefaultRolesForUser.Where(roleName => !RoleExists(roleName)).ToArray();
                var rolesToRemove = new List<string>();
                foreach (var role in gallerySettings.DefaultRolesForUser)
                {
                    if (!await RoleExists(role))
                    {
                        rolesToRemove.Add(role);
                    }
                }

                if (rolesToRemove.Count > 0)
                {
                    // The setting references at least one role that doesn't exist. Remove these from the setting.
                    gallerySettings.DefaultRolesForUser = gallerySettings.DefaultRolesForUser.Where(r => !rolesToRemove.Contains(r)).ToArray();
                    gallerySettings.Save();
                }
            }
        }

        /// <summary>
        /// Verify that any role needed for album ownership exists and is properly configured. If an album owner
        /// is specified and the album is new (IsNew == true), the album is persisted to the data store. This is 
        /// required because the ID is not assigned until it is saved, and a valid ID is required to configure the
        /// role.
        /// </summary>
        /// <param name="album">The album to validate for album ownership. If a null value is passed, the function
        /// returns without error or taking any action.</param>
        public async Task ValidateRoleExistsForAlbumOwner(IAlbum album)
        {
            // For albums, verify that any needed roles for album ownership are present. Create/update as needed.
            if (album == null)
                return;

            if (string.IsNullOrEmpty(album.OwnerUserName))
            {
                // If owner role is specified, delete it.
                if (!string.IsNullOrEmpty(album.OwnerRoleName))
                {
                    DeleteGalleryServerProRole(album.OwnerRoleName);
                    album.OwnerRoleName = string.Empty;
                }
            }
            else
            {
                // If this is a new album, save it before proceeding. We will need its album ID to configure the role, 
                // and it is not assigned until it is saved.
                if (album.IsNew)
                    album.Save();

                // Verify that a role exists that corresponds to the owner.
                IGalleryServerRole role = Factory.LoadGalleryServerRoles().GetRole(album.OwnerRoleName);
                if (role == null)
                {
                    // No role exists. Create it.
                    album.OwnerRoleName = await CreateAlbumOwnerRole(album);
                }
                else
                {
                    // Role exists. Make sure album is assigned to role and owner is a member.
                    if (!role.RootAlbumIds.Contains(album.Id))
                    {
                        // Current album is not a member. This should not typically occur, but just in case
                        // it does let's add the current album to it and save it.
                        role.RootAlbumIds.Add(album.Id);
                        role.Save();
                    }

                    if (!(await _userManager.GetRolesAsync(await _userManager.FindByNameAsync(album.OwnerUserName))).Contains(role.RoleName))
                    {
                        // Owner is not a member. Add.
                        AddUserToRole(album.OwnerUserName, role.RoleName);
                    }
                    //if (Array.IndexOf(rolesForUser, role.RoleName) < 0)
                    //{
                    //    // Owner is not a member. Add.
                    //    AddUserToRole(album.OwnerUserName, role.RoleName);
                    //}
                }
            }
        }

        /// <summary>
        /// Determines whether the <paramref name="roleName"/> is a role that serves as an album owner role. Returns <c>true</c> if the
        /// <paramref name="roleName"/> starts with the same string as the global constant <see cref="GlobalConstants.AlbumOwnerRoleNamePrefix"/>.
        /// Album owner roles are roles that are programmatically created to provide the security context used for the album ownership
        /// and user album features.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <returns>
        /// 	<c>true</c> if <paramref name="roleName"/> is a role that serves as an album owner role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRoleAnAlbumOwnerRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return false;

            return roleName.Trim().StartsWith(GlobalConstants.AlbumOwnerRoleNamePrefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the <paramref name="roleName"/> is a role that serves as an album owner template role. Returns <c>true</c> if the
        /// <paramref name="roleName"/> matches a regular expression that defines the pattern for the template role name.
        /// Album owner roles are created from the album owner template role.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <returns>
        /// 	<c>true</c> if <paramref name="roleName"/> is a role that serves as an album owner template role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRoleAnAlbumOwnerTemplateRole(string roleName)
        {
            return _gspAlbumOwnerTemplateRoleNameRegEx.Match(roleName).Success;
        }

        ///// <summary>
        ///// Removes the roles belonging to the <paramref name="userName" /> from cache.
        ///// This function is not critical for security or correctness, but is useful in keeping the cache cleared of unused items. When
        ///// a user logs on or off, their username changes - and therefore the name of the cache item changes, which causes the next call to
        ///// retrieve the user's roles to return nothing from the cache, which forces a retrieval from the database. Thus the correct roles will
        ///// always be retrieved, even if this function is not invoked during a logon/logoff event.
        ///// </summary>
        ///// <param name="userName">Name of the user.</param>
        //public void RemoveRolesForUserFromCache(string userName)
        //{
        //    var rolesCache = CacheController.GetGalleryServerRolesCache();

        //    if (rolesCache != null)
        //    {
        //        IGalleryServerRoleCollection roles;
        //        rolesCache.TryRemove(GetCacheKeyNameForRoles(userName), out roles);
        //    }
        //}

        /// <summary>
        /// Parses the name of the role from the <paramref name="roleNames" />. Example: If role name = "Administrators_0", return
        /// "Administrators". This function works by using a regular expression to remove all text that matches the "_{GalleryID}"
        /// pattern. If the role name does not have this suffix, the original role name is returned. This function is useful when
        /// GSP is used in an application where the role provider allows multiple roles with the same name, such as DotNetNuke.
        /// The contents of this function is commented out in the trunk (stand-alone) version of GSP and enabled in branched versions
        /// where required (such as DotNetNuke).
        /// </summary>
        /// <param name="roleNames">Name of the roles.</param>
        /// <returns>Returns a copy of the <paramref name="roleNames" /> parameter with the "_{GalleryID}" portion removed from each 
        /// role name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleNames" /> is null.</exception>
        public string[] ParseRoleNameFromGspRoleNames(string[] roleNames)
        {
            if (roleNames == null)
                throw new ArgumentNullException(nameof(roleNames));

            string[] roleNamesCopy = new string[roleNames.Length];

            for (int i = 0; i < roleNames.Length; i++)
            {
                roleNamesCopy[i] = ParseRoleNameFromGspRoleName(roleNames[i]);
            }

            return roleNamesCopy;
        }

        /// <summary>
        /// Parses the name of the role from the <paramref name="roleName" />. Example: If role name = "Administrators_0", return
        /// "Administrators". This function works by using a regular expression to remove all text that matches the "_{GalleryID}"
        /// pattern. If the role name does not have this suffix, the original role name is returned. This function is useful when
        /// GSP is used in an application where the role provider allows multiple roles with the same name, such as DotNetNuke.
        /// The contents of this function is commented out in the trunk (stand-alone) version of GSP and enabled in branched versions
        /// where required (such as DotNetNuke).
        /// </summary>
        /// <param name="roleName">Name of the role. Example: "Administrators_0"</param>
        /// <returns>Returns the role name with the "_{GalleryID}" portion removed.</returns>
        public string ParseRoleNameFromGspRoleName(string roleName)
        {
            return roleName;
            //return _gspRoleNameSuffixRegEx.Replace(roleName, String.Empty); // DotNetNuke only
        }

        /// <summary>
        /// Gets the default roles for a user. They are generated from the <see cref="IGallerySettings.DefaultRolesForUser" /> properties of all
        /// non-template galleries.
        /// </summary>
        /// <returns>System.String[].</returns>
        public string[] GetDefaultRolesForUser()
        {
            return Factory.LoadGallerySettings().Where(gs => !gs.IsTemplate).SelectMany(gs => gs.DefaultRolesForUser).Distinct().ToArray();
        }


        /// <summary>
        /// Determine whether the currently logged-on user is a site administrator. The user is considered a site
        /// administrator if at least one role has Allow Administer Site permission.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if the user is a site administrator; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsCurrentUserSiteAdministrator()
        {
            return SecurityManager.IsUserSiteAdministrator(await GetGalleryServerRolesForUser());
        }

        /// <summary>
        /// Determine whether the currently logged-on user is a gallery administrator for the specified <paramref name="galleryId"/>. 
        /// The user is considered a gallery administrator if at least one role has Allow Administer Gallery permission.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>
        /// 	<c>true</c> if the user is a gallery administrator; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsCurrentUserGalleryAdministrator(int galleryId)
        {
            return SecurityManager.IsUserGalleryAdministrator(await GetGalleryServerRolesForUser(), galleryId);
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
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentOutOfRangeException(nameof(userName), "The parameter 'userName' cannot be null or an empty string.");

            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentOutOfRangeException(nameof(roleName), "The parameter 'roleName' cannot be null or an empty string.");

            var user = await _userManager.FindByNameAsync(userName.Trim());

            return await _userManager.IsInRoleAsync(user, roleName);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Make sure the loggod-on person has authority to save the role and that h/she isn't doing anything stupid, like removing
        /// Administer site permission from the only role that has it.
        /// </summary>
        /// <param name="roleToSave">The role to be saved.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleToSave"/> is null.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
        /// <exception cref="InvalidGalleryServerRoleException">Thrown when an existing role cannot be found in the database that matches the 
        /// role name of the <paramref name="roleToSave" /> parameter.</exception>
        private void ValidateSaveRole(IGalleryServerRole roleToSave)
        {
            #region Parameter Validation

            if (roleToSave == null)
                throw new ArgumentNullException(nameof(roleToSave));

            if (string.IsNullOrEmpty(roleToSave.RoleName))
                return; // Role name will be empty when adding a new one, so the validation below doesn't apply.

            IGalleryServerRole existingRole = Factory.LoadGalleryServerRole(roleToSave.RoleName) ?? roleToSave;

            #endregion

            ValidateCanRemoveSiteAdminPermission(roleToSave, existingRole);

            ValidateUserHasPermissionToSaveRole(roleToSave, existingRole);

            ValidateUserDoesNotLoseAbilityToAdminCurrentGallery(roleToSave, existingRole);
        }

        /// <summary>
        /// If administer site permission is being removed from the <paramref name="roleToSave" />, verify that this action does not violate
        /// business rules. Specifically, ensure that at least one other role has the same permission to prevent the user from removing their
        /// ability to administer the site. Throws a <see cref="GallerySecurityException" /> if the role should not be saved.
        /// </summary>
        /// <param name="roleToSave">The role to save. It's role name must match the role name of <paramref name="existingRole" />.</param>
        /// <param name="existingRole">The existing role, as it is stored in the database. It's role name must match the role name of
        /// <paramref name="roleToSave" />.</param>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
        private async Task ValidateCanRemoveSiteAdminPermission(IGalleryServerRole roleToSave, IGalleryServerRole existingRole)
        {
            if (!roleToSave.RoleName.Equals(existingRole.RoleName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, "The role name of the roleToSave and existingRole parameters must match, but they do not. roleToSave='{0}'; existingRole='{1}'", roleToSave, existingRole));
            }

            if (existingRole.AllowAdministerSite && !roleToSave.AllowAdministerSite)
            {
                // User is trying to remove administer site permission from this role. Make sure
                // at least one other role has this permission, and that the role has at least one member.
                bool atLeastOneOtherRoleHasAdminSitePermission = false;
                foreach (IGalleryServerRole role in GetGalleryServerRoles())
                {
                    if ((!role.RoleName.Equals(existingRole.RoleName, StringComparison.OrdinalIgnoreCase) && role.AllowAdministerSite))
                    {
                        if ((await GetUsersInRole(role.RoleName)).Any())
                        {
                            atLeastOneOtherRoleHasAdminSitePermission = true;
                            break;
                        }
                    }
                }

                if (!atLeastOneOtherRoleHasAdminSitePermission)
                {
                    throw new GallerySecurityException("You are attempting to remove Administer site permission, but doing so would leave Gallery Server without a user with administrative privileges. Make sure at least one user belongs to a role with Administer site permission, and then try again.");
                }
            }
        }

        /// <summary>
        /// Verify the user has permission to save the role.
        /// Specifically, the user is not allowed to add administer site permission or save any gallery she is not a gallery
        /// administrator for. It is up to the caller to verify that only site or gallery administrators call this function!
        /// </summary>
        /// <param name="roleToSave">The role to save. It's role name must match the role name of <paramref name="existingRole" />.</param>
        /// <param name="existingRole">The existing role, as it is stored in the database. It's role name must match the role name of
        /// <paramref name="roleToSave" />.</param>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleToSave" /> or <paramref name="existingRole" /> is null.</exception>
        private async Task ValidateUserHasPermissionToSaveRole(IGalleryServerRole roleToSave, IGalleryServerRole existingRole)
        {
            if (roleToSave == null)
                throw new ArgumentNullException(nameof(roleToSave));

            if (existingRole == null)
                throw new ArgumentNullException(nameof(existingRole));

            if (!roleToSave.RoleName.Equals(existingRole.RoleName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, "The role name of the roleToSave and existingRole parameters must match, but they do not. roleToSave='{0}'; existingRole='{1}'", roleToSave, existingRole));
            }

            var roles = await GetGalleryServerRolesForUser();

            if (!await IsCurrentUserSiteAdministrator())
            {
                await ValidateUserCanEditRole(roleToSave, existingRole, roles);
            }

            if (!SecurityManager.IsUserSiteAdministrator(roles))
            {
                // User is a gallery admin but not a site admin (we deduce this because ONLY site or gallery admins will get this 
                // far in the function. The user CANNOT save add AllowAdminSite permission.
                if (roleToSave.AllowAdministerSite)
                {
                    throw new GallerySecurityException("You must be a site administrator to save changes to a role with Administer site permission. Sadly, you are just a gallery administrator.");
                }
            }
        }

        /// <summary>
        /// Verify the the current user isn't jeopardizing their ability to administer the site or current gallery. Specifically, if
        /// the user is a member of the role being saved and admin site or gallery permissions are being removed from it, make sure
        /// the user is in at least one other role with similar permissions. Verifies only the current gallery: That is, it is possible
        /// for the user to remove their ability to administer another gallery.
        /// </summary>
        /// <param name="roleToSave">The role to save. It's role name must match the role name of <paramref name="existingRole" />.</param>
        /// <param name="existingRole">The existing role, as it is stored in the database. It's role name must match the role name of
        /// <paramref name="roleToSave" />.</param>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
        private async Task ValidateUserDoesNotLoseAbilityToAdminCurrentGallery(IGalleryServerRole roleToSave, IGalleryServerRole existingRole)
        {
            if (!roleToSave.RoleName.Equals(existingRole.RoleName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, "The role name of the roleToSave and existingRole parameters must match, but they do not. roleToSave='{0}'; existingRole='{1}'", roleToSave, existingRole));
            }

            if (await IsUserInRole(UserName, roleToSave.RoleName))
            {
                bool adminSitePermissionBeingRevoked = (!roleToSave.AllowAdministerSite && existingRole.AllowAdministerSite);
                bool adminGalleryPermissionBeingRevoked = (!roleToSave.AllowAdministerGallery && existingRole.AllowAdministerGallery);

                bool userHasAdminSitePermissionThroughAtLeastOneOtherRole = false;
                bool userHasAdminGalleryPermissionThroughAtLeastOneOtherRole = false;

                foreach (IGalleryServerRole roleForUser in await GetGalleryServerRolesForUser())
                {
                    if (!roleForUser.RoleName.Equals(roleToSave.RoleName))
                    {
                        if (roleForUser.AllowAdministerSite)
                        {
                            userHasAdminSitePermissionThroughAtLeastOneOtherRole = true;
                        }
                        if (roleForUser.AllowAdministerGallery)
                        {
                            userHasAdminGalleryPermissionThroughAtLeastOneOtherRole = true;
                        }
                    }
                }

                var msg = "Cannot remove requested roles. You are editing a role you are a member of, and you are attempting to remove your ability to administer this gallery. If you really want to do this, log on as another user and make the changes from that account, or make yourself a member of another role with administrative permission before modifying this role.";
                if (adminSitePermissionBeingRevoked && !userHasAdminSitePermissionThroughAtLeastOneOtherRole)
                {
                    throw new GallerySecurityException(msg);
                }

                if (adminGalleryPermissionBeingRevoked && !userHasAdminGalleryPermissionThroughAtLeastOneOtherRole)
                {
                    throw new GallerySecurityException(msg);
                }
            }
        }

        /// <summary>
        /// Don't let user delete the only role with Administer site permission. This should be called before a role is deleted as a validation step.
        /// </summary>
        /// <param name="roleToDelete">The role to be deleted.</param>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
        private async Task ValidatePreventLastSysAdminRoleDeletion(IGalleryServerRole roleToDelete)
        {
            if (roleToDelete.AllowAdministerSite)
            {
                // User is trying to delete a role with administer site permission. Make sure
                // at least one other role has this permission, and that the role has at least one member.
                bool atLeastOneOtherRoleHasAdminSitePermission = false;
                foreach (IGalleryServerRole role in Factory.LoadGalleryServerRoles())
                {
                    if ((!role.RoleName.Equals(roleToDelete.RoleName, StringComparison.OrdinalIgnoreCase) && role.AllowAdministerSite))
                    {
                        if ((await GetUsersInRole(role.RoleName)).Any())
                        {
                            atLeastOneOtherRoleHasAdminSitePermission = true;
                            break;
                        }
                    }
                }

                if (!atLeastOneOtherRoleHasAdminSitePermission)
                {
                    throw new GallerySecurityException("You are attempting to delete a role that has Administer site permission, but doing so would leave Gallery Server without a user with administrative privileges. Be sure at least one user belongs to a role with Administer site permission, and then try again.");
                }
            }
        }

        /// <summary>
        /// Don't let user delete a role with site admin or gallery admin permissions if that means the user will 
        /// lose their own administrative access. This should be called before a role is deleted as a validation step.
        /// </summary>
        /// <param name="roleToDelete">The role to be deleted.</param>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
        private async Task ValidatePreventLoggedOnUserFromLosingAdminAccess(IGalleryServerRole roleToDelete)
        {
            string roleName = roleToDelete.RoleName;

            if (roleToDelete.AllowAdministerSite || roleToDelete.AllowAdministerGallery)
            {
                bool needToVerify = false;
                IGalleryServerRoleCollection roles = await GetGalleryServerRolesForUser();
                foreach (IGalleryServerRole role in roles)
                {
                    if (role.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                    {
                        needToVerify = true;
                        break;
                    }
                }

                if (needToVerify)
                {
                    // User is deleting a role he is a member of. Make sure user is in at least one other role with the same type of access.
                    bool userIsInAnotherRoleWithAdminAccess = false;
                    if (roleToDelete.AllowAdministerSite)
                    {
                        foreach (IGalleryServerRole role in roles)
                        {
                            if (role.AllowAdministerSite && (!role.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
                            {
                                userIsInAnotherRoleWithAdminAccess = true;
                                break;
                            }
                        }
                    }
                    else if (roleToDelete.AllowAdministerGallery)
                    {
                        foreach (IGalleryServerRole role in roles)
                        {
                            if (role.AllowAdministerGallery && (!role.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
                            {
                                userIsInAnotherRoleWithAdminAccess = true;
                                break;
                            }
                        }
                    }

                    if (!userIsInAnotherRoleWithAdminAccess)
                    {
                        throw new GallerySecurityException("You are attempting to delete a role that would remove or restrict your level of administrative access. If you really want to do this, log on as another user and make the changes from that account.");
                    }
                }
            }
        }

        /// <summary>
        /// Don't let user delete a role that affects any gallery where the user is not a site admin or gallery admin. This should be called before 
        /// a role is deleted as a validation step. The only exception is that we allow a user to delete an album owner role, since that will typically
        /// be assigned to a single album, and we have logic elsewhere that verifies the user has permission to delete the album.
        /// </summary>
        /// <param name="roleToDelete">The role to be deleted.</param>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be deleted because doing so violates one of the business rules.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleToDelete" /> is null.</exception>
        private async Task ValidatePreventRoleDeletionAffectingOtherGalleries(IGalleryServerRole roleToDelete)
        {
            if (roleToDelete == null)
                throw new ArgumentNullException(nameof(roleToDelete));

            if (IsRoleAnAlbumOwnerRole(roleToDelete.RoleName))
            {
                return;
            }

            IGalleryCollection adminGalleries = await UserController.GetGalleriesCurrentUserCanAdminister();

            foreach (IGallery gallery in roleToDelete.Galleries)
            {
                if (!adminGalleries.Contains(gallery))
                {
                    throw new GallerySecurityException($"The role '{roleToDelete.RoleName}' has access to a gallery (Gallery '{gallery.Description}') that you are not an administrator for. To delete a role, one of the following must be true: (1) you are a site administrator, or (2) you are a gallery administrator in every gallery the role has access to.");
                }
            }
        }

        private async Task DeleteAspnetRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return;

            if (await RoleExists(roleName))
                await DeleteRole(roleName); // This also deletes any user/role relationships
        }

        private void DeleteGalleryServerRole(string roleName)
        {
            IGalleryServerRole role = Factory.LoadGalleryServerRole(roleName);

            if (role != null)
            {
                UpdateAlbumOwnerBeforeRoleDelete(role);
                role.Delete();
            }
        }

        /// <summary>
        /// For roles that provide album ownership functionality, remove users belonging to this role from the OwnedBy 
        /// property of any albums this role is assigned to. Since we are deleting the role that provides the ownership
        /// functionality, it is necessary to clear the owner field of all affected albums.
        /// </summary>
        /// <param name="role">Name of the role to be deleted.</param>
        private void UpdateAlbumOwnerBeforeRoleDelete(IGalleryServerRole role)
        {
            // Proceed only when dealing with an album ownership role.
            if (!IsRoleAnAlbumOwnerRole(role.RoleName))
                return;

            // Loop through each album assigned to this role. If this role is assigned as the owner role,
            // clear the OwnerUserName property.
            foreach (int albumId in role.RootAlbumIds)
            {
                // Load the album and clear the owner role name. Warning: Do not load the album with
                // AlbumController.LoadAlbumInstance(), as it will create a recursive loop, leading to a stack overflow
                IAlbum album = Factory.LoadAlbumInstance(new AlbumLoadOptions(albumId) { AllowMetadataLoading = false, IsWritable = true });
                if (album.OwnerRoleName == role.RoleName)
                {
                    album.OwnerUserName = string.Empty;
                    album.LastModifiedByUserName = UserController.UserName;
                    album.DateLastModified = DateTime.UtcNow;
                    album.Save();
                    //GalleryObjectController.SaveGalleryObject(album);
                }
            }
        }

        /// <summary>
        /// Creates the album owner role template. This is the role that is used as the template for roles that define
        /// a user's permission level when the user is assigned as an album owner. Call this method when the role does
        /// not exist. It is set up with all permissions except Administer Site and Administer Gallery. The HideWatermark 
        /// permission is not applied, so this role allows its members to view watermarks if that functionality is enabled.
        /// </summary>
        /// <param name="galleryId">The ID of the gallery for which the album owner template role is to belong.</param>
        /// <returns>
        /// Returns an <see cref="IGalleryServerRole"/> that can be used as a template for all album owner roles.
        /// </returns>
        /// <remarks>Note that we explicitly create the role in this method rather than call 
        /// <see cref="CreateRole(string, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, IIntegerCollection)" />
        /// to avoid the validation that exists in that method. There may be times when an anonymous user is creating an account
        /// having a user album. If an album template role doesn't exist, this method will create the necessary template role.</remarks>
        private async Task<IGalleryServerRole> CreateAlbumOwnerRoleTemplate(int galleryId)
        {
            // GS 4.X used a lock around the following chunck, but that doesn't work in an async world. Instead, we use the technique from
            // https://msdn.microsoft.com/en-us/magazine/jj991977.aspx?f=255&MSPPError=-2147217396
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var roleName = GetAlbumOwnerTemplateRoleName(galleryId);

                // Create the ASP.NET role.
                await CreateRole(roleName);

                // Create the Gallery Server role that extends the functionality of the ASP.NET role.
                var role = Factory.CreateGalleryServerRoleInstance(roleName, true, true, true, true, true, true, true, true, true, false, false, false);

                role.Save();

                return role;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Validates the album owner. If an album is being removed from the <paramref name="roleName"/> and that album is
        /// using this role for album ownership, remove the ownership setting from the album.
        /// </summary>
        /// <param name="roleName">Name of the role that is being modified.</param>
        /// <param name="rootAlbumIdsOld">The list of album ID's that were previously assigned to the role. If an album ID exists
        /// in this object and not in <paramref name="rootAlbumIdsNew"/>, that means the album is being removed from the role.</param>
        /// <param name="rootAlbumIdsNew">The list of album ID's that are now assigned to the role. If an album ID exists
        /// in this object and not in <paramref name="rootAlbumIdsOld"/>, that means it is a newly added album.</param>
        private void ValidateAlbumOwnerRoles(string roleName, IEnumerable<int> rootAlbumIdsOld, ICollection<int> rootAlbumIdsNew)
        {
            foreach (int albumId in rootAlbumIdsOld)
            {
                if (!rootAlbumIdsNew.Contains(albumId))
                {
                    // Album has been removed from role. Remove owner from the album if the album owner role matches the one we are dealing with.
                    IAlbum album = Factory.LoadAlbumInstance(new AlbumLoadOptions(albumId) { IsWritable = true });
                    if (album.OwnerRoleName == roleName)
                    {
                        album.OwnerUserName = string.Empty;
                        album.LastModifiedByUserName = UserController.UserName;
                        album.DateLastModified = DateTime.UtcNow;
                        album.Save();
                        //GalleryObjectController.SaveGalleryObject(album);
                    }
                }
            }
        }

        /// <summary>
        /// Create a role to manage the ownership permissions for the <paramref name="album"/> and user specified in the OwnerUserName
        /// property of the album. The permissions of the new role are copied from the album owner role template. The new role
        /// is persisted to the data store and the user specified as the album owner is added as its sole member. The album is updated
        /// so that the OwnerRoleName property contains the role's name, but the album is not persisted to the data store.
        /// </summary>
        /// <param name="album">The album for which a role to represent owner permissions is to be created.</param>
        /// <returns>Returns the name of the role that is created.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="album" /> is new and has not yet been persisted to the data store.</exception>
        private async Task<string> CreateAlbumOwnerRole(IAlbum album)
        {
            // Create a role modeled after the template owner role, attach it to the album, then add the specified user as its member.
            // Role name: Album Owner - rdmartin - rdmartin's album (album 193)
            if (album == null)
                throw new ArgumentNullException(nameof(album));

            if (album.IsNew)
                throw new ArgumentException("Album must be persisted to data store before calling RoleController.CreateAlbumOwnerRole.");

            string roleName = GenerateAlbumOwnerRoleName(album);

            if (!await RoleExists(roleName))
                await CreateRole(roleName);

            if (!await UserController.IsUserInRole(album.OwnerUserName, roleName))
                await AddUserToRole(album.OwnerUserName, roleName);

            // Remove the roles from the cache. We do this because may may have just created a user album (that is, 
            // AlbumController.CreateUserAlbum() is in the call stack) and we want to make sure the AllAlbumIds property
            // of the album owner template role has the latest list of albums, including potentially the new album 
            // (which will be the case if the administrator has selected a parent album of the user album in the template
            // role).
            CacheController.RemoveCache(CacheItem.GalleryServerRoles);

            IGalleryServerRole role = Factory.LoadGalleryServerRole(roleName);
            if (role == null)
            {
                IGalleryServerRole roleSource = Factory.LoadGalleryServerRole(GetAlbumOwnerTemplateRoleName(album.GalleryId));

                if (roleSource == null)
                    roleSource = await CreateAlbumOwnerRoleTemplate(album.GalleryId);

                role = roleSource.Copy();
                role.RoleName = roleName;
            }

            if (!role.AllAlbumIds.Contains(album.Id))
                role.RootAlbumIds.Add(album.Id);

            role.Save();

            return roleName;
        }

        /// <summary>
        /// Generates the name of the album owner role. Some gymnastics are performed to ensure the length of the role name is less than the 
        /// maximum allowed.
        /// </summary>
        /// <param name="album">The album for which an album owner role is to be created.</param>
        /// <returns>Returns a role name whose length is less than or equal to a value defined in the function.</returns>
        private static string GenerateAlbumOwnerRoleName(IAlbum album)
        {
            const int maxRoleNameLength = 256;
            const int minAlbumTitleLength = 10;
            const string ellipse = "...";

            string roleNameTemplate = string.Format(CultureInfo.InvariantCulture, "{0} - {{UserName}} - {{AlbumTitle}} (album {1})", GlobalConstants.AlbumOwnerRoleNamePrefix, album.Id);

            var albumTitle = album.Title.Replace(",", string.Empty); // Commas are not allowed in a role name

            string roleName = roleNameTemplate.Replace("{UserName}", album.OwnerUserName).Replace("{AlbumTitle}", albumTitle);

            if (roleName.Length > maxRoleNameLength)
            {
                // Role name is too long. Trim the album title and/or user name.
                string newAlbumTitle = albumTitle;
                string newUserName = album.OwnerUserName;
                int numCharsToTrim = roleName.Length - maxRoleNameLength;
                int numCharsTrimmed = 0;

                if ((albumTitle.Length - numCharsToTrim) >= minAlbumTitleLength)
                {
                    // We can do all the trimming we need by shortening the album title.
                    newAlbumTitle = string.Concat(albumTitle.Substring(0, albumTitle.Length - numCharsToTrim - ellipse.Length), ellipse);
                    numCharsTrimmed = numCharsToTrim;
                }
                else
                {
                    // Trim max chars from album title while leaving minAlbumTitleLength chars left. We'll have to trim the username to 
                    // get as short as we need.
                    try
                    {
                        newAlbumTitle = string.Concat(albumTitle.Substring(0, minAlbumTitleLength - ellipse.Length), ellipse);
                        numCharsTrimmed = albumTitle.Length - newAlbumTitle.Length;
                    }
                    catch (ArgumentOutOfRangeException) { }
                }

                if (numCharsTrimmed < numCharsToTrim)
                {
                    // We still need to shorten things up. Trim the user name.
                    numCharsToTrim = numCharsToTrim - numCharsTrimmed;
                    if (album.OwnerUserName.Length > numCharsToTrim)
                    {
                        newUserName = string.Concat(album.OwnerUserName.Substring(0, album.OwnerUserName.Length - numCharsToTrim - ellipse.Length), ellipse);
                    }
                    else
                    {
                        // It is not expected we ever get to this path.
                        throw new WebException(string.Format(CultureInfo.CurrentCulture, "Invalid role name length. Unable to shorten the album owner role name enough to satisfy maximum length restriction. Proposed name='{0}' (length={1}); Max length={2}", roleName, roleName.Length, maxRoleNameLength));
                    }
                }

                roleName = roleNameTemplate.Replace("{UserName}", newUserName).Replace("{AlbumTitle}", newAlbumTitle);

                // Perform one last final check to ensure we shortened things up correctly.
                if (roleName.Length > maxRoleNameLength)
                {
                    throw new WebException(string.Format(CultureInfo.CurrentCulture, "Unable to shorten the album owner role name enough to satisfy maximum length restriction. Proposed name='{0}' (length={1}); Max length={2}", roleName, roleName.Length, maxRoleNameLength));
                }
            }

            return roleName;
        }

        /// <summary>
        /// Gets the name of the album owner template role. Example: "_Album Owner Template (Gallery ID 2: 'Engineering')"
        /// </summary>
        /// <param name="galleryId">The ID of the gallery to which the album owner template role is to belong.</param>
        /// <returns>Returns the name of the album owner template role.</returns>
        private string GetAlbumOwnerTemplateRoleName(int galleryId)
        {
            string galleryDescription = Factory.LoadGallery(galleryId).Description;

            if (galleryDescription.Length > 100)
            {
                // Too long - shorten up... (role name can be only 256 chars)
                galleryDescription = string.Concat(galleryDescription.Substring(0, 100), "...");
            }

            // Note: If you change this, be sure to update _gspAlbumOwnerTemplateRoleNameRegExPattern to that it will match!
            return string.Format(CultureInfo.InvariantCulture, "{0} (Gallery ID {1}: '{2}')", GlobalConstants.AlbumOwnerRoleTemplateName, galleryId, galleryDescription);
        }

        private string GetCacheKeyNameForRoles(string userName)
        {
            return string.Concat(userName, "_Roles");
        }

        /// <summary>
        /// Verify data integrity after removing a user from one or more roles. Specifically, if a role is an album owner role, 
        /// then check all albums in that role to see if current user is an owner for any. If he is, clear out the ownership field.
        /// </summary>
        /// <param name="userName">Name of the user who was removed from one or more roles.</param>
        /// <param name="roleNames">The names of the roles the user were removed from.</param>
        private void ValidateRemoveUserFromRole(string userName, IEnumerable<string> roleNames)
        {
            if (string.IsNullOrEmpty(userName))
                return;

            if (roleNames == null)
                return;

            foreach (string roleName in roleNames)
            {
                if (IsRoleAnAlbumOwnerRole(roleName))
                {
                    IGalleryServerRole role = Factory.LoadGalleryServerRole(roleName);

                    if (role == null)
                    {
                        // Normally shouldn't be null, but might be if role has been deleted outside GSP.
                        continue;
                    }

                    foreach (int albumId in role.RootAlbumIds)
                    {
                        IAlbum album = Factory.LoadAlbumInstance(new AlbumLoadOptions(albumId) { IsWritable = true });
                        if (album.OwnerUserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        {
                            album.OwnerUserName = string.Empty;
                            album.LastModifiedByUserName = UserName;
                            album.DateLastModified = DateTime.UtcNow;
                            album.Save();
                            //GalleryObjectController.SaveGalleryObject(album);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replace the list of root album IDs for the <paramref name="role"/> with the album ID's specified in
        /// <paramref name="topLevelCheckedAlbumIds"/>. Note that this function will cause the <see cref="IGalleryServerRole.AllAlbumIds" />
        /// property to be cleared out (Count = 0). The property can be repopulated by calling <see cref="IGalleryServerRole.Inflate"/>.
        /// </summary>
        /// <param name="role">The role whose root album/role relationships should be updated. When editing
        /// an existing role, specify this.GalleryRole. For new roles, pass the newly created role before
        /// saving it.</param>
        /// <param name="topLevelCheckedAlbumIds">The top level album IDs. May be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="role" /> is null.</exception>
        private void UpdateRoleAlbumRelationships(IGalleryServerRole role, IIntegerCollection topLevelCheckedAlbumIds)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (topLevelCheckedAlbumIds == null)
                topLevelCheckedAlbumIds = new IntegerCollection();

            int[] rootAlbumIdsOld = new int[role.RootAlbumIds.Count];
            role.RootAlbumIds.CopyTo(rootAlbumIdsOld, 0);

            role.RootAlbumIds.Clear();

            if (role.AllowAdministerSite)
            {
                // Administer site permission automatically applies to all albums, so all we need to do is get
                // a reference to the root album ID in each gallery.
                foreach (IGallery gallery in role.Galleries)
                {
                    role.RootAlbumIds.Add(Factory.LoadRootAlbumInstance(gallery.GalleryId).Id);
                }
            }
            else if (role.AllowAdministerGallery)
            {
                // Administer gallery permission automatically applies to all albums in a gallery, so get a reference
                // to the root album for each checked album ID.
                foreach (int albumId in topLevelCheckedAlbumIds)
                {
                    IAlbum album = Factory.LoadAlbumInstance(albumId);

                    while (!(album.Parent is Business.NullObjects.NullGalleryObject))
                    {
                        album = (IAlbum)album.Parent;
                    }

                    if (!role.RootAlbumIds.Contains(album.Id))
                    {
                        role.RootAlbumIds.Add(album.Id);
                    }
                }
            }
            else
            {
                role.RootAlbumIds.AddRange(topLevelCheckedAlbumIds);
            }

            if (IsRoleAnAlbumOwnerRole(role.RoleName))
                ValidateAlbumOwnerRoles(role.RoleName, rootAlbumIdsOld, role.RootAlbumIds);
        }

        /// <summary>
        /// Determines whether the user has permission to view the specified role. Determines this by checking
        /// whether the logged on user is a site administrator or a gallery administrator for at least
        /// one gallery associated with the role. If the role is not assigned to any albums, it verifies the user is 
        /// a gallery admin to at least one gallery (doesn't matter which one).
        /// </summary>
        /// <param name="role">The role to evaluate.</param>
        /// <returns><c>true</c> if the user has permission to edit the specified role; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="role" /> is null.</exception>
        private async Task<bool> UserCanViewRole(IGalleryServerRole role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (await IsCurrentUserSiteAdministrator())
            {
                return true;
            }

            if (role.Galleries.Count == 0)
            {
                // The role isn't assigned to any albums, so let's make sure the user is a gallery admin to at
                // least one gallery.
                return (await UserController.GetGalleriesCurrentUserCanAdminister()).Any();
            }
            else
            {
                return await IsUserGalleryAdminForRole(role);
            }
        }

        /// <summary>
        /// Determines whether the user has permission to edit the specified role. Determines this by checking
        /// whether the logged on user is a site administrator or a gallery administrator for every
        /// gallery associated with the role. If the role is not assigned to any albums, it verifies the user is
        /// a gallery admin to at least one gallery (doesn't matter which one).
        /// </summary>
        /// <param name="roleToSave">The role to save. It's role name must match the role name of <paramref name="existingRole" />.</param>
        /// <param name="existingRole">The existing role, as it is stored in the database. It's role name must match the role name of
        /// <paramref name="roleToSave" />.</param>
        /// <param name="rolesForCurrentUser">The roles for current user.</param>
        /// <exception cref="GallerySecurityException">Thrown when the role cannot be saved because doing so would violate a business rule.</exception>
        private async Task ValidateUserCanEditRole(IGalleryServerRole roleToSave, IGalleryServerRole existingRole, IGalleryServerRoleCollection rolesForCurrentUser)
        {
            if (roleToSave.Galleries.Count == 0)
            {
                // The role isn't assigned to any albums, so let's make sure the user is a gallery admin to at
                // least one gallery.
                if (!(await UserController.GetGalleriesCurrentUserCanAdminister()).Any())
                {
                    throw new GallerySecurityException("Your account does not have permission to make changes to roles.");
                }
            }

            if (existingRole.Galleries.Any(gallery => !UserController.IsUserGalleryAdministrator(rolesForCurrentUser, gallery.GalleryId)))
            {
                throw new GallerySecurityException("You are attempting to save changes to a role that affects at least one gallery you do not have permission to administer. To edit this role, you must be one of the following : (1) a site administrator, or (2) a gallery administrator in every gallery this role applies to.");
            }
        }

        /// <summary>
        /// Determines whether the logged on user has gallery admin permissions for at least one gallery associated with the
        /// <paramref name="role" />.
        /// </summary>
        /// <param name="role">The role to evaluate.</param>
        /// <returns>
        /// 	<c>true</c> if the logged on user has gallery admin permissions for at least one galley associated with the
        /// <paramref name="role" />; otherwise, <c>false</c>.
        /// </returns>
        private async Task<bool> IsUserGalleryAdminForRole(IGalleryServerRole role)
        {
            foreach (IGallery gallery in role.Galleries)
            {
                if (await IsCurrentUserGalleryAdministrator(gallery.GalleryId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the logged on user is a gallery admin for the album owner template role specified in <paramref name="role" />.
        /// This is done by verifying that the gallery ID specified in the role's name is for a gallery the user can administer.
        /// Returns <c>true</c> when the role is an album owner template role and the current user is a gallery admin for it's
        /// associated gallery; otherwise returns <c>false</c>.
        /// </summary>
        /// <param name="role">The role to evaluate. It is expected that the role is an album owner template role, but this is
        /// not a requirement (function always returns false for non-template roles).</param>
        /// <returns>
        /// 	Returns <c>true</c> when the role is an album owner template role and the current user is a gallery admin for it's
        /// associated gallery; otherwise returns <c>false</c>.
        /// </returns>
        private async Task<bool> IsUserGalleryAdminForAlbumOwnerTemplateRole(IGalleryServerRole role)
        {
            System.Text.RegularExpressions.Match match = _gspAlbumOwnerTemplateRoleNameRegEx.Match(role.RoleName);
            if (match.Success)
            {
                // Parse out the gallery ID from the role name. Ex: "_Album Owner Template (Gallery ID 723: My gallery)" yields "723"
                int galleryId = Convert.ToInt32(match.Groups["galleryId"].Value, CultureInfo.InvariantCulture);

                IGallery gallery = null;
                try
                {
                    gallery = Factory.LoadGallery(galleryId);
                }
                catch (InvalidGalleryException) { }

                if ((gallery != null) && (await UserController.GetGalleriesCurrentUserCanAdminister()).Contains(gallery))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<IList<GalleryUser>> GetUsersInRole(string roleName)
        {
            return string.IsNullOrEmpty(roleName) ? new GalleryUser[] { } : await _userManager.GetUsersInRoleAsync(roleName);
        }

        #endregion
    }

    internal class RoleProvider
    {
        public void AddUsersToRoles(string[] strings, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public void RemoveUsersFromRoles(string[] strings, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        internal void CreateRole(string v)
        {
            throw new NotImplementedException();
        }

        internal void DeleteRole(string v1, bool v2)
        {
            throw new NotImplementedException();
        }

        internal string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        internal string[] GetRolesForUser(string v)
        {
            throw new NotImplementedException();
        }

        internal string[] GetUsersInRole(string v)
        {
            throw new NotImplementedException();
        }

        internal bool IsUserInRole(string v1, string v2)
        {
            throw new NotImplementedException();
        }

        internal bool RoleExists(string v)
        {
            throw new NotImplementedException();
        }
    }
}
