using Masaking.Infrastructure;
using System;
using System.Web;

namespace Masaking.Controls
{
    public partial class UserAvatar : System.Web.UI.UserControl
    {
        public void DisplayAvatar(object displayName, object image)
        {
            string imageUrl = Database.AvatarUrl(image);
            bool hasPhoto = imageUrl != Database.DefaultAvatar;

            avatarImage.Visible = hasPhoto;
            avatarImage.ImageUrl = imageUrl;
            avatarInitials.Visible = !hasPhoto;

            string name = Convert.ToString(displayName);

            string[] words = name.Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            string initials = "M";

            if (words.Length > 0)
            {
                initials = words[0].Substring(0, 1);

                if (words.Length > 1)
                {
                    initials += words[words.Length - 1].Substring(0, 1);
                }
            }

            avatarInitials.Text = HttpUtility.HtmlEncode(initials.ToUpperInvariant());
        }
    }
}