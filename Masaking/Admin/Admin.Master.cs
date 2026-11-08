using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class AdminMaster : MasterPage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Database.RequireAdmin(Response);
        }

        protected override void OnPreRender(EventArgs e)
        {
            string page = System.IO.Path.GetFileName(Request.AppRelativeCurrentExecutionFilePath);
            MarkCurrent(lnkDashboard, page == "Dashboard.aspx");
            MarkCurrent(lnkRecipes, page == "ManageRecipes.aspx");
            MarkCurrent(lnkUsers, page == "ManageUsers.aspx");
            MarkCurrent(lnkCommunity, page == "ManageCommunity.aspx");
            MarkCurrent(lnkQuizzes, page == "ManageChallenges.aspx" || page == "QuizQuestions.aspx");
            MarkCurrent(lnkSimulations, page == "ManageSimulations.aspx" || page == "SimulationSteps.aspx");

            string name = Convert.ToString(Session["DisplayName"]).Trim();
            if (name.Length == 0) name = "Administrator";
            lblAdminName.Text = Server.HtmlEncode(name);
            string[] words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = words[0].Substring(0, 1) + (words.Length > 1 ? words[words.Length - 1].Substring(0, 1) : "");
            lblAdminInitials.Text = Server.HtmlEncode(initials.ToUpperInvariant());

            string avatar = Database.AvatarUrl(Session["UserImage"]);
            imgAdminAvatar.ImageUrl = avatar;
            imgAdminAvatar.Visible = avatar != Database.DefaultAvatar && System.IO.File.Exists(Server.MapPath("~" + avatar));
            lblAdminInitials.Visible = !imgAdminAvatar.Visible;
            base.OnPreRender(e); //reads display name and image from existing session 
        }

        private static void MarkCurrent(HyperLink link, bool current)
        {
            if (current) link.Attributes["aria-current"] = "page";
            else link.Attributes.Remove("aria-current");
        }
    }
}