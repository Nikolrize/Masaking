using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Learning
{
    public partial class Challenges : ResponsivePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            if (!IsPostBack) BindChallenges();
        }

        private void BindChallenges()
        {
            const string sql = @"
                SELECT c.challengeID,c.title,c.description,c.difficulty, CAST(c.timeLimit / 60.0 AS DECIMAL(6,1)) AS minutes, (SELECT COUNT(*) FROM dbo.challengeQuestions q WHERE q.challengeID=c.challengeID) AS questionCount, history.attemptCount,history.completedCount,saved.attemptID AS savedAttemptID,saved.status AS savedStatus
                FROM dbo.challenges c
                OUTER APPLY (SELECT COUNT(*) AS attemptCount,COUNT(CASE WHEN a.status='Completed' THEN 1 END) AS completedCount FROM dbo.challengeAttempts a WHERE a.challengeID=c.challengeID AND a.userID=@user) history
                OUTER APPLY (SELECT TOP (1) a.attemptID,a.status FROM dbo.challengeAttempts a WHERE a.challengeID=c.challengeID AND a.userID=@user AND a.status IN ('InProgress','Paused') ORDER BY a.startedAt DESC,a.attemptID DESC) saved
                WHERE c.isActive=1 AND (c.recipeID IS NULL OR EXISTS (SELECT 1 FROM dbo.recipes linkedRecipe WHERE linkedRecipe.recipeID=c.recipeID AND linkedRecipe.status='Approved'))
                    AND EXISTS (SELECT 1 FROM dbo.challengeQuestions q WHERE q.challengeID=c.challengeID)
                    AND (@search='' OR CHARINDEX(@search,c.title)>0)
                    AND (@difficulty='' OR c.difficulty=@difficulty)
                ORDER BY c.createdAt DESC,c.challengeID DESC;";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                cmd.Parameters.Add("@search", SqlDbType.NVarChar, 150).Value = txtSearch.Text.Trim();
                cmd.Parameters.Add("@difficulty", SqlDbType.NVarChar, 20).Value = ddlDifficulty.SelectedValue;
                DataTable challenges = new DataTable();
                adapter.Fill(challenges);
                lvChallenges.DataSource = challenges;
                lvChallenges.DataBind();
                pagerChallenges.Visible = challenges.Rows.Count > pagerChallenges.PageSize;
                lblCount.Text = challenges.Rows.Count + (challenges.Rows.Count == 1 ? " challenge found." : " challenges found.");
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            pagerChallenges.SetPageProperties(0, pagerChallenges.PageSize, false);
            BindChallenges();
        }

        protected void lvChallenges_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            pagerChallenges.SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            BindChallenges();
        }

        protected string DifficultyClass(object value)
        {
            string difficulty = Convert.ToString(value).Trim().ToLowerInvariant();
            if (difficulty != "easy" && difficulty != "medium" && difficulty != "hard") difficulty = "other";
            return "badge rounded-pill level-" + difficulty;
        }

        protected string SavedAttemptText(object status)
        {
            string saved = Convert.ToString(status);
            if (saved == "Paused") return "Saved attempt paused. Continue when you are ready.";
            if (saved == "InProgress") return "An attempt is in progress. Its timer is still running.";
            return "Ready when you are. You can save and pause during the challenge.";
        }

        protected string ChallengeButtonText(object savedAttemptId, object attemptCount)
        {
            if (savedAttemptId != DBNull.Value) return "Open saved attempt";
            return Convert.ToInt32(attemptCount) > 0 ? "Try again" : "Start challenge";
        }

        protected string ChallengeUrl(object challengeId, object savedAttemptId)
        {
            //opening the existing page preserves its server-side timer and ownership checks
            return savedAttemptId == DBNull.Value ? "~/Learning/TakeQuiz.aspx?id=" + Convert.ToInt32(challengeId) : "~/Learning/TakeQuiz.aspx?attempt=" + Convert.ToInt32(savedAttemptId);
        }
    }
}
