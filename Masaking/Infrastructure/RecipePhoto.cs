using System;
using System.IO;
using System.Web.UI;

namespace Masaking.Infrastructure
{
    internal static class RecipePhoto
    {
        public static string Resolve(Page page, object value)
        {
            string path = Convert.ToString(value).Trim().Replace('\\', '/');

            if (Uri.TryCreate(path, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                return path;

            if (path.StartsWith("~/", StringComparison.Ordinal))
                path = path.Substring(1);

            path = path.Replace("/Content/Uploads/Recipes/", "/Content/images/Uploads/Recipes/");
            const string uploadPrefix = "/Content/images/Uploads/Recipes/";
            string file = Path.GetFileName(path);

            bool isUpload = path.StartsWith(uploadPrefix, StringComparison.OrdinalIgnoreCase) &&
                            path.Substring(uploadPrefix.Length) == file &&
                            file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                            Guid.TryParseExact(Path.GetFileNameWithoutExtension(file), "N", out _);

            const string defaultPrefix = "/Content/images/simulation/defaults/dishes/";
            string extension = Path.GetExtension(file);
            bool isDefault = path.StartsWith(defaultPrefix, StringComparison.OrdinalIgnoreCase) &&
                             path.Substring(defaultPrefix.Length) == file &&
                             (string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase));

            return isUpload || (isDefault && File.Exists(page.Server.MapPath("~" + path)))
                ? page.ResolveUrl("~" + path) : "";
        }
    }
}
