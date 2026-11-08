<%@ Page Title="Simulation Stages" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="SimulationStages.aspx.cs" Inherits="Masaking.Admin.SimulationStages" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="container-fluid py-3 px-0">
        <asp:HyperLink ID="lnkSteps" runat="server" Text="Back to simulation steps" CssClass="btn btn-outline-success mb-3" />
        <asp:Label ID="lblHeading" runat="server" Text="Simulation stages" CssClass="h1 d-block mb-3" />
        <asp:Label ID="lblStatus" runat="server" CssClass="d-block text-secondary mb-3" />
        <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />
        <asp:Panel ID="pnlStageEditor" runat="server" CssClass="card p-3 p-md-4 mb-4">
            <h2 class="h5">Stage settings</h2>
            <div class="row g-3">
                <div class="col-12 col-md-3">
                    <asp:Label runat="server" AssociatedControlID="txtStageNumber" Text="Stage number" CssClass="form-label" />
                    <asp:TextBox ID="txtStageNumber" runat="server" TextMode="Number" min="1" max="20" Text="1" CssClass="form-control" />
                </div>
                <div class="col-12 col-md-9">
                    <asp:Label runat="server" AssociatedControlID="txtStageTitle" Text="Stage title" CssClass="form-label" />
                    <asp:TextBox ID="txtStageTitle" runat="server" MaxLength="100" CssClass="form-control" />
                </div>
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtStageInstructions" Text="Stage introduction" CssClass="form-label" />
                    <asp:TextBox ID="txtStageInstructions" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                </div>
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="ddlStageStock" Text="Stock cooking-area picture" CssClass="form-label" />
                    <asp:DropDownList ID="ddlStageStock" runat="server" EnableViewState="False" CssClass="form-select mb-3" AutoPostBack="True" OnSelectedIndexChanged="ddlStageStock_SelectedIndexChanged" />
                    <asp:Image ID="imgStagePreview" runat="server" ClientIDMode="Static" AlternateText="Stage cooking-area preview" CssClass="img-fluid rounded border mb-3" Style="display:none;max-height:180px;object-fit:contain;" />
                    <asp:Label runat="server" AssociatedControlID="uploadStageImage" Text="Or upload your own picture" CssClass="form-label d-block" />
                    <asp:FileUpload ID="uploadStageImage" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif" data-simulation-preview="imgStagePreview" data-simulation-error="stageImageError" />
                    <p class="form-text">Choose the stock picture before choosing a file. An upload takes priority. JPG, PNG or GIF, up to 5 MB. Leave both empty to keep the current picture; a stage without its own picture uses the simulation's main cooking-area picture.</p>
                    <div id="stageImageError" class="small text-danger" role="status"></div>
                    <asp:CheckBox ID="chkRemoveStageImage" runat="server" Text=" Remove this stage's picture" />
                </div>
                <div class="col-12 d-flex flex-wrap gap-2">
                    <asp:Button ID="btnSaveStage" runat="server" Text="Save stage" CssClass="btn btn-success" OnClick="btnSaveStage_Click" />
                    <asp:Button ID="btnClearStage" runat="server" Text="New stage" CssClass="btn btn-outline-secondary" CausesValidation="False" OnClick="btnClearStage_Click" />
                </div>
            </div>
        </asp:Panel>
        <div class="card p-3 p-md-4">
            <h2 class="h5">Existing stages</h2>
            <p class="small text-secondary">A simulation can keep its original single cooking area, or use up to 20 stages. Once stages are added, assign every step to a stage. Keep the step numbers continuous across stages.</p>
            <div class="table-responsive">
                <asp:GridView ID="gvStages" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="table align-middle" EmptyDataText="No stages yet. This simulation uses its main cooking area." OnRowCommand="gvStages_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="stageNumber" HeaderText="Stage" />
                        <asp:BoundField DataField="title" HeaderText="Title" />
                        <asp:BoundField DataField="instructions" HeaderText="Introduction" />
                        <asp:BoundField DataField="stepCount" HeaderText="Steps" />
                        <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                            <div class="d-flex flex-wrap gap-2">
                                <asp:LinkButton ID="btnEditStage" runat="server" Text="Edit" CommandName="EditStage" CommandArgument='<%# Eval("simulationStageID") %>' CssClass="btn btn-sm btn-outline-success" CausesValidation="False" />
                                <asp:LinkButton ID="btnRemoveStage" runat="server" Text="Delete" CommandName="RemoveStage" CommandArgument='<%# Eval("simulationStageID") %>' CssClass="btn btn-sm btn-outline-danger" CausesValidation="False" OnClientClick="return confirm('Delete this stage? Move its steps first.');" />
                            </div>
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </main>
    <script src="<%= ResolveUrl("~/Scripts/simulation-upload.js") %>"></script>
</asp:Content>
