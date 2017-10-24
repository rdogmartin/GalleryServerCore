using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Web.Controller;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GalleryServer.Web.Api
{
    //[Route("api/[controller]")]
    //[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)] // 
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)
    public class EventsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ExceptionController _exController;
        //private SignInManager<GalleryUser> _signInMgr;
        //private GalleryRoleManager _roleManager;

        //private readonly GalleryDb _ctx;
        //private IMemoryCache _cache;

        public EventsController(ExceptionController exController)
        {
            _exController = exController;
        }

        //[HttpGet("{id:int}")]
        //public async Task<EventDto> Get(int id)
        //{
        //    return await _ctx.Events.SingleOrDefaultAsync(e => e.EventId == id);
        //}

        /// <summary>
        /// Gets an HTML formatted string representing the specified event <paramref name="id" />.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>An instance of <see cref="IActionResult" />.</returns>
        [HttpGet]
        [Authorize(Policy = GlobalConstants.PolicyAdministrator)]
        public IActionResult Get(int id)
        {
            // GET /api/events/get/12
            IEvent appEvent = null;
            try
            {
                appEvent = Factory.GetAppEvents().FindById(id);

                if (appEvent == null)
                {

                    return NotFound($"Event {id} does not exist.");
                }

                //// If the event has a non-template gallery ID (not all do), then check the user's permission. For those errors without a gallery ID,
                //// just assume the user has permission, because there is no way to verify the user can view this event. We could do something
                //// that mostly works like verifying the user is a gallery admin for at least one gallery, but the function we are trying to
                //// protect is viewing an event message, which is not that important to worry about.
                //if (appEvent.GalleryId != GalleryController.GetTemplateGalleryId())
                //{
                //    SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.AdministerSite | SecurityActions.AdministerGallery, RoleController.GetGalleryServerRolesForUser(), int.MinValue, appEvent.GalleryId, Utils.IsAuthenticated, false, false);
                //}

                return new ObjectResult(appEvent.ToHtml());
            }
            //catch (GallerySecurityException)
            //{
            //    return Forbid();
            //    //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            //}
            catch (Exception ex)
            {
                if (appEvent != null)
                    AppEventController.LogError(ex, appEvent.GalleryId);
                else
                    AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
                //return StatusCode(500, ex.Message);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //{
                //    Content = Utils.GetExStringContent(ex),
                //    ReasonPhrase = "Server Error"
                //});
            }
        }


        /// <summary>
        /// Deletes the event having the specified <paramref name="id" />.
        /// </summary>
        /// <param name="id">The ID of the event to delete.</param>
        /// <returns>An instance of <see cref="IActionResult" />.</returns>
        [HttpDelete]
        [Authorize(Policy = GlobalConstants.PolicyAdministrator)]
        public IActionResult Delete(int id)
        {
            // DELETE /api/events/delete/12
            IEvent appEvent = null;

            try
            {
                appEvent = Factory.GetAppEvents().FindById(id);

                if (appEvent == null)
                {
                    // HTTP specification says the DELETE method must be idempotent, so deleting a nonexistent item must have 
                    // the same effect as deleting an existing one. So we simply return HttpStatusCode.OK.
                    return Ok(string.Format("Event with ID {0} does not exist.", id));
                    //return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(String.Format("Event with ID = {0} does not exist.", id)) };
                }

                //var isAuthorized = true;

                //// If the event has a non-template gallery ID (not all do), then check the user's permission. For those errors without a gallery ID,
                //// just assume the user has permission, because there is no way to verify the user can delete this event. We could do something
                //// that mostly works like verifying the user is a gallery admin for at least one gallery, but the function we are trying to
                //// protect is deleting an event message, which is not that important to worry about.
                //if (appEvent.GalleryId != GalleryController.GetTemplateGalleryId())
                //{
                //    isAuthorized = Utils.IsUserAuthorized(SecurityActions.AdministerSite | SecurityActions.AdministerGallery, RoleController.GetGalleryServerRolesForUser(), int.MinValue, appEvent.GalleryId, false, false);
                //}

                //if (isAuthorized)
                //{
                Events.EventController.Delete(id);
                CacheController.RemoveCache(CacheItem.AppEvents);

                return Ok("Event deleted...");
                //return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Event deleted...") };
                //}
                //else
                //{
                //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
                //}
            }
            catch (Exception ex)
            {
                if (appEvent != null)
                    AppEventController.LogError(ex, appEvent.GalleryId);
                else
                    AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //{
                //    Content = Utils.GetExStringContent(ex),
                //    ReasonPhrase = "Server Error"
                //});
            }
        }

    }
}
