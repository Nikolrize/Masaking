using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Masaking.Infrastructure
{
    public static class StockImages
    {
        public static Dictionary<string, string> Read(string kind)
        {
            if (kind != "ingredients" && kind != "cookware" && kind != "dishes")
                throw new ArgumentException("Unknown stock-picture category.");
            string folder = "~/Content/images/simulation/defaults/" + kind + "/";
            string directory = HttpContext.Current.Server.MapPath(folder);
            var photos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(directory)) return photos;
            foreach (string file in Directory.GetFiles(directory).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
            {
                string extension = Path.GetExtension(file).ToLowerInvariant();
                if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".gif" || extension == ".svg")
                    photos[Path.GetFileName(file)] = folder + Path.GetFileName(file);
            }
            return photos;
        }

        public static void Bind(DropDownList list, string kind)
        {
            list.Items.Clear();
            list.Items.Add(new ListItem("-- Keep current picture / no stock selection --", ""));
            foreach (var photo in Read(kind))
                list.Items.Add(new ListItem(Path.GetFileNameWithoutExtension(photo.Key).Replace('-', ' '), photo.Key));
        }

        public static string Selected(DropDownList list, string kind)
        {
            if (String.IsNullOrEmpty(list.SelectedValue)) return "";
            string path;
            if (!Read(kind).TryGetValue(list.SelectedValue, out path))
                throw new InvalidDataException("That stock picture is unavailable. Choose another picture.");
            return path;
        }
    }
}
