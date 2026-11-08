using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class ManageChallenges : ActivityEditorPage
    {
        private int? EditingId
        {
            get { return ViewState["EditingId"] as int?; }
            set { ViewState["EditingId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;

            if (!IsPostBack)
            {
                BindRecipes();
                BindQuizzes();
                SelectRequestedRecipe();
            }
        }

        private void BindRecipes()
        {
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT recipeID,title FROM dbo.recipes WHERE status='Approved' AND (@editorAdmin=1 OR userID=@editorUser) ORDER BY title,recipeID;", cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                AddOwnerParameters(cmd);
                DataTable recipes = new DataTable();
                adapter.Fill(recipes);
                ddlRecipe.DataSource = recipes;
                ddlRecipe.DataTextField = "title";
                ddlRecipe.DataValueField = "recipeID";
                ddlRecipe.DataBind();
                ddlRecipe.Items.Insert(0, new ListItem("-- Select an approved recipe --", ""));
            }
        }

        private void SelectRequestedRecipe()
        {
            string requested = Request.QueryString["recipeId"];
            if (ddlRecipe.Items.FindByValue(requested ?? "") != null) ddlRecipe.SelectedValue = requested;
        }

        private void BindQuizzes()
        {
            const string sql = @"
                SELECT c.challengeID, c.recipeID, r.title AS recipeTitle, CASE WHEN r.recipeID IS NULL THEN 'No linked recipe' ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.displayName)), ''), u.username, 'Unknown user') END AS recipeOwner, c.title, c.difficulty, c.timeLimit, c.isActive, c.createdAt,
                (SELECT COUNT(*) FROM dbo.challengeQuestions q WHERE q.challengeID = c.challengeID) AS questionCount,
                (SELECT COUNT(*) FROM dbo.challengeAttempts a WHERE a.challengeID = c.challengeID) AS attemptCount
                FROM dbo.challenges c LEFT JOIN dbo.recipes r ON r.recipeID=c.recipeID
                LEFT JOIN dbo.users u ON u.userID=r.userID
                WHERE (@editorAdmin=1 OR r.userID=@editorUser) AND (@search='' OR CHARINDEX(@search,c.title)>0) AND (@publication='' OR c.isActive=CONVERT(bit,@publication))
                ORDER BY c.createdAt DESC, c.challengeID DESC;";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                AddOwnerParameters(cmd);
                cmd.Parameters.Add("@search", SqlDbType.NVarChar, 150).Value = txtSearch.Text.Trim();
                cmd.Parameters.Add("@publication", SqlDbType.NVarChar, 1).Value = ddlPublication.SelectedValue;
                DataTable challenges = new DataTable();
                adapter.Fill(challenges);
                challenges.Columns.Add("minutes", typeof(decimal));
                challenges.Columns.Add("difficultyOrder", typeof(int));
                foreach (DataRow row in challenges.Rows)
                {
                    row["minutes"] = Convert.ToInt32(row["timeLimit"]) / 60m;
                    row["difficultyOrder"] = AdminTable.DifficultyOrder(row["difficulty"]);
                }
                AdminTable.Bind(gvQuizzes, challenges, ViewState, "Challenges", "createdAt", "DESC");
                lblCount.Text = challenges.Rows.Count + " challenges match the filters.";
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            gvQuizzes.PageIndex = 0;
            BindQuizzes();
        }

        protected void Filters_Changed(object sender, EventArgs e)
        {
            btnSearch_Click(sender, e);
        }

        protected void gvQuizzes_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (!RequireEditor()) { e.Cancel = true; return; }
            AdminTable.ToggleSort(e, ViewState, "Challenges", "createdAt", "DESC", "challengeID", "title", "recipeOwner", "difficultyOrder", "timeLimit", "isActive", "questionCount", "attemptCount");
            gvQuizzes.PageIndex = 0;
            BindQuizzes();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;

            int recipeId;
            if (!Int32.TryParse(ddlRecipe.SelectedValue, out recipeId))
            {
                ShowMessage("Select an approved recipe before saving.", false);
                return;
            }
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

            string sql = editing ? @" UPDATE dbo.challenges SET title = @title, description = @description, difficulty = @difficulty, timeLimit = @seconds, recipeID=@recipe
              WHERE challengeID = @id AND (recipeID=@recipe OR recipeID IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.challengeQuestions WHERE challengeID=@id)) AND isActive = 0 AND NOT EXISTS (SELECT 1 FROM dbo.challengeAttempts WHERE challengeID = @id);"
                : @" INSERT INTO dbo.challenges (title, description, difficulty, timeLimit,isActive, createdAt, userID, recipeID)
                VALUES (@title, @description, @difficulty, @seconds, 0, @createdAt, @userId, @recipe);";

            int changed;

            using (SqlConnection cn = Database.Open())
            using (SqlTransaction tx =
                cn.BeginTransaction(IsolationLevel.Serializable))
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                if (EditingId.HasValue) CheckActivity("challenge", EditingId.Value, cn, tx);
                CheckRecipe(recipeId, cn, tx);
                cmd.Parameters.Add("@recipe", SqlDbType.Int).Value = recipeId;
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
                ShowMessage("Challenge draft saved.", true);
            }
            else
            {
                ResetEditor();
                ShowMessage("The challenge no longer exists, is published, " + "or has attempts. Its settings were not changed.", false);
            }

            gvQuizzes.PageIndex = 0;
            BindQuizzes();
        }

        protected void gvQuizzes_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!RequireEditor()) return;
            if (e.CommandName != "EditQuiz" && e.CommandName != "PublishQuiz" && e.CommandName != "UnpublishQuiz") { return;}

            int id;

            if (!Int32.TryParse(
                Convert.ToString(e.CommandArgument), out id) || id <= 0)
            {
                ShowMessage("Invalid challenge selection.", false);
                return;
            }

            CheckActivity("challenge", id);
            if (e.CommandName == "PublishQuiz" || e.CommandName == "UnpublishQuiz")
            {
                ChangePublication(id, e.CommandName == "PublishQuiz");
                return;
            }

            ResetEditor();

            const string sql = @"
                SELECT title, description, difficulty, timeLimit, recipeID
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
                        ShowMessage("Only unpublished challenges without attempts " + "can be edited.", false);
                        return;
                    }

                    int seconds = Convert.ToInt32(reader["timeLimit"]);

                    if (seconds < 60 || seconds > 3600 || seconds % 60 != 0)
                    {
                        ShowMessage("This challenge has a time limit outside this editor's " + "whole-minute format. Check its stored units first.", false);
                        return;
                    }

                    string difficulty =
                        Convert.ToString(reader["difficulty"]);

                    if (ddlDifficulty.Items.FindByValue(difficulty) == null)
                    {
                        ShowMessage("This challenge uses an unsupported difficulty value.", false);
                        return;
                    }

                    string recipe = Convert.ToString(reader["recipeID"]);
                    if (ddlRecipe.Items.FindByValue(recipe) != null) ddlRecipe.SelectedValue = recipe;
                    txtTitle.Text = Convert.ToString(reader["title"]);
                    txtDescription.Text =
                        Convert.ToString(reader["description"]);

                    ddlDifficulty.SelectedValue = difficulty;
                    txtMinutes.Text = (seconds / 60).ToString();
                    EditingId = id;
                    lblEditorHeading.Text = "Edit challenge #" + id;
                    btnSave.Text = "Save changes";
                    btnNew.Text = "New challenge";
                }
            }
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            ResetEditor();
        }

        private void ResetEditor()
        {
            EditingId = null;
            ddlRecipe.SelectedIndex = 0;
            txtTitle.Text = "";
            txtDescription.Text = "";
            ddlDifficulty.SelectedIndex = 0;
            txtMinutes.Text = "5";
            lblEditorHeading.Text = "New challenge";
            btnSave.Text = "Save draft";
            btnNew.Text = "Reset form";
        }

        protected void gvQuizzes_PageIndexChanging(
            object sender, GridViewPageEventArgs e)
        {
            if (!RequireEditor()) return;
            gvQuizzes.PageIndex = e.NewPageIndex;
            BindQuizzes();
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-warning d-block";
        }

        private void ChangePublication(int quizId, bool publish)
        {
            const string sql = @"DECLARE @found INT;
                SELECT @found=challengeID FROM dbo.challenges WITH (UPDLOCK, HOLDLOCK) WHERE challengeID=@id;
                IF @found IS NULL SELECT 0; ELSE IF @publish=1 AND (EXISTS (SELECT 1 FROM dbo.challenges c INNER JOIN dbo.recipes r ON r.recipeID=c.recipeID WHERE c.challengeID=@id AND r.status<>'Approved') OR EXISTS (SELECT 1 FROM dbo.challenges WHERE challengeID=@id AND (LEN(LTRIM(RTRIM(title)))=0 OR difficulty NOT IN ('Easy','Medium','Hard') OR timeLimit < 60 OR timeLimit > 3600 OR timeLimit % 60 <> 0) )
                    OR NOT EXISTS ( SELECT 1 FROM dbo.challengeQuestions WHERE challengeID=@id)
                    OR EXISTS (SELECT 1 FROM dbo.challengeQuestions WHERE challengeID=@id AND (LEN(LTRIM(RTRIM(questionText)))=0 OR LEN(LTRIM(RTRIM(optionA)))=0 OR LEN(LTRIM(RTRIM(optionB)))=0 OR LEN(LTRIM(RTRIM(optionC)))=0 OR LEN(LTRIM(RTRIM(optionD)))=0 OR correctOption NOT IN ('A','B','C','D') OR questionNumber < 1 OR questionNumber > 100))
                    OR (SELECT MIN(questionNumber) FROM dbo.challengeQuestions WHERE challengeID=@id
                    ) <> 1
                    OR (SELECT MAX(questionNumber) FROM dbo.challengeQuestions WHERE challengeID=@id
                    ) <> (SELECT COUNT(*) FROM dbo.challengeQuestions WHERE challengeID=@id))
                    SELECT -1;
                ELSE
                BEGIN
                    UPDATE dbo.challenges
                    SET isActive=@publish
                    WHERE challengeID=@id;
                    SELECT 1;
                END;";

                    int result;

                    using (SqlConnection cn = Database.Open())
                    using (SqlTransaction tx =
                        cn.BeginTransaction(IsolationLevel.Serializable))
                    using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                    {
                        CheckActivity("challenge", quizId, cn, tx);
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = quizId;

                        cmd.Parameters.Add("@publish", SqlDbType.Bit).Value = publish;

                        result = Convert.ToInt32(cmd.ExecuteScalar());
                        tx.Commit();
                    }

                    if (result == 1)
                    {
                        ResetEditor();
                        ShowMessage(publish ? "Challenge published." : "Challenge unpublished.", true);
                    }
                    else if (result == -1)
                    {
                        ShowMessage("Publishing requires a valid title, difficulty and whole-minute " + "time limit of 1-60 minutes, plus complete questions numbered " + "consecutively from 1. Check the challenge settings and questions.", false);
                    }
                    else
                    {
                        ShowMessage("The challenge no longer exists.", false);
                    }

                    BindQuizzes();
                }
    }
}
