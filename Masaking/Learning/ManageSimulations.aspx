<%@ Page Title="Manage Simulations" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Inherits="Masaking.Admin.ManageSimulations" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/css/activity-editor.css" rel="stylesheet" />
    <section class="activity-editor">
    <asp:HyperLink ID="lnkMyRecipes" runat="server" NavigateUrl="~/Recipes/MyRecipes.aspx" Text="Back to my recipes" CssClass="btn btn-outline-success mb-3" />

    <p class="text-brand fw-semibold">YOUR LEARNING ACTIVITIES</p>
    <h1 class="page-heading">Manage simulations</h1>
    <p class="text-secondary">Create a draft, configure its ingredient steps, then publish. Simulations have no time limit.</p>

    <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />

    <div class="card p-3 p-md-4 mb-4">
        <asp:Label ID="lblEditorHeading" runat="server" Text="New simulation" CssClass="h5 d-block mb-3" />
        <div class="row g-3">
            <div class="col-12">
                <asp:Label ID="lblRecipe" runat="server" Text="Approved recipe" AssociatedControlID="ddlRecipe" CssClass="form-label" />

                <asp:DropDownList ID="ddlRecipe" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12 col-md-8">
                <asp:Label ID="lblTitle" runat="server" Text="Simulation title" AssociatedControlID="txtTitle" CssClass="form-label" />

                <asp:TextBox ID="txtTitle" runat="server" MaxLength="150" CssClass="form-control" />
            </div>

            <div class="col-12 col-md-4">
                <asp:Label ID="lblDifficulty" runat="server" Text="Difficulty" AssociatedControlID="ddlDifficulty" CssClass="form-label" />

                <asp:DropDownList ID="ddlDifficulty" runat="server" CssClass="form-select">
                    <asp:ListItem Value="Easy">Easy</asp:ListItem>
                    <asp:ListItem Value="Medium">Medium</asp:ListItem>
                    <asp:ListItem Value="Hard">Hard</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="col-12">
                <asp:Label ID="lblDescription" runat="server" Text="Description" AssociatedControlID="txtDescription" CssClass="form-label" />

                <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>

            <div class="col-12">
                <asp:Label ID="lblInstructions" runat="server" Text="General instructions" AssociatedControlID="txtInstructions" CssClass="form-label" />

                <asp:TextBox ID="txtInstructions" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>

            <div class="col-12 col-md-6">
                <asp:Label ID="lblCookware" runat="server" Text="Cookware photo"
                    AssociatedControlID="uploadCookware" CssClass="form-label" />
                <asp:Image ID="imgCookwarePreview" runat="server" ClientIDMode="Static"
                    AlternateText="Cookware photo preview" CssClass="img-fluid rounded border mb-2"
                    Style="display:none;max-height:180px;object-fit:contain;" />
                <asp:FileUpload ID="uploadCookware" runat="server" CssClass="form-control"
                    accept="image/jpeg,image/png,image/gif"
                    data-simulation-preview="imgCookwarePreview"
                    data-simulation-error="cookwareImageError" />
                <p class="form-text">JPG, PNG or GIF. Up to 5 MB. PNG transparency is preserved.</p>
                <div id="cookwareImageError" class="small text-danger" role="status"></div>
                <asp:CheckBox ID="chkRemoveCookware" runat="server"
                    Text=" Remove current cookware photo" />
            </div>

            <div class="col-12 col-md-6">
                <asp:Label ID="lblCompletedImage" runat="server" Text="Finished-dish photo"
                    AssociatedControlID="uploadCompletedImage" CssClass="form-label" />
                <asp:Image ID="imgCompletedPreview" runat="server" ClientIDMode="Static"
                    AlternateText="Finished-dish photo preview" CssClass="img-fluid rounded border mb-2"
                    Style="display:none;max-height:180px;object-fit:contain;" />
                <asp:FileUpload ID="uploadCompletedImage" runat="server" CssClass="form-control"
                    accept="image/jpeg,image/png,image/gif"
                    data-simulation-preview="imgCompletedPreview"
                    data-simulation-error="completedImageError" />
                <p class="form-text">JPG, PNG or GIF. Up to 5 MB. Leave blank to keep the current photo.</p>
                <div id="completedImageError" class="small text-danger" role="status"></div>
                <asp:CheckBox ID="chkRemoveCompleted" runat="server"
                    Text=" Remove current finished-dish photo" />
            </div>

            <div class="col-12">
                <p class="small text-secondary mb-0"> Photos are optional. Choose them from your device. Leave a file input empty to keep its current image. If no finished-dish photo is set, gameplay uses the linked recipe photo.
                </p>
            </div>

            <div class="col-12 d-flex flex-wrap gap-2">
                <asp:Button ID="btnSave" runat="server" Text="Save draft" CssClass="btn btn-success" OnClick="btnSave_Click" />

                <asp:Button ID="btnNew" runat="server" Text="New simulation" CausesValidation="False" CssClass="btn btn-outline-secondary" OnClick="btnNew_Click" />
            </div>
        </div>
    </div>

    <div class="card p-3 p-md-4">
        <h2 class="h5">Existing simulations</h2>

        <div class="table-responsive">
            <asp:GridView ID="gvSimulations" runat="server" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" CssClass="table align-middle" GridLines="None" EmptyDataText="No simulations created yet." OnRowCommand="gvSimulations_RowCommand" OnPageIndexChanging="gvSimulations_PageIndexChanging">

                <Columns>
                    <asp:BoundField DataField="simulationID" HeaderText="ID" />
                    <asp:BoundField DataField="title" HeaderText="Simulation" />
                    <asp:BoundField DataField="recipeTitle" HeaderText="Recipe" />
                    <asp:BoundField DataField="difficulty" HeaderText="Difficulty" />
                    <asp:CheckBoxField DataField="isActive" HeaderText="Published" />
                    <asp:BoundField DataField="stepCount" HeaderText="Steps" />
                    <asp:BoundField DataField="attemptCount" HeaderText="Attempts" />

                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="d-flex flex-wrap gap-2">

                                <asp:LinkButton ID="btnEditSimulation" runat="server" Text="Edit settings" CausesValidation="False" CssClass="btn btn-sm btn-outline-success" CommandName="EditSimulation" CommandArgument='<%# Eval("simulationID") %>' Visible='<%# !Convert.ToBoolean(Eval("isActive")) && Convert.ToInt32(Eval("attemptCount")) == 0 %>' />


                                <asp:LinkButton ID="btnEditImages" runat="server"
                                    Text="Edit images" CausesValidation="False"
                                    CssClass="btn btn-sm btn-outline-success"
                                    CommandName="EditSimulationImages"
                                    CommandArgument='<%# Eval("simulationID") %>'
                                    Visible='<%# !Convert.ToBoolean(Eval("isActive")) %>' />
                                <asp:HyperLink ID="lnkSteps" runat="server" Text="Manage steps" CssClass="btn btn-sm btn-outline-success" NavigateUrl='<%# "~/Learning/SimulationSteps.aspx?id=" + Eval("simulationID") %>' />

                                <asp:LinkButton ID="btnPublishSimulation" runat="server" Text="Publish" CausesValidation="False" CssClass="btn btn-sm btn-success" CommandName="PublishSimulation" CommandArgument='<%# Eval("simulationID") %>' Visible='<%# !Convert.ToBoolean(Eval("isActive")) %>' OnClientClick="return confirm('Publish this simulation?');" />

                                <asp:LinkButton ID="btnUnpublishSimulation" runat="server" Text="Unpublish" CausesValidation="False" CssClass="btn btn-sm btn-outline-secondary" CommandName="UnpublishSimulation" CommandArgument='<%# Eval("simulationID") %>' Visible='<%# Convert.ToBoolean(Eval("isActive")) %>' OnClientClick="return confirm('Unpublish this simulation?');" />

                                <asp:LinkButton ID="btnDeleteSimulation" runat="server" Text="Delete" CausesValidation="False" CssClass="btn btn-sm btn-outline-danger" CommandName="RemoveSimulation" CommandArgument='<%# Eval("simulationID") %>' Visible='<%# !Convert.ToBoolean(Eval("isActive")) && Convert.ToInt32(Eval("attemptCount")) == 0 %>' OnClientClick="return confirm('Permanently delete this draft and its steps?');" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>
    <script src="<%= ResolveUrl("~/Scripts/simulation-upload.js") %>"></script>
    </section>
</asp:Content>
