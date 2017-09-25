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

        [HttpGet("{id:int}")]
        public async Task<EventDto> Get(int id)
        {
            return await _ctx.Events.SingleOrDefaultAsync(e => e.EventId == id);
        }
    }
}
