using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Community
{
    public partial class EditPost : CommunityPage
    {
        protected Panel pnlComposer;
        protected TextBox txtContent;
        protected FileUpload uploadImage;
        protected DropDownList ddlRecipe;
        protected CheckBox chkRemoveImage;
        protected Image imgExisting;
        protected Button btnPublish;
        protected HyperLink lnkCancel;

        private bool IsEditing { get { return Request.QueryString["id"] != null; } }
        private int PostId { get { int id; return Int32.TryParse(Request.QueryString["id"], out id) && id > 0 ? id : 0; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            Form.Enctype = "multipart/form-data";
            pnlComposer.Visible = false;
            if (!databaseAvailable || !RequireCurrentUser()) return;
            RunDatabaseAction(() =>
            {
                pnlComposer.Visible = true;
                btnPublish.Text = IsEditing ? "Update Post" : "Publish Post";
                lnkCancel.NavigateUrl = IsEditing && PostId > 0 ? "Post.aspx?id=" + PostId : "Community.aspx";
                if (IsPostBack) return;
                LoadRecipes();
                if (!IsEditing) return;
                DataRow post = ReadEditablePost();
                if (post == null) { pnlComposer.Visible = false; return; }
                txtContent.Text = Convert.ToString(post["content"]);
                imgExisting.ImageUrl = CommunityImageUrl(post["image"]);
                imgExisting.Visible = !String.IsNullOrEmpty(imgExisting.ImageUrl);
                ListItem recipe = ddlRecipe.Items.FindByValue(Convert.ToString(post["recipeID"]));
                if (recipe != null) ddlRecipe.SelectedValue = recipe.Value;
            });
        }

        private void LoadRecipes()
        {
            ddlRecipe.Items.Clear();
            ddlRecipe.Items.Add(new ListItem("No recipe attached", ""));
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT recipeID, title, image, cuisine FROM recipes WHERE status='Approved' ORDER BY title", cn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    ListItem item = new ListItem(Convert.ToString(reader["title"]), Convert.ToString(reader["recipeID"]));
                    item.Attributes["data-image"] = RecipeImageUrl(reader["image"]);
                    item.Attributes["data-summary"] = Convert.ToString(reader["cuisine"]);
                    ddlRecipe.Items.Add(item);
                }
            }
        }

        private DataRow ReadEditablePost()
        {
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT content, image, recipeID FROM posts
                WHERE postID=@post AND userID=@user AND status='Approved'
                AND createdAt>=DATEADD(day,-7,SYSUTCDATETIME()) AND createdAt<=SYSUTCDATETIME()", cn))
            {
                cmd.Parameters.Add("@post", SqlDbType.Int).Value = PostId;
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                DataTable data = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(data);
                if (data.Rows.Count == 1) return data.Rows[0];
            }
            Show("This post is unavailable or cannot be edited. You may edit your own posts for seven days after publishing.", false);
            return null;
        }

        protected void btnPublish_Click(object sender, EventArgs e)
        {
            if (!databaseAvailable || !RequireCurrentUser()) return;
            RunDatabaseAction(() =>
            {
                string content = (txtContent.Text ?? "").Trim();
                if (content.Length == 0 || content.Length > 2000)
                { Show("Write a post of 1–2,000 characters.", false); return; }
                int? recipeId = null;
                int parsedRecipe;
                if (!String.IsNullOrEmpty(ddlRecipe.SelectedValue))
                {
                    if (!Int32.TryParse(ddlRecipe.SelectedValue, out parsedRecipe) || parsedRecipe <= 0
                        || ddlRecipe.Items.FindByValue(ddlRecipe.SelectedValue) == null)
                    { Show("Choose an available recipe or leave it empty.", false); return; }
                    recipeId = parsedRecipe;
                }
                DataRow oldPost = IsEditing ? ReadEditablePost() : null;
                if (IsEditing && oldPost == null) return;
                string image = oldPost == null || chkRemoveImage.Checked ? null : Convert.ToString(oldPost["image"]);
                string uploaded = null;
                bool saved = false;
                try
                {
                    if (uploadImage.HasFile) image = uploaded = ImageUpload.Save(uploadImage.PostedFile, false);
                    string sql = IsEditing ? @"UPDATE posts SET content=@content, image=@image, recipeID=@recipe, updatedAt=SYSUTCDATETIME()
                        OUTPUT INSERTED.postID WHERE postID=@post AND userID=@user AND status='Approved'
                        AND createdAt>=DATEADD(day,-7,SYSUTCDATETIME()) AND createdAt<=SYSUTCDATETIME()
                        AND (@recipe IS NULL OR EXISTS (SELECT 1 FROM recipes WHERE recipeID=@recipe AND status='Approved'))"
                        : @"INSERT INTO posts(content,image,likes,status,createdAt,updatedAt,userID,recipeID)
                        OUTPUT INSERTED.postID
                        SELECT @content,@image,0,'Approved',SYSUTCDATETIME(),NULL,@user,@recipe
                        WHERE @recipe IS NULL OR EXISTS (SELECT 1 FROM recipes WHERE recipeID=@recipe AND status='Approved')";
                    using (SqlConnection cn = Database.Open())
                    using (SqlCommand cmd = new SqlCommand(sql, cn))
                    {
                        cmd.Parameters.Add("@post", SqlDbType.Int).Value = PostId;
                        cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                        cmd.Parameters.Add("@content", SqlDbType.NVarChar, -1).Value = content;
                        cmd.Parameters.Add("@image", SqlDbType.NVarChar, 255).Value = (object)image ?? DBNull.Value;
                        cmd.Parameters.Add("@recipe", SqlDbType.Int).Value = (object)recipeId ?? DBNull.Value;
                        object result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        { Show("The post could not be saved. Its editing window or attached recipe may no longer be available.", false); return; }
                        saved = true;
                        Navigate("Post.aspx?id=" + Convert.ToInt32(result));
                    }
                }
                catch (InvalidDataException ex) { Show(ex.Message, false); }
                catch (IOException ex) { Trace.Warn("Community", "Photo upload failed.", ex); Show("Your photo could not be uploaded. Please try again.", false); }
                catch (UnauthorizedAccessException ex) { Trace.Warn("Community", "Photo upload failed.", ex); Show("Your photo could not be uploaded. Please try again.", false); }
                finally { if (!saved) SafeDeleteUploadedImage(uploaded); }
            });
        }
    }
}
