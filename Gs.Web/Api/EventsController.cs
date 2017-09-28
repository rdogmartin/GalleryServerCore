using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace GalleryServer.Web.Api
{
    [Route("api/[controller]")]
    [Authorize]
    public class EventsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private SignInManager<GalleryUser> _signInMgr;
        private GalleryRoleManager _roleManager;

        //private readonly GalleryDb _ctx;
        //private IMemoryCache _cache;

        public EventsController(GalleryDb ctx, IMemoryCache memoryCache, SignInManager<GalleryUser> signInManager, GalleryRoleManager roleManager)
        {
            //_ctx = ctx;
            //_cache = memoryCache;
            _signInMgr = signInManager;
            _roleManager = roleManager;
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
        /// <returns>A string.</returns>
        [HttpGet("{id:int}")]
        [Authorize(Policy = GlobalConstants.PolicyAdministrator)]
        public IActionResult Get(int id)
        {
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
            catch (GallerySecurityException)
            {
                return Forbid();
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (Exception ex)
            {
                if (appEvent != null)
                    AppEventController.LogError(ex, appEvent.GalleryId);
                else
                    AppEventController.LogError(ex);

                return StatusCode(500, Utils.GetExString(ex));
                //return StatusCode(500, ex.Message);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //{
                //    Content = Utils.GetExStringContent(ex),
                //    ReasonPhrase = "Server Error"
                //});
            }
        }
    }
}
