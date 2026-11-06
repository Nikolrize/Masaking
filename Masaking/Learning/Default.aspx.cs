using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI.HtmlControls;
using Masaking.Infrastructure;

namespace Masaking.Learning
{
    public partial class LearningHome : ResponsivePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;

            if (!IsPostBack)
            {
                using (SqlConnection cn = Database.Open())
                {
                    BindSimulations(cn);
                    BindChallenge(cn);
                    BindProgress(cn);
                }
            }
        }

        private void BindSimulations(SqlConnection cn)
        {
            const string sql = @"
                SELECT TOP (2) s.simulationID,s.title,s.description,s.difficulty,
                    s.completedImage,r.image AS recipeImage,
                    (SELECT COUNT(*) FROM dbo.simulationSteps st
                     WHERE st.simulationID=s.simulationID) AS stepCount,
                    latest.status,latest.currentStepNumber
                FROM dbo.simulations s
                INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID
                OUTER APPLY
                (
                    SELECT TOP (1) p.status,p.currentStepNumber
                    FROM dbo.simulationProgress p
                    WHERE p.simulationID=s.simulationID AND p.userID=@user
                    ORDER BY p.simulationProgressID DESC
                ) latest
                WHERE s.isActive=1 AND r.status='Approved'
                    AND EXISTS (SELECT 1 FROM dbo.simulationSteps st
                                WHERE st.simulationID=s.simulationID)
                ORDER BY CASE WHEN latest.status='InProgress' THEN 0 ELSE 1 END,
                    s.createdAt DESC,s.simulationID DESC;";

            DataTable simulations = new DataTable();
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                adapter.Fill(simulations);
            }

            simulations.Columns.Add("imageUrl", typeof(string));
            simulations.Columns.Add("percentage", typeof(int));
            simulations.Columns.Add("progressText", typeof(string));
            simulations.Columns.Add("actionText", typeof(string));
            simulations.Columns.Add("instructionHeading", typeof(string));
            simulations.Columns.Add("instructionPreview", typeof(string));
            simulations.Columns.Add("ingredientPreview", typeof(string));

            foreach (DataRow simulation in simulations.Rows)
            {
                string status = Convert.ToString(simulation["status"]);
                int total = Convert.ToInt32(simulation["stepCount"]);
                int current = simulation["currentStepNumber"] == DBNull.Value
                    ? 1 : Convert.ToInt32(simulation["currentStepNumber"]);
                int done = status == "Completed" ? total
                    : status == "InProgress" ? Math.Max(0, Math.Min(total, current - 1)) : 0;

                simulation["percentage"] = total == 0 ? 0 : done * 100 / total;
                simulation["progressText"] = done + " of " + total + " steps completed";
                simulation["actionText"] = status == "InProgress" ? "Continue cooking"
                    : status == "Completed" ? "View result / practice again" : "Start simulation";
                simulation["instructionHeading"] = status == "InProgress" ? "Your next step" : "A taste of the activity";
                simulation["description"] = ShortText(simulation["description"], 160);

                string image = ImageUrl(Convert.ToString(simulation["completedImage"]));
                if (image.Length == 0) image = ImageUrl(Convert.ToString(simulation["recipeImage"]));
                simulation["imageUrl"] = image;

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT st.stepNumber,st.instruction,i.ingredientName
                    FROM dbo.simulationSteps st
                    INNER JOIN dbo.ingredients i ON i.ingredientID=st.ingredientID
                    WHERE st.simulationID=@id ORDER BY st.stepNumber;", cn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = simulation["simulationID"];
                    DataTable steps = new DataTable();
                    adapter.Fill(steps);
                    List<string> names = new List<string>();
                    string firstInstruction = "Follow the recipe's ingredient order.";
                    string currentInstruction = "";
                    foreach (DataRow step in steps.Rows)
                    {
                        if (names.Count < 4) names.Add(Convert.ToString(step["ingredientName"]));
                        int number = Convert.ToInt32(step["stepNumber"]);
                        if (number == 1) firstInstruction = Convert.ToString(step["instruction"]);
                        if (number == current) currentInstruction = Convert.ToString(step["instruction"]);
                    }
                    simulation["ingredientPreview"] = String.Join(", ", names)
                        + (steps.Rows.Count > 4 ? ", ..." : "");
                    simulation["instructionPreview"] = ShortText(
                        status == "InProgress" && currentInstruction.Length > 0
                            ? currentInstruction : firstInstruction, 180);
                }
            }

            rptSimulations.DataSource = simulations;
            rptSimulations.DataBind();
            pnlNoSimulations.Visible = simulations.Rows.Count == 0;
        }

        private void BindChallenge(SqlConnection cn)
        {
            const string sql = @"
                SELECT TOP (1) c.challengeID,c.title,c.description,c.difficulty,c.timeLimit,
                    (SELECT COUNT(*) FROM dbo.challengeQuestions q
                     WHERE q.challengeID=c.challengeID) AS questionCount
                FROM dbo.challenges c
                WHERE c.isActive=1 AND EXISTS (SELECT 1 FROM dbo.challengeQuestions q
                                               WHERE q.challengeID=c.challengeID)
                ORDER BY c.createdAt DESC,c.challengeID DESC;";
            DataTable challenges = new DataTable();
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(challenges);

            challenges.Columns.Add("timeText", typeof(string));
            foreach (DataRow challenge in challenges.Rows)
            {
                int seconds = Math.Max(0, Convert.ToInt32(challenge["timeLimit"]));
                challenge["timeText"] = (seconds / 60).ToString("00") + ":"
                    + (seconds % 60).ToString("00");
                challenge["description"] = ShortText(challenge["description"], 180);
            }
            rptChallenge.DataSource = challenges;
            rptChallenge.DataBind();
            pnlNoChallenges.Visible = challenges.Rows.Count == 0;
        }

        private void BindProgress(SqlConnection cn)
        {
            // Count distinct completed activities, so replaying one does not fill the bar twice.
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM dbo.simulations s
                     INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID
                     WHERE s.isActive=1 AND r.status='Approved'
                         AND EXISTS (SELECT 1 FROM dbo.simulationSteps st
                                     WHERE st.simulationID=s.simulationID)) AS simulationTotal,
                    (SELECT COUNT(*) FROM dbo.simulations s
                     INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID
                     WHERE s.isActive=1 AND r.status='Approved'
                         AND EXISTS (SELECT 1 FROM dbo.simulationSteps st
                                     WHERE st.simulationID=s.simulationID)
                         AND EXISTS (SELECT 1 FROM dbo.simulationProgress p
                                     WHERE p.simulationID=s.simulationID
                                         AND p.userID=@user AND p.status='Completed')) AS simulationCompleted,
                    (SELECT COUNT(*) FROM dbo.challenges c WHERE c.isActive=1
                         AND EXISTS (SELECT 1 FROM dbo.challengeQuestions q
                                     WHERE q.challengeID=c.challengeID)) AS quizTotal,
                    (SELECT COUNT(*) FROM dbo.challenges c WHERE c.isActive=1
                         AND EXISTS (SELECT 1 FROM dbo.challengeQuestions q
                                     WHERE q.challengeID=c.challengeID)
                         AND EXISTS (SELECT 1 FROM dbo.challengeAttempts a
                                     WHERE a.challengeID=c.challengeID
                                         AND a.userID=@user AND a.status='Completed')) AS quizCompleted,
                    (SELECT COUNT(*) FROM dbo.simulationProgress p
                     WHERE p.userID=@user AND p.status='InProgress') AS savedSimulations;";
            int unfinished;
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    int simulationTotal = Convert.ToInt32(reader["simulationTotal"]);
                    int simulationDone = Convert.ToInt32(reader["simulationCompleted"]);
                    int quizTotal = Convert.ToInt32(reader["quizTotal"]);
                    int quizDone = Convert.ToInt32(reader["quizCompleted"]);
                    lblSimulationProgress.Text = simulationDone + " / " + simulationTotal;
                    lblQuizProgress.Text = quizDone + " / " + quizTotal;
                    SetProgress(simulationProgressTrack, simulationProgressBar, simulationDone, simulationTotal);
                    SetProgress(quizProgressTrack, quizProgressBar, quizDone, quizTotal);
                    unfinished = Convert.ToInt32(reader["savedSimulations"]);
                }
            }

            using (SqlCommand cmd = new SqlCommand(@"
                SELECT status,remainingSeconds,lastResumedAt FROM dbo.challengeAttempts
                WHERE userID=@user AND status IN ('Paused','InProgress');", cn))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    DateTime now = DateTime.UtcNow;
                    while (reader.Read())
                    {
                        bool expired = Convert.ToString(reader["status"]) == "InProgress"
                            && reader["remainingSeconds"] != DBNull.Value
                            && reader["lastResumedAt"] != DBNull.Value
                            && (now - Convert.ToDateTime(reader["lastResumedAt"])).TotalSeconds
                                >= Convert.ToInt32(reader["remainingSeconds"]);
                        if (!expired) unfinished++;
                    }
                }
            }
            lblSavedAttempts.Text = unfinished.ToString();
        }

        private void SetProgress(HtmlGenericControl track, HtmlGenericControl bar, int done, int total)
        {
            int percentage = total == 0 ? 0 : Math.Max(0, Math.Min(100, done * 100 / total));
            track.Attributes["aria-valuenow"] = percentage.ToString();
            bar.Style["width"] = percentage + "%";
        }

        private string ShortText(object value, int length)
        {
            string text = Convert.ToString(value).Trim();
            return text.Length <= length ? text : text.Substring(0, length).TrimEnd() + "...";
        }

        private string ImageUrl(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return "";
            try
            {
                string path = Uri.UnescapeDataString(value.Trim());
                if (path.StartsWith("/Content/images/", StringComparison.OrdinalIgnoreCase)) path = "~" + path;
                else if (path.StartsWith("Content/images/", StringComparison.OrdinalIgnoreCase)) path = "~/" + path;
                if (!path.StartsWith("~/Content/images/", StringComparison.OrdinalIgnoreCase)
                    || path.Contains("..") || path.Contains("\\")) return "";
                string extension = Path.GetExtension(path).ToLowerInvariant();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png"
                    && extension != ".gif" && extension != ".webp" && extension != ".svg") return "";
                return File.Exists(Server.MapPath(path)) ? ResolveUrl(path) : "";
            }
            catch (ArgumentException) { return ""; }
            catch (HttpException) { return ""; }
            catch (UriFormatException) { return ""; }
        }
    }
}
