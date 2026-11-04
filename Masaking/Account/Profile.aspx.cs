using System.Web.UI.HtmlControls;
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
            if (!IsAdministrator)
                return;
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT
                (SELECT COUNT(*) FROM recipes WHERE status='Pending') AS pendingRecipes,
                (SELECT COUNT(*) FROM reviews WHERE status='Pending') +
                (SELECT COUNT(*) FROM posts WHERE status='Pending') +
                (SELECT COUNT(*) FROM postComments WHERE status='Pending') AS pendingCommunity", cn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                    return;
                PendingRecipes = Convert.ToInt32(reader["pendingRecipes"]);
                PendingCommunity = Convert.ToInt32(reader["pendingCommunity"]);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            // Signing out must still work when profile data cannot be loaded.
            if (IsPostBack && Request.Form["__EVENTTARGET"] == btnSignOut.UniqueID)
                return;
            if (!Database.RequireUser(Response))
                return;
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

        protected override void OnPreRender(EventArgs e)
        {
            if (!IsAdministrator)
                profilePage.Attributes["data-profile-screen"] = "";
            else
                profilePage.Attributes.Remove("data-profile-screen");
            phAdminSection.Visible = IsAdministrator;
            phAdminSection2.Visible = IsAdministrator;
            phMemberSection.Visible = !(IsAdministrator);
            phAdminSection3.Visible = IsAdministrator;
            phMemberSection2.Visible = !(IsAdministrator);
            lblInitials.Text = Server.HtmlEncode(Convert.ToString(Initials));
            imgDecoration.ImageUrl = Convert.ToString(AvatarDisplayUrl);
            lblProfileName.Text = Server.HtmlEncode(Convert.ToString(ProfileName));
            lblProfileUsername.Text = Server.HtmlEncode(Convert.ToString(ProfileUsername));
            lblProfileCountry.Text = Server.HtmlEncode(Convert.ToString(ProfileCountry));
            lblProfileEmail.Text = Server.HtmlEncode(Convert.ToString(ProfileEmail));
            lblPendingRecipes.Text = Server.HtmlEncode(Convert.ToString(PendingRecipes));
            lblPendingCommunity.Text = Server.HtmlEncode(Convert.ToString(PendingCommunity));
            lblRecipeCount.Text = Server.HtmlEncode(Convert.ToString(RecipeCount));
            lblPostCount.Text = Server.HtmlEncode(Convert.ToString(PostCount));
            base.OnPreRender(e);
        }

        protected void rptRecipes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            var row = (System.Data.DataRowView)e.Item.DataItem;
            var lnkRecipe = (HyperLink)e.Item.FindControl("lnkRecipe");
            lnkRecipe.NavigateUrl = Convert.ToString(RecipeUrl(row["recipeID"], row["status"]));
            var imgDecoration2 = (Image)e.Item.FindControl("imgDecoration2");
            imgDecoration2.ImageUrl = Convert.ToString(Picture(row["image"]));
            imgDecoration2.AlternateText = Convert.ToString(row["title"]);
            var lblTitle = (Label)e.Item.FindControl("lblTitle");
            lblTitle.Text = Server.HtmlEncode(Convert.ToString(row["title"]));
            var lblCuisine = (Label)e.Item.FindControl("lblCuisine");
            lblCuisine.Text = Server.HtmlEncode(Convert.ToString(row["cuisine"]));
            var lblPreparationTime = (Label)e.Item.FindControl("lblPreparationTime");
            lblPreparationTime.Text = Server.HtmlEncode(Convert.ToString(row["preparationTime"]));
            var lblSaves = (Label)e.Item.FindControl("lblSaves");
            lblSaves.Text = Server.HtmlEncode(Convert.ToString(row["saves"]));
            var lblStatus = (Label)e.Item.FindControl("lblStatus");
            lblStatus.Text = Server.HtmlEncode(Convert.ToString(Convert.ToString(row["status"]) == "Approved" ? "" : row["status"]));
        }

        protected void rptActivity_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            var row = (System.Data.DataRowView)e.Item.DataItem;
            var lnkPost = (HyperLink)e.Item.FindControl("lnkPost");
            lnkPost.Attributes["data-activity-kind"] = Convert.ToString(row["kind"]);
            lnkPost.NavigateUrl = Convert.ToString("../Community/Post.aspx?ID=" + row["postID"] + (Convert.ToString(row["kind"]) == "comment" ? "#comments" : ""));
            var lblInitials2 = (Label)e.Item.FindControl("lblInitials2");
            lblInitials2.Text = Server.HtmlEncode(Convert.ToString(Initials));
            var imgDecoration3 = (Image)e.Item.FindControl("imgDecoration3");
            imgDecoration3.ImageUrl = Convert.ToString(AvatarDisplayUrl);
            var lblKind = (Label)e.Item.FindControl("lblKind");
            lblKind.Text = Server.HtmlEncode(Convert.ToString(ActivityText(row["kind"])));
            var lblContent = (Label)e.Item.FindControl("lblContent");
            lblContent.Text = Server.HtmlEncode(Convert.ToString(Excerpt(row["content"])));
            var postCard = (HtmlGenericControl)e.Item.FindControl("postCard");
            postCard.Attributes["datetime"] = Convert.ToString(Convert.ToDateTime(row["createdAt"]).ToString("s") + "Z");
            var lblActivityAge = (Label)e.Item.FindControl("lblActivityAge");
            lblActivityAge.Text = Server.HtmlEncode(Convert.ToString(Age(row["createdAt"])));
        }
    }
}
