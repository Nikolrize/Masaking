using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class QuizQuestions : System.Web.UI.Page
    {
        private int QuizId
        {
            get
            {
                int id;
                return Int32.TryParse(Request.QueryString["id"], out id) && id > 0 ? id : 0;
            }
        }

        private int? EditingId
        {
            get { return ViewState["QuestionId"] as int?; }
            set { ViewState["QuestionId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            if (!IsPostBack)
            {
                BindQuestions();
            }
        }

        private void BindQuestions()
        {
            using (SqlConnection cn = Database.Open())
            {
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT title FROM dbo.challenges WHERE challengeID=@id", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = QuizId;
                    object title = cmd.ExecuteScalar();

                    if (title == null)
                    {
                        btnSave.Enabled = false;
                        ShowMessage("Quiz not found.", false);
                        return;
                    }

                    lblHeading.Text = Server.HtmlEncode(
                        "Questions: " + Convert.ToString(title));
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT questionID, questionNumber, questionText, correctOption
                    FROM dbo.challengeQuestions
                    WHERE challengeID=@id
                    ORDER BY questionNumber;", cn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = QuizId;
                    DataTable questions = new DataTable();
                    adapter.Fill(questions);
                    gvQuestions.DataSource = questions;
                    gvQuestions.DataBind();
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            int number;
            string question = txtQuestion.Text.Trim();
            string explanation = txtExplanation.Text.Trim();
            string[] options = {
                txtA.Text.Trim(), txtB.Text.Trim(),
                txtC.Text.Trim(), txtD.Text.Trim()
            };

            if (QuizId == 0 || !Int32.TryParse(txtNumber.Text, out number)
                || number < 1 || number > 100 || question.Length == 0 || question.Length > 1000 || explanation.Length > 1000
                || (ddlCorrect.SelectedValue != "A" && ddlCorrect.SelectedValue != "B" && ddlCorrect.SelectedValue != "C" && ddlCorrect.SelectedValue != "D"))
            {
                ShowMessage("Enter a question number from 1–100, question text " + "within 1,000 characters and a valid correct answer. " + "Keep explanations within 1,000 characters.", false);
                return;
            }

            foreach (string option in options)
            {
                if (option.Length == 0 || option.Length > 500)
                {
                    ShowMessage("Each option needs between 1 and 500 characters.", false);
                    return;
                }
            }

            try
            {
                using (SqlConnection cn = Database.Open())
                using (SqlTransaction tx =
                    cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    const string eligibility = @"
                        SELECT challengeID FROM dbo.challenges WITH (UPDLOCK, HOLDLOCK) WHERE challengeID=@id AND isActive=0 AND NOT EXISTS (SELECT 1 FROM dbo.challengeAttempts WHERE challengeID=@id);";

                    using (SqlCommand check =
                        new SqlCommand(eligibility, cn, tx))
                    {
                        check.Parameters.Add("@id", SqlDbType.Int).Value = QuizId;

                        if (check.ExecuteScalar() == null)
                        {
                            ShowMessage("Questions can only change on unpublished " + "quizzes without attempts.", false);
                            return;
                        }
                    }

                    string sql = EditingId.HasValue ? @"UPDATE dbo.challengeQuestions
                            SET questionNumber=@number, questionText=@question, optionA=@a, optionB=@b, optionC=@c, optionD=@d, correctOption=@correct, explanation=@explanation
                            WHERE questionID=@questionId AND challengeID=@id;"
                        : @"INSERT INTO dbo.challengeQuestions (challengeID, questionNumber, questionText, optionA, optionB, optionC, optionD, correctOption, explanation)
                            VALUES (@id, @number, @question, @a, @b, @c, @d, @correct, @explanation);";

                    using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = QuizId;
                        cmd.Parameters.Add("@number", SqlDbType.Int).Value = number;
                        cmd.Parameters.Add("@question", SqlDbType.NVarChar, 1000).Value = question;

                        string[] names = { "@a", "@b", "@c", "@d" };

                        for (int i = 0; i < options.Length; i++)
                        {
                            cmd.Parameters.Add(names[i], SqlDbType.NVarChar, 500).Value = options[i];
                        }

                        cmd.Parameters.Add("@correct", SqlDbType.Char, 1).Value = ddlCorrect.SelectedValue;

                        cmd.Parameters.Add("@explanation", SqlDbType.NVarChar, 1000).Value = explanation;

                        if (EditingId.HasValue)
                        {
                            cmd.Parameters.Add("@questionId", SqlDbType.Int).Value = EditingId.Value;
                        }

                        int changed = cmd.ExecuteNonQuery();
                        tx.Commit();

                        ShowMessage(changed == 1 ? "Question saved." : "The question no longer exists.", changed == 1);
                    }
                }

                ResetEditor();
                BindQuestions();
            }
            catch (SqlException ex)
                when (ex.Number == 2601 || ex.Number == 2627)
            {
                ShowMessage("That question number is already used in this quiz.", false);
            }
        }

        protected void gvQuestions_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
            if (e.CommandName != "EditQuestion" && e.CommandName != "RemoveQuestion") return;

            int id;
            if (!Int32.TryParse(
                Convert.ToString(e.CommandArgument), out id) || id <= 0)
            {
                ShowMessage("Invalid question selection.", false);
                return;
            }

            if (e.CommandName == "RemoveQuestion")
            {
                DeleteQuestion(id);
                return;
            }

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT questionNumber, questionText, optionA, optionB, optionC, optionD, correctOption, explanation
                FROM dbo.challengeQuestions
                WHERE questionID=@question AND challengeID=@quiz;", cn))
            {
                cmd.Parameters.Add("@question", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@quiz", SqlDbType.Int).Value = QuizId;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        ShowMessage("Question not found.", false);
                        return;
                    }

                    EditingId = id;
                    txtNumber.Text = Convert.ToString(reader["questionNumber"]);
                    txtQuestion.Text = Convert.ToString(reader["questionText"]);
                    txtA.Text = Convert.ToString(reader["optionA"]);
                    txtB.Text = Convert.ToString(reader["optionB"]);
                    txtC.Text = Convert.ToString(reader["optionC"]);
                    txtD.Text = Convert.ToString(reader["optionD"]);
                    ddlCorrect.SelectedValue =
                        Convert.ToString(reader["correctOption"]).Trim();
                    txtExplanation.Text =
                        Convert.ToString(reader["explanation"]);
                    btnSave.Text = "Update question";
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
            txtNumber.Text = "1";
            txtQuestion.Text = "";
            txtA.Text = "";
            txtB.Text = "";
            txtC.Text = "";
            txtD.Text = "";
            txtExplanation.Text = "";
            ddlCorrect.SelectedIndex = 0;
            btnSave.Text = "Save question";
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-warning d-block";
        }

        private void DeleteQuestion(int questionId)
        {
            const string sql = @"
                IF EXISTS (SELECT challengeID FROM dbo.challenges WITH (UPDLOCK, HOLDLOCK)
                    WHERE challengeID=@quiz AND isActive=0
                      AND NOT EXISTS (SELECT 1 FROM dbo.challengeAttempts WHERE challengeID=@quiz))
                BEGIN
                    DELETE FROM dbo.challengeQuestions WHERE questionID=@question AND challengeID=@quiz;

                    SELECT @@ROWCOUNT;
                END
                ELSE
                    SELECT -1;";

                    try
                    {
                        int result;

                        using (SqlConnection cn = Database.Open())
                        using (SqlTransaction tx =
                            cn.BeginTransaction(IsolationLevel.Serializable))
                        using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                        {
                            cmd.Parameters.Add("@quiz", SqlDbType.Int).Value = QuizId;

                            cmd.Parameters.Add("@question", SqlDbType.Int).Value = questionId;

                            result = Convert.ToInt32(cmd.ExecuteScalar());
                            tx.Commit();
                        }

                        if (result == 1)
                        {
                            ResetEditor();
                            ShowMessage("Question deleted. Check the remaining question numbers.", true);
                        }
                        else
                        {
                            ShowMessage(
                                result == -1 ? "Only unpublished quizzes without attempts can change." : "The question no longer exists.", false);
                        }

                        BindQuestions();
                    }
                    catch (SqlException ex) when (ex.Number == 547)
                    {
                        ShowMessage("This question has saved answers and cannot be deleted.", false);
                    }
                }
    }
}