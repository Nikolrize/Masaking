using Masaking.Controls;
using Masaking.Infrastructure;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Masaking.Recipes
{
    public partial class MyRecipes : Page
    {
        private string SelectedTab
        {
            get
            {
                return (string)(ViewState["SelectedTab"] ?? "All");
            }
            set
            {
                ViewState["SelectedTab"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response))
            {
                return;
            }

            if (!IsPostBack)
            {
                recipeFilters.Initialise();
                LoadRecipes();
            }
        }

        private void LoadRecipes()
        {
            UpdateTab(tabAll);
            UpdateTab(tabApproved);
            UpdateTab(tabPending);
            UpdateTab(tabRejected);
            UpdateTab(tabFavourites);

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
                ddlSort.SelectedValue,
                SelectedTab);

            rptRecipes.DataSource = recipes;
            rptRecipes.DataBind();

            collectionTitle.Text = SelectedTab;
            recipeCount.Text = recipes.Rows.Count + " recipes";
            pnlEmpty.Visible = recipes.Rows.Count == 0;
        }

        private void UpdateTab(LinkButton tab)
        {
            bool selected = tab.CommandArgument == SelectedTab;

            tab.CssClass = selected
                ? "recipe-collection-tab active"
                : "recipe-collection-tab";
        }

        protected void RecipeTab_Command(object sender, CommandEventArgs e)
        {
            SelectedTab = e.CommandArgument.ToString();
            LoadRecipes();
        }

        protected void ReloadRecipes(object sender, EventArgs e)
        {
            LoadRecipes();
        }

        protected void Filters_ApplyFilters(object sender, EventArgs e)
        {
            LoadRecipes();
        }

        protected void rptRecipes_ItemDataBound(
            object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RecipeCard card = (RecipeCard)e.Item.FindControl("recipeCard");

                card.FromMyRecipes = true;
                DataRowView recipe = (DataRowView)e.Item.DataItem;
                card.DisplayRecipe(recipe);
                Panel activities = (Panel)e.Item.FindControl("pnlActivities");
                activities.Visible = Database.UserId.HasValue
                    && Convert.ToInt32(recipe["userID"]) == Database.UserId.Value
                    && Convert.ToString(recipe["status"]) == "Approved";
            }
        }
    }
}