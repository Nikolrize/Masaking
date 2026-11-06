using Masaking.Controls;
using Masaking.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Masaking.Recipes
{
    public partial class BrowseRecipes : ResponsivePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCategories();
                LoadCuisineAndRegion();
                LoadSliderLimits();
                SelectCategory();
                LoadRecipes();
            }
        }

        private void LoadCategories()
        {
            DataTable categories = CategoryRepository.GetActiveCategories(false);

            cblCategories.DataSource = categories;
            cblCategories.DataTextField = "categoryName";
            cblCategories.DataValueField = "categoryID";
            cblCategories.DataBind();
        }

        private void LoadCuisineAndRegion()
        {
            SqlConnection con = Database.Open();

            SqlCommand cuisineCmd = new SqlCommand(@"
                SELECT DISTINCT cuisine
                FROM dbo.recipes
                WHERE status = 'Approved'
                    AND cuisine <> ''
                ORDER BY cuisine;", con);

            SqlDataAdapter cuisineAdapter = new SqlDataAdapter(cuisineCmd);
            DataTable cuisines = new DataTable();
            cuisineAdapter.Fill(cuisines);

            ddlCuisine.DataSource = cuisines;
            ddlCuisine.DataTextField = "cuisine";
            ddlCuisine.DataValueField = "cuisine";
            ddlCuisine.DataBind();
            ddlCuisine.Items.Insert(0, new ListItem("All cuisines", ""));

            SqlCommand regionCmd = new SqlCommand(@"
                SELECT DISTINCT region
                FROM dbo.recipes
                WHERE status = 'Approved'
                    AND region <> ''
                ORDER BY region;", con);

            SqlDataAdapter regionAdapter = new SqlDataAdapter(regionCmd);
            DataTable regions = new DataTable();
            regionAdapter.Fill(regions);

            ddlRegion.DataSource = regions;
            ddlRegion.DataTextField = "region";
            ddlRegion.DataValueField = "region";
            ddlRegion.DataBind();
            ddlRegion.Items.Insert(0, new ListItem("All regions", ""));

            con.Close();
        }

        private void LoadSliderLimits()
        {
            SqlConnection con = Database.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT
                    CEILING(COALESCE(MAX(estimatedCost), 0)) AS maxCost,
                    COALESCE(MAX(preparationTime), 0) AS maxTime
                FROM dbo.recipes
                WHERE status = 'Approved';", con);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable limits = new DataTable();
            adapter.Fill(limits);

            int maximumCost = Convert.ToInt32(limits.Rows[0]["maxCost"]);
            int maximumTime = Convert.ToInt32(limits.Rows[0]["maxTime"]);

            rngBudget.Attributes["max"] = Math.Max(1, maximumCost).ToString();
            rngTime.Attributes["max"] = Math.Max(1, maximumTime).ToString();

            con.Close();
        }

        private void LoadRecipes()
        {
            int minServings = 0;
            lblFilterError.Visible = false;

            if (!string.IsNullOrWhiteSpace(txtServings.Text))
            {
                if (!int.TryParse(txtServings.Text, out minServings) || minServings < 0)
                {
                    lblFilterError.Text = "Enter a whole number of servings, starting from 0.";
                    lblFilterError.Visible = true;
                    return;
                }
            }

            int maxCost;
            int maximumCost = Convert.ToInt32(rngBudget.Attributes["max"]);

            if (!int.TryParse(rngBudget.Text, out maxCost) || maxCost < 0 || maxCost > maximumCost)
            {
                lblFilterError.Text = "Choose a valid budget using the slider.";
                lblFilterError.Visible = true;
                return;
            }

            int maxTime;
            int maximumTime = Convert.ToInt32(rngTime.Attributes["max"]);

            if (!int.TryParse(rngTime.Text, out maxTime) || maxTime < 0 || maxTime > maximumTime)
            {
                lblFilterError.Text = "Choose a valid time using the slider.";
                lblFilterError.Visible = true;
                return;
            }

            List<int> categoryIds = new List<int>();

            foreach (ListItem item in cblCategories.Items)
            {
                if (item.Selected)
                {
                    categoryIds.Add(Convert.ToInt32(item.Value));
                }
            }

            DataTable recipes = RecipeRepository.GetRecipesByFilters(
                txtSearch.Text.Trim(),
                rblDifficulty.SelectedValue,
                maxCost,
                maxTime,
                ddlCuisine.SelectedValue,
                ddlRegion.SelectedValue,
                minServings,
                categoryIds,
                ddlSort.SelectedValue);

            rptRecipes.DataSource = recipes;
            rptRecipes.DataBind();

            recipeCount.Text = recipes.Rows.Count + " recipes";
        }

        protected void rptRecipes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RecipeCard card = (RecipeCard)e.Item.FindControl("recipeCard");
                card.DisplayRecipe((DataRowView)e.Item.DataItem);
            }
        }

        private void SelectCategory()
        {
            string category = Request.QueryString["category"];

            foreach (ListItem item in cblCategories.Items)
            {
                item.Selected = string.Equals(
                    item.Text,
                    category,
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadRecipes();
        }

        protected void btnApplyFilters_Click(object sender, EventArgs e)
        {
            LoadRecipes();
        }

        protected void ddlSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecipes();
        }
    }
}
