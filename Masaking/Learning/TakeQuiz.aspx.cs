using System;
using System.Data;
using System.Data.SqlClient;
using Masaking.Infrastructure;

namespace Masaking.Learning
{
    public partial class TakeQuiz : System.Web.UI.Page
    {
        private int QuizId
        {
            get
            {
                int id;
                return Int32.TryParse(Request.QueryString["id"], out id) && id > 0 ? id : 0;
            }
        }

        private int AttemptId
        {
            get
            {
                int id;
                return Int32.TryParse(Request.QueryString["attempt"], out id) && id > 0 ? id : 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;

            if (!IsPostBack)
            {
                if (AttemptId > 0)
                {
                    LoadAttempt();
                }
                else
                {
                    LoadQuiz();
                }
            }
        }

        private void LoadQuiz()
        {
            const string sql = @"
                SELECT c.title, c.description, c.timeLimit,
                       (SELECT COUNT(*) FROM dbo.challengeQuestions q WHERE q.challengeID=c.challengeID) AS questionCount
                FROM dbo.challenges c
                WHERE c.challengeID=@id AND c.isActive=1 AND EXISTS ( SELECT 1 FROM dbo.challengeQuestions q WHERE q.challengeID=c.challengeID);";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = QuizId;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        ShowError("This quiz is unavailable.");
                        return;
                    }

                    lblTitle.Text = Server.HtmlEncode(Convert.ToString(reader["title"]));

                    lblDescription.Text = Server.HtmlEncode(Convert.ToString(reader["description"]));

                    lblTimeLimit.Text = "Time limit: " + Convert.ToInt32(reader["timeLimit"]) + " seconds";

                    lblQuestionCount.Text = "Questions: " + Convert.ToInt32(reader["questionCount"]);

                    pnlStart.Visible = true;
                }
            }
        }

        protected void btnStart_Click(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;

            int attemptId = 0;

            using (SqlConnection cn = Database.Open())
            using (SqlTransaction tx =
                cn.BeginTransaction(IsolationLevel.Serializable))
            {
                //lock the parent quiz to prevent concurrent attempts from creating duplicates
                using (SqlCommand quiz = new SqlCommand(@"
                    SELECT challengeID
                    FROM dbo.challenges WITH (UPDLOCK, HOLDLOCK)
                    WHERE challengeID=@id;", cn, tx))
                {
                    quiz.Parameters.Add("@id", SqlDbType.Int).Value = QuizId;

                    if (quiz.ExecuteScalar() == null)
                    {
                        ShowError("Quiz not found.");
                        return;
                    }
                }

                //reopen an existing attempt rather than duplicating it 
                using (SqlCommand existing = new SqlCommand(@"
                    SELECT TOP (1) attemptID
                    FROM dbo.challengeAttempts
                    WHERE challengeID=@quiz AND userID=@user AND status IN ('InProgress', 'Paused')
                    ORDER BY startedAt DESC, attemptID DESC;", cn, tx))
                {
                    existing.Parameters.Add("@quiz", SqlDbType.Int).Value = QuizId;

                    existing.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;

                    object saved = existing.ExecuteScalar();

                    if (saved != null)
                    {
                        attemptId = Convert.ToInt32(saved);
                    }
                }

                if (attemptId == 0)
                {
                    //only published quizzes that are populated accept new attempts
                    const string createSql = @"
                        INSERT INTO dbo.challengeAttempts
                        (score, timeTaken, status, startedAt, completedAt, userID, challengeID, remainingSeconds, lastResumedAt, currentQuestionNumber)
                        SELECT 0, 0, 'Paused', @now, NULL, @user, c.challengeID, c.timeLimit, NULL, 1
                        FROM dbo.challenges c
                        WHERE c.challengeID=@quiz AND c.isActive=1 AND c.timeLimit BETWEEN 60 AND 3600
                          AND EXISTS (SELECT 1 FROM dbo.challengeQuestions q WHERE q.challengeID=c.challengeID);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    using (SqlCommand create =
                        new SqlCommand(createSql, cn, tx))
                    {
                        create.Parameters.Add("@now", SqlDbType.DateTime2).Value = DateTime.UtcNow;

                        create.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;

                        create.Parameters.Add("@quiz", SqlDbType.Int).Value = QuizId;

                        object created = create.ExecuteScalar();

                        if (created == null || created == DBNull.Value)
                        {
                            ShowError("This quiz no longer accepts new attempts.");
                            return;
                        }

                        attemptId = Convert.ToInt32(created);
                    }
                }

                tx.Commit();
            }

            Response.Redirect("TakeQuiz.aspx?attempt=" + attemptId, false);

            Context.ApplicationInstance.CompleteRequest();
        }

        private void LoadAttempt()
        {
            ProcessAttempt("Refresh");
        }

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "alert alert-warning d-block";
        }

        //(shared) attempt-reading
        public int CountdownSeconds { get; private set; }
        public bool TimerRunning { get; private set; }

        private DataRow ReadAttempt(SqlConnection cn, SqlTransaction tx)
        {
            string hint = tx == null ? "" : " WITH (UPDLOCK, HOLDLOCK)";

            string sql = @"SELECT a.*, c.title,
             (SELECT COUNT(*) FROM dbo.challengeQuestions q WHERE q.challengeID=a.challengeID) AS questionCount FROM dbo.challengeAttempts a" + hint + @" INNER JOIN dbo.challenges c ON c.challengeID=a.challengeID WHERE a.attemptID=@attempt AND a.userID=@user;";

            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@attempt", SqlDbType.Int).Value = AttemptId;

                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;

                DataTable table = new DataTable();
                adapter.Fill(table);

                return table.Rows.Count == 0 ? null : table.Rows[0];
            }
        }
        //processing answers, timing, status
        private void ProcessAttempt(string action)
        {
            if (!Database.RequireUser(Response)) return;

            using (SqlConnection cn = Database.Open())
            using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                DataRow attempt = ReadAttempt(cn, tx);

                if (attempt == null)
                {
                    ShowError("Attempt not found or unavailable to this account.");
                    pnlAttempt.Visible = false;
                    return;
                }

                string status = Convert.ToString(attempt["status"]);

                if (status == "Completed" || status == "TimedOut")
                {
                    tx.Commit();
                }
                else
                {
                    if (attempt["remainingSeconds"] == DBNull.Value || attempt["currentQuestionNumber"] == DBNull.Value)
                    {
                        ShowError("This older attempt has no saved timer information.");
                        return;
                    }

                    int remaining = Convert.ToInt32(attempt["remainingSeconds"]);
                    int timeTaken = Convert.ToInt32(attempt["timeTaken"]);
                    int number = Convert.ToInt32(attempt["currentQuestionNumber"]);
                    int total = Convert.ToInt32(attempt["questionCount"]);
                    int quizId = Convert.ToInt32(attempt["challengeID"]);
                    DateTime now = DateTime.UtcNow;
                    DateTime? anchor = null;

                    if (total == 0 || number < 1 || number > total || remaining < 0 || (status != "Paused" && status != "InProgress"))
                    {
                        ShowError("This attempt has invalid saved data.");
                        return;
                    }

                    if (status == "InProgress")
                    {
                        if (attempt["lastResumedAt"] == DBNull.Value)
                        {
                            ShowError("This attempt has no running timer timestamp.");
                            return;
                        }

                        DateTime started =
                            Convert.ToDateTime(attempt["lastResumedAt"]);

                        double elapsed =
                            Math.Max(0, (now - started).TotalSeconds);

                        //round up for pause, preserve fractions while running
                        double charge = action == "Pause" || action == "Submit" ? Math.Ceiling(elapsed) : Math.Floor(elapsed);

                        int used = (int)Math.Min(remaining, charge);

                        remaining -= used;
                        timeTaken += used;
                        anchor = started.AddSeconds(used);

                        if (remaining == 0)
                        {
                            status = "TimedOut";
                            anchor = null;
                        }
                    }

                    if (status == "Paused")
                    {
                        if (remaining == 0)
                        {
                            status = "TimedOut";
                        }
                        else if (action == "Resume")
                        {
                            status = "InProgress";
                            anchor = now;
                        }
                    }
                    else if (status == "InProgress"
                        && action != "Refresh"
                        && action != "Resume"
                        && action != "Timeout")
                    {
                        int questionId;

                        using (SqlCommand cmd = new SqlCommand(@" SELECT questionID FROM dbo.challengeQuestions WHERE challengeID=@quiz AND questionNumber=@number;",
                            cn, tx))
                        {
                            cmd.Parameters.Add("@quiz", SqlDbType.Int).Value = quizId;
                            cmd.Parameters.Add("@number", SqlDbType.Int).Value = number;

                            object value = cmd.ExecuteScalar();

                            if (value == null)
                            {
                                ShowError("The current question could not be found.");
                                return;
                            }

                            questionId = Convert.ToInt32(value);
                        }

                        int? displayedQuestion =
                            ViewState["DisplayedQuestionId"] as int?;

                        if (displayedQuestion != questionId)
                        {
                            ShowError("This page was out of date. The current question " + "has been reloaded; your action was not applied.");
                        }
                        else
                        {
                            string selected = rblAnswers.SelectedValue;

                            if (selected == "A" || selected == "B"
                                || selected == "C" || selected == "D")
                            {
                                using (SqlCommand cmd = new SqlCommand(@"IF EXISTS (SELECT 1 FROM dbo.challengeAnswers WHERE attemptID=@attempt AND questionID=@question)
                                UPDATE dbo.challengeAnswers
                                SET selectedOption=@selected, answeredAt=@now
                                WHERE attemptID=@attempt AND questionID=@question;
                                ELSE
                                INSERT INTO dbo.challengeAnswers (attemptID, questionID, selectedOption, answeredAt)
                                VALUES (@attempt, @question, @selected, @now);",
                                    cn, tx))
                                {
                                    cmd.Parameters.Add("@attempt", SqlDbType.Int).Value = AttemptId;
                                    cmd.Parameters.Add("@question", SqlDbType.Int).Value = questionId;
                                    cmd.Parameters.Add("@selected", SqlDbType.Char, 1).Value = selected;
                                    cmd.Parameters.Add("@now", SqlDbType.DateTime2).Value = now;

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            if (action == "Next" && number < total) number++;
                            if (action == "Previous" && number > 1) number--;

                            if (action == "Pause")
                            {
                                status = "Paused";
                                anchor = null;
                            }

                            if (action == "Submit")
                            {
                                status = "Completed";
                                anchor = null;
                            }
                        }
                    }

                    bool finished = status == "Completed" || status == "TimedOut";

                    using (SqlCommand cmd = new SqlCommand(@" UPDATE dbo.challengeAttempts SET status=@status, remainingSeconds=@remaining, timeTaken=@taken, lastResumedAt=@anchor, currentQuestionNumber=@number, completedAt=CASE WHEN @finished=1 THEN @now
                        ELSE completedAt END, score=CASE WHEN @finished=1 THEN (
                        SELECT COUNT(*) FROM dbo.challengeAnswers a
                        INNER JOIN dbo.challengeQuestions q ON q.questionID=a.questionID
                        WHERE a.attemptID=@attempt AND q.challengeID=@quiz AND a.selectedOption=q.correctOption
                        ) ELSE score END WHERE attemptID=@attempt AND userID=@user;",
                        cn, tx))
                    {
                        cmd.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = status;
                        cmd.Parameters.Add("@remaining", SqlDbType.Int).Value = remaining;
                        cmd.Parameters.Add("@taken", SqlDbType.Int).Value = timeTaken;
                        cmd.Parameters.Add("@number", SqlDbType.Int).Value = number;
                        cmd.Parameters.Add("@finished", SqlDbType.Bit).Value = finished;
                        cmd.Parameters.Add("@now", SqlDbType.DateTime2).Value = now;
                        cmd.Parameters.Add("@attempt", SqlDbType.Int).Value = AttemptId;
                        cmd.Parameters.Add("@quiz", SqlDbType.Int).Value = quizId;
                        cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;

                        cmd.Parameters.Add("@anchor", SqlDbType.DateTime2).Value =
                            anchor.HasValue ? (object)anchor.Value : DBNull.Value;

                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }

            RenderAttempt();
        }
        //display saved state
        private void RenderAttempt()
        {
            TimerRunning = false;
            CountdownSeconds = 0;

            pnlStart.Visible = false;
            pnlAttempt.Visible = false;
            pnlPaused.Visible = false;
            pnlQuestion.Visible = false;
            pnlResult.Visible = false;

            using (SqlConnection cn = Database.Open())
            {
                DataRow attempt = ReadAttempt(cn, null);

                if (attempt == null)
                {
                    ShowError("Attempt not found or unavailable to this account.");
                    return;
                }

                string status = Convert.ToString(attempt["status"]);
                int total = Convert.ToInt32(attempt["questionCount"]);

                lblTitle.Text = Server.HtmlEncode(Convert.ToString(attempt["title"]));
                lblAttempt.Text = "Attempt ID: " + AttemptId;
                lblStatus.Text = "Status: " + Server.HtmlEncode(status);
                pnlAttempt.Visible = true;

                if (status == "Completed" || status == "TimedOut")
                {
                    int score = Convert.ToInt32(attempt["score"]);
                    decimal percentage = total == 0 ? 0 : 100m * score / total;

                    lblResult.Text = score + " / " + total + " (" + percentage.ToString("0.#") + "%)";

                    lblRemaining.Text = "Time used: " + Convert.ToInt32(attempt["timeTaken"]) + " seconds";

                    pnlResult.Visible = true;
                    return;
                }

                int remaining = Convert.ToInt32(attempt["remainingSeconds"]);
                lblRemaining.Text = "Time remaining: " + remaining + " seconds";

                if (status == "Paused")
                {
                    pnlPaused.Visible = true;
                    return;
                }

                int number = Convert.ToInt32(attempt["currentQuestionNumber"]);

                using (SqlCommand cmd = new SqlCommand(@" SELECT q.questionID, q.questionText, q.optionA, q.optionB, q.optionC, q.optionD, a.selectedOption FROM dbo.challengeQuestions q LEFT JOIN dbo.challengeAnswers a ON a.questionID=q.questionID AND a.attemptID=@attempt WHERE q.challengeID=@quiz AND q.questionNumber=@number;", cn))
                {
                    cmd.Parameters.Add("@attempt", SqlDbType.Int).Value = AttemptId;
                    cmd.Parameters.Add("@quiz", SqlDbType.Int).Value = attempt["challengeID"];
                    cmd.Parameters.Add("@number", SqlDbType.Int).Value = number;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            ShowError("Question not found.");
                            return;
                        }

                        ViewState["DisplayedQuestionId"] = Convert.ToInt32(reader["questionID"]);

                        lblQuestionNumber.Text = "Question " + number + " of " + total;
                        lblQuestion.Text = Server.HtmlEncode(Convert.ToString(reader["questionText"]));

                        string[] columns = { "optionA", "optionB", "optionC", "optionD" };

                        rblAnswers.ClearSelection();

                        for (int i = 0; i < columns.Length; i++)
                        {
                            rblAnswers.Items[i].Text = Server.HtmlEncode(Convert.ToString(reader[columns[i]]));
                        }

                        string selected = Convert.ToString(reader["selectedOption"]).Trim();

                        if (rblAnswers.Items.FindByValue(selected) != null)
                        {
                            rblAnswers.SelectedValue = selected;
                        }
                    }
                }

                btnPrevious.Enabled = number > 1;
                btnNext.Enabled = number < total;
                pnlQuestion.Visible = true;
                CountdownSeconds = remaining;
                TimerRunning = true;
            }
        }

        //event handlers for buttons
        protected void btnResume_Click(object sender, EventArgs e)
        {
            ProcessAttempt("Resume");
        }

        protected void rblAnswers_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProcessAttempt("Save");
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            ProcessAttempt("Previous");
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            ProcessAttempt("Next");
        }

        protected void btnPause_Click(object sender, EventArgs e)
        {
            ProcessAttempt("Pause");
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            ProcessAttempt("Submit");
        }

        protected void btnTimeout_Click(object sender, EventArgs e)
        {
            ProcessAttempt("Timeout");
        }
    }
}