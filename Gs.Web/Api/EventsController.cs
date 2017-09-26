using System;
using System.Net;
using System.Net.Http;
using GalleryServer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;
using Microsoft.Extensions.Caching.Memory;

namespace GalleryServer.Web.Api
{
    [Route("api/[controller]")]
    public class EventsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly GalleryDb _ctx;
        private IMemoryCache _cache;

        public EventsController(GalleryDb ctx, IMemoryCache memoryCache)
        {
            _ctx = ctx;
            _cache = memoryCache;
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
        public IActionResult Get(int id)
        {
            IEvent appEvent = null;
            try
            {
                appEvent = Factory.GetAppEvents().FindById(id);

                if (appEvent == null)
                {

                    return NotFound();
                    //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                    //{
                    //    Content = new StringContent(String.Format("Could not find event with ID = {0}", id)),
                    //    ReasonPhrase = "Event Not Found"
                    //});
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

                return StatusCode(500, ex.Message);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //{
                //    Content = Utils.GetExStringContent(ex),
                //    ReasonPhrase = "Server Error"
                //});
            }
        }
    }
}
