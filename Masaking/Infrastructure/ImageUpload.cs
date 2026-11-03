using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace Masaking.Infrastructure
{
    public static class ImageUpload
    {
        public const int MaxBytes = 5 * 1024 * 1024;

        public static string Save(HttpPostedFile file, bool avatar)
        {
            return Save(file, avatar ? "Profiles" : "Community", avatar);
        }

        public static string Save(HttpPostedFile file, string folder, bool square)
        {
            if (folder != "Profiles" && folder != "Community" && folder != "Recipes" && folder != "Steps")
                throw new ArgumentException("Unsupported image upload location.", "folder");
            if (file == null || file.ContentLength == 0 || file.ContentLength > MaxBytes)
                throw new InvalidDataException("Choose an image smaller than 5 MB.");
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension))
                throw new InvalidDataException("Choose a JPG, PNG or GIF image.");

            try
            {
                using (Image source = Image.FromStream(file.InputStream, true, true))
                {
                    if (source.Width < 1 || source.Height < 1 || source.Width > 8000 || source.Height > 8000)
                        throw new InvalidDataException("The image dimensions are too large.");
                    int width, height, x = 0, y = 0;
                    if (square) { width = height = Math.Min(source.Width, source.Height); x = (source.Width - width) / 2; y = (source.Height - height) / 2; }
                    else { double scale = Math.Min(1d, 1600d / Math.Max(source.Width, source.Height)); width = Math.Max(1, (int)(source.Width * scale)); height = Math.Max(1, (int)(source.Height * scale)); }
                    string directory = HttpContext.Current.Server.MapPath("~/Content/images/Uploads/" + folder);
                    Directory.CreateDirectory(directory);
                    string filename = Guid.NewGuid().ToString("N") + ".jpg";
                    string fullPath = Path.Combine(directory, filename);
                    using (Bitmap output = new Bitmap(width, height))
                    using (Graphics graphics = Graphics.FromImage(output))
                    {
                        graphics.Clear(Color.White);
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.DrawImage(source, new Rectangle(0, 0, width, height), new Rectangle(x, y, square ? width : source.Width, square ? height : source.Height), GraphicsUnit.Pixel);
                        ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");
                        using (EncoderParameters parameters = new EncoderParameters(1))
                        { parameters.Param[0] = new EncoderParameter(Encoder.Quality, 86L); output.Save(fullPath, codec, parameters); }
                    }
                    return "/Content/images/Uploads/" + folder + "/" + filename;
                }
            }
            catch (InvalidDataException) { throw; }
            catch (Exception ex) when (ex is ArgumentException || ex is ExternalException || ex is OutOfMemoryException)
            { throw new InvalidDataException("The selected file is not a valid image."); }
        }

        public static void DeleteUploaded(string webPath, string folder)
        {
            string prefix = "/Content/images/Uploads/" + folder + "/";
            string legacyPrefix = "/Content/Uploads/" + folder + "/";
            if (String.IsNullOrEmpty(webPath)) return;
            if (webPath.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase)) webPath = prefix + webPath.Substring(legacyPrefix.Length);
            if (!webPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return;
            string filename = webPath.Substring(prefix.Length);
            Guid ignored;
            if (!filename.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(filename), "N", out ignored)) return;
            string path = HttpContext.Current.Server.MapPath("~/Content/images/Uploads/" + folder + "/" + filename);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
