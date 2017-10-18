using System.Web;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Web.Controller
{
    public class HtmlController
    {
        private readonly UrlController _urlController;

        public HtmlController(UrlController urlController)
        {
            _urlController = urlController;
        }

        public MediaObjectHtmlBuilder GetMediaObjectHtmlBuilder(MediaObjectHtmlBuilderOptions options)
        {
            return new MediaObjectHtmlBuilder(options);
        }

        public MediaObjectHtmlBuilderOptions GetMediaObjectHtmlBuilderOptions(IGalleryObject galleryObject)
        {
            return MediaObjectHtmlBuilder.GetMediaObjectHtmlBuilderOptions(galleryObject, _urlController);
        }


        /// <summary>
        /// HtmlEncodes a string using System.Web.HttpUtility.HtmlEncode().
        /// </summary>
        /// <param name="html">The text to HTML encode.</param>
        /// <returns>Returns <paramref name="html"/> as an HTML-encoded string.</returns>
        public string HtmlEncode(string html)
        {
            return HttpUtility.HtmlEncode(html);
        }

        /// <summary>
        /// HtmlDecodes a string using System.Web.HttpUtility.HtmlDecode().
        /// </summary>
        /// <param name="html">The text to HTML decode.</param>
        /// <returns>Returns <paramref name="html"/> as an HTML-decoded string.</returns>
        public string HtmlDecode(string html)
        {
            return HttpUtility.HtmlDecode(html);
        }
    }
}
