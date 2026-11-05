using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class ManageChallenges : System.Web.UI.Page
    {
        private int? EditingId
        {
            get { return ViewState["EditingId"] as int?; }
            set { ViewState["EditingId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            if (!IsPostBack)
            {
                BindQuizzes();
            }
        }

        private void BindQuizzes()
        {
            const string sql = @"
                SELECT c.challengeID, c.title, c.difficulty, c.timeLimit, c.isActive,
                (SELECT COUNT(*) FROM dbo.challengeQuestions q WHERE q.challengeID = c.challengeID) AS questionCount,
                (SELECT COUNT(*) FROM dbo.challengeAttempts a WHERE a.challengeID = c.challengeID) AS attemptCount
                FROM dbo.challenges c
                ORDER BY c.createdAt DESC, c.challengeID DESC;";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataTable quizzes = new DataTable();
                adapter.Fill(quizzes);
                gvQuizzes.DataSource = quizzes;
                gvQuizzes.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            string title = txtTitle.Text.Trim();
            string description = txtDescription.Text.Trim();
            string difficulty = ddlDifficulty.SelectedValue;
            int minutes;

            if (title.Length == 0 || title.Length > 150
                || description.Length > 1000
                || !Int32.TryParse(txtMinutes.Text, out minutes)
                || minutes < 1 || minutes > 60
                || (difficulty != "Easy"
                    && difficulty != "Medium"
                    && difficulty != "Hard"))
            {
                ShowMessage( "Enter a title, a description of up to 1,000 characters, " + "and a time limit between 1 and 60 whole minutes.", false);
                return;
            }

            bool editing = EditingId.HasValue;

            string sql = editing ? @" UPDATE dbo.challenges SET title = @title, description = @description, difficulty = @difficulty, timeLimit = @seconds
              WHERE challengeID = @id AND isActive = 0 AND NOT EXISTS (SELECT 1 FROM dbo.challengeAttempts WHERE challengeID = @id);"
                : @" INSERT INTO dbo.challenges (title, description, difficulty, timeLimit,isActive, createdAt, userID)
                VALUES (@title, @description, @difficulty, @seconds, 0, @createdAt, @userId);";

            int changed;

            using (SqlConnection cn = Database.Open())
            using (SqlTransaction tx =
                cn.BeginTransaction(IsolationLevel.Serializable))
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@title", SqlDbType.NVarChar, 150).Value = title;

                cmd.Parameters.Add( "@description", SqlDbType.NVarChar, 1000).Value = description;

                cmd.Parameters.Add("@difficulty", SqlDbType.NVarChar, 20).Value = difficulty;

                cmd.Parameters.Add("@seconds", SqlDbType.Int).Value = minutes * 60;

                if (editing)
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = EditingId.Value;
                }
                else
                {
                    cmd.Parameters.Add("@createdAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;

                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = Database.UserId.Value;
                }

                changed = cmd.ExecuteNonQuery();
                tx.Commit();
            }

            if (changed == 1)
            {
                ResetEditor();
                ShowMessage("Quiz draft saved.", true);
            }
            else
            {
                ResetEditor();
                ShowMessage("The quiz no longer exists, is published, " + "or has attempts. Its settings were not changed.", false);
            }

            gvQuizzes.PageIndex = 0;
            BindQuizzes();
        }

        protected void gvQuizzes_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
            if (e.CommandName != "EditQuiz") return;

            int id;

            if (!Int32.TryParse(
                Convert.ToString(e.CommandArgument), out id) || id <= 0)
            {
                ShowMessage("Invalid quiz selection.", false);
                return;
            }

            ResetEditor();

            const string sql = @"
                SELECT title, description, difficulty, timeLimit
                FROM dbo.challenges
                WHERE challengeID = @id AND isActive = 0 AND NOT EXISTS (SELECT 1 FROM dbo.challengeAttempts WHERE challengeID = @id);";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        ShowMessage("Only unpublished quizzes without attempts " + "can be edited.", false);
                        return;
                    }

                    int seconds = Convert.ToInt32(reader["timeLimit"]);

                    if (seconds < 60 || seconds > 3600 || seconds % 60 != 0)
                    {
                        ShowMessage("This quiz has a time limit outside this editor's " + "whole-minute format. Check its stored units first.", false);
                        return;
                    }

                    string difficulty =
                        Convert.ToString(reader["difficulty"]);

                    if (ddlDifficulty.Items.FindByValue(difficulty) == null)
                    {
                        ShowMessage("This quiz uses an unsupported difficulty value.", false);
                        return;
                    }

                    txtTitle.Text = Convert.ToString(reader["title"]);
                    txtDescription.Text =
                        Convert.ToString(reader["description"]);

                    ddlDifficulty.SelectedValue = difficulty;
                    txtMinutes.Text = (seconds / 60).ToString();
                    EditingId = id;
                    lblEditorHeading.Text = "Edit quiz #" + id;
                }
            }
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
            ResetEditor();
        }

        private void ResetEditor()
        {
            EditingId = null;
            txtTitle.Text = "";
            txtDescription.Text = "";
            ddlDifficulty.SelectedIndex = 0;
            txtMinutes.Text = "5";
            lblEditorHeading.Text = "New quiz";
        }

        protected void gvQuizzes_PageIndexChanging(
            object sender, GridViewPageEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
            gvQuizzes.PageIndex = e.NewPageIndex;
            BindQuizzes();
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-warning d-block";
        }
    }
}