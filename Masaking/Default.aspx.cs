using Masaking.Controls;
using Masaking.Infrastructure;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Masaking
{
    public partial class _Default : ResponsivePage
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
            DataTable categories = CategoryRepository.GetActiveCategories(false);

            rptCategories.DataSource = categories;
            rptCategories.DataBind();
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
                    ResolveUrl("~/Recipes/BrowseRecipes") +
                    "?category=" + Server.UrlEncode(name);
            }
        }

        private void LoadPopularRecipes()
        {
            DataTable recipes = RecipeRepository.GetPopularRecipes();

            rptPopularRecipes.DataSource = recipes;
            rptPopularRecipes.DataBind();

            lblNoPopularRecipes.Visible = recipes.Rows.Count == 0;
        }

        protected void rptPopularRecipes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RecipeCard card = (RecipeCard)e.Item.FindControl("recipeCard");
                card.DisplayRecipe((DataRowView)e.Item.DataItem);
            }
        }
    }
}
