using Masaking.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;

namespace Masaking.Recipes
{
    public partial class SubmitRecipe : ResponsivePage
    {
        private int? editId;

        private class Ingredient
        {
            public string Name;
            public decimal Quantity;
            public string Unit;
        }

        private class CookingStep
        {
            public int UploadIndex;
            public string Instruction;
            public string Image;
            public int Minutes;
        }

        protected override void OnInit(EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response))
            {
                return;
            }

            Form.Enctype = "multipart/form-data";

            if (!ReadEditId())
            {
                return;
            }

            SetPageText();

            if (!IsPostBack)
            {
                LoadCategories();

                if (editId.HasValue)
                {
                    if (!LoadRecipe())
                    {
                        return;
                    }
                }
                else
                {
                    ingredientEditor.Initialise();
                    stepEditor.Initialise();
                }
            }
        }

        private bool ReadEditId()
        {
            string value = Request.QueryString["id"];

            if (value == null)
            {
                editId = null;
                return true;
            }

            int id;

            if (!int.TryParse(value, out id) || id <= 0)
            {
                ShowUnavailableRecipe();
                return false;
            }

            editId = id;
            return true;
        }

        private void SetPageText()
        {
            Page.Title = editId.HasValue ? "Edit Recipe" : "Submit Recipe";
            recipeHeading.InnerText = editId.HasValue
                ? "Edit your recipe" : "Share your recipe";

            btnSave.Text = editId.HasValue
                ? "Save and resubmit" : "Submit for review";

            lblApprovalNotice.Text = editId.HasValue
                ? "Saving changes returns your recipe to Pending for approval."
                : "Your recipe will appear publicly after approval.";
        }

        // The same data is read again when saving: ownership is never trusted
        // just because the user was allowed to open the form earlier
        private DataSet ReadOwnedRecipe(
            SqlConnection con, SqlTransaction transaction, int recipeId)
        {
            SqlCommand cmd = new SqlCommand(@"
                SELECT
                    r.title,
                    r.description,
                    r.cuisine,
                    r.region,
                    r.difficulty,
                    r.preparationTime,
                    r.estimatedCost,
                    r.servings,
                    r.image,
                    (
                        SELECT rc.categoryID
                        FROM dbo.recipesCategories AS rc
                        WHERE rc.recipeID = r.recipeID
                    ) AS categoryID
                FROM dbo.recipes AS r
                WHERE r.recipeID = @recipeID
                    AND r.userID = @userID
                    AND r.status IN ('Approved', 'Pending', 'Rejected');

                SELECT 
                    i.ingredientName, 
                    i.quantity, 
                    i.unit, 
                    i.image,                      
                    i.isOptional, 
                    i.displayOrder
                FROM dbo.ingredients AS i
                INNER JOIN dbo.recipes AS r 
                    ON r.recipeID = i.recipeID
                WHERE r.recipeID = @recipeID 
                    AND r.userID = @userID
                    AND r.status IN ('Approved', 'Pending', 'Rejected')
                ORDER BY i.displayOrder,
                    i.ingredientID;

                SELECT s.instruction,
                    s.image, 
                    s.estimatedMinutes
                FROM dbo.steps AS s
                INNER JOIN dbo.recipes AS r 
                    ON r.recipeID = s.recipeID
                WHERE r.recipeID = @recipeID 
                    AND r.userID = @userID
                    AND r.status IN ('Approved', 'Pending', 'Rejected')
                ORDER BY s.stepNumber, 
                    s.stepID;", con, transaction);

            cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;
            cmd.Parameters.Add("@userID", SqlDbType.Int).Value = Database.UserId.Value;

            DataSet recipe = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(recipe);

            return recipe;
        }

        private bool LoadRecipe()
        {
            SqlConnection con = Database.Open();
            DataSet data;

            try
            {
                data = ReadOwnedRecipe(con, null, editId.Value);
            }
            finally
            {
                con.Close();
            }

            if (data.Tables[0].Rows.Count == 0)
            {
                ShowUnavailableRecipe();
                return false;
            }

            DataRow recipe = data.Tables[0].Rows[0];

            txtTitle.Text = recipe["title"].ToString();
            txtDescription.Text = recipe["description"].ToString();
            txtCuisine.Text = recipe["cuisine"].ToString();
            txtRegion.Text = recipe["region"].ToString();
            ddlDifficulty.SelectedValue = recipe["difficulty"].ToString();
            txtPrep.Text = recipe["preparationTime"].ToString();
            txtServings.Text = recipe["servings"].ToString();
            txtCost.Text = Convert.ToDecimal(recipe["estimatedCost"])
                .ToString("0.00", CultureInfo.InvariantCulture);

            imgExistingRecipe.ImageUrl = GetPhotoPreview(recipe["image"].ToString());
            imgExistingRecipe.Visible = !string.IsNullOrEmpty(imgExistingRecipe.ImageUrl);

            ListItem category = ddlCategory.Items.FindByValue(recipe["categoryID"].ToString());

            if (category != null)
            {
                ddlCategory.ClearSelection();
                category.Selected = true;
            }

            // Prepare the column names expected by IngredientEditor
            DataTable ingredients = data.Tables[1].Copy();
            ingredients.Columns["ingredientName"].ColumnName = "name";
            ingredientEditor.Initialise(ingredients);

            // Prepare the column names expected by CookingStepEditor
            DataTable steps = data.Tables[2].Copy();
            steps.Columns["estimatedMinutes"].ColumnName = "minutes";
            stepEditor.Initialise(steps);

            return true;
        }

        private void LoadCategories()
        {
            ddlCategory.DataSource =
                CategoryRepository.GetActiveCategories(true);

            ddlCategory.DataTextField = "categoryName";
            ddlCategory.DataValueField = "categoryID";
            ddlCategory.DataBind();

            ddlCategory.Items.Insert(
                0, new ListItem("Choose a category", ""));
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response) || !ReadEditId())
            {
                return;
            }

            int servings;
            int preparationTime;
            decimal cost;

            if (!ValidateDetails(out servings, out preparationTime, out cost))
            {
                return;
            }

            List<Ingredient> ingredients = ReadIngredients();
            if (ingredients == null)
            {
                return;
            }

            List<CookingStep> steps = ReadSteps();
            if (steps == null)
            {
                return;
            }

            if (!SaveRecipe(servings, preparationTime, cost, ingredients, steps))
            {
                return;
            }

            Response.Redirect(ResolveUrl("~/Recipes/MyRecipes") +
                (editId.HasValue ? "?saved=updated" : "?saved=1"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private bool ValidateDetails(
            out int servings, out int preparationTime, out decimal cost)
        {
            servings = 0;
            preparationTime = 0;
            cost = 0;

            if (string.IsNullOrWhiteSpace(txtTitle.Text) || txtTitle.Text.Trim().Length > 150)
            {
                return ShowError("Enter a recipe name within 150 characters.");
            }

            if (txtDescription.Text.Length > 1000)
            {
                return ShowError("Keep the description within 1,000 characters.");
            }

            if (string.IsNullOrWhiteSpace(txtCuisine.Text) || txtCuisine.Text.Trim().Length > 80 ||
                string.IsNullOrWhiteSpace(txtRegion.Text) || txtRegion.Text.Trim().Length > 80)
            {
                return ShowError("Enter a cuisine and region within 80 characters each.");
            }

            string difficulty = ddlDifficulty.SelectedValue;
            if (difficulty != "Easy" && difficulty != "Medium" && difficulty != "Hard")
            {
                return ShowError("Choose Easy, Medium or Hard.");
            }

            if (!int.TryParse(txtServings.Text, out servings) || servings < 1 || servings > 100)
            {
                return ShowError("Enter between 1 and 100 servings.");
            }

            if (!int.TryParse(txtPrep.Text, out preparationTime) || preparationTime < 0 || preparationTime > 1440)
            {
                return ShowError("Enter a preparation time from 0 to 1,440 minutes.");
            }

            if (!TryReadDecimal(txtCost.Text, out cost) || cost < 0 || cost > 999999.99m ||
                decimal.Round(cost, 2) != cost)
            {
                return ShowError("Enter a valid cost with no more than two decimal places.");
            }

            int categoryId;

            if (!int.TryParse(ddlCategory.SelectedValue, out categoryId) ||
                categoryId <= 0)
            {
                return ShowError("Choose a category.");
            }

            return true;
        }

        private List<Ingredient> ReadIngredients()
        {
            DataTable rows;

            try
            {
                rows = ingredientEditor.GetIngredients();
            }
            catch (InvalidDataException ex)
            {
                ShowError(ex.Message);
                return null;
            }

            List<Ingredient> ingredients = new List<Ingredient>();

            foreach (DataRow row in rows.Rows)
            {
                string name = row["name"].ToString().Trim();
                string unit = row["unit"].ToString().Trim();
                decimal quantity;

                if (string.IsNullOrWhiteSpace(name) || name.Length > 50 ||
                    string.IsNullOrWhiteSpace(unit) || unit.Length > 30 ||
                    name.Contains("|") || unit.Contains("|") ||
                    name.Contains("\n") || name.Contains("\r") ||
                    unit.Contains("\n") || unit.Contains("\r") ||
                    !TryReadDecimal(row["quantity"].ToString(), out quantity) ||
                    quantity < 0 || quantity > 99999999.99m || decimal.Round(quantity, 2) != quantity)
                {
                    ShowError("Check ingredient names, quantities and units. Do not use vertical bars or line breaks.");
                    return null;
                }

                ingredients.Add(new Ingredient { Name = name, Quantity = quantity, Unit = unit });
            }

            if (ingredients.Count == 0)
            {
                ShowError("Add at least one ingredient.");
                return null;
            }

            return ingredients;
        }

        private List<CookingStep> ReadSteps()
        {
            DataTable rows;

            try
            {
                rows = stepEditor.GetSteps();
            }
            catch (InvalidDataException ex)
            {
                ShowError(ex.Message);
                return null;
            }

            List<CookingStep> steps = new List<CookingStep>();

            for (int i = 0; i < rows.Rows.Count; i++)
            {
                DataRow row = rows.Rows[i];

                string instruction = row["instruction"].ToString().Trim();
                int minutes;

                if (string.IsNullOrWhiteSpace(instruction) ||
                    instruction.Length > 2000)
                {
                    ShowError("Enter an instruction within 2,000 characters for each step.");
                    return null;
                }

                if (!int.TryParse(row["minutes"].ToString(), out minutes) ||
                    minutes < 0 || minutes > 1440)
                {
                    ShowError("Enter between 0 and 1,440 minutes for each step.");
                    return null;
                }

                steps.Add(new CookingStep
                {
                    UploadIndex = i,
                    Instruction = instruction,
                    Image = row["image"].ToString(),
                    Minutes = minutes
                });
            }

            if (steps.Count == 0)
            {
                ShowError("Add at least one cooking step.");
                return null;
            }

            return steps;
        }

        private bool TryReadDecimal(string value, out decimal number)
        {
            return decimal.TryParse(value.Trim(), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out number);
        }

        private bool SaveRecipe(int servings, int preparationTime, decimal cost,
            List<Ingredient> ingredients, List<CookingStep> steps)
        {
            SqlConnection con = null;
            SqlTransaction transaction = null;
            List<string> newUploads = new List<string>();

            try
            {
                con = Database.Open();
                transaction = con.BeginTransaction();

                DataSet existing = null;
                string image = null;

                if (editId.HasValue)
                {
                    // Lock this recipe while its related rows are replaced
                    SqlCommand check = new SqlCommand(@"
                        SELECT image FROM dbo.recipes WITH (UPDLOCK, HOLDLOCK)
                        WHERE recipeID = @recipeID AND userID = @userID;", con, transaction);

                    check.Parameters.Add("@recipeID", SqlDbType.Int).Value = editId.Value;
                    check.Parameters.Add("@userID", SqlDbType.Int).Value = Database.UserId.Value;

                    object oldImage = check.ExecuteScalar();
                    if (oldImage == null)
                    {
                        throw new InvalidOperationException("This recipe is unavailable or does not belong to you.");
                    }

                    image = oldImage == DBNull.Value ? null : oldImage.ToString();
                    existing = ReadOwnedRecipe(con, transaction, editId.Value);
                }

                if (uploadRecipeImage.HasFile)
                {
                    image = ImageUpload.Save(uploadRecipeImage.PostedFile, "Recipes", false);
                    newUploads.Add(image);
                }

                int recipeId = SaveRecipeDetails(con, transaction, servings, preparationTime, cost, image);
                SaveCategory(con, transaction, recipeId);

                if (editId.HasValue)
                {
                    SqlCommand clear = new SqlCommand(@"
                        DELETE FROM dbo.ingredients WHERE recipeID = @recipeID;
                        DELETE FROM dbo.steps WHERE recipeID = @recipeID;", con, transaction);

                    clear.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;
                    clear.ExecuteNonQuery();
                }

                SaveIngredients(con, transaction, recipeId, ingredients, existing);
                SaveSteps(con, transaction, recipeId, steps, existing, newUploads);

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try { transaction.Rollback(); }
                    catch (Exception rollbackError)
                    {
                        Trace.Warn("SubmitRecipe", "Could not roll back the recipe save.", rollbackError);
                    }
                }

                foreach (string image in newUploads)
                {
                    try
                    {
                        string folder = image.Contains("/Recipes/") ? "Recipes" : "Steps";
                        ImageUpload.DeleteUploaded(image, folder);
                    }
                    catch (Exception cleanupError)
                    {
                        Trace.Warn("SubmitRecipe", "Could not clean up a new upload.", cleanupError);
                    }
                }

                Trace.Warn("SubmitRecipe", "Recipe save failed.", ex);
                ShowError(ex is InvalidDataException || ex is InvalidOperationException
                    ? ex.Message : "Your recipe could not be saved. Please try again.");
                return false;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                if (con != null) con.Close();
            }
        }

        private int SaveRecipeDetails(SqlConnection con, SqlTransaction transaction,
            int servings, int preparationTime, decimal cost, string image)
        {
            string sql;

            if (editId.HasValue)
            {
                sql = @"
                    UPDATE dbo.recipes
                    SET title = @title, description = @description,
                        cuisine = @cuisine, region = @region, difficulty = @difficulty,
                        preparationTime = @prep, estimatedCost = @cost,
                        servings = @servings, image = @image,
                        status = 'Pending', updatedAt = SYSUTCDATETIME()
                    WHERE recipeID = @recipeID
                        AND userID = @userID
                        AND status IN ('Approved', 'Pending', 'Rejected');";
            }
            else
            {
                sql = @"
                    INSERT INTO dbo.recipes
                        (title, description, cuisine, region, difficulty,
                         preparationTime, estimatedCost, servings, image,
                         status, createdAt, updatedAt, userID)
                    VALUES
                        (@title, @description, @cuisine, @region, @difficulty,
                         @prep, @cost, @servings, @image,
                         'Pending', SYSUTCDATETIME(), NULL, @userID);
                    SELECT CAST(SCOPE_IDENTITY() AS int);";
            }

            SqlCommand cmd = new SqlCommand(sql, con, transaction);
            AddRecipeParameters(cmd, servings, preparationTime, cost, image);

            if (!editId.HasValue)
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }

            cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = editId.Value;

            if (cmd.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException("You can only edit your own recipes.");
            }

            return editId.Value;
        }

        private void AddRecipeParameters(SqlCommand cmd,
            int servings, int preparationTime, decimal cost, string image)
        {
            cmd.Parameters.Add("@title", SqlDbType.NVarChar, 150).Value = txtTitle.Text.Trim();
            cmd.Parameters.Add("@description", SqlDbType.NVarChar, 1000).Value =
                string.IsNullOrWhiteSpace(txtDescription.Text)
                    ? (object)DBNull.Value : txtDescription.Text.Trim();
            cmd.Parameters.Add("@cuisine", SqlDbType.NVarChar, 100).Value = txtCuisine.Text.Trim();
            cmd.Parameters.Add("@region", SqlDbType.NVarChar, 100).Value = txtRegion.Text.Trim();
            cmd.Parameters.Add("@difficulty", SqlDbType.NVarChar, 20).Value = ddlDifficulty.SelectedValue;
            cmd.Parameters.Add("@prep", SqlDbType.Int).Value = preparationTime;
            SqlParameter costParameter = cmd.Parameters.Add("@cost", SqlDbType.Decimal);
            costParameter.Precision = 10;
            costParameter.Scale = 2;
            costParameter.Value = cost;
            cmd.Parameters.Add("@servings", SqlDbType.Int).Value = servings;
            cmd.Parameters.Add("@image", SqlDbType.NVarChar, 255).Value = (object)image ?? DBNull.Value;
            cmd.Parameters.Add("@userID", SqlDbType.Int).Value = Database.UserId.Value;
        }

        private void SaveIngredients(SqlConnection con, SqlTransaction transaction,
            int recipeId, List<Ingredient> ingredients, DataSet existing)
        {
            for (int i = 0; i < ingredients.Count; i++)
            {
                Ingredient ingredient = ingredients[i];
                object image = DBNull.Value;
                bool optional = false;

                // Preserve metadata that the current editor does not expose.
                if (existing != null)
                {
                    foreach (DataRow old in existing.Tables[1].Rows)
                    {
                        if (old["ingredientName"].ToString() == ingredient.Name)
                        {
                            image = old["image"];
                            optional = Convert.ToBoolean(old["isOptional"]);
                            break;
                        }
                    }
                }

                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO dbo.ingredients
                        (ingredientName, quantity, unit, image, isOptional, displayOrder, recipeID)
                    VALUES (@name, @quantity, @unit, @image, @optional, @order, @recipeID);", con, transaction);

                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = ingredient.Name;
                SqlParameter quantity = cmd.Parameters.Add("@quantity", SqlDbType.Decimal);
                quantity.Precision = 10;
                quantity.Scale = 2;
                quantity.Value = ingredient.Quantity;
                cmd.Parameters.Add("@unit", SqlDbType.NVarChar, 30).Value = ingredient.Unit;
                cmd.Parameters.Add("@image", SqlDbType.NVarChar, 255).Value = image;
                cmd.Parameters.Add("@optional", SqlDbType.Bit).Value = optional;
                cmd.Parameters.Add("@order", SqlDbType.Int).Value = i + 1;
                cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;
                cmd.ExecuteNonQuery();
            }
        }

        private void SaveSteps(SqlConnection con, SqlTransaction transaction,
            int recipeId, List<CookingStep> steps, DataSet existing, List<string> newUploads)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                CookingStep step = steps[i];
                string image = step.Image;
                int minutes = step.Minutes;
                bool allowedExistingImage = string.IsNullOrWhiteSpace(image);

                if (existing != null)
                {
                    foreach (DataRow old in existing.Tables[2].Rows)
                    {
                        if (old["image"].ToString() == image)
                        {
                            allowedExistingImage = true;
                        }
                    }
                }

                HttpPostedFile upload = Request.Files["stepUpload_" + step.UploadIndex];

                if (upload != null && upload.ContentLength > 0)
                {
                    image = ImageUpload.Save(upload, "Steps", false);
                    newUploads.Add(image);
                }
                else if (!allowedExistingImage)
                {
                    throw new InvalidDataException("Choose step photos from your device. New image links are not accepted.");
                }

                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO dbo.steps
                        (stepNumber, instruction, image, estimatedMinutes, recipeID)
                    VALUES (@number, @instruction, @image, @minutes, @recipeID);", con, transaction);

                cmd.Parameters.Add("@number", SqlDbType.Int).Value = i + 1;
                cmd.Parameters.Add("@instruction", SqlDbType.NVarChar, -1).Value = step.Instruction;
                cmd.Parameters.Add("@image", SqlDbType.NVarChar, 255).Value =
                    string.IsNullOrWhiteSpace(image) ? (object)DBNull.Value : image;
                cmd.Parameters.Add("@minutes", SqlDbType.Int).Value = minutes;
                cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;
                cmd.ExecuteNonQuery();
            }
        }

        private void SaveCategory(SqlConnection con, SqlTransaction transaction, int recipeId)
        {
            int categoryId = Convert.ToInt32(ddlCategory.SelectedValue);

            SqlCommand check = new SqlCommand(@"
                SELECT COUNT(*)
                FROM dbo.categories WITH (HOLDLOCK)
                WHERE categoryID = @categoryID
                    AND isActive = 1;", con, transaction);

            check.Parameters.Add("@categoryID", SqlDbType.Int).Value =
                categoryId;

            if (Convert.ToInt32(check.ExecuteScalar()) != 1)
            {
                throw new InvalidOperationException(
                    "That category is no longer available. Choose another category.");
            }

            SqlCommand cmd = new SqlCommand(@"
                DELETE FROM dbo.recipesCategories
                WHERE recipeID = @recipeID;

                INSERT INTO dbo.recipesCategories (recipeID, categoryID)
                VALUES (@recipeID, @categoryID);", con, transaction);

            cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;
            cmd.Parameters.Add("@categoryID", SqlDbType.Int).Value = categoryId;

            cmd.ExecuteNonQuery();
        }

        private string GetPhotoPreview(string image)
        {
            if (string.IsNullOrWhiteSpace(image)) return "";

            Uri uri;
            if (Uri.TryCreate(image, UriKind.Absolute, out uri))
            {
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps
                    ? image : "";
            }

            image = image.Replace("/Content/Uploads/Recipes/", "/Content/images/Uploads/Recipes/");
            if (!image.StartsWith("/Content/images/", StringComparison.OrdinalIgnoreCase) ||
                image.Contains("..") || image.Contains("\\"))
            {
                return "";
            }

            string extension = Path.GetExtension(image).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".gif")
            {
                return "";
            }

            return ResolveUrl(image);
        }

        private bool ShowError(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.CssClass = "alert alert-danger d-block";
            return false;
        }

        private void ShowUnavailableRecipe()
        {
            btnSave.Enabled = false;
            ShowError("This recipe is unavailable or does not belong to you.");
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}
