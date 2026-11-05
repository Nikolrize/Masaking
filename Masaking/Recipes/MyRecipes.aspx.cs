using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Recipes
{
    public partial class MyRecipes : ResponsivePage
    {
        protected Repeater rptRecipes;
        protected Panel pnlEmpty;
        protected Label lblMessage;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response))
                return;
            if (!IsPostBack)
            {
                if (Request.QueryString["saved"] == "1")
                    Show("Your recipe has been submitted for approval.", true);
                else if (Request.QueryString["saved"] == "updated")
                    Show("Your recipe changes have been saved.", true);
                BindRecipes();
            }
        }

        private void BindRecipes()
        {
            string sql = "SELECT r.recipeID,r.title,r.cuisine,r.region,r.difficulty,r.preparationTime,r.estimatedCost,r.servings,r.image,r.status,r.createdAt,COALESCE(AVG(CAST(v.rating AS decimal(4,2))),0) averageRating,COUNT(v.reviewID) reviewCount FROM recipes r LEFT JOIN reviews v ON v.recipeID=r.recipeID AND v.status='Approved' WHERE r.userID=@user GROUP BY r.recipeID,r.title,r.cuisine,r.region,r.difficulty,r.preparationTime,r.estimatedCost,r.servings,r.image,r.status,r.createdAt ORDER BY r.createdAt DESC";
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@user", Database.UserId.Value);
                DataTable data = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(data);
                rptRecipes.DataSource = data;
                rptRecipes.DataBind();
                pnlEmpty.Visible = data.Rows.Count == 0;
            }
        }

        protected string OwnedImageUrl(object value)
        {
            string image = Convert.ToString(value ?? "");
            Uri uri;
            if (Uri.TryCreate(image, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                return image;
            const string oldPrefix = "/Content/Uploads/Recipes/";
            const string prefix = "/Content/images/Uploads/Recipes/";
            if (image.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase))
                image = prefix + image.Substring(oldPrefix.Length);
            if (image.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                string file = image.Substring(prefix.Length);
                Guid id;
                if (Path.GetFileName(file) == file && file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && Guid.TryParseExact(Path.GetFileNameWithoutExtension(file), "N", out id))
                    return image;
            }

            return "../Content/images/Masaking%20Logo.png";
        }

        protected void rptRecipes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "deleteRecipe")
                return;
            int id;
            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id))
                return;
            using (SqlConnection cn = Database.Open())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    using (SqlCommand own = new SqlCommand("SELECT COUNT(*) FROM recipes WHERE recipeID=@id AND userID=@user", cn, tx))
                    {
                        own.Parameters.AddWithValue("@id", id);
                        own.Parameters.AddWithValue("@user", Database.UserId.Value);
                        if (Convert.ToInt32(own.ExecuteScalar()) != 1)
                            throw new InvalidOperationException();
                    }

                    string[] deletes =
                    {
                        "DELETE FROM postComments WHERE postID IN (SELECT postID FROM posts WHERE recipeID=@id)",
                        "DELETE FROM posts WHERE recipeID=@id",
                        "DELETE FROM simulationProgress WHERE simulationID IN (SELECT simulationID FROM simulations WHERE recipeID=@id)",
                        "DELETE FROM simulations WHERE recipeID=@id",
                        "DELETE FROM recipeProgress WHERE recipeID=@id",
                        "DELETE FROM reviews WHERE recipeID=@id",
                        "DELETE FROM favourites WHERE recipeID=@id",
                        "DELETE FROM recipesCategories WHERE recipeID=@id",
                        "DELETE FROM ingredients WHERE recipeID=@id",
                        "DELETE FROM steps WHERE recipeID=@id",
                        "DELETE FROM recipes WHERE recipeID=@id AND userID=@user"
                    };
                    foreach (string sql in deletes)
                        using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@user", Database.UserId.Value);
                            cmd.ExecuteNonQuery();
                        }

                    tx.Commit();
                    Show("Recipe and its related community activity have been deleted.", true);
                }
                catch
                {
                    tx.Rollback();
                    Show("That recipe could not be deleted. Refresh the page and try again.", false);
                }
            }

            BindRecipes();
        }

        private void Show(string text, bool success)
        {
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Text = Server.HtmlEncode(text);
        }

        protected void rptRecipes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            var row = (System.Data.DataRowView)e.Item.DataItem;
            var lnkRecipeDetails = (HyperLink)e.Item.FindControl("lnkRecipeDetails");
            lnkRecipeDetails.NavigateUrl = Convert.ToString("RecipeDetails.aspx?ID=" + row["recipeID"]);
            var imgDecoration = (Image)e.Item.FindControl("imgDecoration");
            imgDecoration.ImageUrl = Convert.ToString(OwnedImageUrl(row["image"]));
            imgDecoration.AlternateText = Convert.ToString(row["title"]);
            var lblCuisine = (Label)e.Item.FindControl("lblCuisine");
            lblCuisine.Text = Server.HtmlEncode(Convert.ToString(row["cuisine"]));
            var lblRegion = (Label)e.Item.FindControl("lblRegion");
            lblRegion.Text = Server.HtmlEncode(Convert.ToString(row["region"]));
            var lnkRecipeDetails2 = (HyperLink)e.Item.FindControl("lnkRecipeDetails2");
            lnkRecipeDetails2.NavigateUrl = Convert.ToString("RecipeDetails.aspx?ID=" + row["recipeID"]);
            var lblTitle = (Label)e.Item.FindControl("lblTitle");
            lblTitle.Text = Server.HtmlEncode(Convert.ToString(row["title"]));
            var lblStatus = (Label)e.Item.FindControl("lblStatus");
            lblStatus.Text = Server.HtmlEncode(Convert.ToString(row["status"]));
            var lblCreatedAt = (Label)e.Item.FindControl("lblCreatedAt");
            lblCreatedAt.Text = Server.HtmlEncode(Convert.ToString(String.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:dd MMM yyyy}", row["createdAt"])));
            var lblAverageRating = (Label)e.Item.FindControl("lblAverageRating");
            lblAverageRating.Text = Server.HtmlEncode(Convert.ToString(String.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:0.0}", row["averageRating"])));
            var lblReviewCount = (Label)e.Item.FindControl("lblReviewCount");
            lblReviewCount.Text = Server.HtmlEncode(Convert.ToString(row["reviewCount"]));
            var lblPreparationTime = (Label)e.Item.FindControl("lblPreparationTime");
            lblPreparationTime.Text = Server.HtmlEncode(Convert.ToString(row["preparationTime"]));
            var lblDifficulty = (Label)e.Item.FindControl("lblDifficulty");
            lblDifficulty.Text = Server.HtmlEncode(Convert.ToString(row["difficulty"]));
            var lblServings = (Label)e.Item.FindControl("lblServings");
            lblServings.Text = Server.HtmlEncode(Convert.ToString(row["servings"]));
            var lblEstimatedCost = (Label)e.Item.FindControl("lblEstimatedCost");
            lblEstimatedCost.Text = Server.HtmlEncode(Convert.ToString(String.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:0.00}", row["estimatedCost"])));
            var lnkEditRecipe = (HyperLink)e.Item.FindControl("lnkEditRecipe");
            lnkEditRecipe.NavigateUrl = Convert.ToString("SubmitRecipe.aspx?ID=" + row["recipeID"]);
            var btnDelete = (LinkButton)e.Item.FindControl("btnDelete");
            btnDelete.CommandArgument = Convert.ToString(row["recipeID"]);
            var lnkRecipeDetails3 = (HyperLink)e.Item.FindControl("lnkRecipeDetails3");
            lnkRecipeDetails3.NavigateUrl = Convert.ToString("RecipeDetails.aspx?ID=" + row["recipeID"]);
        }
    }
}
