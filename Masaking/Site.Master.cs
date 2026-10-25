using System;
using System.Web.UI;
using Masaking.Infrastructure;

namespace Masaking
{
    public partial class SiteMaster : MasterPage
    {
        protected override void OnPreRender(EventArgs e)
        {
            bool signedIn = Database.UserId.HasValue;
            phGuestNav.Visible = !signedIn;
            phAuthenticatedNav.Visible = signedIn;
            if (signedIn)
            {
                string name = Convert.ToString(Session["DisplayName"]);
                string[] words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string initials = words.Length == 0 ? "M" : words[0].Substring(0, 1)
                    + (words.Length > 1 ? words[words.Length - 1].Substring(0, 1) : "");
                litNavInitials.Text = Server.HtmlEncode(initials.ToUpperInvariant());
                lnkNavProfile.ToolTip = String.IsNullOrWhiteSpace(name) ? "My profile" : name + " — My profile";
                string avatar = Database.AvatarUrl(Session["UserImage"]);
                imgNavAvatar.ImageUrl = avatar;
                imgNavAvatar.Visible = avatar != Database.DefaultAvatar;
            }
            base.OnPreRender(e);
        }
    }
}
