using Masaking.Controls;
using Masaking.Infrastructure;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace Masaking.Recipes
{
    public partial class BrowseRecipes : ResponsivePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                recipeFilters.Initialise(Request.QueryString["category"]);
                LoadRecipes();
            }
        }

        private void LoadRecipes()
        {
            if (!recipeFilters.ValidateFilters())
            {
                return;
            }

            DataTable recipes = RecipeRepository.GetRecipesByFilters(
                txtSearch.Text.Trim(),
                recipeFilters.Difficulty,
                recipeFilters.MaximumCost,
                recipeFilters.MaximumTime,
                recipeFilters.Cuisine,
                recipeFilters.Region,
                recipeFilters.MinimumServings,
                recipeFilters.CategoryIds,
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

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadRecipes();
        }

        protected void Filters_ApplyFilters(object sender, EventArgs e)
        {
            LoadRecipes();
        }

        protected void ddlSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecipes();
        }
    }
}
