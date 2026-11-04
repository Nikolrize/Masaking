using Masaking.Infrastructure;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Masaking
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCategories();
                LoadPopularRecipes();
            }
        }

        private void LoadCategories()
        {
            SqlConnection con = Database.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT categoryID, categoryName
                FROM dbo.categories
                WHERE isActive = 1
                ORDER BY categoryID;", con);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable categories = new DataTable();
            adapter.Fill(categories);

            rptCategories.DataSource = categories;
            rptCategories.DataBind();

            con.Close();
        }

        protected void rptCategories_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView category = (DataRowView)e.Item.DataItem;
                HyperLink categoryLink = (HyperLink)e.Item.FindControl("categoryLink");
                HtmlGenericControl categoryIcon = (HtmlGenericControl)e.Item.FindControl("categoryIcon");
                Label categoryName = (Label)e.Item.FindControl("categoryName");

                string name = category["categoryName"].ToString();
                string tileClass = "tile-others";
                string iconClass = "bi bi-three-dots";

                switch (name)
                {
                    case "Main Dishes":
                        tileClass = "tile-main";
                        iconClass = "bi bi-egg-fried";
                        break;

                    case "Desserts":
                        tileClass = "tile-desserts";
                        iconClass = "bi bi-cake2";
                        break;

                    case "Beverages":
                        tileClass = "tile-beverages";
                        iconClass = "bi bi-cup-straw";
                        break;

                    case "Snacks":
                        tileClass = "tile-snacks";
                        iconClass = "bi bi-cookie";
                        break;

                    case "Traditional":
                        tileClass = "tile-traditional";
                        iconClass = "bi bi-basket";
                        break;
                }

                categoryName.Text = Server.HtmlEncode(name);
                categoryLink.CssClass = "category-card " + tileClass;
                categoryIcon.Attributes["class"] = iconClass;

                categoryLink.NavigateUrl =
                    ResolveUrl("~/Recipes/BrowseRecipes.aspx") +
                    "?category=" + Server.UrlEncode(name);
            }
        }

        private void LoadPopularRecipes()
        {
            SqlConnection con = Database.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT TOP (4)
                    r.recipeID,
                    r.title,
                    r.image,
                    r.cuisine,
                    r.difficulty,
                    r.preparationTime,
                    c.categoryName,
                    COALESCE(ratings.reviewCount, 0) AS reviewCount,
                    ratings.averageRating
                FROM dbo.recipes AS r
                LEFT JOIN dbo.recipesCategories AS rc
                    ON rc.recipeID = r.recipeID
                LEFT JOIN dbo.categories AS c
                    ON c.categoryID = rc.categoryID
                LEFT JOIN (
                    SELECT
                        recipeID,
                        COUNT(*) AS reviewCount,
                        AVG(CAST(rating AS decimal(10, 2))) AS averageRating
                    FROM dbo.reviews
                    WHERE status = 'Approved'
                    GROUP BY recipeID
                ) AS ratings ON ratings.recipeID = r.recipeID
                WHERE r.status = 'Approved'
                ORDER BY
                    reviewCount DESC,
                    ratings.averageRating DESC,
                    r.createdAt DESC,
                    r.recipeID DESC;", con);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable recipes = new DataTable();
            adapter.Fill(recipes);

            rptPopularRecipes.DataSource = recipes;
            rptPopularRecipes.DataBind();

            lblNoPopularRecipes.Visible = recipes.Rows.Count == 0;

            con.Close();
        }

        protected void rptPopularRecipes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView recipe = (DataRowView)e.Item.DataItem;

                Label recipeTitle = (Label)e.Item.FindControl("recipeTitle");
                Label recipeCategory = (Label)e.Item.FindControl("recipeCategory");
                Image recipeImage = (Image)e.Item.FindControl("recipeImage");
                HyperLink recipeLink = (HyperLink)e.Item.FindControl("recipeLink");
                Label recipeRating = (Label)e.Item.FindControl("recipeRating");
                Label recipeCuisine = (Label)e.Item.FindControl("recipeCuisine");
                Label recipePreparationTime = (Label)e.Item.FindControl("recipePreparationTime");
                Label recipeDifficulty = (Label)e.Item.FindControl("recipeDifficulty");
                PlaceHolder phRatingStar = (PlaceHolder)e.Item.FindControl("phRatingStar");

                recipeTitle.Text = Server.HtmlEncode(recipe["title"].ToString());
                recipeCategory.Text = Server.HtmlEncode(recipe["categoryName"].ToString());
                recipeImage.ImageUrl = recipe["image"].ToString();
                recipeImage.AlternateText = recipe["title"].ToString();
                recipeLink.NavigateUrl = ResolveUrl("~/Recipes/RecipeDetails.aspx") + "?id=" + recipe["recipeID"].ToString();
                recipeCuisine.Text = Server.HtmlEncode(recipe["cuisine"].ToString());
                recipePreparationTime.Text = recipe["preparationTime"].ToString() + " mins";

                string difficulty = recipe["difficulty"].ToString();
                recipeDifficulty.Text = Server.HtmlEncode(difficulty);
                recipeDifficulty.CssClass = "badge rounded-pill recipe-difficulty " + difficulty.ToLowerInvariant();

                int reviewCount = Convert.ToInt32(recipe["reviewCount"]);
                phRatingStar.Visible = reviewCount > 0;
                if (reviewCount > 0)
                {
                    decimal averageRating = Convert.ToDecimal(recipe["averageRating"]);
                    recipeRating.Text = averageRating.ToString("0.0") + " (" + reviewCount + " reviews)";
                }
                else
                {
                    recipeRating.Text = "No reviews yet";
                }
            }
        }
    }
}