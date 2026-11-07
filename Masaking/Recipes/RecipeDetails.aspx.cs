using Masaking.Controls;
using Masaking.Infrastructure;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Masaking.Recipes
{
    public partial class RecipeDetails : ResponsivePage
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
            DataTable recipes = RecipeRepository.GetRecipeById(recipeId);

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
            recipeAuthor.Text = Server.HtmlEncode(recipe["displayName"].ToString());
            authorAvatar.DisplayAvatar(recipe["displayName"], recipe["authorImage"]);

            lnkAuthor.NavigateUrl = ResolveUrl("~/Account/Profile") + "?id=" + recipe["userID"].ToString();

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

            bool signedIn = Database.UserId.HasValue;
            bool isAuthor = signedIn &&
                Database.UserId.Value == Convert.ToInt32(recipe["userID"]);
            bool isApproved = recipe["status"].ToString() == "Approved";

            btnFavourite.Visible = isApproved && !isAuthor;
            lnkStartLearning.Visible = isApproved;
            pnlReviewForm.Visible = signedIn && !isAuthor;
            lblReviewHint.Visible = !pnlReviewForm.Visible;
            lblReviewHint.Text = isAuthor
                ? "You cannot review your own recipe."
                : "Sign in to rate this recipe and leave a comment.";

            LoadReviews(recipeId);
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

        private void LoadReviews(int recipeId)
        {
            SqlConnection con = Database.Open();
            DataTable reviews = new DataTable();

            try
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        v.rating,
                        v.comment,
                        v.createdAt,
                        v.updatedAt,
                        u.displayName,
                        u.image
                    FROM dbo.reviews AS v
                    INNER JOIN dbo.users AS u
                        ON u.userID = v.userID
                    WHERE v.recipeID = @recipeID
                      AND v.status = 'Approved'
                    ORDER BY v.createdAt DESC, v.reviewID DESC;", con);

                cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(reviews);
            }
            finally
            {
                con.Close();
            }

            rptReviews.DataSource = reviews;
            rptReviews.DataBind();

            lblNoReviews.Visible = reviews.Rows.Count == 0;
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

        protected void rptReviews_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView review = (DataRowView)e.Item.DataItem;

                UserAvatar avatar = (UserAvatar)e.Item.FindControl("reviewAvatar");
                Label name = (Label)e.Item.FindControl("reviewName");
                Label date = (Label)e.Item.FindControl("reviewDate");
                Label stars = (Label)e.Item.FindControl("reviewStars");
                Label comment = (Label)e.Item.FindControl("reviewComment");

                avatar.DisplayAvatar(review["displayName"], review["image"]);
                name.Text = Server.HtmlEncode(review["displayName"].ToString());

                bool wasUpdated = review["updatedAt"] != DBNull.Value;
                DateTime timestamp = Convert.ToDateTime(wasUpdated ? review["updatedAt"] : review["createdAt"]);
                DateTime utcDate = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
                TimeZoneInfo malaysiaTime = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                date.Text = (wasUpdated ? "Updated on " : "") + TimeZoneInfo.ConvertTimeFromUtc(utcDate, malaysiaTime).ToString("dd MMM yyyy, hh:mm tt");

                int rating = Convert.ToInt32(review["rating"]);
                stars.Text = new string('★', rating) +
                    new string('☆', 5 - rating);
                comment.Text = Server.HtmlEncode(review["comment"].ToString());
                comment.Visible = !string.IsNullOrWhiteSpace(review["comment"].ToString());
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

        private void ShowReviewMessage(string message, bool success)
        {
            lblReviewMessage.Text = Server.HtmlEncode(message);

            lblReviewMessage.CssClass = success
                ? "alert alert-success d-block"
                : "alert alert-danger d-block";

            lblReviewMessage.Visible = true;
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
                              AND userID <> @userID
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
        protected void btnSubmitReview_Click(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response))
            {
                return;
            }

            int recipeId;

            if (!int.TryParse(Request.QueryString["id"], out recipeId) ||
                recipeId <= 0)
            {
                ShowRecipeNotFound();
                return;
            }

            int rating;
            string comment = txtReviewComment.Text.Trim();

            if (!int.TryParse(ddlReviewRating.SelectedValue, out rating) ||
                rating < 1 || rating > 5)
            {
                ShowReviewMessage("Choose a rating from 1 to 5.", false);
                return;
            }

            if (comment.Length > 1000)
            {
                ShowReviewMessage("Keep your comment within 1,000 characters.", false);
                return;
            }

            SqlConnection con = Database.Open();

            try
            {
                SqlTransaction transaction = con.BeginTransaction(IsolationLevel.Serializable);

                SqlCommand check = new SqlCommand(@"
                    SELECT userID
                    FROM dbo.recipes
                    WHERE recipeID = @recipeID
                        AND status = 'Approved';", con, transaction);

                check.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;

                object authorId = check.ExecuteScalar();

                if (authorId == null)
                {
                    transaction.Rollback();
                    ShowRecipeNotFound();
                    return;
                }

                if (Convert.ToInt32(authorId) == Database.UserId.Value)
                {
                    transaction.Rollback();
                    ShowReviewMessage("You cannot review your own recipe.", false);
                    return;
                }

                SqlCommand cmd = new SqlCommand(@"
                    IF EXISTS (
                        SELECT 1
                        FROM dbo.reviews WITH (UPDLOCK, HOLDLOCK)
                        WHERE userID = @userID
                          AND recipeID = @recipeID
                    )
                    BEGIN
                        UPDATE dbo.reviews
                        SET rating = @rating,
                            comment = @comment,
                            status = 'Approved',
                            updatedAt = SYSUTCDATETIME()
                        WHERE userID = @userID
                          AND recipeID = @recipeID;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO dbo.reviews
                            (rating, comment, status, createdAt,
                             updatedAt, userID, recipeID)
                        VALUES
                            (@rating, @comment, 'Approved', SYSUTCDATETIME(),
                             NULL, @userID, @recipeID);
                    END;", con, transaction);

                cmd.Parameters.Add("@userID", SqlDbType.Int).Value = Database.UserId.Value;
                cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;
                cmd.Parameters.Add("@rating", SqlDbType.TinyInt).Value = rating;
                cmd.Parameters.Add("@comment", SqlDbType.NVarChar, 1000).Value =
                    string.IsNullOrWhiteSpace(comment)
                        ? (object)DBNull.Value
                        : comment;

                cmd.ExecuteNonQuery();
                transaction.Commit();
            }
            finally
            {
                con.Close();
            }

            LoadRecipe(recipeId);

            txtReviewComment.Text = "";
            ddlReviewRating.ClearSelection();

            ShowReviewMessage("Your review has been saved.", true);
        }
    }
}
