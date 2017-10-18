using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Web.Controller
{
    public static class AppController
    {
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
        private static bool _isInitialized;
        private static UserController _userController;

        /// <summary>
        /// Gets a value indicating whether the Gallery Server code has been initializaed.
        /// The code is initialized by calling <see cref="InitializeGspApplication" />.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the code is initialized; otherwise, <c>false</c>.
        /// </value>
        public static bool IsInitialized
        {
            get { return _isInitialized; }
        }

        /// <summary>
        /// Initialize the Gallery Server application. This method is designed to be run at application startup. The business layer
        /// is initialized with the current trust level and a few configuration settings. The business layer also initializes
        /// the data store, including verifying a minimal level of data integrity, such as at least one record for the root album.
        /// Initialization that requires an HttpContext is also performed. When this method completes, <see cref="IAppSetting.IsInitialized" />
        /// will be <c>true</c>, but <see cref="GalleryController.IsInitialized" /> will be <c>true</c> only when an HttpContext instance
        /// exists. If this function is initially called from a place where an HttpContext doesn't exist, it will automatically be called 
        /// again later, eventually being called from a place where an HttpContext does exist, thus completing app initialization.
        /// </summary>
        public static async Task InitializeGspApplication(UserController userController)
        {
            _userController = userController;
            //try
            //{
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (IsInitialized)
                    return;

                InitializeApplication();

                await AddMembershipDataToGallerySettings();

                _isInitialized = true;

                AppEventController.LogEvent("Application has started.");
            }
            finally
            {
                _lock.Release();
            }
            //}
            //catch (ThreadAbortException)
            //{
            //}
            //catch (CannotWriteToDirectoryException ex)
            //{
            //    // Let the error handler log it and try to redirect to a dedicated page for this error. The transfer will fail when the error occurs
            //    // during the app's init event, so when this happens don't re-throw (like we do in the generic catch below). This will allow the
            //    // initialize routine to run again from the GalleryPage constructor, and when the error happens again, this time the handler will be able to redirect.
            //    AppEventController.HandleGalleryException(ex);
            //    //throw; // Don't re-throw
            //}
            //catch (Exception ex)
            //{
            //    // Let the error handler deal with it. It will decide whether to transfer the user to a friendly error page.
            //    // If the function returns, that means it didn't redirect, so we should re-throw the exception.
            //    AppEventController.HandleGalleryException(ex);
            //    throw;
            //}
        }

        /// <summary>
        /// Initialize the components of the Gallery Server application that do not require access to an HttpContext.
        /// This method is designed to be run at application startup. The business layer
        /// is initialized with the current trust level and a few configuration settings. The business layer also initializes
        /// the data store, including verifying a minimal level of data integrity, such as at least one record for the root album.
        /// </summary>
        /// <remarks>This is the only method, apart from those invoked through web services, that is not handled by the global error
        /// handling routine in Gallery.cs. This method wraps its calls in a try..catch that passes any exceptions to
        /// <see cref="AppEventController.HandleGalleryException(Exception, int?)"/>. If that method does not transfer the user to a friendly error page, the exception
        /// is re-thrown.</remarks>
        private static void InitializeApplication()
        {
            //string msg = CheckForDbCompactionRequest();

            GallerySettings.GallerySettingsSaved += new EventHandler<GallerySettingsEventArgs>(GallerySettingsSaved);

            // Set web-related variables in the business layer and initialize the data store.
            InitializeBusinessLayer();

            //AppSetting.Instance.InstallationRequested = Utils.InstallRequested;

            // Make sure installation has its own unique encryption key.
            //ValidateEncryptionKey();

            //MediaConversionQueue.Instance.Process();

            //ValidateActiveDirectoryRequirements();

            //// If there is a message from the DB compaction, record it now. We couldn't do it before because the DB
            //// wasn't fully initialized.
            //if (!String.IsNullOrEmpty(msg))
            //  AppEventController.LogEvent(msg);
        }

        /// <summary>
        /// Set up the business layer with information about this web application, such as its trust level and a few settings
        /// from the configuration file.
        /// </summary>
        /// <exception cref="CannotWriteToDirectoryException">
        /// Thrown when Gallery Server is unable to write to, or delete from, the media objects directory.</exception>
        private static void InitializeBusinessLayer()
        {
            // Determine the trust level this web application is running in and set to a global variable. This will be used 
            // throughout the application to gracefully degrade when we are not at Full trust.
            //ApplicationTrustLevel trustLevel = Utils.GetCurrentTrustLevel();
            ApplicationTrustLevel trustLevel = ApplicationTrustLevel.Full;

            // Get the application path so that the business layer (and any dependent layers) has access to it. Don't use 
            // HttpContext.Current.Request.PhysicalApplicationPath because in some cases HttpContext.Current won't be available
            // (for example, when the DotNetNuke search engine indexer causes this code to trigger).
            string physicalApplicationPath = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 1);
            physicalApplicationPath = physicalApplicationPath.Replace("/", "\\");

            // Pass these values to our global app settings instance, where the values can be used throughout the application.
            AppSetting.Instance.Initialize(trustLevel, physicalApplicationPath, Constants.APP_NAME, "/");

            //Business.Entity.VersionKey.GenerateEncryptedVersionKeys();
        }

        /// <summary>
        /// Adds the user account information to gallery settings. Since the business layer does not have a reference to System.Web.dll,
        /// it could not load membership data when the gallery settings were first initialized. We know that information now, so let's
        /// populate the user accounts with the user data.
        /// </summary>
        private static async Task AddMembershipDataToGallerySettings()
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

        /// <summary>
        /// Handles the <see cref="GallerySettings.GallerySettingsSaved" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static async void GallerySettingsSaved(object sender, GallerySettingsEventArgs e)
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

    }
}
