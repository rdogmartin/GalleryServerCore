using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace GalleryServer.Web.Controller
{
    public class ExceptionController
    {
        private readonly IHostingEnvironment _env;

        public ExceptionController(IHostingEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Gets a string with details about the specified <paramref name="ex" />. Returns a generic message when the hosting 
        /// environment is anything other than "development"; otherwise returns the exception message.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>An instance of <see cref="string" />.</returns>
        public string GetExString(Exception ex)
        {
            var msg = "An error occurred on the server. Check the gallery's event log for details. ";

            if (_env.IsDevelopment())
            {
                msg += string.Concat(ex.GetType(), ": ", ex.Message);
            }

            return msg;
        }
    }
}
