using System;
using System.Web.UI;
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
            string page = System.IO.Path.GetFileName(
                Request.AppRelativeCurrentExecutionFilePath);

            MarkCurrent(lnkDashboard, page == "Dashboard.aspx");
            MarkCurrent(lnkRecipes, page == "ManageRecipes.aspx");
            MarkCurrent(lnkUsers, page == "ManageUsers.aspx");
            MarkCurrent(lnkCommunity, page == "ManageCommunity.aspx");
            MarkCurrent(lnkQuizzes,
                page == "ManageChallenges.aspx" || page == "QuizQuestions.aspx");

            base.OnPreRender(e);
        }

        private static void MarkCurrent(
            System.Web.UI.WebControls.HyperLink link, bool current)
        {
            if (current)
            {
                link.Attributes["aria-current"] = "page";
            }
            else
            {
                link.Attributes.Remove("aria-current");
            }
        }
    }
}
