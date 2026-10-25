using System;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Account
{
    public partial class Profile : ProfilePage
    {
        protected Label lblMessage;
        protected Repeater rptRecipes, rptActivity;
        protected Panel pnlNoRecipes;
        protected LinkButton btnSignOut;
        protected int PendingRecipes, PendingCommunity;
        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            Response.Redirect("~/Account/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void LoadModerationSummary()
        {
            // The role has been refreshed by RequireUser before reading admin data.
            if (!IsAdministrator) return;
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT
                (SELECT COUNT(*) FROM recipes WHERE status='Pending') AS pendingRecipes,
                (SELECT COUNT(*) FROM reviews WHERE status='Pending') +
                (SELECT COUNT(*) FROM posts WHERE status='Pending') +
                (SELECT COUNT(*) FROM postComments WHERE status='Pending') AS pendingCommunity", cn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read()) return;
                PendingRecipes = Convert.ToInt32(reader["pendingRecipes"]);
                PendingCommunity = Convert.ToInt32(reader["pendingCommunity"]);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            // Signing out must still work when profile data cannot be loaded.
            if (IsPostBack && Request.Form["__EVENTTARGET"] == btnSignOut.UniqueID) return;
            if (!Database.RequireUser(Response)) return;
            try
            {
                LoadAccount();
                if (IsAdministrator)
                {
                    LoadModerationSummary();
                }
                else if (!IsPostBack)
                {
                    var recipes = UserRecipes();
                    rptRecipes.DataSource = recipes;
                    rptRecipes.DataBind();
                    pnlNoRecipes.Visible = recipes.Rows.Count == 0;
                    rptActivity.DataSource = UserActivity();
                    rptActivity.DataBind();
                }
            }
            catch (SqlException ex)
            {
                Trace.Warn("Profile", "Unable to load profile information.", ex);
                lblMessage.CssClass = "alert alert-warning d-block";
                lblMessage.Text = "Your profile information is temporarily unavailable. Please try again later.";
            }
        }
    }
}
