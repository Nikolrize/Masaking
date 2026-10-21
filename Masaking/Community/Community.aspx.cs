using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Community
{
    public partial class Community : System.Web.UI.Page
    {
        protected Label lblMessage;
        protected Panel pnlShare, pnlGuest, pnlThread, pnlNoComments, pnlCommentForm, pnlEmpty, pnlAddPost;
        protected DropDownList ddlRecipe, ddlSort;
        protected TextBox txtPost, txtComment;
        protected FileUpload uploadPostImage;
        protected Image imgThread;
        protected Literal litThreadTitle, litThreadPost;
        protected Repeater rptPosts, rptComments;
        protected HyperLink lnkCommentLogin;

        private int ThreadId
        {
            get
            {
                int id;
                return Int32.TryParse(Request.QueryString["post"], out id) ? id : 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadRecipeOptions();
                BindPosts();
            }

            pnlShare.Visible = Database.UserId.HasValue;
            pnlAddPost.Visible = Database.UserId.HasValue;
            pnlGuest.Visible = !Database.UserId.HasValue;

            if (ThreadId > 0)
            {
                BindThread(ThreadId);
            }
        }

        private void LoadRecipeOptions()
        {
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT recipeID,title FROM recipes WHERE status='Approved' ORDER BY title", cn))
            {
                ddlRecipe.DataSource = cmd.ExecuteReader();
                ddlRecipe.DataTextField = "title";
                ddlRecipe.DataValueField = "recipeID";
                ddlRecipe.DataBind();
                ddlRecipe.Items.Insert(0, new ListItem("Choose a recipe…", ""));
            }
        }

        private void BindPosts()
        {
            string orderBy = ddlSort == null || ddlSort.SelectedValue == "newest" ? "p.createdAt DESC" : ddlSort.SelectedValue == "oldest" ? "p.createdAt ASC" : "commentCount DESC,p.createdAt DESC";
            string sql = "SELECT p.postID,p.content,p.image,p.createdAt,p.recipeID,r.title,u.displayName,(SELECT COUNT(*) FROM postComments c WHERE c.postID=p.postID AND c.status='Approved') commentCount FROM posts p JOIN recipes r ON r.recipeID=p.recipeID JOIN users u ON u.userID=p.userID WHERE p.status='Approved' ORDER BY " + orderBy;

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                DataTable data = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(data);
                }

                rptPosts.DataSource = data;
                rptPosts.DataBind();
                pnlEmpty.Visible = data.Rows.Count == 0;
            }
        }

        protected void ddlSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindPosts();
        }

        private void BindThread(int postId)
        {
            using (SqlConnection cn = Database.Open())
            {
                using (SqlCommand cmd = new SqlCommand("SELECT r.title,p.content,p.image FROM posts p JOIN recipes r ON r.recipeID=p.recipeID WHERE p.postID=@id AND p.status='Approved'", cn))
                {
                    cmd.Parameters.AddWithValue("@id", postId);
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return;

                        pnlThread.Visible = true;
                        litThreadTitle.Text = Server.HtmlEncode(rd.GetString(0));
                        litThreadPost.Text = Server.HtmlEncode(rd.GetString(1));
                        imgThread.Visible = !rd.IsDBNull(2) && !String.IsNullOrWhiteSpace(Convert.ToString(rd[2]));
                        if (imgThread.Visible)
                        {
                            imgThread.ImageUrl = CommunityImageUrl(rd[2]);
                        }
                    }
                }

                using (SqlCommand cmd = new SqlCommand("SELECT c.content,c.createdAt,u.displayName FROM postComments c JOIN users u ON u.userID=c.userID WHERE c.postID=@id AND c.status='Approved' ORDER BY c.createdAt", cn))
                {
                    cmd.Parameters.AddWithValue("@id", postId);
                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    rptComments.DataSource = dt;
                    rptComments.DataBind();
                    pnlNoComments.Visible = dt.Rows.Count == 0;
                }
            }

            pnlCommentForm.Visible = Database.UserId.HasValue;
            lnkCommentLogin.Visible = !Database.UserId.HasValue;
            lnkCommentLogin.NavigateUrl = "../Account/Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl);
        }

        protected void btnPost_Click(object sender, EventArgs e)
        {
            int recipeId;
            if (!Database.UserId.HasValue)
            {
                Response.Redirect("../Account/Login.aspx");
                return;
            }

            if (!Int32.TryParse(ddlRecipe.SelectedValue, out recipeId) || String.IsNullOrWhiteSpace(txtPost.Text))
            {
                Show("Choose an approved recipe and add a short cooking note.", false);
                return;
            }

            string image = null;
            try
            {
                if (uploadPostImage.HasFile)
                {
                    image = ImageUpload.Save(uploadPostImage.PostedFile, false);
                }
            }
            catch (InvalidDataException ex)
            {
                Show(ex.Message, false);
                return;
            }

            try
            {
                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand("INSERT INTO posts(content,image,likes,status,createdAt,updatedAt,userID,recipeID) SELECT @content,@image,0,'Pending',@created,NULL,@user,recipeID FROM recipes WHERE recipeID=@recipe AND status='Approved'", cn))
                {
                    cmd.Parameters.AddWithValue("@content", txtPost.Text.Trim());
                    cmd.Parameters.AddWithValue("@image", (object)image ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@created", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@user", Database.UserId.Value);
                    cmd.Parameters.AddWithValue("@recipe", recipeId);

                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        ImageUpload.DeleteUploaded(image, "Community");
                        Show("That recipe is unavailable. Refresh and try again.", false);
                        return;
                    }
                }
            }
            catch
            {
                ImageUpload.DeleteUploaded(image, "Community");
                Show("Your post could not be saved. Please try again.", false);
                return;
            }

            txtPost.Text = "";
            Show("Thanks for sharing. Your post is waiting for community moderation.", true);
            BindPosts();
        }

        protected void btnComment_Click(object sender, EventArgs e)
        {
            if (!Database.UserId.HasValue)
            {
                Response.Redirect("../Account/Login.aspx");
                return;
            }

            if (ThreadId <= 0 || String.IsNullOrWhiteSpace(txtComment.Text) || txtComment.Text.Trim().Length > 1000)
            {
                Show("Write a comment of at most 1000 characters before posting.", false);
                return;
            }

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand("INSERT INTO postComments(content,status,createdAt,updatedAt,postID,userID) SELECT @content,'Pending',@created,NULL,postID,@user FROM posts WHERE postID=@post AND status='Approved'", cn))
            {
                cmd.Parameters.AddWithValue("@content", txtComment.Text.Trim());
                cmd.Parameters.AddWithValue("@created", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@user", Database.UserId.Value);
                cmd.Parameters.AddWithValue("@post", ThreadId);

                if (cmd.ExecuteNonQuery() != 1)
                {
                    Show("This post is no longer available.", false);
                    return;
                }
            }

            txtComment.Text = "";
            Show("Your comment is waiting for moderation.", true);
            BindThread(ThreadId);
        }

        private void Show(string text, bool success)
        {
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Text = Server.HtmlEncode(text);
        }

        protected string CommunityImageUrl(object value)
        {
            string path = Convert.ToString(value ?? "");
            const string legacyPrefix = "/Content/Uploads/Community/";
            const string prefix = "/Content/images/Uploads/Community/";

            if (path.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                path = prefix + path.Substring(legacyPrefix.Length);
            }

            if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            Guid id;
            string name = Path.GetFileName(path);
            return Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "N", out id) && name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ? path : "";
        }
    }
}