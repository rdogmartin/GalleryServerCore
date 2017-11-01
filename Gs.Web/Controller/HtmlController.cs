using System.Web;
using GalleryServer.Business;
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


        /// <summary>
        /// Removes potentially dangerous HTML and JavaScript in <paramref name="html" />.
        /// When the current user is a gallery or site admin, no validation is performed and the
        /// <paramref name="html" /> is returned without any processing. If the configuration
        /// setting <see cref="IGallerySettings.AllowUserEnteredHtml" /> is true, then the input is cleaned so that all
        /// HTML tags that are not in a predefined list are HTML-encoded and invalid HTML attributes are deleted. If
        /// <see cref="IGallerySettings.AllowUserEnteredHtml" /> is false, then all HTML tags are deleted. If the setting
        /// <see cref="IGallerySettings.AllowUserEnteredJavascript" /> is true, then script tags and the text "javascript:"
        /// is allowed. Note that if script is not in the list of valid HTML tags defined in <see cref="IGallerySettings.AllowedHtmlTags" />,
        /// it will be deleted even when <see cref="IGallerySettings.AllowUserEnteredJavascript" /> is true. When the setting
        /// is false, all script tags and instances of the text "javascript:" are deleted.
        /// </summary>
        /// <param name="html">The string containing the HTML tags.</param>
        /// <param name="currentUserIsGalleryAdministrator">if set to <c>true</c> the current user is a gallery administrator.</param>
        /// <param name="galleryId">The gallery ID. This is used to look up the appropriate configuration values for the gallery.</param>
        /// <returns>Returns a string with potentially dangerous HTML tags deleted.</returns>
        /// <remarks>TODO: Refactor this so that the Clean method knows whether the user is a gallery admin, rendering this
        /// function unnecessary. When this is done, update <see cref="GalleryObject.MetaRegEx" /> so that all meta items are
        /// passed to the Clean method.</remarks>
        public string CleanHtmlTags(string html, bool currentUserIsGalleryAdministrator, int galleryId)
        {
            if (currentUserIsGalleryAdministrator)
                return html;
            else
                return HtmlValidator.Clean(html, galleryId);
        }

        /// <overloads>Remove all HTML tags from the specified string.</overloads>
        /// <summary>
        /// Remove all HTML tags from the specified string.
        /// </summary>
        /// <param name="html">The string containing HTML tags to remove.</param>
        /// <returns>Returns a string with all HTML tags removed.</returns>
        public string RemoveHtmlTags(string html)
        {
            return RemoveHtmlTags(html, false);
        }

        /// <summary>
        /// Remove all HTML tags from the specified string. If <paramref name="escapeQuotes"/> is true, then all 
        /// apostrophes and quotation marks are replaced with &quot; and &apos; so that the string can be specified in HTML 
        /// attributes such as title tags. If the escapeQuotes parameter is not specified, no replacement is performed.
        /// </summary>
        /// <param name="html">The string containing HTML tags to remove.</param>
        /// <param name="escapeQuotes">When true, all apostrophes and quotation marks are replaced with &quot; and &apos;.</param>
        /// <returns>Returns a string with all HTML tags removed.</returns>
        public string RemoveHtmlTags(string html, bool escapeQuotes)
        {
            return HtmlValidator.RemoveHtml(html, escapeQuotes);
        }
    }
}
