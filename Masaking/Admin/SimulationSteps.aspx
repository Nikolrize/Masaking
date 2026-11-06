<%@ Page Title="Simulation Steps" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="SimulationSteps.aspx.cs" Inherits="Masaking.Admin.SimulationSteps" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/Admin/ManageSimulations.aspx" Text="Back to simulations" CssClass="btn btn-outline-success mb-3" />

    <asp:Label ID="lblHeading" runat="server" Text="Simulation steps" CssClass="h1 d-block mb-3" />
    <asp:Label ID="lblMessage" runat="server" EnableViewState="False" CssClass="d-block" role="status" />
    <asp:Panel ID="pnlContent" runat="server" Visible="False">
        <asp:Label ID="lblStatus" runat="server" CssClass="d-block text-secondary mb-3" />

        <asp:Panel ID="pnlEditor" runat="server" CssClass="card p-3 p-md-4 mb-4">

            <asp:Label ID="lblEditorHeading" runat="server" Text="New step" CssClass="h5 d-block mb-3" />
            <div class="row g-3">
                <div class="col-12 col-md-4">
                    <asp:Label ID="lblNumber" runat="server" Text="Step number" AssociatedControlID="txtNumber" CssClass="form-label" />

                    <asp:TextBox ID="txtNumber" runat="server" TextMode="Number" Text="1" min="1" max="100" step="1" CssClass="form-control" />
                </div>

                <div class="col-12 col-md-8">
                    <asp:Label ID="lblIngredient" runat="server" Text="Ingredient" AssociatedControlID="ddlIngredient" CssClass="form-label" />

                    <asp:DropDownList ID="ddlIngredient" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="ddlIngredient_SelectedIndexChanged" />
                </div>

                <div class="col-12">
                    <asp:Label ID="lblInstruction" runat="server" Text="Instruction" AssociatedControlID="txtInstruction" CssClass="form-label" />

                    <asp:TextBox ID="txtInstruction" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                </div>

                <div class="col-12">
                    <asp:Label ID="lblImage" runat="server" Text="Ingredient photo (optional)"
                        AssociatedControlID="uploadStepImage" CssClass="form-label" />
                    <asp:Image ID="imgStepPreview" runat="server" ClientIDMode="Static"
                        AlternateText="Ingredient photo preview" CssClass="img-fluid rounded border mb-2"
                        Style="display:none;max-height:180px;object-fit:contain;" />
                    <asp:FileUpload ID="uploadStepImage" runat="server" CssClass="form-control"
                        accept="image/jpeg,image/png,image/gif"
                        data-simulation-preview="imgStepPreview"
                        data-simulation-error="stepImageError" />
                    <p class="form-text">
                        JPG, PNG or GIF, up to 5 MB. Leave empty to keep the current step image.
                        New steps use the ingredient's default image unless you upload a replacement. Select the ingredient before choosing a photo.
                    </p>
                    <div id="stepImageError" class="small text-danger" role="status"></div>
                    <asp:Label ID="lblImageSource" runat="server"
                        CssClass="small text-secondary d-block mb-2" />
                    <asp:CheckBox ID="chkUseDefaultImage" runat="server"
                        Text=" Use ingredient default image instead of the step override (applies when saved)" />
                </div>

                <div class="col-12">
                    <p class="small text-secondary mb-0"> Number the steps consecutively: 1, 2, 3 and so on. After deleting a step, edit any remaining numbers to remove gaps.
                    </p>
                </div>

                <div class="col-12 d-flex flex-wrap gap-2">
                    <asp:Button ID="btnSave" runat="server" Text="Save step" CssClass="btn btn-success" OnClick="btnSave_Click" />

                    <asp:Button ID="btnClear" runat="server" Text="Clear" CausesValidation="False" CssClass="btn btn-outline-secondary" OnClick="btnClear_Click" />
                </div>

            </div>
        </asp:Panel>

        <div class="card p-3 p-md-4">
            <h2 class="h5">Existing steps</h2>

            <div class="table-responsive">
                <asp:GridView ID="gvSteps" runat="server" AutoGenerateColumns="False" CssClass="table align-middle" GridLines="None" EmptyDataText="No steps added yet." OnRowCommand="gvSteps_RowCommand">

                    <Columns>
                        <asp:BoundField DataField="stepNumber" HeaderText="Step" />
                        <asp:BoundField DataField="ingredientName" HeaderText="Ingredient" />
                        <asp:BoundField DataField="instruction" HeaderText="Instruction" />
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="d-flex flex-wrap gap-2">
                                    <asp:LinkButton ID="btnEditStep" runat="server" Text="Edit" CausesValidation="False" CssClass="btn btn-sm btn-outline-success" Visible='<%# Convert.ToBoolean(Eval("canEdit")) %>' CommandName="EditStep" CommandArgument='<%# Eval("simulationStepID") %>' />


                                    <asp:LinkButton ID="btnEditStepImage" runat="server" Text="Edit image"
                                        CausesValidation="False" CssClass="btn btn-sm btn-outline-success"
                                        CommandName="EditStepImage" CommandArgument='<%# Eval("simulationStepID") %>' />
                                    <asp:LinkButton ID="btnDeleteStep" runat="server" Text="Delete" CausesValidation="False" CssClass="btn btn-sm btn-outline-danger" Visible='<%# Convert.ToBoolean(Eval("canEdit")) %>' CommandName="RemoveStep" CommandArgument='<%# Eval("simulationStepID") %>' OnClientClick="return confirm('Delete this simulation step?');" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </asp:Panel>
    <script src="<%= ResolveUrl("~/Scripts/simulation-upload.js") %>"></script>
</asp:Content>