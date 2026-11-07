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
    public partial class Simulations : ResponsivePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            if (!IsPostBack) BindSimulations();
        }
        private void BindSimulations()
        {
            const string sql = @" SELECT s.simulationID,s.title,s.description,s.difficulty, r.title AS recipeTitle, (SELECT COUNT(*) FROM dbo.simulationSteps st WHERE st.simulationID=s.simulationID) AS stepCount, latest.status FROM dbo.simulations s INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID
                OUTER APPLY(SELECT TOP (1) p.status FROM dbo.simulationProgress p WHERE p.simulationID=s.simulationID AND p.userID=@user ORDER BY p.simulationProgressID DESC) latest
                WHERE s.isActive=1 AND r.status='Approved' AND EXISTS (SELECT 1 FROM dbo.simulationSteps st WHERE st.simulationID=s.simulationID) ORDER BY s.title,s.simulationID;";
            DataTable simulations = new DataTable();
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                adapter.Fill(simulations);
            }
            simulations.Columns.Add("actionText", typeof(string));
            foreach (DataRow row in simulations.Rows)
            {
                string status = Convert.ToString(row["status"]);
                row["actionText"] = status == "InProgress" ? "Continue cooking" : status == "Completed" ? "View result / practice again" : "Start cooking";
            }
            rptSimulations.DataSource = simulations;
            rptSimulations.DataBind();
            pnlEmpty.Visible = simulations.Rows.Count == 0;
        }
    }
}