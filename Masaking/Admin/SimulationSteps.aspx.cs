using Masaking.Infrastructure;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Masaking.Admin
{
    public partial class SimulationSteps : ActivityEditorPage
    {
        protected override string ActivityKind { get { return "simulation"; } }
        protected override int ActivityId { get { return SimulationId; } }

        private int SimulationId
        {
            get
            {
                int id;

                return Int32.TryParse(Request.QueryString["id"], out id) && id > 0 ? id : 0;
            }
        }

        private int? EditingId
        {
            get { return ViewState["StepId"] as int?; }
            set { ViewState["StepId"] = value; }
        }

        private bool ImagesOnly
        {
            get { return ViewState["ImagesOnly"] as bool? ?? false; }
            set { ViewState["ImagesOnly"] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            StockImages.Bind(ddlStockImage, "ingredients");
        }

        private void BindStages(SqlConnection cn)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT simulationStageID,CONVERT(nvarchar(10),stageNumber)+N'. '+title AS label FROM dbo.simulationStages WHERE simulationID=@id ORDER BY stageNumber;", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    ddlStage.DataSource = reader;
                    ddlStage.DataValueField = "simulationStageID";
                    ddlStage.DataTextField = "label";
                    ddlStage.DataBind();
                }
                ddlStage.Items.Insert(0, new ListItem("-- Main cooking area (no stage) --", ""));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;

            Page.Form.Enctype = "multipart/form-data";
            lnkStages.NavigateUrl = EditorUrl("SimulationStages.aspx?id=") + SimulationId;
            SetEditorMode(ImagesOnly);

            if (!IsPostBack)
            {
                BindPage();
            }
        }

        private void BindPage()
        {
            pnlContent.Visible = false;
            bool editable = false;
            bool canEditImages = false;

            using (SqlConnection cn = Database.Open())
            {
                const string detailsSql = @" SELECT s.title, s.isActive, (SELECT COUNT(*) FROM dbo.simulationProgress p WHERE p.simulationID=s.simulationID) AS attemptCount
                    FROM dbo.simulations s WHERE s.simulationID=@id;";

                using (SqlCommand cmd =
                    new SqlCommand(detailsSql, cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            ShowMessage("Simulation not found.", false);
                            return;
                        }

                        lblHeading.Text = Server.HtmlEncode("Steps: " + Convert.ToString(reader["title"]));

                        canEditImages = !Convert.ToBoolean(reader["isActive"]);
                        editable = canEditImages && Convert.ToInt32(reader["attemptCount"]) == 0;

                        pnlEditor.Visible = canEditImages && (editable || ImagesOnly);
                        gvSteps.Columns[3].Visible = canEditImages;

                        lblStatus.Text = editable ? "Draft: steps can be added, edited or deleted."
                            : canEditImages ? "Step content is locked because attempts exist. Edit image changes artwork only." : "Published: unpublish before changing images. Step content stays locked once attempts exist.";
                    }
                }

                BindStages(cn);

                const string ingredientsSql = @" SELECT i.ingredientID, i.ingredientName FROM dbo.ingredients i INNER JOIN dbo.simulations s ON s.recipeID=i.recipeID WHERE s.simulationID=@id ORDER BY i.displayOrder, i.ingredientID;";

                using (SqlCommand cmd = new SqlCommand(ingredientsSql, cn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;

                    DataTable ingredients = new DataTable();
                    adapter.Fill(ingredients);

                    ddlIngredient.DataSource = ingredients;
                    ddlIngredient.DataTextField = "ingredientName";
                    ddlIngredient.DataValueField = "ingredientID";
                    ddlIngredient.DataBind();

                    ddlIngredient.Items.Insert(0, new ListItem("--Select an ingredient --", ""));
                }

                const string stepsSql = @"SELECT st.simulationStepID,st.stepNumber,COALESCE(i.ingredientName,'Cooking action') AS ingredientName,st.instruction,st.actionType,COALESCE(g.title,'Main cooking area') AS stageTitle FROM dbo.simulationSteps st LEFT JOIN dbo.ingredients i ON i.ingredientID=st.ingredientID LEFT JOIN dbo.simulationStages g ON g.simulationStageID=st.simulationStageID WHERE st.simulationID=@id ORDER BY st.stepNumber;";

                using (SqlCommand cmd = new SqlCommand(stepsSql, cn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;

                    DataTable steps = new DataTable();
                    adapter.Fill(steps);

                    steps.Columns.Add("canEdit", typeof(bool));
                    foreach (DataRow row in steps.Rows) row["canEdit"] = editable;

                    gvSteps.DataSource = steps;
                    gvSteps.DataBind();

                    if (!EditingId.HasValue)
                    {
                        int nextNumber = steps.Rows.Count == 0 ? 1 : Convert.ToInt32(steps.Rows[steps.Rows.Count - 1]["stepNumber"]) + 1;

                        txtNumber.Text = nextNumber.ToString();
                    }
                }
            }

            pnlContent.Visible = true;
            RefreshImagePreview();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            int number = 0;
            int ingredientId = 0;
            string instruction = txtInstruction.Text.Trim();

            if (!ImagesOnly && (SimulationId == 0
                || !Int32.TryParse(txtNumber.Text, out number) || number < 1 || number > 100
                || (ddlActionType.SelectedValue == "Ingredient" && (!Int32.TryParse(ddlIngredient.SelectedValue, out ingredientId) || ingredientId <= 0))
                || (ddlActionType.SelectedValue != "Ingredient" && ddlActionType.SelectedValue != "Action")
                || instruction.Length == 0 || instruction.Length > 1000))
            {
                ShowMessage("Enter a step number from 1-100, select an ingredient, "
                    + "and enter an instruction within 1,000 characters.", false);
                return;
            }
            if (ImagesOnly && !EditingId.HasValue)
            {
                ShowMessage("Select an existing step before changing its image.", false);
                return;
            }
            if ((uploadStepImage.HasFile || ddlStockImage.SelectedValue.Length > 0) && chkUseDefaultImage.Checked)
            {
                ShowMessage("Choose a picture or reset to the ingredient picture, not both.", false);
                return;
            }

            bool imagesOnly = ImagesOnly;
            if (RunChange(imagesOnly ? "SaveImages" : "SaveStep",
                EditingId, number, ingredientId, instruction))
            {
                ResetEditor();
                BindPage();
                ShowMessage(imagesOnly ? "Step image saved." : "Step saved.", true);
            }
        }

        private bool RunChange(
            string action, int? stepId, int number, int ingredientId, string instruction)
        {
            bool imagesOnly = action == "SaveImages";
            bool deleting = action == "RemoveStep";
            if (!imagesOnly && !deleting && action != "SaveStep") return false;

            string newUpload = null;
            bool committed = false;
            try
            {
                using (SqlConnection cn = Database.Open())
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    CheckActivity("simulation", SimulationId, cn, tx);
                    //image only edits preserve ingredient, order, instruction and saved results
                    const string eligibilitySql = @"SELECT s.recipeID FROM dbo.simulations s WITH (UPDLOCK,HOLDLOCK) WHERE s.simulationID=@id AND s.isActive=0 AND (@imagesOnly=1 OR (NOT EXISTS (SELECT 1 FROM dbo.simulationProgress p WHERE p.simulationID=@id) AND EXISTS (SELECT 1 FROM dbo.recipes r WHERE r.recipeID=s.recipeID AND r.status='Approved')));";
                    object recipe;
                    using (SqlCommand check = new SqlCommand(eligibilitySql, cn, tx))
                    {
                        check.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                        check.Parameters.Add("@imagesOnly", SqlDbType.Bit).Value = imagesOnly;
                        recipe = check.ExecuteScalar();
                    }
                    if (recipe == null)
                    {
                        ShowMessage("Unpublish before changing images. Step content also requires " + "a simulation without attempts and an approved recipe.", false);
                        return false;
                    }

                    string actionType = ddlActionType.SelectedValue;
                    int parsedStage;
                    int? stageId = Int32.TryParse(ddlStage.SelectedValue, out parsedStage) ? (int?)parsedStage : null;
                    string image = "";
                    int previousIngredient = 0;
                    if (stepId.HasValue)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"SELECT ingredientID,image,actionType,simulationStageID FROM dbo.simulationSteps WITH (UPDLOCK,HOLDLOCK) WHERE simulationStepID=@step AND simulationID=@id;", cn, tx))
                        {
                            cmd.Parameters.Add("@step", SqlDbType.Int).Value = stepId.Value;
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    ShowMessage("This step no longer exists in the selected simulation.", false);
                                    return false;
                                }
                                image = Convert.ToString(reader["image"]);
                                previousIngredient = reader["ingredientID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ingredientID"]);
                                if (imagesOnly)
                                {
                                    actionType = Convert.ToString(reader["actionType"]);
                                    stageId = reader["simulationStageID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["simulationStageID"]);
                                }
                            }
                        }
                    }
                    else if (imagesOnly || deleting)
                    {
                        ShowMessage("Select an existing step.", false);
                        return false;
                    }

                    if (!imagesOnly && !deleting && actionType == "Ingredient")
                    {
                        using (SqlCommand check = new SqlCommand(@"
                            SELECT ingredientID FROM dbo.ingredients
                            WHERE ingredientID=@ingredient AND recipeID=@recipe;", cn, tx))
                        {
                            check.Parameters.Add("@ingredient", SqlDbType.Int).Value = ingredientId;
                            check.Parameters.Add("@recipe", SqlDbType.Int).Value = Convert.ToInt32(recipe);
                            if (check.ExecuteScalar() == null)
                            {
                                ShowMessage("Select an ingredient from the linked recipe.", false);
                                return false;
                            }
                        }
                        // Clear the image when the selected ingredient changes
                        if (stepId.HasValue && previousIngredient != ingredientId) image = "";
                    }

                    if (!imagesOnly && !deleting)
                    {
                        using (SqlCommand stageCheck = new SqlCommand("SELECT COUNT(*) FROM dbo.simulationStages WHERE simulationID=@id AND (@stage IS NULL OR simulationStageID=@stage);", cn, tx))
                        {
                            stageCheck.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                            stageCheck.Parameters.Add("@stage", SqlDbType.Int).Value = stageId.HasValue ? (object)stageId.Value : DBNull.Value;
                            int count = Convert.ToInt32(stageCheck.ExecuteScalar());
                            if ((stageId.HasValue && count != 1) || (!stageId.HasValue && count > 0))
                            {
                                ShowMessage("Select a stage belonging to this simulation. Every step needs a stage once stages exist.", false);
                                return false;
                            }
                        }
                        if (actionType != "Ingredient" && actionType != "Action") return false;
                    }
                    if (!deleting && actionType == "Ingredient")
                    {
                        if (uploadStepImage.HasFile)
                        {
                            newUpload = SimulationImages.Save(uploadStepImage.PostedFile);
                            image = newUpload;
                        }
                        else if (ddlStockImage.SelectedValue.Length > 0)
                            image = StockImages.Selected(ddlStockImage, "ingredients");
                        else if (chkUseDefaultImage.Checked) image = "";
                        // No upload or reset: keep the existing override. A new step starts without one.
                    }

                    if (actionType == "Action") image = "";

                    string sql = deleting
                        ? @"DELETE FROM dbo.simulationSteps WHERE simulationStepID=@step AND simulationID=@id;"
                        : imagesOnly
                            ? @"UPDATE dbo.simulationSteps SET image=@image WHERE simulationStepID=@step AND simulationID=@id;"
                            : stepId.HasValue
                                ? @"UPDATE dbo.simulationSteps SET stepNumber=@number,ingredientID=@ingredient,instruction=@instruction,image=@image,actionType=@type,simulationStageID=@stage WHERE simulationStepID=@step AND simulationID=@id;"
                                : @"INSERT INTO dbo.simulationSteps (simulationID,stepNumber,ingredientID,instruction,image,actionType,simulationStageID) VALUES (@id,@number,@ingredient,@instruction,@image,@type,@stage);";

                    using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                        cmd.Parameters.Add("@step", SqlDbType.Int).Value =
                            stepId.HasValue ? (object)stepId.Value : DBNull.Value;
                        cmd.Parameters.Add("@number", SqlDbType.Int).Value = number;
                        cmd.Parameters.Add("@ingredient", SqlDbType.Int).Value = actionType == "Ingredient" ? (object)ingredientId : DBNull.Value;
                        cmd.Parameters.Add("@type", SqlDbType.NVarChar, 20).Value = actionType;
                        cmd.Parameters.Add("@stage", SqlDbType.Int).Value = stageId.HasValue ? (object)stageId.Value : DBNull.Value;
                        cmd.Parameters.Add("@instruction", SqlDbType.NVarChar, 1000).Value = instruction;
                        cmd.Parameters.Add("@image", SqlDbType.NVarChar, 255).Value =
                            image.Length == 0 ? (object)DBNull.Value : image;
                        if (cmd.ExecuteNonQuery() != 1)
                        {
                            ShowMessage("No change made. The step may have been removed.", false);
                            return false;
                        }
                    }
                    tx.Commit();
                    committed = true;
                }
                return true;
            }
            catch (InvalidDataException ex) { ShowMessage(ex.Message, false); }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                    ShowMessage("That step number is already used in this simulation.", false);
                else
                {
                    Trace.Warn("Simulation step", "Save failed.", ex);
                    ShowMessage("The step could not be saved. Check the details and try again.", false);
                }
            }
            catch (IOException ex)
            {
                Trace.Warn("Simulation step", "Image save failed.", ex);
                ShowMessage("The image could not be saved. Choose it again and retry.", false);
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.Warn("Simulation step", "Upload folder is not writable.", ex);
                ShowMessage("The image folder could not be written. Check its permissions.", false);
            }
            finally
            {
                if (!committed && newUpload != null)
                {
                    try { SimulationImages.DeleteFailedUpload(newUpload); }
                    catch (IOException ex) { Trace.Warn("Simulation step", "Cleanup failed.", ex); }
                    catch (UnauthorizedAccessException ex) { Trace.Warn("Simulation step", "Cleanup failed.", ex); }
                }
            }
            return false;
        }

        protected void gvSteps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!RequireEditor()) return;
            bool editImage = e.CommandName == "EditStepImage";
            if (!editImage && e.CommandName != "EditStep" && e.CommandName != "RemoveStep") return;
            int stepId;
            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out stepId) || stepId <= 0)
            {
                ShowMessage("Invalid step selection.", false);
                return;
            }

            if (e.CommandName == "RemoveStep")
            {
                if (RunChange("RemoveStep", stepId, 0, 0, ""))
                {
                    ResetEditor();
                    BindPage();
                    ShowMessage("Step deleted. Check that the remaining numbers are consecutive.", true);
                }
                return;
            }

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT st.stepNumber,st.ingredientID,st.instruction,st.actionType,st.simulationStageID FROM dbo.simulationSteps st INNER JOIN dbo.simulations s ON s.simulationID=st.simulationID WHERE st.simulationStepID=@step AND st.simulationID=@id AND s.isActive=0 AND (@imagesOnly=1 OR NOT EXISTS (SELECT 1 FROM dbo.simulationProgress WHERE simulationID=@id));", cn))
            {
                cmd.Parameters.Add("@step", SqlDbType.Int).Value = stepId;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                cmd.Parameters.Add("@imagesOnly", SqlDbType.Bit).Value = editImage;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        ShowMessage("The step is unavailable for editing. Unpublish first; " + "content edits also require no attempts.", false);
                        return;
                    }
                    string ingredient = Convert.ToString(reader["ingredientID"]);
                    if (reader["ingredientID"] != DBNull.Value && ddlIngredient.Items.FindByValue(ingredient) == null)
                    {
                        ShowMessage("The ingredient is no longer available for this recipe.", false);
                        return;
                    }
                    EditingId = stepId;
                    ImagesOnly = editImage;
                    txtNumber.Text = Convert.ToString(reader["stepNumber"]);
                    ddlIngredient.SelectedValue = ingredient;
                    ddlActionType.SelectedValue = Convert.ToString(reader["actionType"]);
                    string stageValue = Convert.ToString(reader["simulationStageID"]);
                    ddlStage.SelectedValue = ddlStage.Items.FindByValue(stageValue) != null ? stageValue : "";
                    ddlStockImage.SelectedIndex = 0;
                    txtInstruction.Text = Convert.ToString(reader["instruction"]);
                    chkUseDefaultImage.Checked = false;
                    lblEditorHeading.Text = editImage ? "Edit step image" : "Edit step";
                    SetEditorMode(editImage);
                    pnlEditor.Visible = true;
                }
            }
            RefreshImagePreview();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;

            ResetEditor();
            BindPage();
        }

        private void ResetEditor()
        {
            EditingId = null;
            ImagesOnly = false;
            ddlActionType.SelectedValue = "Ingredient";
            SetEditorMode(false);
            lblEditorHeading.Text = "New step";
            txtInstruction.Text = "";
            ddlActionType.SelectedValue = "Ingredient";
            ddlStockImage.SelectedIndex = 0;
            chkUseDefaultImage.Checked = false;
            if (ddlIngredient.Items.Count > 0) ddlIngredient.SelectedIndex = 0;
            SetImagePreview("", "");
        }

        private void SetEditorMode(bool imagesOnly)
        {
            txtNumber.Enabled = txtInstruction.Enabled = ddlStage.Enabled = ddlActionType.Enabled = !imagesOnly;
            bool ingredient = ddlActionType.SelectedValue == "Ingredient";
            ddlIngredient.Enabled = !imagesOnly && ingredient;
            ddlStockImage.Enabled = uploadStepImage.Enabled = chkUseDefaultImage.Enabled = ingredient;
            btnSave.Text = imagesOnly ? "Save image" : "Save step";
        }

        protected void ddlIngredient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            ddlStockImage.SelectedIndex = 0;
            RefreshImagePreview();
        }

        protected void ddlActionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            if (ddlActionType.SelectedValue == "Action") ddlIngredient.SelectedIndex = 0;
            ddlStockImage.SelectedIndex = 0;
            SetEditorMode(ImagesOnly);
            RefreshImagePreview();
        }

        protected void ddlStockImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            RefreshImagePreview();
        }

        private void RefreshImagePreview()
        {
            if (ddlActionType.SelectedValue == "Action")
            {
                SetImagePreview("", "");
                lblImageSource.Text = "Cooking actions use a Complete step button and do not need an ingredient picture.";
                return;
            }
            if (ddlStockImage.SelectedValue.Length > 0)
            {
                SetImagePreview(StockImages.Selected(ddlStockImage, "ingredients"), "");
                lblImageSource.Text = "Selected stock picture. An uploaded picture takes priority.";
                return;
            }
            int ingredientId;
            if (!Int32.TryParse(ddlIngredient.SelectedValue, out ingredientId))
            {
                SetImagePreview("", "");
                return;
            }
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT i.image AS defaultImage,st.image AS stepImage FROM dbo.ingredients i INNER JOIN dbo.simulations s ON s.recipeID=i.recipeID LEFT JOIN dbo.simulationSteps st ON st.simulationID=s.simulationID AND st.simulationStepID=@step AND st.ingredientID=i.ingredientID WHERE s.simulationID=@id AND i.ingredientID=@ingredient;", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                cmd.Parameters.Add("@ingredient", SqlDbType.Int).Value = ingredientId;
                cmd.Parameters.Add("@step", SqlDbType.Int).Value =
                    EditingId.HasValue ? (object)EditingId.Value : DBNull.Value;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string stepImage = chkUseDefaultImage.Checked ? ""
                            : Convert.ToString(reader["stepImage"]);
                        SetImagePreview(stepImage, Convert.ToString(reader["defaultImage"]));
                    }
                    else SetImagePreview("", "");
                }
            }
        }

        private void SetImagePreview(string stepImage, string defaultImage)
        {
            bool hasOverride = !String.IsNullOrWhiteSpace(stepImage);
            string url = PreviewUrl(hasOverride ? stepImage : defaultImage);
            imgStepPreview.ImageUrl = url.Length == 0 ? Database.DefaultAvatar : url;
            imgStepPreview.Style["display"] = url.Length == 0 ? "none" : "block";
            if (hasOverride)
                lblImageSource.Text = url.Length == 0
                    ? "The current step image is missing. Upload a replacement or use the ingredient default."
                    : "Current step image. Leave the upload empty to keep this picture.";
            else
                lblImageSource.Text = url.Length == 0
                    ? "No ingredient default photo is available. A photo is optional; gameplay can show the ingredient name."
                    : "Using the ingredient's default image. Uploading a photo creates a step-specific override.";
        }

        private string PreviewUrl(string value)
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
                if (extension != ".png" && extension != ".jpg" && extension != ".jpeg"
                    && extension != ".webp" && extension != ".gif" && extension != ".svg") return "";
                return File.Exists(Server.MapPath(path)) ? ResolveUrl(path) : "";
            }
            catch (ArgumentException) { return ""; }
            catch (HttpException) { return ""; }
            catch (UriFormatException) { return ""; }
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = success ? "alert alert-success d-block mb-3" : "alert alert-warning d-block mb-3";
        }
    }
}