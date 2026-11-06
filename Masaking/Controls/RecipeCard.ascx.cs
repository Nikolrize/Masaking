using Masaking.Infrastructure;
using System;
using System.Data;

namespace Masaking.Controls
{
    public partial class RecipeCard : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void DisplayRecipe(DataRowView recipe)
        {
            recipeTitle.Text = Server.HtmlEncode(recipe["title"].ToString());
            recipeCategory.Text = Server.HtmlEncode(recipe["categoryName"].ToString());
            recipeCuisine.Text = Server.HtmlEncode(recipe["cuisine"].ToString());
            recipeImage.ImageUrl = recipe["image"].ToString();
            recipeImage.AlternateText = recipe["title"].ToString();
            recipeLink.NavigateUrl = ResolveUrl("~/Recipes/RecipeDetails") + "?id=" + recipe["recipeID"].ToString();
            recipePreparationTime.Text = recipe["preparationTime"].ToString() + " mins";
            recipeBudget.Text = "RM" + Convert.ToDecimal(recipe["estimatedCost"]).ToString("0.00");
            recipeServings.Text = recipe["servings"].ToString();
            recipeAuthor.Text = Server.HtmlEncode(recipe["displayName"].ToString());
            authorAvatar.ImageUrl = Database.AvatarUrl(recipe["authorImage"]);

            string difficulty = recipe["difficulty"].ToString();
            recipeDifficulty.Text = Server.HtmlEncode(difficulty);
            recipeDifficulty.CssClass =
                "badge rounded-pill recipe-difficulty " +
                difficulty.ToLowerInvariant();

            int reviewCount = Convert.ToInt32(recipe["reviewCount"]);
            phRatingStar.Visible = reviewCount > 0;

            if (reviewCount > 0)
            {
                decimal averageRating =
                    Convert.ToDecimal(recipe["averageRating"]);

                recipeRating.Text =
                    averageRating.ToString("0.0") +
                    " (" + reviewCount + " reviews)";
            }
            else
            {
                recipeRating.Text = "No reviews yet";
            }
        }
    }
}