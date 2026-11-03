using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Community
{
    public partial class Community : CommunityPage
    {
        protected Repeater rptPosts, rptDeletedPosts;
        protected Panel pnlEmpty, pnlDeletedPosts, pnlAddPost, pnlGuest;
        protected DropDownList ddlSort;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Preserve older discussion links without keeping two discussion views.
            int legacyPost;
            if (!IsPostBack && Int32.TryParse(Request.QueryString["post"], out legacyPost) && legacyPost > 0)
            { Navigate("Post.aspx?id=" + legacyPost); return; }
            pnlAddPost.Visible = currentUserIsActive;
            pnlGuest.Visible = !currentUserIsActive;
            if (!IsPostBack) RunDatabaseAction(() => { BindPosts(); BindMyDeletedPosts(); });
        }

        private void BindPosts()
        {
            DataTable data = ReadPosts(null, ddlSort.SelectedValue);
            rptPosts.DataSource = data;
            rptPosts.DataBind();
            pnlEmpty.Visible = data.Rows.Count == 0;
        }

        protected override void AfterPostDeleted()
        {
            Show("The post has been removed from the community feed.", true);
            BindPosts();
            BindMyDeletedPosts();
        }

        private void BindMyDeletedPosts()
        {
            if (!Database.UserId.HasValue)
            {
                rptDeletedPosts.DataSource = null;
                rptDeletedPosts.DataBind();
                pnlDeletedPosts.Visible = false;
                return;
            }

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT postID, content, createdAt, updatedAt " +
                "FROM posts WHERE userID=@user AND status='Deleted' " +
                "ORDER BY createdAt DESC", cn))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                DataTable data = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(data);
                }
                rptDeletedPosts.DataSource = data;
                rptDeletedPosts.DataBind();
                pnlDeletedPosts.Visible = data.Rows.Count > 0;
            }
        }

        protected void ddlSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            RunDatabaseAction(() =>
            {
                BindPosts();
            });
        }

        protected void rptDeletedPosts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            RunDatabaseAction(() =>
            {
                if (e.CommandName != "restorePost") return;
                if (!RequireCurrentUser()) return;
                if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out int postId) || postId <= 0) return;

                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE posts SET status='Approved' WHERE postID=@post AND userID=@user AND status='Deleted' " +
                    "AND (recipeID IS NULL OR EXISTS (SELECT 1 FROM recipes WHERE recipes.recipeID=posts.recipeID AND recipes.status='Approved'))", cn))
                {
                    cmd.Parameters.Add("@post", SqlDbType.Int).Value = postId;
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;

                    if (cmd.ExecuteNonQuery() != 1) Show("This post could not be restored. Its attached recipe may no longer be available.", false);
                    else Show("Your post has been restored to the community feed.", true);
                }
                BindPosts();
                BindMyDeletedPosts();
            });
        }

    }
}
