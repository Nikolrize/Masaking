using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Community
{
    public partial class Reviews : System.Web.UI.Page
    {
        protected Label lblMessage;
        protected Panel pnlForm, pnlSignIn, pnlEmpty;
        protected DropDownList ddlRecipe, ddlRating;
        protected TextBox txtComment;
        protected Repeater rptReviews;

        protected void Page_Load(object sender, EventArgs e)
        {
            pnlForm.Visible = Database.UserId.HasValue; pnlSignIn.Visible = !Database.UserId.HasValue;
            if (!IsPostBack) { BindRecipeOptions(); BindReviews(); }
        }
        private void BindRecipeOptions()
        {
            using (SqlConnection cn = Database.Open()) using (SqlCommand cmd = new SqlCommand("SELECT recipeID,title FROM recipes WHERE status='Approved' ORDER BY title", cn))
            { using (SqlDataReader rd = cmd.ExecuteReader()) { ddlRecipe.DataSource = rd; ddlRecipe.DataTextField = "title"; ddlRecipe.DataValueField = "recipeID"; ddlRecipe.DataBind(); } ddlRecipe.Items.Insert(0, new ListItem("Choose a recipe…", "")); }
        }
        private void BindReviews()
        {
            using (SqlConnection cn = Database.Open()) using (SqlCommand cmd = new SqlCommand("SELECT v.recipeID,r.title,u.displayName,REPLICATE(N'★',v.rating) stars,COALESCE(v.comment,N'Rated this recipe.') comment,v.createdAt FROM reviews v JOIN recipes r ON r.recipeID=v.recipeID JOIN users u ON u.userID=v.userID WHERE v.status='Approved' AND r.status='Approved' ORDER BY v.createdAt DESC", cn))
            { DataTable dt = new DataTable(); using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt); rptReviews.DataSource = dt; rptReviews.DataBind(); pnlEmpty.Visible = dt.Rows.Count == 0; }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int recipeId, rating;
            if (!Database.UserId.HasValue) { Response.Redirect("../Account/Login.aspx"); return; }
            if (!Int32.TryParse(ddlRecipe.SelectedValue, out recipeId) || !Int32.TryParse(ddlRating.SelectedValue, out rating) || rating < 1 || rating > 5 || txtComment.Text.Trim().Length > 1000) { Show("Choose a recipe, a rating from 1 to 5, and keep the review within 1000 characters.", false); return; }
            using (SqlConnection cn = Database.Open()) using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    using (SqlCommand available = new SqlCommand("SELECT COUNT(*) FROM recipes WHERE recipeID=@recipe AND status='Approved'", cn, tx))
                    { available.Parameters.AddWithValue("@recipe", recipeId); if (Convert.ToInt32(available.ExecuteScalar()) != 1) throw new InvalidOperationException("That recipe is no longer available."); }
                    using (SqlCommand cmd = new SqlCommand("IF EXISTS(SELECT 1 FROM reviews WITH (UPDLOCK,HOLDLOCK) WHERE userID=@user AND recipeID=@recipe) UPDATE reviews SET rating=@rating,comment=@comment,status='Pending',updatedAt=@now WHERE userID=@user AND recipeID=@recipe ELSE INSERT INTO reviews(rating,comment,status,createdAt,updatedAt,userID,recipeID) VALUES(@rating,@comment,'Pending',@now,NULL,@user,@recipe)", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@user", Database.UserId.Value); cmd.Parameters.AddWithValue("@recipe", recipeId); cmd.Parameters.AddWithValue("@rating", rating);
                        cmd.Parameters.AddWithValue("@comment", String.IsNullOrWhiteSpace(txtComment.Text) ? (object)DBNull.Value : txtComment.Text.Trim()); cmd.Parameters.AddWithValue("@now", DateTime.UtcNow); cmd.ExecuteNonQuery();
                    }
                    tx.Commit(); Show("Thank you! Your review is waiting for community moderation.", true); txtComment.Text = "";
                }
                catch (InvalidOperationException ex) { tx.Rollback(); Show(ex.Message, false); }
            }
            BindReviews();
        }
        private void Show(string text, bool success) { lblMessage.Text = Server.HtmlEncode(text); lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block"; }
    }
}
