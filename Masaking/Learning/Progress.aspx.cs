using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Learning
{
    public partial class Progress : ResponsivePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            if (!IsPostBack)
            {
                BindProgress();
                BindSimulations();
            }
        }

        private void BindProgress()
        {
            const string sql = @" SELECT a.attemptID,a.challengeID,a.status,a.score,a.startedAt,a.completedAt, a.remainingSeconds,a.lastResumedAt,c.title, (SELECT COUNT(*) FROM dbo.challengeQuestions q WHERE q.challengeID=a.challengeID) AS questionCount FROM dbo.challengeAttempts a INNER JOIN dbo.challenges c ON c.challengeID=a.challengeID WHERE a.userID=@user ORDER BY a.startedAt DESC,a.attemptID DESC;";
            DataTable attempts = new DataTable();
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                adapter.Fill(attempts);
            }

            ChallengeSummary summary = PrepareChallengeHistory(attempts, DateTime.UtcNow);
            lblChallengesCompleted.Text = summary.CompletedChallenges.ToString();
            lblFinished.Text = (summary.Submitted + summary.Expired).ToString();
            lblCompleted.Text = summary.Submitted.ToString();
            lblTimedOut.Text = summary.Expired.ToString();
            lblUnfinished.Text = summary.Unfinished.ToString();
            lblBest.Text = summary.Best.HasValue ? summary.Best.Value.ToString("0.#") + "%" : "No results yet";
            lblPendingSummary.Visible = summary.PendingFinalisation > 0;
            lblPendingSummary.Text = summary.PendingFinalisation + (summary.PendingFinalisation == 1 ? " expired attempt needs" : " expired attempts need") + " finalisation. These are included in finished and time-expired counts, but not your best saved result. Select Finalise result in the history below.";
            BindHistory(gvAttempts, attempts);
        }

        // Prepare display values without modifying any saved attempts.
        //prepare display values without modifying any saved attempts
        internal static ChallengeSummary PrepareChallengeHistory(DataTable attempts, DateTime utcNow)
        {
            attempts.Columns.Add("displayStatus", typeof(string));
            attempts.Columns.Add("historyStatus", typeof(string));
            attempts.Columns.Add("resultText", typeof(string));
            attempts.Columns.Add("actionText", typeof(string));
            attempts.Columns.Add("startedLocal", typeof(DateTime));
            attempts.Columns.Add("finishedText", typeof(string));
            ChallengeSummary summary = new ChallengeSummary();
            HashSet<int> completedChallenges = new HashSet<int>();

            foreach (DataRow row in attempts.Rows)
            {
                string status = Convert.ToString(row["status"]);
                int total = Convert.ToInt32(row["questionCount"]);
                bool expired = false;
                if (status == "InProgress" && row["remainingSeconds"] != DBNull.Value && row["lastResumedAt"] != DBNull.Value)
                {
                    int remaining = Convert.ToInt32(row["remainingSeconds"]);
                    DateTime anchor = Convert.ToDateTime(row["lastResumedAt"]);
                    expired = (utcNow - anchor).TotalSeconds >= remaining;
                }
                bool savedResult = status == "Completed" || status == "TimedOut";
                row["startedLocal"] = LocalDate(row["startedAt"]);
                row["historyStatus"] = expired ? "AwaitingFinalisation" : status;
                row["finishedText"] = savedResult && row["completedAt"] != DBNull.Value ? MalaysianTime(Convert.ToDateTime(row["completedAt"])).ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) : expired ? "Not finalised" : "Not finished";

                if (status == "Completed")
                {
                    summary.Submitted++;
                    completedChallenges.Add(Convert.ToInt32(row["challengeID"]));
                }
                else if (status == "TimedOut" || expired) summary.Expired++;
                else if (status == "Paused" || status == "InProgress") summary.Unfinished++;

                if (expired)
                {
                    summary.PendingFinalisation++;
                    row["displayStatus"] = "Expired - not finalised";
                    row["resultText"] = "Awaiting finalisation";
                    row["actionText"] = "Finalise result";
                }
                else if (savedResult)
                {
                    row["displayStatus"] = status == "TimedOut" ? "Time expired" : "Submitted";
                    row["actionText"] = "View result";
                    if (total > 0)
                    {
                        int score = Convert.ToInt32(row["score"]);
                        decimal percentage = 100m * score / total;
                        row["resultText"] = score + " / " + total + " (" + percentage.ToString("0.#") + "%)";
                        if (!summary.Best.HasValue || percentage > summary.Best.Value) summary.Best = percentage;
                    }
                    else row["resultText"] = "Result unavailable";
                }
                else
                {
                    row["displayStatus"] = status == "InProgress" ? "In progress" : status;
                    row["resultText"] = "Not submitted";
                    row["actionText"] = status == "Paused" ? "Open paused attempt" : "Open attempt";
                }
            }
            summary.CompletedChallenges = completedChallenges.Count;
            return summary;
        }

        private void BindSimulations()
        {
            const string sql = @"SELECT p.simulationProgressID,p.simulationID,p.status,p.score,p.currentStepNumber, p.mistakes,p.startedAt,p.completedAt,s.title,s.isActive,r.status AS recipeStatus, (SELECT COUNT(*) FROM dbo.simulationSteps st WHERE st.simulationID=s.simulationID) AS stepCount FROM dbo.simulationProgress p
             INNER JOIN dbo.simulations s ON s.simulationID=p.simulationID INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID WHERE p.userID=@user ORDER BY p.startedAt DESC,p.simulationProgressID DESC;";
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
            history.Columns.Add("actionText", typeof(string));
            history.Columns.Add("availabilityText", typeof(string));
            history.Columns.Add("startedLocal", typeof(DateTime));
            history.Columns.Add("finishedText", typeof(string));
            int completed = 0, active = 0, restarted = 0;
            HashSet<int> completedSimulations = new HashSet<int>();
            foreach (DataRow row in history.Rows)
            {
                string status = Convert.ToString(row["status"]);
                int total = Convert.ToInt32(row["stepCount"]);
                int nextStep = row["currentStepNumber"] == DBNull.Value ? 1 : Convert.ToInt32(row["currentStepNumber"]);
                int done = status == "Completed" ? total : Math.Max(0, Math.Min(total, nextStep - 1));
                row["displayStatus"] = status == "InProgress" ? "In progress" : status == "Abandoned" ? "Restarted" : status;
                row["progressText"] = done + " / " + total;
                row["resultText"] = status == "Completed" ? Convert.ToInt32(row["score"]) + "%" : "Not completed";
                row["canOpen"] = Convert.ToBoolean(row["isActive"]) && Convert.ToString(row["recipeStatus"]) == "Approved" && (status == "Completed" || status == "InProgress");
                row["actionText"] = status == "Completed" ? "View run" : "Continue";
                row["availabilityText"] = status == "Abandoned" ? "Restarted run" : "Currently unavailable";
                row["startedLocal"] = LocalDate(row["startedAt"]);
                row["finishedText"] = status == "Completed" && row["completedAt"] != DBNull.Value
                    ? MalaysianTime(Convert.ToDateTime(row["completedAt"])).ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture)
                    : "Not completed";
                if (status == "Completed")
                {
                    completed++;
                    completedSimulations.Add(Convert.ToInt32(row["simulationID"]));
                }
                else if (status == "InProgress") active++;
                else if (status == "Abandoned") restarted++;
            }
            lblSimulationSummary.Text = "Completed runs: " + completed + ". Different simulations completed: " + completedSimulations.Count
                + ". In progress: " + active + ". Restarted runs: " + restarted + ".";
            BindHistory(gvSimulations, history);
        }

        internal static DateTime MalaysianTime(DateTime utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc),
                TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
        }

        private static object LocalDate(object value)
        {
            return value == DBNull.Value ? DBNull.Value : (object)MalaysianTime(Convert.ToDateTime(value));
        }

        protected string StatusClass(object value)
        {
            string status = Convert.ToString(value);
            string name = status == "Completed" ? "completed" : status == "TimedOut" ? "expired"
                : status == "AwaitingFinalisation" ? "awaiting" : status == "InProgress" ? "running"
                : status == "Paused" ? "paused" : "other";
            return "badge rounded-pill history-" + name;
        }

        private static void BindHistory(GridView grid, DataTable rows)
        {
            int lastPage = rows.Rows.Count == 0 ? 0 : (rows.Rows.Count - 1) / grid.PageSize;
            grid.PageIndex = Math.Max(0, Math.Min(grid.PageIndex, lastPage));
            grid.DataSource = rows;
            grid.DataBind();
            if (grid.HeaderRow != null) grid.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected void gvSimulations_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            gvSimulations.PageIndex = e.NewPageIndex;
            BindProgress();
            BindSimulations();
        }

        protected void gvAttempts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            gvAttempts.PageIndex = e.NewPageIndex;
            BindProgress();
            BindSimulations();
        }

        internal sealed class ChallengeSummary
        {
            public int Submitted;
            public int Expired;
            public int Unfinished;
            public int CompletedChallenges;
            public int PendingFinalisation;
            public decimal? Best;
        }
    }
}
