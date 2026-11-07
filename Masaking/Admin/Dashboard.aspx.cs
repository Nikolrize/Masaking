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
                LoadRecentActivity();
            }
        }

        private void LoadStatistics()
        {
            const string sql = @"
                SELECT 'Total users' AS Title, COUNT(*) AS Total, 'users' AS Theme, 'bi-people' AS Icon FROM dbo.users
                UNION ALL
                SELECT 'Approved recipes', COUNT(*), 'approved', 'bi-book'  FROM dbo.recipes WHERE status='Approved'
                UNION ALL
                SELECT 'Pending recipes', COUNT(*), 'pending', 'bi-clock' FROM dbo.recipes WHERE status='Pending'
                UNION ALL
                SELECT 'Pending community items', (SELECT COUNT(*) FROM dbo.posts WHERE status='Pending') + (SELECT COUNT(*) FROM dbo.postComments WHERE status='Pending') + (SELECT COUNT(*) FROM dbo.reviews WHERE status='Pending'), 'community', 'bi-chat-dots'
                UNION ALL
                SELECT 'Published challenges', COUNT(*), 'challenges', 'bi-stopwatch' FROM dbo.challenges WHERE isActive=1
                UNION ALL
                SELECT 'Published simulations', COUNT(*), 'simulations', 'bi-egg-fried' FROM dbo.simulations WHERE isActive=1;";
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
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
                SELECT TOP (5) r.recipeID,r.title,u.displayName,r.status,r.createdAt
                FROM dbo.recipes r INNER JOIN dbo.users u ON u.userID=r.userID
                ORDER BY r.createdAt DESC,r.recipeID DESC;";
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataTable recipes = new DataTable();
                adapter.Fill(recipes);
                recipes.Columns.Add("submittedLocal", typeof(DateTime));
                foreach (DataRow row in recipes.Rows)
                    row["submittedLocal"] = MalaysiaTime(row["createdAt"]);
                gvRecentRecipes.DataSource = recipes;
                gvRecentRecipes.DataBind();
            }
        }

        //combines completed challenge and simulation runs 
        private void LoadRecentActivity()
        {
            const string sql = @"
                SELECT TOP (5) activityType,title,displayName,score,total,completedAt FROM (SELECT 'Challenge' AS activityType,c.title,u.displayName, a.score, (SELECT COUNT(*) FROM dbo.challengeQuestions q WHERE q.challengeID=a.challengeID) AS total, a.completedAt FROM dbo.challengeAttempts a
                    INNER JOIN dbo.challenges c ON c.challengeID=a.challengeID
                    INNER JOIN dbo.users u ON u.userID=a.userID
                    WHERE a.status='Completed' AND a.completedAt IS NOT NULL
                    UNION ALL
                    SELECT 'Simulation',s.title,u.displayName,p.score,100,p.completedAt
                    FROM dbo.simulationProgress p
                    INNER JOIN dbo.simulations s ON s.simulationID=p.simulationID
                    INNER JOIN dbo.users u ON u.userID=p.userID
                    WHERE p.status='Completed' AND p.completedAt IS NOT NULL
                ) activity
                ORDER BY completedAt DESC,activityType,title;";
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataTable activity = new DataTable();
                adapter.Fill(activity);
                activity.Columns.Add("resultText", typeof(string));
                activity.Columns.Add("completedLocal", typeof(DateTime));
                foreach (DataRow row in activity.Rows)
                {
                    int score = Convert.ToInt32(row["score"]);
                    int total = Convert.ToInt32(row["total"]);
                    if (Convert.ToString(row["activityType"]) == "Simulation")
                        row["resultText"] = score + "% accuracy";
                    else
                        row["resultText"] = total > 0
                            ? score + " / " + total + " (" + (100m * score / total).ToString("0.#") + "%)"
                            : "Result unavailable";
                    row["completedLocal"] = MalaysiaTime(row["completedAt"]);
                }
                gvRecentActivity.DataSource = activity;
                gvRecentActivity.DataBind();
            }
        }

        private DateTime MalaysiaTime(object value)
        {
            DateTime utc = DateTime.SpecifyKind(Convert.ToDateTime(value), DateTimeKind.Utc);
            TimeZoneInfo malaysia = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, malaysia);
        }

        protected string StatusClass(object value)
        {
            string status = Convert.ToString(value);
            string colour = status == "Approved" ? "approved" : status == "Pending" ? "pending" : status == "Rejected" ? "rejected" : "other";
            return "badge rounded-pill status-" + colour;
        }
    }
}