using System;
using System.Data;
using System.Data.SqlClient;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            if (!IsPostBack)
            {
                LoadStatistics();
                LoadRecentRecipes();
            }
        }

        private void LoadStatistics()
        {
            const string sql = @"
                SELECT 'Total users' AS Title, COUNT(*) AS Total FROM dbo.users

                UNION ALL

                SELECT 'Approved recipes', COUNT(*) FROM dbo.recipes WHERE status = 'Approved'

                UNION ALL

                SELECT 'Pending recipes', COUNT(*) FROM dbo.recipes WHERE status = 'Pending'

                UNION ALL

                SELECT 'Pending community items',
                    (SELECT COUNT(*) FROM dbo.posts WHERE status = 'Pending')
                    +
                    (SELECT COUNT(*) FROM dbo.postComments WHERE status = 'Pending')
                    +
                    (SELECT COUNT(*) FROM dbo.reviews WHERE status = 'Pending')

                UNION ALL

                SELECT 'Active quizzes', COUNT(*) FROM dbo.challenges WHERE isActive = 1

                UNION ALL

                SELECT 'Active simulations', COUNT(*) FROM dbo.simulations WHERE isActive = 1;";

            using (SqlConnection connection = Database.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataTable statistics = new DataTable();
                adapter.Fill(statistics);

                rptStatistics.DataSource = statistics;
                rptStatistics.DataBind();
            }
        }

        private void LoadRecentRecipes()
        {
            const string sql = @"
                SELECT TOP (5) r.recipeID, r.title, u.displayName, r.status, r.createdAt
                FROM dbo.recipes AS r
                INNER JOIN dbo.users AS u ON r.userID = u.userID
                ORDER BY r.createdAt DESC, r.recipeID DESC;";

            using (SqlConnection connection = Database.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataTable recipes = new DataTable();
                adapter.Fill(recipes);

                gvRecentRecipes.DataSource = recipes;
                gvRecentRecipes.DataBind();
            }
        }
    }
}