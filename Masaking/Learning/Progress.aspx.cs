using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Learning
{
    public partial class Progress : Masaking.Infrastructure.ResponsivePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) { return; }

            if (!IsPostBack)
            {
                BindProgress();
                BindSimulations();
            }
        }

        private void BindProgress()
        {
            const string sql = @" SELECT a.attemptID, a.status, a.score, a.startedAt, a.completedAt, a.remainingSeconds, a.lastResumedAt, c.title, (SELECT COUNT(*) FROM dbo.challengeQuestions q WHERE q.challengeID=a.challengeID) AS questionCount,
              (SELECT COUNT(*) FROM dbo.challengeAnswers answer INNER JOIN dbo.challengeQuestions question ON question.questionID=answer.questionID
                WHERE answer.attemptID=a.attemptID AND question.challengeID=a.challengeID AND answer.selectedOption=question.correctOption) AS savedCorrect
              FROM dbo.challengeAttempts a INNER JOIN dbo.challenges c ON c.challengeID=a.challengeID WHERE a.userID=@user ORDER BY a.startedAt DESC, a.attemptID DESC";

            DataTable attempts = new DataTable();

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                adapter.Fill(attempts);
            }

            attempts.Columns.Add("displayStatus", typeof(string));
            attempts.Columns.Add("resultText", typeof(string));
            attempts.Columns.Add("actionText", typeof(string));

            int completed = 0;
            int timedOut = 0;
            int unfinished = 0;
            decimal? best = null;

            DateTime now = DateTime.UtcNow;

            foreach (DataRow attempt in attempts.Rows)
            {
                string status = Convert.ToString(attempt["status"]);
                int total = Convert.ToInt32(attempt["questionCount"]);
                int score = Convert.ToInt32(attempt["score"]);

                bool expired = false;

                if (status == "InProgress" && attempt["remainingSeconds"] != DBNull.Value && attempt["lastResumedAt"] != DBNull.Value)
                {
                    int remaining = Convert.ToInt32(attempt["remainingSeconds"]);
                    DateTime resumed = Convert.ToDateTime(attempt["lastResumedAt"]);

                    expired = (now - resumed).TotalSeconds >= remaining;
                }

                if (expired)
                {
                    status = "TimedOut";
                    score = Convert.ToInt32(attempt["savedCorrect"]);
                }

                bool finished = status == "Completed" || status == "TimedOut";

                if (status == "Completed")
                {
                    completed++;
                }
                else if (status == "TimedOut")
                {
                    timedOut++;
                }
                else if (status == "Paused" || status == "InProgress")
                {
                    unfinished++;
                }

                if (finished)
                {
                    if (total > 0)
                    {
                        decimal percentage = 100m * score / total;

                        attempt["resultText"] = score + " / " + total + " (" + percentage.ToString("0.#") + "%)";

                        if (!best.HasValue || percentage > best.Value)
                        {
                            best = percentage;
                        }
                    }
                    else
                    {
                        attempt["resultText"] = "Result unavailable";
                    }

                    attempt["displayStatus"] = expired ? "Timed out - open to finalise" : status == "TimedOut" ? "Timed out" : "Completed";

                    attempt["actionText"] = "View result";
                }
                else
                {
                    attempt["displayStatus"] =
                        status == "InProgress" ? "In progress" : status;

                    attempt["resultText"] = "Not submitted";
                    attempt["actionText"] = "Open attempt";
                }
            }

            lblCompleted.Text = completed.ToString();
            lblTimedOut.Text = timedOut.ToString();
            lblUnfinished.Text = unfinished.ToString();

            lblBest.Text = best.HasValue ? best.Value.ToString("0.#") + "%" : "-";
            gvAttempts.DataSource = attempts;
            gvAttempts.DataBind();
        }

        private void BindSimulations()
        {
            const string sql = @"SELECT p.simulationProgressID,p.simulationID,p.status,p.score, p.currentStepNumber,p.mistakes,p.startedAt,p.completedAt, s.title,s.isActive,r.status AS recipeStatus,
              (SELECT COUNT(*) FROM dbo.simulationSteps st WHERE st.simulationID=s.simulationID) AS stepCount FROM dbo.simulationProgress p
                INNER JOIN dbo.simulations s ON s.simulationID=p.simulationID
                INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID WHERE p.userID=@user
                ORDER BY p.startedAt DESC,p.simulationProgressID DESC;";
            DataTable history = new DataTable();
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                adapter.Fill(history);
            }
            history.Columns.Add("displayStatus", typeof(string));
            history.Columns.Add("progressText", typeof(string));
            history.Columns.Add("resultText", typeof(string));
            history.Columns.Add("canOpen", typeof(bool));
            int completed = 0, active = 0, restarted = 0;
            foreach (DataRow row in history.Rows)
            {
                string status = Convert.ToString(row["status"]);
                int total = Convert.ToInt32(row["stepCount"]);
                int nextStep = row["currentStepNumber"] == DBNull.Value ? 1 : Convert.ToInt32(row["currentStepNumber"]);
                int done = status == "Completed" ? total : Math.Max(0, Math.Min(total, nextStep - 1));
                row["displayStatus"] = status == "InProgress" ? "In progress" : status == "Abandoned" ? "Restarted" : status;
                row["progressText"] = done + " / " + total;
                row["resultText"] = status == "Completed" ? Convert.ToInt32(row["score"]) + "%" : "-";
                row["canOpen"] = Convert.ToBoolean(row["isActive"]) && Convert.ToString(row["recipeStatus"]) == "Approved" && (status == "Completed" || status == "InProgress");
                if (status == "Completed") completed++;
                else if (status == "InProgress") active++;
                else if (status == "Abandoned") restarted++;
            }
            lblSimulationSummary.Text = "Completed runs: " + completed + ". In progress: " + active + ". Restarted: " + restarted + ".";
            gvSimulations.DataSource = history;
            gvSimulations.DataBind();
        }

        protected void gvSimulations_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            gvSimulations.PageIndex = e.NewPageIndex;
            BindSimulations();
        }

        protected void gvAttempts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (!Database.RequireUser(Response)) { return; }
            gvAttempts.PageIndex = e.NewPageIndex;
            BindProgress();
        }
    }
}