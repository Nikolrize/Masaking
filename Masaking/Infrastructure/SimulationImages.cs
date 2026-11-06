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
    public static class SimulationImages
    {
        public const int MaxBytes = ImageUpload.MaxBytes;
        private const string Folder = "~/Content/images/Uploads/Simulations/";

        public static string Save(HttpPostedFile file)
        {
            if (file == null || file.ContentLength == 0 || file.ContentLength > MaxBytes)
                throw new InvalidDataException("Choose a JPG, PNG or GIF image up to 5 MB.");

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg"
                && extension != ".png" && extension != ".gif")
                throw new InvalidDataException("Choose a JPG, PNG or GIF image.");

            string createdFile = null;
            bool saved = false;
            try
            {
                using (Image source = Image.FromStream(file.InputStream, true, true))
                {
                    if (source.Width < 1 || source.Height < 1
                        || source.Width > 8000 || source.Height > 8000)
                        throw new InvalidDataException("The image dimensions are too large.");

                    double scale = Math.Min(1d, 1600d / Math.Max(source.Width, source.Height));
                    int width = Math.Max(1, (int)(source.Width * scale));
                    int height = Math.Max(1, (int)(source.Height * scale));
                    bool keepTransparency = extension == ".png" || extension == ".gif";
                    string filename = Guid.NewGuid().ToString("N")
                        + (keepTransparency ? ".png" : ".jpg");
                    string directory = HttpContext.Current.Server.MapPath(Folder);
                    Directory.CreateDirectory(directory);
                    string fullPath = Path.Combine(directory, filename);

                    using (Bitmap output = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                    using (Graphics graphics = Graphics.FromImage(output))
                    {
                        graphics.Clear(keepTransparency ? Color.Transparent : Color.White);
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.DrawImage(source, new Rectangle(0, 0, width, height),
                            new Rectangle(0, 0, source.Width, source.Height), GraphicsUnit.Pixel);

                        using (FileStream destination = new FileStream(
                            fullPath, FileMode.CreateNew, FileAccess.Write))
                        {
                            createdFile = fullPath;
                            if (keepTransparency) output.Save(destination, ImageFormat.Png);
                            else
                            {
                                ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders()
                                    .First(c => c.MimeType == "image/jpeg");
                                using (EncoderParameters parameters = new EncoderParameters(1))
                                {
                                    parameters.Param[0] =
                                        new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 86L);
                                    output.Save(destination, codec, parameters);
                                }
                            }
                        }
                    }
                    saved = true;
                    return Folder + filename;
                }
            }
            catch (InvalidDataException) { throw; }
            catch (Exception ex) when (ex is ArgumentException
                || ex is ExternalException || ex is OutOfMemoryException)
            {
                throw new InvalidDataException("The selected file is not a valid image.");
            }
            finally
            {
                if (!saved && createdFile != null)
                {
                    try { if (File.Exists(createdFile)) File.Delete(createdFile); }
                    catch (IOException ex) { System.Diagnostics.Trace.TraceWarning(ex.Message); }
                    catch (UnauthorizedAccessException ex) { System.Diagnostics.Trace.TraceWarning(ex.Message); }
                }
            }
        }

        // Use only for NEW files whose database save failed.
        public static void DeleteFailedUpload(string path)
        {
            if (String.IsNullOrEmpty(path)
                || !path.StartsWith(Folder, StringComparison.OrdinalIgnoreCase)) return;
            string filename = path.Substring(Folder.Length);
            string extension = Path.GetExtension(filename).ToLowerInvariant();
            Guid id;
            if (Path.GetFileName(filename) != filename
                || (extension != ".png" && extension != ".jpg")
                || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(filename), "N", out id)) return;
            string fullPath = HttpContext.Current.Server.MapPath(Folder + filename);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
    }
}