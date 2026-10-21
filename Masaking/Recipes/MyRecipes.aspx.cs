using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Recipes
{
    public partial class MyRecipes : System.Web.UI.Page
    {
        protected Repeater rptRecipes;
        protected Panel pnlEmpty;
        protected Label lblMessage;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            if (!IsPostBack)
            {
                if (Request.QueryString["saved"] == "1") Show("Your recipe has been submitted for approval.", true);
                BindRecipes();
            }
        }

        private void BindRecipes()
        {
            string sql = "SELECT r.recipeID,r.title,r.cuisine,r.region,r.difficulty,r.preparationTime,r.estimatedCost,r.servings,r.image,r.status,r.createdAt,COALESCE(AVG(CAST(v.rating AS decimal(4,2))),0) averageRating,COUNT(v.reviewID) reviewCount FROM recipes r LEFT JOIN reviews v ON v.recipeID=r.recipeID AND v.status='Approved' WHERE r.userID=@user GROUP BY r.recipeID,r.title,r.cuisine,r.region,r.difficulty,r.preparationTime,r.estimatedCost,r.servings,r.image,r.status,r.createdAt ORDER BY r.createdAt DESC";
            using (SqlConnection cn = Database.Open()) using (SqlCommand cmd = new SqlCommand(sql, cn))
            { cmd.Parameters.AddWithValue("@user", Database.UserId.Value); DataTable data = new DataTable(); using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(data); rptRecipes.DataSource = data; rptRecipes.DataBind(); pnlEmpty.Visible = data.Rows.Count == 0; }
        }

        protected string OwnedImageUrl(object value)
        {
            string image = Convert.ToString(value ?? "");
            Uri uri;
            if (Uri.TryCreate(image, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) return HttpUtility.HtmlAttributeEncode(image);
            const string oldPrefix = "/Content/Uploads/Recipes/";
            const string prefix = "/Content/images/Uploads/Recipes/";
            if (image.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase)) image = prefix + image.Substring(oldPrefix.Length);
            if (image.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                string file = image.Substring(prefix.Length); Guid id;
                if (Path.GetFileName(file) == file && file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && Guid.TryParseExact(Path.GetFileNameWithoutExtension(file), "N", out id)) return HttpUtility.HtmlAttributeEncode(image);
            }
            return "../Content/images/Masaking%20Logo.png";
        }

        protected void rptRecipes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "deleteRecipe") return;
            int id; if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id)) return;
            using (SqlConnection cn = Database.Open()) using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    using (SqlCommand own = new SqlCommand("SELECT COUNT(*) FROM recipes WHERE recipeID=@id AND userID=@user", cn, tx))
                    { own.Parameters.AddWithValue("@id", id); own.Parameters.AddWithValue("@user", Database.UserId.Value); if (Convert.ToInt32(own.ExecuteScalar()) != 1) throw new InvalidOperationException(); }
                    string[] deletes = {
                        "DELETE FROM postComments WHERE postID IN (SELECT postID FROM posts WHERE recipeID=@id)",
                        "DELETE FROM posts WHERE recipeID=@id",
                        "DELETE FROM simulationProgress WHERE simulationID IN (SELECT simulationID FROM simulations WHERE recipeID=@id)",
                        "DELETE FROM simulations WHERE recipeID=@id", "DELETE FROM recipeProgress WHERE recipeID=@id",
                        "DELETE FROM reviews WHERE recipeID=@id", "DELETE FROM favourites WHERE recipeID=@id",
                        "DELETE FROM recipesCategories WHERE recipeID=@id", "DELETE FROM ingredients WHERE recipeID=@id",
                        "DELETE FROM steps WHERE recipeID=@id", "DELETE FROM recipes WHERE recipeID=@id AND userID=@user"
                    };
                    foreach (string sql in deletes) using (SqlCommand cmd = new SqlCommand(sql, cn, tx)) { cmd.Parameters.AddWithValue("@id", id); cmd.Parameters.AddWithValue("@user", Database.UserId.Value); cmd.ExecuteNonQuery(); }
                    tx.Commit(); Show("Recipe and its related community activity have been deleted.", true);
                }
                catch { tx.Rollback(); Show("That recipe could not be deleted. Refresh the page and try again.", false); }
            }
            BindRecipes();
        }

        private void Show(string text, bool success) { lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block"; lblMessage.Text = Server.HtmlEncode(text); }
    }
}
