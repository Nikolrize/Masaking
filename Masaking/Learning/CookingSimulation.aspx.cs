using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;


namespace Masaking.Learning
{
    public partial class CookingSimulation : ResponsivePage
    {
        private DataRow simulation;
        private DataTable steps;
        private bool available;
        private string feedback;
        private string effect = "";

        private int SimulationId
        {
            get
            {
                int id;
                return Int32.TryParse(Request.QueryString["id"], out id) && id > 0 ? id : 0;
            }
        }

        private int? AttemptId
        {
            get { return ViewState["AttemptId"] as int?; }
            set { ViewState["AttemptId"] = value; }
        }

        private int ShownStep
        {
            get { return ViewState["ShownStep"] as int? ?? 0; }
            set { ViewState["ShownStep"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireUser(Response)) return;
            if (SimulationId == 0)
            {
                Response.Redirect("~/Learning/Simulations.aspx");
                return;
            }

            using (SqlConnection cn = Database.Open())
            {
                using (SqlCommand cmd = new SqlCommand(@"SELECT s.title,s.description,s.instructions,s.cookwareImage,s.completedImage,s.recipeID,r.image AS recipeImage, s.isActive,r.status AS recipeStatus FROM dbo.simulations s INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID WHERE s.simulationID=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                    DataTable table = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(table);
                    if (table.Rows.Count == 0) { feedback = "Simulation not found."; return; }
                    simulation = table.Rows[0];
                }
                lblTitle.Text = Server.HtmlEncode(Convert.ToString(simulation["title"]));
                lblDescription.Text = Server.HtmlEncode(Convert.ToString(simulation["description"]));
                available = Convert.ToBoolean(simulation["isActive"]) && Convert.ToString(simulation["recipeStatus"]) == "Approved";
                if (!available)
                {
                    feedback = "This simulation is currently unavailable. Your saved progress is preserved.";
                    return;
                }
                steps = ReadSteps(cn, null);
                if (!StepsAreValid())
                {
                    available = false;
                    feedback = "This simulation needs its steps checked by an administrator.";
                    return;
                }
                //recreate the ingredient controls before their postback events run
                BindIngredients();
                if (!IsPostBack)
                {
                    int requestedAttempt;
                    bool specific = Int32.TryParse(Request.QueryString["attempt"], out requestedAttempt)
                        && requestedAttempt > 0;
                    string sql = specific
                        ? @"SELECT simulationProgressID FROM dbo.simulationProgress WHERE simulationProgressID=@attempt AND simulationID=@id AND userID=@user;"
                        : @"SELECT TOP (1) simulationProgressID FROM dbo.simulationProgress WHERE simulationID=@id AND userID=@user ORDER BY simulationProgressID DESC;";
                    using (SqlCommand cmd = new SqlCommand(sql, cn))
                    {
                        cmd.Parameters.Add("@attempt", SqlDbType.Int).Value = requestedAttempt;
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                        cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                        object result = cmd.ExecuteScalar();
                        if (result != null) AttemptId = Convert.ToInt32(result);
                        else if (specific) feedback = "That attempt was not found for your account.";
                    }
                }
            }
        }

        private DataTable ReadSteps(SqlConnection cn, SqlTransaction tx)
        {
            return SimulationStructure.ReadSteps(cn, tx, SimulationId);
        }

        private bool StepsAreValid()
        {
            string message;
            return SimulationStructure.IsValid(steps, Convert.ToInt32(simulation["recipeID"]), out message);
        }

        private void BindIngredients()
        {
            DataTable ingredients = new DataTable();
            ingredients.Columns.Add("ingredientID", typeof(int));
            ingredients.Columns.Add("ingredientName", typeof(string));
            ingredients.Columns.Add("imageUrl", typeof(string));
            HashSet<int> added = new HashSet<int>();
            foreach (DataRow step in steps.Rows)
            {
                if (step["ingredientID"] == DBNull.Value) continue;
                int id = Convert.ToInt32(step["ingredientID"]);
                if (added.Add(id))
                    ingredients.Rows.Add(id, Convert.ToString(step["ingredientName"]),
                        SafeImage(Convert.ToString(step["ingredientImage"])));
            }
            ingredients.DefaultView.Sort = "ingredientName ASC";
            rptIngredients.DataSource = ingredients.DefaultView;
            rptIngredients.DataBind();
        }

        private string SafeImage(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return "";
            try
            {
                string path = Uri.UnescapeDataString(value.Trim());
                if (path.StartsWith("/Content/images/", StringComparison.OrdinalIgnoreCase)) path = "~" + path;
                else if (path.StartsWith("Content/images/", StringComparison.OrdinalIgnoreCase)) path = "~/" + path;
                if (!path.StartsWith("~/Content/images/", StringComparison.OrdinalIgnoreCase) || path.Contains("..") || path.Contains("\\")) return "";
                string extension = Path.GetExtension(path).ToLowerInvariant();
                if (extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".gif" && extension != ".webp" && extension != ".svg") return "";
                return File.Exists(Server.MapPath(path)) ? ResolveUrl(path) : "";
            }
            catch (ArgumentException) { return ""; }
            catch (HttpException) { return ""; }
            catch (UriFormatException) { return ""; }
        }

        protected override void OnPreRender(EventArgs e)
        {
            pnlStart.Visible = pnlGame.Visible = pnlResult.Visible = pnlAttemptActions.Visible = false;
            hfEffect.Value = effect;
            if (available)
            {
                lblInstructions.Text = Server.HtmlEncode(Convert.ToString(simulation["instructions"]));
                lnkRecipe.Visible = true;
                lnkRecipe.NavigateUrl = "~/Recipes/RecipeDetails.aspx?id=" + Convert.ToInt32(simulation["recipeID"]);
                DataRow attempt = ReadAttempt();
                if (attempt == null) pnlStart.Visible = true;
                else
                {
                    string status = Convert.ToString(attempt["status"]);
                    int mistakes = Convert.ToInt32(attempt["mistakes"]);
                    pnlAttemptActions.Visible = true;
                    if (status == "Completed")
                    {
                        pnlResult.Visible = true;
                        lblResult.Text = Convert.ToInt32(attempt["score"]) + "% accuracy. Incorrect choices: " + mistakes + ".";
                        string image = SafeImage(Convert.ToString(simulation["completedImage"]));
                        if (image.Length == 0) image = SafeImage(Convert.ToString(simulation["recipeImage"]));
                        imgCompleted.ImageUrl = image;
                        imgCompleted.Visible = image.Length > 0;
                    }
                    else if (status == "InProgress")
                    {
                        int current = attempt["currentStepNumber"] == DBNull.Value ? 1 : Convert.ToInt32(attempt["currentStepNumber"]);
                        if (current < 1 || current > steps.Rows.Count)
                            feedback = "This saved attempt needs an administrator to check its step number.";
                        else
                        {
                            pnlGame.Visible = true;
                            ShownStep = current;
                            lblStep.Text = "Step " + current + " of " + steps.Rows.Count;
                            lblMistakes.Text = "Incorrect choices: " + mistakes;
                            lblInstruction.Text = Server.HtmlEncode(Convert.ToString(steps.Rows[current - 1]["instruction"]));
                            int percentage = (current - 1) * 100 / steps.Rows.Count;
                            progressBar.Style["width"] = percentage + "%";
                            progressTrack.Attributes["aria-valuemin"] = "0";
                            progressTrack.Attributes["aria-valuemax"] = "100";
                            progressTrack.Attributes["aria-valuenow"] = percentage.ToString();
                            DataRow currentStep = steps.Rows[current - 1];
                            bool cookingAction = Convert.ToString(currentStep["actionType"]) == "Action";
                            pnlCookingAction.Visible = cookingAction;
                            pnlIngredientChoices.Visible = !cookingAction;
                            lblAreaHint.Text = cookingAction ? "Follow the instruction, then select Complete step."
                                : "Drop an ingredient here, or use its Add button.";
                            string stageTitle = Convert.ToString(currentStep["stageTitle"]);
                            lblStage.Text = stageTitle.Length == 0 ? "Cooking area"
                                : Server.HtmlEncode("Stage " + Convert.ToInt32(currentStep["stageNumber"]) + " of " + Convert.ToInt32(currentStep["stageCount"]) + ": " + stageTitle);
                            lblStageInstructions.Text = Server.HtmlEncode(Convert.ToString(currentStep["stageInstructions"]));
                            imgCookware.AlternateText = stageTitle.Length == 0 ? "Cooking area" : stageTitle;
                            string image = SafeImage(Convert.ToString(currentStep["stageImage"]));
                            if (image.Length == 0) image = SafeImage(Convert.ToString(simulation["cookwareImage"]));
                            imgCookware.ImageUrl = image;
                            imgCookware.Visible = image.Length > 0;
                            pnlCookwarePlaceholder.Visible = image.Length == 0;
                        }
                    }
                    else feedback = "This run was restarted. Start a new attempt to practice again.";
                }
            }
            lblMessage.Text = Server.HtmlEncode(feedback ?? (pnlGame.Visible
                ? (pnlCookingAction.Visible ? "Follow the cooking instruction and select Complete step." : "Choose the ingredient for the current instruction.")
                : "Take your time and practice the ingredient order."));
            lblMessage.CssClass = effect == "wrong" ? "alert alert-warning d-block mb-3" : "alert alert-info d-block mb-3";
            base.OnPreRender(e);
        }

        private DataRow ReadAttempt()
        {
            if (!AttemptId.HasValue) return null;
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT status,score,currentStepNumber,mistakes FROM dbo.simulationProgress WHERE simulationProgressID=@attempt AND simulationID=@id AND userID=@user;", cn))
            {
                cmd.Parameters.Add("@attempt", SqlDbType.Int).Value = AttemptId.Value;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                DataTable table = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(table);
                return table.Rows.Count == 0 ? null : table.Rows[0];
            }
        }

        private bool LockAvailableSimulation(SqlConnection cn, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(@"SELECT s.simulationID FROM dbo.simulations s WITH (UPDLOCK,HOLDLOCK)
                INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID WHERE s.simulationID=@id AND s.isActive=1 AND r.status='Approved';", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                return cmd.ExecuteScalar() != null;
            }
        }

        protected void btnStart_Click(object sender, EventArgs e) { StartAttempt(false); }
        protected void btnRestart_Click(object sender, EventArgs e) { StartAttempt(true); }

        private void StartAttempt(bool restart)
        {
            if (!Database.RequireUser(Response) || !available) return;
            using (SqlConnection cn = Database.Open())
            using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                if (!LockAvailableSimulation(cn, tx)) { feedback = "This simulation is no longer available."; return; }
                steps = ReadSteps(cn, tx);
                if (!StepsAreValid()) { feedback = "The simulation needs its steps checked."; return; }
                int? latestId = null;
                string latestStatus = "";
                using (SqlCommand cmd = new SqlCommand(@"SELECT TOP (1) simulationProgressID,status FROM dbo.simulationProgress WITH (UPDLOCK,HOLDLOCK)
                    WHERE simulationID=@id AND userID=@user ORDER BY simulationProgressID DESC;", cn, tx))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                        if (reader.Read())
                        {
                            latestId = Convert.ToInt32(reader["simulationProgressID"]);
                            latestStatus = Convert.ToString(reader["status"]);
                        }
                }
                //repeated clicks and old tabs will reopen the newest run instead of duplicating it
                if (latestId.HasValue && ((!restart && latestStatus == "InProgress")
                    || (restart && latestId != AttemptId)))
                {
                    AttemptId = latestId;
                    feedback = "Opened your latest saved attempt.";
                    tx.Commit();
                    return;
                }
                if (latestId.HasValue && latestStatus == "InProgress")
                    using (SqlCommand cmd = new SqlCommand(@"UPDATE dbo.simulationProgress SET status='Abandoned' WHERE simulationProgressID=@attempt AND userID=@user;", cn, tx))
                    {
                        cmd.Parameters.Add("@attempt", SqlDbType.Int).Value = latestId.Value;
                        cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                        cmd.ExecuteNonQuery();
                    }
                using (SqlCommand cmd = new SqlCommand(@" INSERT INTO dbo.simulationProgress(score,status,startedAt,completedAt,userID,simulationID,currentStepNumber,mistakes)
                    OUTPUT INSERTED.simulationProgressID VALUES (0,'InProgress',@now,NULL,@user,@id,1,0);", cn, tx))
                {
                    cmd.Parameters.Add("@now", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                    AttemptId = Convert.ToInt32(cmd.ExecuteScalar());
                }
                tx.Commit();
                feedback = "Let's begin. Follow the instruction above the cooking area.";
            }
            BindIngredients();
        }

        protected void rptIngredients_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "AddIngredient") return;
            int ingredientId;
            if (Int32.TryParse(Convert.ToString(e.CommandArgument), out ingredientId)) AddIngredient(ingredientId);
        }

        protected void btnDrop_Click(object sender, EventArgs e)
        {
            int ingredientId;
            if (Int32.TryParse(hfIngredient.Value, out ingredientId)) AddIngredient(ingredientId);
            hfIngredient.Value = "";
        }

        protected void btnCompleteAction_Click(object sender, EventArgs e)
        {
            AdvanceStep(null);
        }

        private void AddIngredient(int ingredientId)
        {
            AdvanceStep(ingredientId);
        }

        private void AdvanceStep(int? ingredientId)
        {
            if (!Database.RequireUser(Response) || !available || !AttemptId.HasValue) return;
            using (SqlConnection cn = Database.Open())
            using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                if (!LockAvailableSimulation(cn, tx)) { feedback = "This simulation is no longer available."; return; }
                steps = ReadSteps(cn, tx);
                if (!StepsAreValid()) { feedback = "The simulation needs its steps checked."; return; }
                int current, mistakes;
                string status;
                using (SqlCommand cmd = new SqlCommand(@"SELECT currentStepNumber,mistakes,status FROM dbo.simulationProgress WITH (UPDLOCK,HOLDLOCK)
                    WHERE simulationProgressID=@attempt AND simulationID=@id AND userID=@user;", cn, tx))
                {
                    cmd.Parameters.Add("@attempt", SqlDbType.Int).Value = AttemptId.Value;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) { feedback = "Attempt not found."; return; }
                        current = reader["currentStepNumber"] == DBNull.Value ? 1 : Convert.ToInt32(reader["currentStepNumber"]);
                        mistakes = Convert.ToInt32(reader["mistakes"]);
                        status = Convert.ToString(reader["status"]);
                    }
                }
                if (status != "InProgress" || current != ShownStep || current < 1 || current > steps.Rows.Count)
                {
                    feedback = "Your saved progress changed. The latest state is shown below.";
                    return;
                }
                DataRow currentStep = steps.Rows[current - 1];
                bool action = Convert.ToString(currentStep["actionType"]) == "Action";
                if (action != !ingredientId.HasValue)
                {
                    feedback = action ? "Follow the instruction and select Complete step." : "Select the ingredient shown in the instruction.";
                    return;
                }
                if (!action && !steps.AsEnumerable().Any(row => row["ingredientID"] != DBNull.Value && Convert.ToInt32(row["ingredientID"]) == ingredientId.Value))
                {
                    feedback = "Choose one of the displayed ingredients.";
                    return;
                }
                bool correct = action || ingredientId.Value == Convert.ToInt32(currentStep["ingredientID"]);
                if (correct) current++; else mistakes++;
                int correctCount = steps.AsEnumerable().Take(current - 1).Count(row => Convert.ToString(row["actionType"]) == "Ingredient");
                bool completed = current > steps.Rows.Count;
                int choices = correctCount + mistakes;
                int score = choices == 0 ? (completed ? 100 : 0) : (int)Math.Round(100m * correctCount / choices, MidpointRounding.AwayFromZero);
                using (SqlCommand cmd = new SqlCommand(@"UPDATE dbo.simulationProgress SET currentStepNumber=@step,mistakes=@mistakes,score=@score,status=@status,completedAt=@completed
                    WHERE simulationProgressID=@attempt AND simulationID=@id AND userID=@user;", cn, tx))
                {
                    cmd.Parameters.Add("@step", SqlDbType.Int).Value = current;
                    cmd.Parameters.Add("@mistakes", SqlDbType.Int).Value = mistakes;
                    cmd.Parameters.Add("@score", SqlDbType.Int).Value = score;
                    cmd.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = completed ? "Completed" : "InProgress";
                    cmd.Parameters.Add("@completed", SqlDbType.DateTime2).Value = completed ? (object)DateTime.UtcNow : DBNull.Value;
                    cmd.Parameters.Add("@attempt", SqlDbType.Int).Value = AttemptId.Value;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
                effect = completed ? "complete" : correct ? (action ? "action" : "correct") : "wrong";
                feedback = completed ? "Well done! Your completed result has been saved." : correct ? (action ? "Step completed. Your progress has been saved." : "Correct! Your progress has been saved.") : "Not yet. That ingredient stays in the tray. Follow the current instruction.";
            }
        }
    }
}
