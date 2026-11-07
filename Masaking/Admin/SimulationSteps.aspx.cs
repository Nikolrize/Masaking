using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class SimulationSteps : System.Web.UI.Page
    {
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            Page.Form.Enctype = "multipart/form-data";
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

                        lblHeading.Text = Server.HtmlEncode( "Steps: " + Convert.ToString(reader["title"]));

                        canEditImages = !Convert.ToBoolean(reader["isActive"]);
                        editable = canEditImages && Convert.ToInt32(reader["attemptCount"]) == 0;

                        pnlEditor.Visible = canEditImages && (editable || ImagesOnly);
                        gvSteps.Columns[3].Visible = canEditImages;

                        lblStatus.Text = editable
                            ? "Draft: steps can be added, edited or deleted."
                            : canEditImages
                                ? "Step content is locked because attempts exist. Edit image changes artwork only."
                                : "Published: unpublish before changing images. Step content stays locked once attempts exist.";
                    }
                }

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

                const string stepsSql = @"SELECT st.simulationStepID, st.stepNumber, i.ingredientName, st.instruction FROM dbo.simulationSteps st INNER JOIN dbo.ingredients i ON i.ingredientID=st.ingredientID WHERE st.simulationID=@id ORDER BY st.stepNumber;";

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
            if (!Database.RequireAdmin(Response)) return;
            int number = 0;
            int ingredientId = 0;
            string instruction = txtInstruction.Text.Trim();

            if (!ImagesOnly && (SimulationId == 0
                || !Int32.TryParse(txtNumber.Text, out number) || number < 1 || number > 100
                || !Int32.TryParse(ddlIngredient.SelectedValue, out ingredientId) || ingredientId <= 0
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
            if (uploadStepImage.HasFile && chkUseDefaultImage.Checked)
            {
                ShowMessage("Choose a replacement photo OR use the ingredient default, not both.", false);
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

                    string image = "";
                    int previousIngredient = 0;
                    if (stepId.HasValue)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"SELECT ingredientID,image FROM dbo.simulationSteps WITH (UPDLOCK,HOLDLOCK) WHERE simulationStepID=@step AND simulationID=@id;", cn, tx))
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
                                previousIngredient = Convert.ToInt32(reader["ingredientID"]);
                            }
                        }
                    }
                    else if (imagesOnly || deleting)
                    {
                        ShowMessage("Select an existing step.", false);
                        return false;
                    }

                    if (!imagesOnly && !deleting)
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
                        // An old garlic image must not remain attached after choosing a different ingredient.
                        if (stepId.HasValue && previousIngredient != ingredientId) image = "";
                    }

                    if (!deleting)
                    {
                        if (uploadStepImage.HasFile)
                        {
                            newUpload = SimulationImages.Save(uploadStepImage.PostedFile);
                            image = newUpload;
                        }
                        else if (chkUseDefaultImage.Checked) image = "";
                        // No upload or reset: keep the existing override. A new step starts without one.
                    }

                    string sql = deleting
                        ? @"DELETE FROM dbo.simulationSteps WHERE simulationStepID=@step AND simulationID=@id;"
                        : imagesOnly
                            ? @"UPDATE dbo.simulationSteps SET image=@image WHERE simulationStepID=@step AND simulationID=@id;"
                            : stepId.HasValue
                                ? @"UPDATE dbo.simulationSteps SET stepNumber=@number,ingredientID=@ingredient, instruction=@instruction,image=@image WHERE simulationStepID=@step AND simulationID=@id;"
                                : @"INSERT INTO dbo.simulationSteps (simulationID,stepNumber,ingredientID,instruction,image) VALUES (@id,@number,@ingredient,@instruction,@image);";

                    using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = SimulationId;
                        cmd.Parameters.Add("@step", SqlDbType.Int).Value =
                            stepId.HasValue ? (object)stepId.Value : DBNull.Value;
                        cmd.Parameters.Add("@number", SqlDbType.Int).Value = number;
                        cmd.Parameters.Add("@ingredient", SqlDbType.Int).Value = ingredientId;
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
            if (!Database.RequireAdmin(Response)) return;
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
            using (SqlCommand cmd = new SqlCommand(@"SELECT st.stepNumber,st.ingredientID,st.instruction FROM dbo.simulationSteps st INNER JOIN dbo.simulations s ON s.simulationID=st.simulationID WHERE st.simulationStepID=@step AND st.simulationID=@id AND s.isActive=0 AND (@imagesOnly=1 OR NOT EXISTS (SELECT 1 FROM dbo.simulationProgress WHERE simulationID=@id));", cn))
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
                    if (ddlIngredient.Items.FindByValue(ingredient) == null)
                    {
                        ShowMessage("The ingredient is no longer available for this recipe.", false);
                        return;
                    }
                    EditingId = stepId;
                    ImagesOnly = editImage;
                    txtNumber.Text = Convert.ToString(reader["stepNumber"]);
                    ddlIngredient.SelectedValue = ingredient;
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
            if (!Database.RequireAdmin(Response)) return;

            ResetEditor();
            BindPage();
        }

        private void ResetEditor()
        {
            EditingId = null;
            ImagesOnly = false;
            SetEditorMode(false);
            lblEditorHeading.Text = "New step";
            txtInstruction.Text = "";
            chkUseDefaultImage.Checked = false;
            if (ddlIngredient.Items.Count > 0) ddlIngredient.SelectedIndex = 0;
            SetImagePreview("", "");
        }

        private void SetEditorMode(bool imagesOnly)
        {
            txtNumber.Enabled = ddlIngredient.Enabled = txtInstruction.Enabled = !imagesOnly;
            btnSave.Text = imagesOnly ? "Save image" : "Save step";
        }

        protected void ddlIngredient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
            RefreshImagePreview();
        }

        private void RefreshImagePreview()
        {
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