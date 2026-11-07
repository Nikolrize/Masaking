using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Learning
{
    public partial class Challenges : Masaking.Infrastructure.ResponsivePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)){return;}

            if (!IsPostBack)
            {
                BindQuizzes();
            }
        }

        private void BindQuizzes()
        {
            const string sql = @"
                SELECT c.challengeID, c.title, c.description, c.difficulty,
                    CAST(c.timeLimit / 60.0 AS DECIMAL(6,1)) AS minutes,
                    (SELECT COUNT(*) FROM dbo.challengeQuestions q WHERE q.challengeID = c.challengeID) AS questionCount
                FROM dbo.challenges c
                WHERE c.isActive = 1 AND EXISTS (SELECT 1 FROM dbo.challengeQuestions q WHERE q.challengeID = c.challengeID)
                  AND (@search = '' OR CHARINDEX(@search, c.title) > 0)
                  AND (@difficulty = '' OR c.difficulty = @difficulty
                  )
                ORDER BY c.createdAt DESC, c.challengeID DESC;";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@search", SqlDbType.NVarChar, 150).Value = txtSearch.Text.Trim();

                cmd.Parameters.Add("@difficulty", SqlDbType.NVarChar, 20).Value = ddlDifficulty.SelectedValue;

                DataTable quizzes = new DataTable();
                adapter.Fill(quizzes);

                gvQuizzes.DataSource = quizzes;
                gvQuizzes.DataBind();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)){return;}

            gvQuizzes.PageIndex = 0;
            BindQuizzes();
        }

        protected void gvQuizzes_PageIndexChanging(
            object sender, GridViewPageEventArgs e)
        {
            if (!Database.RequireUser(Response)){return;}

            gvQuizzes.PageIndex = e.NewPageIndex;
            BindQuizzes();
        }
    }
}
