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
            const string sql = @" SELECT r.recipeID, r.title, u.displayName, r.difficulty, r.status, r.createdAt FROM dbo.recipes AS r 
              INNER JOIN dbo.users AS u ON r.userID = u.userID
              WHERE (@status = '' OR r.status = @status) AND (@search = '' OR CHARINDEX(@search, r.title) > 0) 
              ORDER BY r.createdAt DESC, r.recipeID DESC";

            using (SqlConnection connection = Database.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add(
                 "@status", SqlDbType.NVarChar, 20
                ).Value = ddlStatus.SelectedValue;

                command.Parameters.Add(
                    "@search", SqlDbType.NVarChar, 150
                ).Value = txtSearch.Text.Trim();

                DataTable recipes = new DataTable();
                adapter.Fill(recipes);

                gvRecipes.DataSource = recipes;
                gvRecipes.DataBind();

                
            }
            gvAllRecipes.PageIndex = 0;
            BindAllRecipes();

        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response))
            {
                return;
            }

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
            BindAllRecipes();
        }

        private void BindAllRecipes()
        {
            const string sql = @"SELECT r.recipeID, r.title, u.displayName, r.status FROM dbo.recipes AS r 
              INNER JOIN dbo.users AS u ON r.userID = u.userID WHERE (@status = '' OR r.status = @status) AND (@search = '' OR CHARINDEX(@search, r.title) > 0)
              ORDER BY r.createdAt DESC, r.recipeID DESC";
            using (SqlConnection connection = Database.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = ddlStatus.SelectedValue;

                command.Parameters.Add(
                    "@search", SqlDbType.NVarChar, 150
                ).Value = txtSearch.Text.Trim();

                DataTable recipes = new DataTable();
                adapter.Fill(recipes);

                gvAllRecipes.DataSource = recipes;
                gvAllRecipes.DataBind();
            }
            
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
                EXISTS (SELECT 1 FROM dbo.favourites
                        WHERE recipeID = @id)
                OR EXISTS (SELECT 1 FROM dbo.posts
                           WHERE recipeID = @id)
                OR EXISTS (SELECT 1 FROM dbo.recipeProgress
                           WHERE recipeID = @id)
                OR EXISTS (SELECT 1 FROM dbo.reviews
                           WHERE recipeID = @id)
                OR EXISTS (SELECT 1 FROM dbo.simulations
                           WHERE recipeID = @id)
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
                    lblMessage.Text =
                        "This recipe has favourites, community activity or learning records "
                        + "and cannot be permanently deleted.";

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
                lblMessage.Text =
                    "Deletion was blocked by a related database record. "
                    + "No changes were saved.";

                lblMessage.CssClass = "alert alert-warning d-block";
            }
            catch (SqlException ex) when (ex.Number == 1205)
            {
                lblMessage.Text =
                    "Another database operation interrupted deletion. Please try again.";

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
        }

        private void ShowRecipe(string value)
        {
            pnlDetails.Visible = false;

            int recipeId;

            if (!Int32.TryParse(value, out recipeId) || recipeId <= 0)
            {
                lblMessage.Text = "Invalid recipe selection.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            const string sql = @"
            SELECT r.recipeID, r.title, r.description, u.displayName, r.status, r.cuisine, r.region, r.difficulty, r.preparationTime, r.estimatedCost, r.servings
            FROM dbo.recipes AS r INNER JOIN dbo.users AS u ON r.userID = u.userID WHERE r.recipeID = @id;

            SELECT ingredientName, quantity, unit FROM dbo.ingredients WHERE recipeID = @id ORDER BY displayOrder, ingredientID;

            SELECT stepNumber, instruction FROM dbo.steps WHERE recipeID = @id ORDER BY stepNumber, stepID;";

            using (SqlConnection connection = Database.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = recipeId;

                DataSet details = new DataSet();
                adapter.Fill(details);

                if (details.Tables[0].Rows.Count == 0)
                {
                    lblMessage.Text = "This recipe no longer exists.";
                    lblMessage.CssClass = "alert alert-warning d-block";
                    return;
                }

                dvRecipe.DataSource = details.Tables[0];
                dvRecipe.DataBind();

                gvIngredients.DataSource = details.Tables[1];
                gvIngredients.DataBind();

                gvSteps.DataSource = details.Tables[2];
                gvSteps.DataBind();
            }

            pnlDetails.Visible = true;
        }
    }
}

