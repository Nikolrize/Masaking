using Masaking.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Masaking.Admin
{
    public partial class ManageRecipes : System.Web.UI.Page
    {
        private int? DetailsRecipeId
        {
            get { return ViewState["DetailsRecipeId"] as int?; }
            set { ViewState["DetailsRecipeId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) { return; }

            if (!IsPostBack)
            {
                BindRecipes();
            }
        }

        private void BindRecipes()
        {
            pnlDetails.Visible = false;
            DetailsRecipeId = null;
            const string sql = @"
                SELECT r.recipeID,r.title,u.displayName,r.difficulty,r.status,r.createdAt,r.image
                FROM dbo.recipes r INNER JOIN dbo.users u ON u.userID=r.userID
                WHERE (@status='' OR r.status=@status) AND (@search='' OR CHARINDEX(@search,r.title)>0)
                ORDER BY r.createdAt DESC,r.recipeID DESC;";
            DataTable recipes = new DataTable();
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = ddlStatus.SelectedValue;
                cmd.Parameters.Add("@search", SqlDbType.NVarChar, 150).Value = txtSearch.Text.Trim();
                adapter.Fill(recipes);
            }
            PrepareRecipeRows(recipes);
            AdminTable.Bind(gvRecipes, recipes, ViewState, "Review", "createdAt", "DESC");
            AdminTable.Bind(gvAllRecipes, recipes, ViewState, "AllRecipes", "createdAt", "DESC");
            lblRecipeCount.Text = recipes.Rows.Count
                + (recipes.Rows.Count == 1 ? " recipe matches" : " recipes match") + " the filters.";
        }

        private void PrepareRecipeRows(DataTable recipes)
        {
            recipes.Columns.Add("imageUrl", typeof(string));
            recipes.Columns.Add("difficultyOrder", typeof(int));
            recipes.Columns.Add("submittedLocal", typeof(DateTime));
            foreach (DataRow row in recipes.Rows)
            {
                row["imageUrl"] = AdminTable.RecipeImage(this, row["image"]);
                row["difficultyOrder"] = AdminTable.DifficultyOrder(row["difficulty"]);
                row["submittedLocal"] = row["createdAt"] == DBNull.Value
                    ? DBNull.Value : (object)AdminTable.MalaysiaTime(row["createdAt"]);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
            gvRecipes.PageIndex = 0;
            gvAllRecipes.PageIndex = 0;
            BindRecipes();
        }

        protected void Filters_Changed(object sender, EventArgs e)
        {
            btnSearch_Click(sender, e);
        }

        protected void gvRecipes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
            gvRecipes.PageIndex = e.NewPageIndex;
            BindRecipes();
        }

        protected void gvRecipes_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) { e.Cancel = true; return; }
            AdminTable.ToggleSort(e, ViewState, "Review", "createdAt", "DESC", "recipeID", "title", "displayName", "difficultyOrder", "status", "createdAt");
            gvRecipes.PageIndex = 0;
            BindRecipes();
        }

        protected void gvAllRecipes_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) { e.Cancel = true; return; }
            AdminTable.ToggleSort(e, ViewState, "AllRecipes", "createdAt", "DESC", "recipeID", "title", "displayName", "difficultyOrder", "status", "createdAt");
            gvAllRecipes.PageIndex = 0;
            BindRecipes();
        }

        protected void gvRecipes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) { return; }
            if (e.CommandName == "ViewRecipe")
            {
                ShowRecipe(Convert.ToString(e.CommandArgument));
                return;
            }

            if (e.CommandName != "ApproveRecipe" && e.CommandName != "RejectRecipe")
            {
                return;
            }

            

            int recipeId;

            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out recipeId) || recipeId <= 0)
            {
                lblMessage.Text = "Invalid recipe selection";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            string newStatus = e.CommandName == "ApproveRecipe" ? "Approved" : "Rejected";

            const string sql = @"UPDATE dbo.recipes SET status = @status, updatedAt = @updatedAt WHERE recipeID = @recipeID AND status = 'Pending';";

            int changed;

            using (SqlConnection connection = Database.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = newStatus;
                command.Parameters.Add("@updatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                command.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;

                changed = command.ExecuteNonQuery();
            }
            lblMessage.Text = changed == 1 ? "Recipe " + newStatus.ToLowerInvariant() + "." : "No  change made. The recipe may already have been reviewed.";

            lblMessage.CssClass = changed == 1 ? "alert alert-success d-block" : "alert alert-danger d-block";

            BindRecipes();
        }

        protected void gvAllRecipes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (!Database.RequireAdmin(Response))
            {
                return;
            }

            pnlDetails.Visible = false;

            gvAllRecipes.PageIndex = e.NewPageIndex;
            BindRecipes();
        }

        //delete handler
        protected void gvAllRecipes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) { return; }

            if (e.CommandName == "ViewRecipe")
            {
                ShowRecipe(Convert.ToString(e.CommandArgument));
                return;
            }

            if (e.CommandName != "RemoveRecipe")
            {
                return;
            }

            int recipeId; 

            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out recipeId) || recipeId <= 0)
            {
                lblMessage.Text = "Invalid recipe selection";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            const string sql = @" IF NOT EXISTS (SELECT 1 FROM dbo.recipes WHERE recipeID = @id) SELECT 0;
            ELSE IF
                EXISTS (SELECT 1 FROM dbo.favourites WHERE recipeID = @id)
                OR EXISTS (SELECT 1 FROM dbo.posts WHERE recipeID = @id)
                OR EXISTS (SELECT 1 FROM dbo.recipeProgress WHERE recipeID = @id)
                OR EXISTS (SELECT 1 FROM dbo.reviews WHERE recipeID = @id)
                OR EXISTS (SELECT 1 FROM dbo.simulations WHERE recipeID = @id)
                OR EXISTS (SELECT 1 FROM dbo.challenges WHERE recipeID = @id)
                SELECT -1;
            ELSE
            BEGIN
                DELETE FROM dbo.ingredients WHERE recipeID = @id;
                DELETE FROM dbo.steps WHERE recipeID = @id;
                DELETE FROM dbo.recipesCategories WHERE recipeID = @id;
                DELETE FROM dbo.recipes WHERE recipeID = @id;
                SELECT 1;
            END;";

            try
            {
                int result;

                using (SqlConnection connection = Database.Open())
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                using (SqlCommand command =
                    new SqlCommand(sql, connection, transaction))
                {
                    command.Parameters.Add(
                        "@id", SqlDbType.Int
                    ).Value = recipeId;

                    result = Convert.ToInt32(command.ExecuteScalar());

                    transaction.Commit();
                }

                if (result == 1)
                {
                    lblMessage.Text = "Recipe permanently deleted.";
                    lblMessage.CssClass = "alert alert-success d-block";
                }
                else if (result == -1)
                {
                    lblMessage.Text = "This recipe has favourites, community activity or learning records " + "and cannot be permanently deleted.";

                    lblMessage.CssClass = "alert alert-warning d-block";
                }
                else
                {
                    lblMessage.Text = "The recipe no longer exists.";
                    lblMessage.CssClass = "alert alert-warning d-block";
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                lblMessage.Text = "Deletion was blocked by a related database record. " + "No changes were saved.";

                lblMessage.CssClass = "alert alert-warning d-block";
            }
            catch (SqlException ex) when (ex.Number == 1205)
            {
                lblMessage.Text = "Another database operation interrupted deletion. Please try again.";

                lblMessage.CssClass = "alert alert-warning d-block";
            }

            gvAllRecipes.PageIndex = 0;
            BindRecipes();
        }

        protected void btnCloseDetails_Click(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response))
            {
                return;
            }

            pnlDetails.Visible = false;
            DetailsRecipeId = null;
        }

        private void ShowRecipe(string value)
        {
            pnlDetails.Visible = false;
            int recipeId;
            if (!Int32.TryParse(value, out recipeId) || recipeId <= 0)
            {
                DetailsRecipeId = null;
                lblMessage.Text = "Invalid recipe selection.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            const string sql = @" SELECT r.recipeID,r.title,r.description,u.displayName,r.status,r.cuisine,r.region, r.difficulty,r.preparationTime,r.estimatedCost,r.servings,r.image,r.createdAt FROM dbo.recipes r INNER JOIN dbo.users u ON u.userID=r.userID WHERE r.recipeID=@id;
             SELECT ingredientID,ingredientName,quantity,unit,displayOrder FROM dbo.ingredients WHERE recipeID=@id ORDER BY displayOrder,ingredientID; SELECT stepID,stepNumber,instruction FROM dbo.steps WHERE recipeID=@id ORDER BY stepNumber,stepID;";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = recipeId;
                DataSet details = new DataSet();
                adapter.Fill(details);
                if (details.Tables[0].Rows.Count == 0)
                {
                    DetailsRecipeId = null;
                    lblMessage.Text = "This recipe no longer exists.";
                    lblMessage.CssClass = "alert alert-warning d-block";
                    return;
                }

                if (DetailsRecipeId != recipeId)
                {
                    AdminTable.ResetSort(ViewState, "Ingredients");
                    AdminTable.ResetSort(ViewState, "Steps");
                }
                string image = AdminTable.RecipeImage(this, details.Tables[0].Rows[0]["image"]);
                imgRecipeDetails.ImageUrl = image;
                imgRecipeDetails.AlternateText = Convert.ToString(details.Tables[0].Rows[0]["title"]);
                imgRecipeDetails.Visible = image.Length > 0;
                pnlDetailsImagePlaceholder.Visible = image.Length == 0;
                dvRecipe.DataSource = details.Tables[0];
                dvRecipe.DataBind();
                AdminTable.Bind(gvIngredients, details.Tables[1], ViewState, "Ingredients", "displayOrder", "ASC");
                AdminTable.Bind(gvSteps, details.Tables[2], ViewState, "Steps", "stepNumber", "ASC");
            }
            DetailsRecipeId = recipeId;
            pnlDetails.Visible = true;
        }

        protected void gvIngredients_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) { e.Cancel = true; return; }
            AdminTable.ToggleSort(e, ViewState, "Ingredients", "displayOrder", "ASC", "ingredientName", "quantity", "unit");
            if (DetailsRecipeId.HasValue) ShowRecipe(DetailsRecipeId.Value.ToString());
        }

        protected void gvSteps_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) { e.Cancel = true; return; }
            AdminTable.ToggleSort(e, ViewState, "Steps", "stepNumber", "ASC", "stepNumber", "instruction");
            if (DetailsRecipeId.HasValue) ShowRecipe(DetailsRecipeId.Value.ToString());
        }
    }
}
