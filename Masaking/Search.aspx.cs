using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking
{
    public partial class Search : ResponsivePage
    {
        protected Label lblSearchSummary;
        protected Label lblSearchMessage;
        protected Panel pnlRecipeResults, pnlPostResults;
        protected Repeater rptRecipeResults, rptPostResults;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            if (IsPostBack) return;
            string query = (Request.QueryString["q"] ?? "").Trim();
            if (query.Length == 0 || query.Length > 100)
            {
                lblSearchSummary.Text = "Use the search icon above to search recipes and community posts (up to 100 characters).";
                return;
            }
            lblSearchSummary.Text = "Results for “" + Server.HtmlEncode(query) + "”";
            string pattern = "%" + query.Replace("~", "~~").Replace("%", "~%").Replace("_", "~_").Replace("[", "~[") + "%";
            try
            {
                using (SqlConnection cn = Database.Open())
                {
                    DataTable recipes = Read(cn, @"SELECT TOP (12) r.recipeID, r.title, r.cuisine, u.displayName
                        FROM recipes r JOIN users u ON u.userID=r.userID
                        WHERE r.status='Approved' AND u.isActive=1 AND
                        (r.title LIKE @query ESCAPE '~' OR r.cuisine LIKE @query ESCAPE '~')
                        ORDER BY r.createdAt DESC, r.recipeID DESC", pattern);
                    DataTable posts = Read(cn, @"SELECT TOP (12) p.postID, p.content, u.displayName, u.username
                        FROM posts p JOIN users u ON u.userID=p.userID
                        WHERE p.status='Approved' AND u.isActive=1 AND
                        (p.content LIKE @query ESCAPE '~' OR u.displayName LIKE @query ESCAPE '~' OR u.username LIKE @query ESCAPE '~')
                        ORDER BY p.createdAt DESC, p.postID DESC", pattern);
                    rptRecipeResults.DataSource = recipes; rptRecipeResults.DataBind(); pnlRecipeResults.Visible = recipes.Rows.Count > 0;
                    rptPostResults.DataSource = posts; rptPostResults.DataBind(); pnlPostResults.Visible = posts.Rows.Count > 0;
                    if (recipes.Rows.Count + posts.Rows.Count == 0) lblSearchMessage.Text = "No matches found. Try another recipe name or cooking topic.";
                }
            }
            catch (SqlException ex)
            {
                Trace.Warn("Search", "Unable to search.", ex);
                lblSearchMessage.CssClass = "alert alert-warning d-block";
                lblSearchMessage.Text = "Search is temporarily unavailable. Please try again later.";
            }
        }
        private static DataTable Read(SqlConnection connection, string sql, string query)
        {
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.Add("@query", SqlDbType.NVarChar, 402).Value = query;
                DataTable result = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(result);
                return result;
            }
        }
        protected void rptRecipeResults_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            var row = (DataRowView)e.Item.DataItem;
            ((HyperLink)e.Item.FindControl("lnkRecipeResult")).NavigateUrl = "~/Recipes/RecipeDetails.aspx?id=" + Convert.ToInt32(row["recipeID"]);
            ((Label)e.Item.FindControl("lblRecipeTitle")).Text = System.Web.HttpUtility.HtmlEncode(Convert.ToString(row["title"]));
            ((Label)e.Item.FindControl("lblCuisine")).Text = System.Web.HttpUtility.HtmlEncode(Convert.ToString(row["cuisine"]));
            ((Label)e.Item.FindControl("lblRecipeAuthor")).Text = System.Web.HttpUtility.HtmlEncode(Convert.ToString(row["displayName"]));
        }

        protected void rptPostResults_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            var row = (DataRowView)e.Item.DataItem;
            ((HyperLink)e.Item.FindControl("lnkPostResult")).NavigateUrl = "~/Community/Post.aspx?id=" + Convert.ToInt32(row["postID"]);
            ((Label)e.Item.FindControl("lblPostAuthor")).Text = System.Web.HttpUtility.HtmlEncode(Convert.ToString(row["displayName"]));
            ((Label)e.Item.FindControl("lblUsername")).Text = System.Web.HttpUtility.HtmlEncode(Convert.ToString(row["username"]));
            ((Label)e.Item.FindControl("lblSnippet")).Text = System.Web.HttpUtility.HtmlEncode(Snippet(row["content"]));
        }

        protected string Snippet(object value) { string text = Convert.ToString(value); return text.Length > 220 ? text.Substring(0, 217) + "…" : text; }
    }
}
