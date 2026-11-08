using System;
using System.Web.UI;
using System.Globalization;
using Masaking.Infrastructure;

namespace Masaking
{
    public partial class SiteMaster : MasterPage
    {
        protected override void OnPreRender(EventArgs e)
        {
            Page.Header.Title = Page.Title + " - Masaking";
            lblCopyrightYear.Text = DateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
            navSearch.Attributes["data-search-url"] = ResolveUrl("~/Search.aspx");
            bool signedIn = Database.UserId.HasValue;
            phGuestNav.Visible = !signedIn;
            phAuthenticatedNav.Visible = signedIn;
            phMyRecipesNav.Visible = signedIn;
            if (signedIn)
            {
                bool isAdmin = String.Equals(Database.UserRole, "Admin", StringComparison.OrdinalIgnoreCase);
                string name = Convert.ToString(Session["DisplayName"]);
                string[] words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string initials = words.Length == 0 ? "M" : words[0].Substring(0, 1)
                    + (words.Length > 1 ? words[words.Length - 1].Substring(0, 1) : "");
                lblNavInitials.Text = Server.HtmlEncode(initials.ToUpperInvariant());
                lnkNavProfile.NavigateUrl = isAdmin ? "~/Admin/Dashboard.aspx" : "~/Account/Profile.aspx";
                lnkNavProfile.ToolTip = isAdmin ? "Admin dashboard" : (String.IsNullOrWhiteSpace(name) ? "My profile" : name + " - My profile");
                lnkNavProfile.Attributes["aria-label"] = isAdmin ? "Open admin dashboard" : "Open my profile";
                phAdminDashboardIcon.Visible = isAdmin;
                lblNavInitials.Visible = !isAdmin;
                string avatar = Database.AvatarUrl(Session["UserImage"]);
                imgNavAvatar.ImageUrl = avatar;
                imgNavAvatar.Visible = !isAdmin && avatar != Database.DefaultAvatar;
            }
            base.OnPreRender(e);
        }
    }
}
