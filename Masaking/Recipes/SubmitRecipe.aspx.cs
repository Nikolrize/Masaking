using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Recipes
{
    public partial class SubmitRecipe : System.Web.UI.Page
    {
        protected TextBox txtTitle, txtDescription, txtCuisine, txtRegion, txtServings, txtPrep, txtCost, txtIngredients, txtSteps, txtStepImages;
        protected FileUpload uploadRecipeImage;
        protected Image imgExistingRecipe;
        protected DropDownList ddlDifficulty;
        protected Label lblMessage;
        protected Button btnSave;
        private int? EditId { get { int id; return Int32.TryParse(Request.QueryString["id"], out id) ? (int?)id : null; } }

        private sealed class RecipeStep
        {
            public int LineIndex;
            public string Instruction;
            public string ImageUrl;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            Form.Enctype = "multipart/form-data";
            if (!IsPostBack && EditId.HasValue) LoadRecipe();
        }

        private void LoadRecipe()
        {
            using (SqlConnection cn = Database.Open())
            {
                using (SqlCommand cmd = new SqlCommand("SELECT title,description,cuisine,region,difficulty,preparationTime,estimatedCost,servings,image FROM recipes WHERE recipeID=@id AND userID=@user", cn))
                {
                    cmd.Parameters.AddWithValue("@id", EditId.Value); cmd.Parameters.AddWithValue("@user", Database.UserId.Value);
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) { Response.Redirect("MyRecipes.aspx"); return; }
                        txtTitle.Text = rd.GetString(0); txtDescription.Text = rd.IsDBNull(1) ? "" : rd.GetString(1); txtCuisine.Text = rd.GetString(2); txtRegion.Text = rd.GetString(3);
                        ddlDifficulty.SelectedValue = rd.GetString(4); txtPrep.Text = rd.GetInt32(5).ToString(); txtCost.Text = rd.GetDecimal(6).ToString(CultureInfo.InvariantCulture); txtServings.Text = rd.GetInt32(7).ToString();
                        string image = rd.IsDBNull(8) ? "" : rd.GetString(8);
                        imgExistingRecipe.ImageUrl = PhotoPreviewUrl(image);
                        imgExistingRecipe.Visible = !String.IsNullOrEmpty(imgExistingRecipe.ImageUrl);
                    }
                }
                using (SqlCommand cmd = new SqlCommand("SELECT ingredientName,quantity,unit FROM ingredients WHERE recipeID=@id ORDER BY displayOrder", cn))
                { cmd.Parameters.AddWithValue("@id", EditId.Value); using (SqlDataReader rd = cmd.ExecuteReader()) while (rd.Read()) txtIngredients.Text += (txtIngredients.Text.Length == 0 ? "" : Environment.NewLine) + rd.GetString(0) + " | " + rd.GetDecimal(1).ToString(CultureInfo.InvariantCulture) + " | " + rd.GetString(2); }
                List<string> stepImageUrls = new List<string>();
                using (SqlCommand cmd = new SqlCommand("SELECT instruction,image FROM steps WHERE recipeID=@id ORDER BY stepNumber", cn))
                {
                    cmd.Parameters.AddWithValue("@id", EditId.Value);
                    using (SqlDataReader rd = cmd.ExecuteReader()) while (rd.Read())
                    {
                        txtSteps.Text += (txtSteps.Text.Length == 0 ? "" : Environment.NewLine) + rd.GetString(0);
                        stepImageUrls.Add(rd.IsDBNull(1) ? "" : rd.GetString(1));
                    }
                }
                txtStepImages.Text = String.Join(Environment.NewLine, stepImageUrls.ToArray());
            }
            btnSave.Text = "Save changes for review";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int servings, prep; decimal cost;
            if (String.IsNullOrWhiteSpace(txtTitle.Text) || txtTitle.Text.Trim().Length > 150 || txtDescription.Text.Length > 1000 || String.IsNullOrWhiteSpace(txtCuisine.Text) || txtCuisine.Text.Trim().Length > 80 || String.IsNullOrWhiteSpace(txtRegion.Text) || txtRegion.Text.Trim().Length > 80 ||
                !Int32.TryParse(txtServings.Text, out servings) || servings < 1 || servings > 100 || !Int32.TryParse(txtPrep.Text, out prep) || prep < 0 || prep > 1440 ||
                !Decimal.TryParse(txtCost.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out cost) || cost < 0 || cost > 999999.99m)
            { Show("Check the recipe name, cuisine, region, serving count, preparation time and cost.", false); return; }

            List<string[]> ingredients = new List<string[]>();
            foreach (string line in txtIngredients.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = line.Split('|'); decimal qty;
                if (parts.Length != 3 || String.IsNullOrWhiteSpace(parts[0]) || parts[0].Trim().Length > 50 || !Decimal.TryParse(parts[1].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out qty) || qty < 0 || qty > 99999999.99m || Decimal.Round(qty, 2) != qty || String.IsNullOrWhiteSpace(parts[2]) || parts[2].Trim().Length > 30)
                { Show("Check each ingredient row: enter a name, a quantity with up to two decimal places, and a unit.", false); return; }
                ingredients.Add(new[] { parts[0].Trim(), qty.ToString(CultureInfo.InvariantCulture), parts[2].Trim() });
            }

            string[] stepLines = txtSteps.Text.Replace("\r\n", "\n").Split('\n');
            string[] urlLines = txtStepImages.Text.Replace("\r\n", "\n").Split('\n');
            List<RecipeStep> steps = new List<RecipeStep>();
            for (int i = 0; i < stepLines.Length; i++)
            {
                string instruction = stepLines[i].Trim();
                if (instruction.Length == 0) continue;
                if (instruction.Length > 2000) { Show("Keep each cooking step within 2,000 characters.", false); return; }
                string imageUrl = i < urlLines.Length ? urlLines[i].Trim() : "";
                steps.Add(new RecipeStep { LineIndex = i, Instruction = instruction, ImageUrl = imageUrl });
            }
            if (ingredients.Count == 0 || steps.Count == 0) { Show("Add at least one ingredient and one cooking step.", false); return; }

            List<string> newUploads = new List<string>();
            List<string> oldStepImages = new List<string>();
            List<string> keptStepImages = new List<string>();
            string oldRecipeImage = null, savedRecipeImage = null;
            bool committed = false;
            using (SqlConnection cn = Database.Open()) using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    if (EditId.HasValue)
                    {
                        using (SqlCommand check = new SqlCommand("SELECT image FROM recipes WHERE recipeID=@id AND userID=@user", cn, tx))
                        {
                            check.Parameters.AddWithValue("@id", EditId.Value); check.Parameters.AddWithValue("@user", Database.UserId.Value);
                            object value = check.ExecuteScalar(); if (value == null) throw new InvalidOperationException("You can only edit your own recipes.");
                            oldRecipeImage = value == DBNull.Value ? null : Convert.ToString(value);
                        }
                        using (SqlCommand old = new SqlCommand("SELECT image FROM steps WHERE recipeID=@id ORDER BY stepNumber", cn, tx))
                        { old.Parameters.AddWithValue("@id", EditId.Value); using (SqlDataReader rd = old.ExecuteReader()) while (rd.Read()) oldStepImages.Add(rd.IsDBNull(0) ? null : rd.GetString(0)); }
                    }

                    if (uploadRecipeImage.HasFile)
                    {
                        savedRecipeImage = ImageUpload.Save(uploadRecipeImage.PostedFile, "Recipes", false); newUploads.Add(savedRecipeImage);
                    }
                    else savedRecipeImage = oldRecipeImage;

                    int recipeId;
                    if (EditId.HasValue)
                    {
                        recipeId = EditId.Value;
                        using (SqlCommand cmd = new SqlCommand("UPDATE recipes SET title=@title,description=@description,cuisine=@cuisine,region=@region,difficulty=@difficulty,preparationTime=@prep,estimatedCost=@cost,servings=@servings,image=@image,status='Pending',updatedAt=@updated WHERE recipeID=@id AND userID=@user", cn, tx))
                        { AddRecipeParameters(cmd, servings, prep, cost, savedRecipeImage); cmd.Parameters.AddWithValue("@updated", DateTime.UtcNow); cmd.Parameters.AddWithValue("@id", recipeId); cmd.Parameters.AddWithValue("@user", Database.UserId.Value); cmd.ExecuteNonQuery(); }
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM ingredients WHERE recipeID=@id; DELETE FROM steps WHERE recipeID=@id", cn, tx)) { cmd.Parameters.AddWithValue("@id", recipeId); cmd.ExecuteNonQuery(); }
                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO recipes(title,description,cuisine,region,difficulty,preparationTime,estimatedCost,servings,image,status,createdAt,updatedAt,userID) VALUES(@title,@description,@cuisine,@region,@difficulty,@prep,@cost,@servings,@image,'Pending',@created,NULL,@user); SELECT CAST(SCOPE_IDENTITY() AS int)", cn, tx))
                        { AddRecipeParameters(cmd, servings, prep, cost, savedRecipeImage); cmd.Parameters.AddWithValue("@created", DateTime.UtcNow); cmd.Parameters.AddWithValue("@user", Database.UserId.Value); recipeId = Convert.ToInt32(cmd.ExecuteScalar()); }
                    }
                    for (int i = 0; i < ingredients.Count; i++)
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO ingredients(ingredientName,quantity,unit,image,isOptional,displayOrder,recipeID) VALUES(@name,@quantity,@unit,NULL,0,@order,@recipe)", cn, tx))
                        { cmd.Parameters.AddWithValue("@name", ingredients[i][0]); cmd.Parameters.AddWithValue("@quantity", Decimal.Parse(ingredients[i][1], CultureInfo.InvariantCulture)); cmd.Parameters.AddWithValue("@unit", ingredients[i][2]); cmd.Parameters.AddWithValue("@order", i + 1); cmd.Parameters.AddWithValue("@recipe", recipeId); cmd.ExecuteNonQuery(); }
                    }
                    for (int i = 0; i < steps.Count; i++)
                    {
                        RecipeStep step = steps[i]; string stepImage = step.ImageUrl;
                        HttpPostedFile upload = Request.Files["stepUpload_" + step.LineIndex];
                        if (upload != null && upload.ContentLength > 0) { stepImage = ImageUpload.Save(upload, "Steps", false); newUploads.Add(stepImage); }
                        else if (!String.IsNullOrWhiteSpace(stepImage) && !oldStepImages.Contains(stepImage))
                            throw new InvalidDataException("Choose step photos from your device. New image links are not accepted.");
                        else if (String.IsNullOrWhiteSpace(stepImage)) stepImage = null;
                        if (!String.IsNullOrEmpty(stepImage) && oldStepImages.Contains(stepImage)) keptStepImages.Add(stepImage);
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO steps(stepNumber,instruction,image,estimatedMinutes,recipeID) VALUES(@number,@instruction,@image,0,@recipe)", cn, tx))
                        { cmd.Parameters.AddWithValue("@number", i + 1); cmd.Parameters.AddWithValue("@instruction", step.Instruction); cmd.Parameters.AddWithValue("@image", (object)stepImage ?? DBNull.Value); cmd.Parameters.AddWithValue("@recipe", recipeId); cmd.ExecuteNonQuery(); }
                    }
                    tx.Commit(); committed = true;
                    if (!String.IsNullOrEmpty(oldRecipeImage) && !String.Equals(oldRecipeImage, savedRecipeImage, StringComparison.Ordinal)) ImageUpload.DeleteUploaded(oldRecipeImage, "Recipes");
                    foreach (string old in oldStepImages) if (!String.IsNullOrEmpty(old) && !keptStepImages.Contains(old)) ImageUpload.DeleteUploaded(old, "Steps");
                    Response.Redirect("MyRecipes.aspx?saved=1", false); Context.ApplicationInstance.CompleteRequest();
                }
                catch (Exception ex)
                {
                    if (!committed) { try { tx.Rollback(); } catch { } foreach (string upload in newUploads) ImageUpload.DeleteUploaded(upload, upload.IndexOf("/Recipes/", StringComparison.OrdinalIgnoreCase) >= 0 ? "Recipes" : "Steps"); }
                    Show(ex is InvalidOperationException || ex is InvalidDataException ? ex.Message : "Your recipe could not be saved. Check the details and try again.", false);
                }
            }
        }

        private void AddRecipeParameters(SqlCommand cmd, int servings, int prep, decimal cost, string image)
        {
            cmd.Parameters.AddWithValue("@title", txtTitle.Text.Trim()); cmd.Parameters.AddWithValue("@description", String.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim());
            cmd.Parameters.AddWithValue("@cuisine", txtCuisine.Text.Trim()); cmd.Parameters.AddWithValue("@region", txtRegion.Text.Trim()); cmd.Parameters.AddWithValue("@difficulty", ddlDifficulty.SelectedValue);
            cmd.Parameters.AddWithValue("@prep", prep); cmd.Parameters.AddWithValue("@cost", cost); cmd.Parameters.AddWithValue("@servings", servings); cmd.Parameters.AddWithValue("@image", (object)image ?? DBNull.Value);
        }

        private string PhotoPreviewUrl(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return "";
            Uri uri;
            if (Uri.TryCreate(value, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) return value;
            string image = value.Replace("/Content/Uploads/Recipes/", "/Content/images/Uploads/Recipes/");
            const string prefix = "/Content/images/Uploads/Recipes/";
            Guid id;
            string name = Path.GetFileName(image);
            if (!image.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || image.Substring(prefix.Length) != name || !name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "N", out id)) return "";
            return ResolveUrl(image);
        }

        private void Show(string text, bool success) { lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block"; lblMessage.Text = Server.HtmlEncode(text); }
    }
}
