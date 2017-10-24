using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Web.Controller
{
    public class GallerySettingController
    {
        private readonly UserController _userController;

        public GallerySettingController(UserController userController)
        {
            _userController = userController;
        }

        /// <summary>
        /// Handles the <see cref="GallerySettings.GallerySettingsSaved" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public async void GallerySettingsSaved(object sender, GallerySettingsEventArgs e)
        {
            // Finish populating those properties that weren't populated in the business layer.
            await AddMembershipDataToGallerySettings();

            // If the default roles setting has changed, add or remove users to/from roles on a background thread.
            if ((e.DefaultRolesForUserAdded != null && e.DefaultRolesForUserAdded.Length > 0) || (e.DefaultRolesForUserRemoved != null && e.DefaultRolesForUserRemoved.Length > 0))
            {
                //System.Threading.Tasks.Task.Factory.StartNew(() =>
                // {
                //     try
                //     {
                // For each added role, find the users *NOT* in the role and add them to the role
                var allUsers = _userController.GetAllUsers();
                foreach (var roleName in e.DefaultRolesForUserAdded)
                {
                    if (await _userController.RoleExists(roleName))
                    {
                        var usersInRole = (await _userController.GetUsersInRole(roleName)).Select(u => u.UserName);
                        foreach (var userName in allUsers.Select(u => u.UserName).Except(usersInRole))
                        {
                            await _userController.AddUserToRole(userName, roleName);
                        }

                        //RoleController.AddUsersToRole(allUsers.Select(u => u.UserName).Except(RoleController.GetUsersInRole(roleName)).ToArray(), roleName);
                    }
                }

                // For each removed role, find the users in the role and remove them from the role
                foreach (var roleName in e.DefaultRolesForUserRemoved)
                {
                    if (await _userController.RoleExists(roleName))
                    {
                        foreach (var user in await _userController.GetUsersInRole(roleName))
                        {
                            await _userController.RemoveUserFromRole(user, roleName);
                        }
                        //RoleController.RemoveUsersFromRole(RoleController.GetUsersInRole(roleName), roleName);
                    }
                }

                CacheController.RemoveCache(CacheItem.GalleryServerRoles);
                //    }
                //    catch (Exception ex)
                //    {
                //        AppEventController.LogError(ex, e.GalleryId);
                //    }
                //});
            }
        }

        /// <summary>
        /// Adds the user account information to gallery settings. Since the business layer does not have a reference to System.Web.dll,
        /// it could not load membership data when the gallery settings were first initialized. We know that information now, so let's
        /// populate the user accounts with the user data.
        /// </summary>
        public async Task AddMembershipDataToGallerySettings()
        {
            // The UserAccount objects should have been created and initially populated with the UserName property,
            // so we'll use the user name to retrieve the user's info and populate the rest of the properties on each object.
            foreach (IGallery gallery in Factory.LoadGalleries())
            {
                IGallerySettings gallerySetting = Factory.LoadGallerySetting(gallery.GalleryId);

                // Populate user account objects with membership data
                foreach (IUserAccount userAccount in gallerySetting.UsersToNotifyWhenAccountIsCreated)
                {
                    await _userController.LoadUser(userAccount);
                }

                foreach (IUserAccount userAccount in gallerySetting.UsersToNotifyWhenErrorOccurs)
                {
                    await _userController.LoadUser(userAccount);
                }
            }
        }
    }
}
