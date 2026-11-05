using Masaking.Infrastructure;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Masaking.Recipes
{
    public partial class RecipeDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int recipeId;

            if (!int.TryParse(Request.QueryString["id"], out recipeId) || recipeId <= 0)
            {
                ShowRecipeNotFound();
                return;
            }

            if (!IsPostBack)
            {
                LoadRecipe(recipeId);

                if (pnlRecipe.Visible)
                {
                    LoadFavourite(recipeId);
                }
            }
        }


        private void LoadRecipe(int recipeId)
        {
            SqlConnection con = Database.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT
                    r.title,
                    r.description,
                    r.cuisine,
                    r.difficulty,
                    r.preparationTime,
                    r.estimatedCost,
                    r.servings,
                    r.image,
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
                ) AS ratings
                    ON ratings.recipeID = r.recipeID
                WHERE r.recipeID = @recipeID
                    AND r.status = 'Approved';", con);

            cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable recipes = new DataTable();
            adapter.Fill(recipes);

            con.Close();

            if (recipes.Rows.Count == 0)
            {
                ShowRecipeNotFound();
                return;
            }

            DataRow recipe = recipes.Rows[0];

            recipeTitle.Text = Server.HtmlEncode(recipe["title"].ToString());
            recipeCuisine.Text = Server.HtmlEncode(recipe["cuisine"].ToString());
            recipeCategory.Text = Server.HtmlEncode(recipe["categoryName"].ToString());
            recipeDifficulty.Text = Server.HtmlEncode(recipe["difficulty"].ToString());
            recipeDescription.Text = Server.HtmlEncode(recipe["description"].ToString());
            recipeTime.Text = recipe["preparationTime"].ToString() + " mins";
            recipeServings.Text = recipe["servings"].ToString();
            recipeCost.Text = Convert.ToDecimal(recipe["estimatedCost"]).ToString("0.00");
            recipeImage.ImageUrl = recipe["image"].ToString();
            recipeImage.AlternateText = recipe["title"].ToString();

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

            Page.Title = recipe["title"].ToString();

            ingredientServings.Text = recipe["servings"].ToString();
            LoadRecipeContents(recipeId);
        }

        private void LoadRecipeContents(int recipeId)
        {
            SqlConnection con = Database.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT ingredientName, quantity, unit, isOptional
                FROM dbo.ingredients
                WHERE recipeID = @recipeID
                ORDER BY displayOrder, ingredientID;

                SELECT stepNumber, instruction, image, estimatedMinutes
                FROM dbo.steps
                WHERE recipeID = @recipeID
                ORDER BY stepNumber, stepID;", con);

            cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet contents = new DataSet();
            adapter.Fill(contents);

            con.Close();

            rptIngredients.DataSource = contents.Tables[0];
            rptIngredients.DataBind();

            rptSteps.DataSource = contents.Tables[1];
            rptSteps.DataBind();

            lblIngredientsEmpty.Visible = contents.Tables[0].Rows.Count == 0;
            lblStepsEmpty.Visible = contents.Tables[1].Rows.Count == 0;
        }

        private void LoadFavourite(int recipeId)
        {
            bool isFavourite = false;

            if (Database.UserId.HasValue)
            {
                SqlConnection con = Database.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand(@"
                        SELECT COUNT(*)
                        FROM dbo.favourites
                        WHERE userID = @userID
                            AND recipeID = @recipeID;", con);

                    cmd.Parameters.Add("@userID", SqlDbType.Int).Value =
                        Database.UserId.Value;

                    cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value =
                        recipeId;

                    isFavourite = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                finally
                {
                    con.Close();
                }
            }

            favouriteIcon.Attributes["class"] =
                isFavourite ? "bi bi-heart-fill" : "bi bi-heart";

            btnFavourite.Attributes["aria-pressed"] =
                isFavourite ? "true" : "false";

            btnFavourite.Attributes["aria-label"] =
                isFavourite ? "Remove from favourites" : "Add to favourites";

            btnFavourite.Attributes["title"] =
                btnFavourite.Attributes["aria-label"];
        }

        protected void rptIngredients_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView ingredient = (DataRowView)e.Item.DataItem;
                CheckBox ingredientCheck = (CheckBox)e.Item.FindControl("ingredientCheck");
                Label ingredientQuantity = (Label)e.Item.FindControl("ingredientQuantity");

                string name = ingredient["ingredientName"].ToString();

                if (Convert.ToBoolean(ingredient["isOptional"]))
                {
                    name += " (optional)";
                }

                ingredientCheck.Text = Server.HtmlEncode(name);

                string quantity = Convert.ToDecimal(ingredient["quantity"]).ToString("0.##");

                ingredientQuantity.Text = Server.HtmlEncode(quantity + " " + ingredient["unit"].ToString());
            }
        }

        protected void rptSteps_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView step = (DataRowView)e.Item.DataItem;

                Label stepNumber = (Label)e.Item.FindControl("stepNumber");
                Image stepImage = (Image)e.Item.FindControl("stepImage");
                Label stepMinutes = (Label)e.Item.FindControl("stepMinutes");
                Label stepInstruction = (Label)e.Item.FindControl("stepInstruction");

                stepNumber.Text = step["stepNumber"].ToString();

                stepInstruction.Text = Server.HtmlEncode(step["instruction"].ToString());

                stepMinutes.Text = step["estimatedMinutes"].ToString() + " mins";

                string image = step["image"].ToString();

                stepImage.Visible = !string.IsNullOrWhiteSpace(image);
                stepImage.ImageUrl = image;
                stepImage.AlternateText = "Step " + step["stepNumber"].ToString();
            }
        }

        private void ShowRecipeNotFound()
        {
            pnlRecipe.Visible = false;

            lblMessage.Text = "This recipe could not be found.";
            lblMessage.Visible = true;

            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
        }

        protected void btnFavourite_ServerClick(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response))
            {
                return;
            }

            int recipeId;

            if (!int.TryParse(Request.QueryString["id"], out recipeId) || recipeId <= 0)
            {
                ShowRecipeNotFound();
                return;
            }

            SqlConnection con = Database.Open();

            try
            {
                SqlCommand cmd = new SqlCommand(@"
                    IF EXISTS (
                        SELECT 1
                        FROM dbo.recipes
                        WHERE recipeID = @recipeID
                          AND status = 'Approved'
                    )
                    BEGIN
                        IF EXISTS (
                            SELECT 1
                            FROM dbo.favourites
                            WHERE userID = @userID
                              AND recipeID = @recipeID
                        )
                        BEGIN
                            DELETE FROM dbo.favourites
                            WHERE userID = @userID
                              AND recipeID = @recipeID;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO dbo.favourites
                                (userID, recipeID, savedAt)
                            VALUES
                                (@userID, @recipeID, SYSUTCDATETIME());
                        END
                    END;", con);

                cmd.Parameters.Add("@userID", SqlDbType.Int).Value = Database.UserId.Value;
                cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;

                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }

            LoadRecipe(recipeId);

            if (pnlRecipe.Visible)
            {
                LoadFavourite(recipeId);
            }
        }
    }
}