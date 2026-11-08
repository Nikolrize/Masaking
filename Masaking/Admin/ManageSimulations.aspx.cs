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
    public partial class ManageSimulations : ActivityEditorPage
    {
        private int? EditingId
        {
            get { return ViewState["EditingId"] as int?; }
            set { ViewState["EditingId"] = value; }
        }

        private bool ImagesOnly
        {
            get { return ViewState["ImagesOnly"] as bool? ?? false; }
            set { ViewState["ImagesOnly"] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            StockImages.Bind(ddlCookwareStock, "cookware");
            StockImages.Bind(ddlCompletedStock, "dishes");
        }

        protected void ddlStock_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            if (sender == ddlCookwareStock && ddlCookwareStock.SelectedValue.Length > 0)
                SetPreview(imgCookwarePreview, StockImages.Selected(ddlCookwareStock, "cookware"));
            else if (sender == ddlCompletedStock && ddlCompletedStock.SelectedValue.Length > 0)
                SetPreview(imgCompletedPreview, StockImages.Selected(ddlCompletedStock, "dishes"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;

            Page.Form.Enctype = "multipart/form-data";
            SetEditorMode(ImagesOnly);

            if (!IsPostBack)
            {
                BindRecipes();
                BindSimulations();
                ResetEditor();
                SelectRequestedRecipe();
            }
        }

        private void SelectRequestedRecipe()
        {
            string requested = Request.QueryString["recipeId"];
            if (ddlRecipe.Items.FindByValue(requested ?? "") != null)
                ddlRecipe.SelectedValue = requested;
        }

        private void BindRecipes()
        {
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@" SELECT recipeID, title FROM dbo.recipes WHERE status='Approved' AND (@editorAdmin=1 OR userID=@editorUser) ORDER BY title, recipeID;", cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                AddOwnerParameters(cmd);
                DataTable recipes = new DataTable();
                adapter.Fill(recipes);

                ddlRecipe.DataSource = recipes;
                ddlRecipe.DataTextField = "title";
                ddlRecipe.DataValueField = "recipeID";
                ddlRecipe.DataBind();

                ddlRecipe.Items.Insert(0, new ListItem("-- Select a recipe --", ""));
            }
        }

        private void BindSimulations()
        {
            const string sql = @" SELECT s.simulationID, s.title, s.difficulty, s.isActive, r.title AS recipeTitle,
              COALESCE(NULLIF(LTRIM(RTRIM(u.displayName)), ''), u.username, 'Unknown user') AS recipeOwner,
              (SELECT COUNT(*) FROM dbo.simulationSteps step WHERE step.simulationID=s.simulationID) AS stepCount,
              (SELECT COUNT(*) FROM dbo.simulationProgress progress WHERE progress.simulationID=s.simulationID) AS attemptCount
              FROM dbo.simulations s INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID
              LEFT JOIN dbo.users u ON u.userID=r.userID
              WHERE (@editorAdmin=1 OR r.userID=@editorUser) ORDER BY s.createdAt DESC, s.simulationID DESC;";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                AddOwnerParameters(cmd);
                DataTable simulations = new DataTable();
                adapter.Fill(simulations);
                gvSimulations.DataSource = simulations;
                gvSimulations.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;

            int recipeId = 0;
            string title = txtTitle.Text.Trim();
            string description = txtDescription.Text.Trim();
            string instructions = txtInstructions.Text.Trim();
            string difficulty = ddlDifficulty.SelectedValue;

            if (!ImagesOnly && (!Int32.TryParse(ddlRecipe.SelectedValue, out recipeId)
                || recipeId <= 0 || title.Length == 0 || title.Length > 150
                || description.Length > 1000 || instructions.Length == 0
                || instructions.Length > 4000
                || (difficulty != "Easy" && difficulty != "Medium" && difficulty != "Hard")))
            {
                ShowMessage("Select an approved recipe and difficulty. Enter a title within "
                    + "150 characters, description within 1,000 characters and general "
                    + "instructions within 4,000 characters.", false);
                return;
            }

            if (((uploadCookware.HasFile || ddlCookwareStock.SelectedValue.Length > 0) && chkRemoveCookware.Checked)
                || ((uploadCompletedImage.HasFile || ddlCompletedStock.SelectedValue.Length > 0) && chkRemoveCompleted.Checked))
            {
                ShowMessage("Choose a replacement image OR remove the current image, not both.", false);
                return;
            }

            if (ImagesOnly && !EditingId.HasValue)
            {
                ShowMessage("Select an existing simulation before editing images.", false);
                return;
            }

            List<string> newUploads = new List<string>();
            bool committed = false;
            bool imagesOnly = ImagesOnly;

            try
            {
                using (SqlConnection cn = Database.Open())
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    if (EditingId.HasValue) CheckActivity("simulation", EditingId.Value, cn, tx);
                    if (!imagesOnly) CheckRecipe(recipeId, cn, tx);
                    string cookware = "";
                    string completedImage = "";

                    if (EditingId.HasValue)
                    {
                        using (SqlCommand check = new SqlCommand(@"
                            SELECT s.cookwareImage,s.completedImage
                            FROM dbo.simulations s WITH (UPDLOCK,HOLDLOCK)
                            WHERE s.simulationID=@id AND s.isActive=0
                                AND (@imagesOnly=1 OR NOT EXISTS
                                    (SELECT 1 FROM dbo.simulationProgress p
                                     WHERE p.simulationID=@id));", cn, tx))
                        {
                            check.Parameters.Add("@id", SqlDbType.Int).Value = EditingId.Value;
                            check.Parameters.Add("@imagesOnly", SqlDbType.Bit).Value = imagesOnly;
                            using (SqlDataReader reader = check.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    ShowMessage("Unpublish the simulation before changing images. "
                                        + "Settings can only change when there are no attempts.", false);
                                    return;
                                }
                                cookware = Convert.ToString(reader["cookwareImage"]);
                                completedImage = Convert.ToString(reader["completedImage"]);
                            }
                        }
                    }

                    if (uploadCookware.HasFile)
                    {
                        cookware = SimulationImages.Save(uploadCookware.PostedFile);
                        newUploads.Add(cookware);
                    }
                    else if (ddlCookwareStock.SelectedValue.Length > 0)
                        cookware = StockImages.Selected(ddlCookwareStock, "cookware");
                    else if (chkRemoveCookware.Checked) cookware = "";

                    if (uploadCompletedImage.HasFile)
                    {
                        completedImage = SimulationImages.Save(uploadCompletedImage.PostedFile);
                        newUploads.Add(completedImage);
                    }
                    else if (ddlCompletedStock.SelectedValue.Length > 0)
                        completedImage = StockImages.Selected(ddlCompletedStock, "dishes");
                    else if (chkRemoveCompleted.Checked) completedImage = "";

                    string sql;
                    if (imagesOnly)
                        sql = @"UPDATE dbo.simulations SET cookwareImage=@cookware,completedImage=@completed WHERE simulationID=@id AND isActive=0;";
                    else if (EditingId.HasValue)
                        sql = @"UPDATE dbo.simulations SET title=@title,description=@description,difficulty=@difficulty, instructions=@instructions,recipeID=@recipe, cookwareImage=@cookware,completedImage=@completed
                            WHERE simulationID=@id AND isActive=0 AND NOT EXISTS (SELECT 1 FROM dbo.simulationProgress WHERE simulationID=@id) AND (recipeID=@recipe OR NOT EXISTS (SELECT 1 FROM dbo.simulationSteps WHERE simulationID=@id)) AND EXISTS (SELECT 1 FROM dbo.recipes WHERE recipeID=@recipe AND status='Approved');";
                    else
                        sql = @"INSERT INTO dbo.simulations (title,description,difficulty,instructions,isActive,createdAt, recipeID,cookwareImage,completedImage)
                            SELECT @title,@description,@difficulty,@instructions,0,@now, @recipe,@cookware,@completed
                            WHERE EXISTS (SELECT 1 FROM dbo.recipes WHERE recipeID=@recipe AND status='Approved');";

                    using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value =
                            EditingId.HasValue ? (object)EditingId.Value : DBNull.Value;
                        cmd.Parameters.Add("@title", SqlDbType.NVarChar, 150).Value = title;
                        cmd.Parameters.Add("@description", SqlDbType.NVarChar, 1000).Value = description;
                        cmd.Parameters.Add("@difficulty", SqlDbType.NVarChar, 20).Value = difficulty;
                        cmd.Parameters.Add("@instructions", SqlDbType.NVarChar, -1).Value = instructions;
                        cmd.Parameters.Add("@recipe", SqlDbType.Int).Value = recipeId;
                        cmd.Parameters.Add("@now", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                        cmd.Parameters.Add("@cookware", SqlDbType.NVarChar, 255).Value =
                            cookware.Length == 0 ? (object)DBNull.Value : cookware;
                        cmd.Parameters.Add("@completed", SqlDbType.NVarChar, 255).Value =
                            completedImage.Length == 0 ? (object)DBNull.Value : completedImage;

                        if (cmd.ExecuteNonQuery() != 1)
                        {
                            ShowMessage("No change made. Check that the recipe is still approved " + "and the draft is eligible. Remove its steps before changing " + "the linked recipe.", false);
                            return;
                        }
                    }
                    tx.Commit();
                    committed = true;
                }

                ResetEditor();
                BindRecipes();
                gvSimulations.PageIndex = 0;
                BindSimulations();
                ShowMessage(imagesOnly ? "Simulation images saved." : "Simulation draft saved.", true);
            }
            catch (InvalidDataException ex) { ShowMessage(ex.Message, false); }
            catch (SqlException ex)
            {
                Trace.Warn("Simulation upload", "Database save failed.", ex);
                ShowMessage("The simulation could not be saved. Check the details and try again.", false);
            }
            catch (IOException ex)
            {
                Trace.Warn("Simulation upload", "Image save failed.", ex);
                ShowMessage("The image could not be saved. Please choose it again and retry.", false);
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.Warn("Simulation upload", "Image folder is not writable.", ex);
                ShowMessage("The image folder could not be written. Check its permissions.", false);
            }
            finally
            {
                // a database rollback cannot remove physical files. if the database save failed, delete any new files that were uploaded
                if (!committed)
                    foreach (string path in newUploads)
                    {
                        try { SimulationImages.DeleteFailedUpload(path); }
                        catch (IOException ex) { Trace.Warn("Simulation upload", "Cleanup failed.", ex); }
                        catch (UnauthorizedAccessException ex) { Trace.Warn("Simulation upload", "Cleanup failed.", ex); }
                    }
            }
        }

        protected void gvSimulations_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!RequireEditor()) return;
            bool editImages = e.CommandName == "EditSimulationImages";
            bool editSettings = e.CommandName == "EditSimulation";
            if (!editImages && !editSettings && e.CommandName != "PublishSimulation"
                && e.CommandName != "UnpublishSimulation" && e.CommandName != "RemoveSimulation") return;

            int id;
            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0)
            {
                ShowMessage("Invalid simulation selection.", false);
                return;
            }

            CheckActivity("simulation", id);
            if (!editImages && !editSettings)
            {
                if (ChangeSimulation(id, e.CommandName))
                {
                    ResetEditor();
                    BindRecipes();
                }
                BindSimulations();
                return;
            }

            ResetEditor();
            BindRecipes();
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@" SELECT title,description,difficulty,instructions,recipeID, cookwareImage,completedImage FROM dbo.simulations
                WHERE simulationID=@id AND isActive=0 AND (@imagesOnly=1 OR NOT EXISTS (SELECT 1 FROM dbo.simulationProgress WHERE simulationID=@id));", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@imagesOnly", SqlDbType.Bit).Value = editImages;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        ShowMessage("Unpublish before editing images. Settings also require " + "a simulation without attempts.", false);
                        return;
                    }

                    string recipe = Convert.ToString(reader["recipeID"]);
                    string difficulty = Convert.ToString(reader["difficulty"]);
                    if (editImages)
                    {
                        if (ddlRecipe.Items.FindByValue(recipe) == null)
                            ddlRecipe.Items.Add(new ListItem("Linked recipe #" + recipe, recipe));
                        if (ddlDifficulty.Items.FindByValue(difficulty) == null)
                            ddlDifficulty.Items.Add(new ListItem(difficulty, difficulty));
                    }
                    if (ddlRecipe.Items.FindByValue(recipe) == null
                        || ddlDifficulty.Items.FindByValue(difficulty) == null)
                    {
                        ShowMessage("This simulation uses an unapproved recipe " + "or unsupported difficulty.", false);
                        return;
                    }

                    EditingId = id;
                    ImagesOnly = editImages;
                    txtTitle.Text = Convert.ToString(reader["title"]);
                    txtDescription.Text = Convert.ToString(reader["description"]);
                    txtInstructions.Text = Convert.ToString(reader["instructions"]);
                    ddlRecipe.SelectedValue = recipe;
                    ddlDifficulty.SelectedValue = difficulty;
                    ddlCookwareStock.SelectedIndex = ddlCompletedStock.SelectedIndex = 0;
                    SetPreview(imgCookwarePreview, Convert.ToString(reader["cookwareImage"]));
                    SetPreview(imgCompletedPreview, Convert.ToString(reader["completedImage"]));
                    SetEditorMode(editImages);
                    lblEditorHeading.Text = editImages ? "Edit images for simulation #" + id : "Edit simulation #" + id;
                }
            }
        }

        private bool ChangeSimulation(int id, string action)
        {
            using (SqlConnection cn = Database.Open())
            using (SqlTransaction tx =
                cn.BeginTransaction(IsolationLevel.Serializable))
            {
                CheckActivity("simulation", id, cn, tx);
                int recipeId;
                int attempts;
                bool published;
                string recipeStatus;
                string title;
                string instructions;

                const string detailsSql = @"SELECT s.recipeID, s.isActive, s.title, s.instructions, r.status AS recipeStatus,
                (SELECT COUNT(*) FROM dbo.simulationProgress p WHERE p.simulationID=s.simulationID) AS attemptCount FROM dbo.simulations s WITH (UPDLOCK, HOLDLOCK) INNER JOIN dbo.recipes r ON r.recipeID=s.recipeID WHERE s.simulationID=@id;";

                using (SqlCommand cmd = new SqlCommand(detailsSql, cn, tx))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            ShowMessage("Simulation not found.", false);
                            return false;
                        }

                        recipeId = Convert.ToInt32(reader["recipeID"]);
                        attempts = Convert.ToInt32(reader["attemptCount"]);
                        published = Convert.ToBoolean(reader["isActive"]);
                        recipeStatus = Convert.ToString(reader["recipeStatus"]);
                        title = Convert.ToString(reader["title"]);
                        instructions = Convert.ToString(reader["instructions"]);
                    }
                }

                string sql;
                string successMessage;

                if (action == "PublishSimulation")
                {
                    if (published)
                    {
                        ShowMessage("This simulation is already published.", false);
                        return false;
                    }

                    if (recipeStatus != "Approved" || String.IsNullOrWhiteSpace(title) || String.IsNullOrWhiteSpace(instructions))
                    {
                        ShowMessage("Publishing requires an approved recipe, " + "a simulation title and general instructions.", false);
                        return false;
                    }

                    if (!ValidSteps(cn, tx, id, recipeId))
                    {
                        return false;
                    }

                    sql = @"UPDATE dbo.simulations SET isActive=1 WHERE simulationID=@id;";

                    successMessage = "Simulation published.";
                }
                else if (action == "UnpublishSimulation")
                {
                    if (!published)
                    {
                        ShowMessage("This simulation is already unpublished.", false);
                        return false;
                    }

                    sql = @"UPDATE dbo.simulations SET isActive=0 WHERE simulationID=@id;";

                    successMessage =
                        "Simulation unpublished. Existing progress has been preserved.";
                }
                else if (action == "RemoveSimulation")
                {
                    if (published || attempts > 0)
                    {
                        ShowMessage("Only unpublished simulations without attempts " + "can be deleted.", false);
                        return false;
                    }

                    //remove the child records before the parent 
                    using (SqlCommand cmd = new SqlCommand(@" DELETE FROM dbo.simulationSteps WHERE simulationID=@id; DELETE FROM dbo.simulationStages WHERE simulationID=@id;", cn, tx))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        cmd.ExecuteNonQuery();
                    }

                    sql = @"DELETE FROM dbo.simulations WHERE simulationID=@id;";

                    successMessage = "Simulation draft and its steps deleted.";
                }
                else
                {
                    ShowMessage("Unsupported simulation action.", false);
                    return false;
                }

                using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        ShowMessage("No change made. Refresh the list and try again.", false);
                        return false;
                    }
                }

                tx.Commit();
                ShowMessage(successMessage, true);
                return true;
            }
        }

        private bool ValidSteps(
            SqlConnection cn,
            SqlTransaction tx,
            int simulationId,
            int recipeId)
        {
            string message;
            bool valid = SimulationStructure.IsValid(SimulationStructure.ReadSteps(cn, tx, simulationId), recipeId, out message);
            if (!valid) ShowMessage(message, false);
            return valid;
        }
        protected void btnNew_Click(object sender, EventArgs e)
        {
            if (!RequireEditor()) return;
            ResetEditor();
            BindRecipes();
        }

        private void ResetEditor()
        {
            EditingId = null;
            ImagesOnly = false;
            SetEditorMode(false);
            ddlCookwareStock.SelectedIndex = ddlCompletedStock.SelectedIndex = 0;
            txtTitle.Text = "";
            txtDescription.Text = "";
            txtInstructions.Text = "";
            chkRemoveCookware.Checked = false;
            chkRemoveCompleted.Checked = false;
            SetPreview(imgCookwarePreview, "");
            SetPreview(imgCompletedPreview, "");
            if (ddlRecipe.Items.Count > 0) ddlRecipe.SelectedIndex = 0;
            ddlDifficulty.SelectedIndex = 0;
            lblEditorHeading.Text = "New simulation";
        }

        private void SetEditorMode(bool imagesOnly)
        {
            ddlRecipe.Enabled = txtTitle.Enabled = txtDescription.Enabled =
                txtInstructions.Enabled = ddlDifficulty.Enabled = !imagesOnly;
            btnSave.Text = imagesOnly ? "Save images" : "Save draft";
        }

        private void SetPreview(System.Web.UI.WebControls.Image image, string value)
        {
            string url = "";
            if (!String.IsNullOrWhiteSpace(value))
            {
                try
                {
                    string path = Uri.UnescapeDataString(value.Trim());
                    if (path.StartsWith("/Content/images/", StringComparison.OrdinalIgnoreCase))
                        path = "~" + path;
                    if (path.StartsWith("~/Content/images/", StringComparison.OrdinalIgnoreCase)
                        && !path.Contains("..") && !path.Contains("\\")
                        && File.Exists(Server.MapPath(path))) url = ResolveUrl(path);
                }
                catch (ArgumentException) { }
                catch (HttpException) { }
                catch (UriFormatException) { }
            }
            image.ImageUrl = url.Length == 0 ? Database.DefaultAvatar : url;
            image.Style["display"] = url.Length == 0 ? "none" : "block";
        }

        protected void gvSimulations_PageIndexChanging(
            object sender, GridViewPageEventArgs e)
        {
            if (!RequireEditor()) return;
            gvSimulations.PageIndex = e.NewPageIndex;
            BindSimulations();
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-warning d-block";
        }
    }
}
