using System;
using System.Net;
using System.Net.Http;
using GalleryServer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using Microsoft.VisualBasic.CompilerServices;

namespace GalleryServer.Web.Api
{
    [Route("api/[controller]")]
    public class EventsController : Controller
    {
        private readonly GalleryDb _ctx;

        public EventsController(GalleryDb ctx)
        {
            _ctx = ctx;
        }

        /// <summary>
        /// Gets an HTML formatted string representing the specified event <paramref name="id" />.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>A string.</returns>
        /// <exception cref="WebRequestMethods.Http.HttpResponseException">Thrown when the event does not exist in the data store,
        /// the user does not have permission to view it, or some other error occurs.</exception>
        [HttpGet("{id:int}")]
        public string Get(int id)
        {
            IEvent appEvent = null;
            try
            {
                appEvent = Factory.GetAppEvents().FindById(id);

                if (appEvent == null)
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(String.Format("Could not find event with ID = {0}", id)),
                        ReasonPhrase = "Event Not Found"
                    });
                }

                // If the event has a non-template gallery ID (not all do), then check the user's permission. For those errors without a gallery ID,
                // just assume the user has permission, because there is no way to verify the user can view this event. We could do something
                // that mostly works like verifying the user is a gallery admin for at least one gallery, but the function we are trying to
                // protect is viewing an event message, which is not that important to worry about.
                if (appEvent.GalleryId != GalleryController.GetTemplateGalleryId())
                {
                    SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.AdministerSite | SecurityActions.AdministerGallery, RoleController.GetGalleryServerRolesForUser(), int.MinValue, appEvent.GalleryId, Utils.IsAuthenticated, false, false);
                }

                return appEvent.ToHtml();
            }
            catch (GallerySecurityException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (Exception ex)
            {
                if (appEvent != null)
                    AppEventController.LogError(ex, appEvent.GalleryId);
                else
                    AppEventController.LogError(ex);

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = Utils.GetExStringContent(ex),
                    ReasonPhrase = "Server Error"
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<EventDto> Get(int id)
        {
            return await _ctx.Events.SingleOrDefaultAsync(e => e.EventId == id);
        }
    }
}
