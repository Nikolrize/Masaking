using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class SimulationStages : ActivityEditorPage
    {
        private int SimulationId
        {
            get { int id; return Int32.TryParse(Request.QueryString["id"], out id) && id > 0 ? id : 0; }
        }
        protected override string ActivityKind { get { return "simulation"; } }
        protected override int ActivityId { get { return SimulationId; } }
        private int? EditingId
        {
            get { return ViewState["StageId"] as int?; }
            set { ViewState["StageId"] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            StockImages.Bind(ddlStageStock, "cookware");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            Page.Form.Enctype = "multipart/form-data";
            lnkSteps.NavigateUrl = EditorUrl("SimulationSteps.aspx?id=") + SimulationId;
            if (!IsPostBack) BindPage();
        }

        private void BindPage()
        {
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT s.title,s.isActive, (SELECT COUNT(*) FROM dbo.simulationProgress p WHERE p.simulationID=s.simulationID) AS attempts FROM dbo.simulations s WHERE s.simulationID=@id; SELECT simulationStageID,stageNumber,title,instructions, (SELECT COUNT(*) FROM dbo.simulationSteps st WHERE st.simulationStageID=g.simulationStageID) AS stepCount FROM dbo.simulationStages g WHERE simulationID=@id ORDER BY stageNumber;", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                DataSet data = new DataSet();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(data);
                if (data.Tables[0].Rows.Count == 0) return;
                DataRow simulation = data.Tables[0].Rows[0];
                bool editable = !Convert.ToBoolean(simulation["isActive"]) && Convert.ToInt32(simulation["attempts"]) == 0;
                lblHeading.Text = Server.HtmlEncode("Stages: " + Convert.ToString(simulation["title"]));
                lblStatus.Text = editable ? "Create stages, then assign every step to a stage. Stage and step numbers must run consecutively." : "Stages are locked. Structural changes require an unpublished simulation without attempts.";
                pnlStageEditor.Visible = editable;
                gvStages.Columns[4].Visible = editable;
                gvStages.DataSource = data.Tables[1];
                gvStages.DataBind();
                if (!EditingId.HasValue)
                    txtStageNumber.Text = (data.Tables[1].Rows.Count == 0 ? 1
                        : Convert.ToInt32(data.Tables[1].Rows[data.Tables[1].Rows.Count - 1]["stageNumber"]) + 1).ToString();
            }
        }

        protected void btnSaveStage_Click(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            int number;
            string title = txtStageTitle.Text.Trim();
            string instructions = txtStageInstructions.Text.Trim();
            if (!Int32.TryParse(txtStageNumber.Text, out number) || number < 1 || number > 20 || title.Length == 0 || title.Length > 100 || instructions.Length > 1000)
            {
                Message("Enter a stage number from 1-20, a title within 100 characters and instructions within 1,000 characters.", false);
                return;
            }
            if (chkRemoveStageImage.Checked && (uploadStageImage.HasFile || ddlStageStock.SelectedValue.Length > 0))
            {
                Message("Choose a picture or remove the current picture, not both.", false);
                return;
            }
            SaveStage(false, number, title, instructions);
        }

        private void SaveStage(bool deleting, int number, string title, string instructions)
        {
            string newUpload = null;
            bool committed = false;
            try
            {
                using (SqlConnection cn = Database.Open())
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    CheckActivity("simulation", SimulationId, cn, tx);
                    using (SqlCommand check = new SqlCommand(@"SELECT s.simulationID FROM dbo.simulations s WITH (UPDLOCK,HOLDLOCK) INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID WHERE s.simulationID=@id AND s.isActive=0 AND r.status='Approved' AND NOT EXISTS (SELECT 1 FROM dbo.simulationProgress p WHERE p.simulationID=s.simulationID);", cn, tx))
                    {
                        check.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                        if (check.ExecuteScalar() == null)
                        {
                            Message("Stages require an unpublished simulation without attempts and an approved recipe.", false);
                            return;
                        }
                    }
                    string picture = "";
                    if (EditingId.HasValue)
                    {
                        using (SqlCommand read = new SqlCommand("SELECT cookwareImage FROM dbo.simulationStages WITH (UPDLOCK,HOLDLOCK) WHERE simulationStageID=@stage AND simulationID=@id;", cn, tx))
                        {
                            read.Parameters.Add("@stage", SqlDbType.Int).Value = EditingId.Value;
                            read.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                            object value = read.ExecuteScalar();
                            if (value == null) { Message("That stage is no longer available.", false); return; }
                            picture = Convert.ToString(value);
                        }
                    }
                    if (deleting)
                    {
                        using (SqlCommand used = new SqlCommand("SELECT COUNT(*) FROM dbo.simulationSteps WHERE simulationID=@id AND simulationStageID=@stage;", cn, tx))
                        {
                            used.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                            used.Parameters.Add("@stage", SqlDbType.Int).Value = EditingId.Value;
                            if (Convert.ToInt32(used.ExecuteScalar()) > 0)
                            {
                                Message("Move or delete this stage's steps before deleting the stage.", false);
                                return;
                            }
                        }
                    }
                    else if (uploadStageImage.HasFile)
                        picture = newUpload = SimulationImages.Save(uploadStageImage.PostedFile);
                    else if (ddlStageStock.SelectedValue.Length > 0)
                        picture = StockImages.Selected(ddlStageStock, "cookware");
                    else if (chkRemoveStageImage.Checked) picture = "";

                    string sql = deleting ? "DELETE FROM dbo.simulationStages WHERE simulationStageID=@stage AND simulationID=@id;"
                        : EditingId.HasValue ? @"UPDATE dbo.simulationStages SET stageNumber=@number,title=@title,instructions=@instructions,cookwareImage=@image WHERE simulationStageID=@stage AND simulationID=@id;"
                        : @"INSERT INTO dbo.simulationStages(simulationID,stageNumber,title,instructions,cookwareImage) VALUES(@id,@number,@title,@instructions,@image);";
                    using (SqlCommand save = new SqlCommand(sql, cn, tx))
                    {
                        save.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                        save.Parameters.Add("@stage", SqlDbType.Int).Value = EditingId.HasValue ? (object)EditingId.Value : DBNull.Value;
                        save.Parameters.Add("@number", SqlDbType.Int).Value = number;
                        save.Parameters.Add("@title", SqlDbType.NVarChar, 100).Value = title;
                        save.Parameters.Add("@instructions", SqlDbType.NVarChar, 1000).Value = instructions;
                        save.Parameters.Add("@image", SqlDbType.NVarChar, 255).Value = picture.Length > 0 ? (object)picture : DBNull.Value;
                        if (save.ExecuteNonQuery() != 1) throw new InvalidDataException("The stage could not be saved.");
                    }
                    tx.Commit();
                    committed = true;
                }
                ResetEditor();
                BindPage();
                Message(deleting ? "Stage deleted. Check the remaining stage numbers." : "Stage saved. Assign its steps on the steps page.", true);
            }
            catch (InvalidDataException ex) { Message(ex.Message, false); }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) { Message("That stage number is already used.", false); }
            catch (IOException ex) { Trace.Warn("Stage image", "Image save failed.", ex); Message("The picture could not be saved. Choose it again and retry.", false); }
            catch (UnauthorizedAccessException ex) { Trace.Warn("Stage image", "Image folder is not writable.", ex); Message("The picture folder could not be written.", false); }
            finally
            {
                if (!committed && newUpload != null)
                {
                    try { SimulationImages.DeleteFailedUpload(newUpload); }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { Trace.Warn("Stage image", "Upload cleanup failed.", ex); }
                }
            }
        }

        protected void gvStages_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!RequireEditor() || (e.CommandName != "EditStage" && e.CommandName != "RemoveStage")) return;
            int stage;
            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out stage)) return;
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT g.* FROM dbo.simulationStages g INNER JOIN dbo.simulations s ON s.simulationID=g.simulationID WHERE g.simulationStageID=@stage AND g.simulationID=@id AND s.isActive=0 AND NOT EXISTS (SELECT 1 FROM dbo.simulationProgress p WHERE p.simulationID=s.simulationID);", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                cmd.Parameters.Add("@stage", SqlDbType.Int).Value = stage;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) { Message("This stage is unavailable for editing.", false); return; }
                    EditingId = stage;
                    txtStageNumber.Text = Convert.ToString(reader["stageNumber"]);
                    txtStageTitle.Text = Convert.ToString(reader["title"]);
                    txtStageInstructions.Text = Convert.ToString(reader["instructions"]);
                    ddlStageStock.SelectedIndex = 0;
                    chkRemoveStageImage.Checked = false;
                    Preview(Convert.ToString(reader["cookwareImage"]));
                }
            }
            if (e.CommandName == "RemoveStage") SaveStage(true, 0, "", "");
        }

        protected void ddlStageStock_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            if (ddlStageStock.SelectedValue.Length > 0) Preview(StockImages.Selected(ddlStageStock, "cookware"));
            else Preview("");
        }

        protected void btnClearStage_Click(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            ResetEditor();
            BindPage();
        }

        private void ResetEditor()
        {
            EditingId = null;
            txtStageTitle.Text = txtStageInstructions.Text = "";
            ddlStageStock.SelectedIndex = 0;
            chkRemoveStageImage.Checked = false;
            Preview("");
        }
        private void Preview(string path)
        {
            string url = AdminTable.RecipeImage(this, path);
            imgStagePreview.ImageUrl = url.Length > 0 ? url : Database.DefaultAvatar;
            imgStagePreview.Style["display"] = url.Length > 0 ? "block" : "none";
        }
        private void Message(string text, bool success)
        {
            lblMessage.Text = Server.HtmlEncode(text);
            lblMessage.CssClass = "alert d-block " + (success ? "alert-success" : "alert-warning");
        }
    }
}
