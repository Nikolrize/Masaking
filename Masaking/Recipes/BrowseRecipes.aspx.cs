using Masaking.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Masaking.Recipes
{
    public partial class BrowseRecipes : System.Web.UI.Page
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
            SqlConnection con = Database.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT categoryID, categoryName
                FROM dbo.categories
                WHERE isActive = 1
                ORDER BY categoryName;", con);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable categories = new DataTable();
            adapter.Fill(categories);

            cblCategories.DataSource = categories;
            cblCategories.DataTextField = "categoryName";
            cblCategories.DataValueField = "categoryID";
            cblCategories.DataBind();

            con.Close();
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

            SqlConnection con = Database.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT
                    r.recipeID,
                    r.title,
                    r.description,
                    r.cuisine,
                    r.region,
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
                ) AS ratings ON ratings.recipeID = r.recipeID
                WHERE r.status = 'Approved'
                    AND (@search = ''
                        OR r.title LIKE '%' + @search + '%')
                    AND (@difficulty = ''
                        OR r.difficulty = @difficulty)
                    AND (@maxCost = 0
                        OR r.estimatedCost <= @maxCost)
                    AND (@maxTime = 0
                        OR r.preparationTime <= @maxTime)
                    AND (@cuisine = ''
                        OR r.cuisine = @cuisine)
                    AND (@region = ''
                        OR r.region = @region)
                    AND (@minServings = 0
                        OR r.servings >= @minServings)", con);

            cmd.Parameters.Add("@search", SqlDbType.NVarChar, -1).Value = txtSearch.Text.Trim();
            cmd.Parameters.Add("@difficulty", SqlDbType.NVarChar, 30).Value = rblDifficulty.SelectedValue;
            cmd.Parameters.Add("@maxCost", SqlDbType.Decimal).Value = maxCost;
            cmd.Parameters.Add("@maxTime", SqlDbType.Int).Value = maxTime;
            cmd.Parameters.Add("@cuisine", SqlDbType.NVarChar, -1).Value = ddlCuisine.SelectedValue;
            cmd.Parameters.Add("@region", SqlDbType.NVarChar, -1).Value = ddlRegion.SelectedValue;
            cmd.Parameters.Add("@minServings", SqlDbType.Int).Value = minServings;

            ApplyCategoryFilter(cmd);
            ApplySorting(cmd);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable recipes = new DataTable();
            adapter.Fill(recipes);

            rptRecipes.DataSource = recipes;
            rptRecipes.DataBind();

            recipeCount.Text = recipes.Rows.Count + " recipes";

            con.Close();
        }

        protected void rptRecipes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView recipe = (DataRowView)e.Item.DataItem;

                Label recipeTitle = (Label)e.Item.FindControl("recipeTitle");
                Label recipeCategory = (Label)e.Item.FindControl("recipeCategory");
                Image recipeImage = (Image)e.Item.FindControl("recipeImage");
                HyperLink recipeLink = (HyperLink)e.Item.FindControl("recipeLink");
                Label recipeRating = (Label)e.Item.FindControl("recipeRating");
                Label recipeCuisine = (Label)e.Item.FindControl("recipeCuisine");
                Label recipePreparationTime = (Label)e.Item.FindControl("recipePreparationTime");
                Label recipeDifficulty = (Label)e.Item.FindControl("recipeDifficulty");
                Label recipeBudget = (Label)e.Item.FindControl("recipeBudget");
                Label recipeServings = (Label)e.Item.FindControl("recipeServings");
                PlaceHolder phRatingStar = (PlaceHolder)e.Item.FindControl("phRatingStar");

                recipeTitle.Text = Server.HtmlEncode(recipe["title"].ToString());
                recipeCategory.Text = Server.HtmlEncode(recipe["categoryName"].ToString());
                recipeImage.ImageUrl = recipe["image"].ToString();
                recipeImage.AlternateText = recipe["title"].ToString();
                recipeLink.NavigateUrl = "RecipeDetails.aspx?id=" + recipe["recipeID"].ToString();
                recipeCuisine.Text = Server.HtmlEncode(recipe["cuisine"].ToString());
                recipePreparationTime.Text = recipe["preparationTime"].ToString() + " mins";
                recipeBudget.Text = "RM" + recipe["estimatedCost"].ToString();
                recipeServings.Text = recipe["servings"].ToString();

                string difficulty = recipe["difficulty"].ToString();
                recipeDifficulty.Text = Server.HtmlEncode(difficulty);
                recipeDifficulty.CssClass = "badge rounded-pill recipe-difficulty " + difficulty.ToLowerInvariant();

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

        private void ApplyCategoryFilter(SqlCommand cmd)
        {
            List<string> parameters = new List<string>();

            foreach (ListItem item in cblCategories.Items)
            {
                if (item.Selected)
                {
                    string name = "@category" + parameters.Count;
                    parameters.Add(name);

                    cmd.Parameters.Add(name, SqlDbType.Int).Value = Convert.ToInt32(item.Value);
                }
            }

            if (parameters.Count > 0)
            {
                cmd.CommandText += " AND c.categoryID IN (" + string.Join(",", parameters) + ")";
            }
        }

        private void ApplySorting(SqlCommand cmd)
        {
            string orderBy = "r.title ASC";

            switch (ddlSort.SelectedValue)
            {
                case "nameDesc":
                    orderBy = "r.title DESC";
                    break;

                case "newest":
                    orderBy = "r.createdAt DESC";
                    break;

                case "rating":
                    orderBy = "ratings.averageRating DESC, reviewCount DESC";
                    break;

                case "reviews":
                    orderBy = "reviewCount DESC";
                    break;

                case "cost":
                    orderBy = "r.estimatedCost ASC";
                    break;

                case "time":
                    orderBy = "r.preparationTime ASC";
                    break;
            }

            cmd.CommandText += " ORDER BY " + orderBy + ", r.recipeID ASC;";
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