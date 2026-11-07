using Masaking.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Masaking.Controls
{
    public partial class RecipeFilters : System.Web.UI.UserControl
    {
        public event EventHandler ApplyFilters;
        public bool IncludeOwnRecipes { get; set; }

        public string Difficulty
        {
            get { return rblDifficulty.SelectedValue; }
        }

        public string Cuisine
        {
            get { return ddlCuisine.SelectedValue; }
        }

        public string Region
        {
            get { return ddlRegion.SelectedValue; }
        }

        public int MaximumCost
        {
            get { return Convert.ToInt32(rngBudget.Text); }
        }

        public int MaximumTime
        {
            get { return Convert.ToInt32(rngTime.Text); }
        }

        public int MinimumServings
        {
            get
            {
                return string.IsNullOrWhiteSpace(txtServings.Text)
                    ? 0
                    : Convert.ToInt32(txtServings.Text);
            }
        }

        public List<int> CategoryIds
        {
            get
            {
                List<int> ids = new List<int>();

                foreach (ListItem item in cblCategories.Items)
                {
                    if (item.Selected)
                    {
                        ids.Add(Convert.ToInt32(item.Value));
                    }
                }

                return ids;
            }
        }

        public void Initialise(string categoryFromHome = null)
        {
            LoadCategories();
            LoadOptions();

            foreach (ListItem item in cblCategories.Items)
            {
                item.Selected = item.Text == categoryFromHome;
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

        private void LoadOptions()
        {
            string condition = "status = 'Approved'";

            if (IncludeOwnRecipes)
            {
                condition = @"
                    (status = 'Approved'
                     OR (userID = @viewerID
                         AND status IN ('Pending', 'Rejected')))";
            }

            SqlConnection con = Database.Open();
            DataSet options = new DataSet();

            try
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT DISTINCT cuisine
                    FROM dbo.recipes
                    WHERE " + condition + @"
                      AND cuisine <> ''
                    ORDER BY cuisine;

                    SELECT DISTINCT region
                    FROM dbo.recipes
                    WHERE " + condition + @"
                      AND region <> ''
                    ORDER BY region;

                    SELECT
                        CEILING(COALESCE(MAX(estimatedCost), 0)) AS maxCost,
                        COALESCE(MAX(preparationTime), 0) AS maxTime
                    FROM dbo.recipes
                    WHERE " + condition + ";", con);

                cmd.Parameters.Add("@viewerID", SqlDbType.Int).Value =
                    Database.UserId.HasValue
                        ? (object)Database.UserId.Value
                        : DBNull.Value;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(options);
            }
            finally
            {
                con.Close();
            }

            ddlCuisine.DataSource = options.Tables[0];
            ddlCuisine.DataTextField = "cuisine";
            ddlCuisine.DataValueField = "cuisine";
            ddlCuisine.DataBind();
            ddlCuisine.Items.Insert(0, new ListItem("All cuisines", ""));

            ddlRegion.DataSource = options.Tables[1];
            ddlRegion.DataTextField = "region";
            ddlRegion.DataValueField = "region";
            ddlRegion.DataBind();
            ddlRegion.Items.Insert(0, new ListItem("All regions", ""));

            DataRow limits = options.Tables[2].Rows[0];

            rngBudget.Attributes["max"] =
                Math.Max(1, Convert.ToInt32(limits["maxCost"])).ToString();

            rngTime.Attributes["max"] =
                Math.Max(1, Convert.ToInt32(limits["maxTime"])).ToString();
        }

        public bool ValidateFilters()
        {
            lblFilterError.Visible = false;

            int servings;

            if (!string.IsNullOrWhiteSpace(txtServings.Text) && (!int.TryParse(txtServings.Text, out servings) || servings < 0))
            {
                return ShowError(
                    "Enter a whole number of servings, starting from 0.");
            }

            int cost;
            int maximumCost = Convert.ToInt32(rngBudget.Attributes["max"]);

            if (!int.TryParse(rngBudget.Text, out cost) || cost < 0 || cost > maximumCost)
            {
                return ShowError("Choose a valid budget using the slider.");
            }

            int time;
            int maximumTime = Convert.ToInt32(rngTime.Attributes["max"]);

            if (!int.TryParse(rngTime.Text, out time) || time < 0 || time > maximumTime)
            {
                return ShowError("Choose a valid time using the slider.");
            }

            return true;
        }

        private bool ShowError(string message)
        {
            lblFilterError.Text = message;
            lblFilterError.Visible = true;
            return false;
        }

        protected void btnApplyFilters_Click(object sender, EventArgs e)
        {
            if (ApplyFilters != null)
            {
                ApplyFilters(this, EventArgs.Empty);
            }
        }

    }
}