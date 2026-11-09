<%@ Page Title="Manage Recipes | Masaking" Language="C#" MasterPageFile="~/Admin/Admin.Master" MaintainScrollPositionOnPostBack="True" AutoEventWireup="true" CodeBehind="ManageRecipes.aspx.cs" Inherits="Masaking.Admin.ManageRecipes" %>
<%@ Import Namespace="Masaking.Infrastructure" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <p class="text-brand fw-semibold">ADMINISTRATION</p>
    <h1 class="page-heading">Manage recipes</h1>
    <p class="text-secondary">Search submissions and review recipes awaiting approval.</p>

    <asp:Panel ID="pnlDetails" runat="server" CssClass="card p-3 p-md-4 mb-4" Visible="False">
        <div class="d-flex flex-wrap justify-content-between gap-2 mb-3">
            <asp:Label ID="lblDetailsHeading" runat="server" CssClass="h5 mb-0" Text="Recipe submission" />
            <asp:Button ID="btnCloseDetails" runat="server" CausesValidation="False" CssClass="btn btn-outline-secondary" Text="Close details" OnClick="btnCloseDetails_Click" />
        </div>
        <asp:Image ID="imgRecipeDetails" runat="server" Visible="False" AlternateText="Recipe photo" CssClass="recipe-detail-photo mb-3" />
        <asp:Panel ID="pnlDetailsImagePlaceholder" runat="server" Visible="False" CssClass="recipe-detail-placeholder text-secondary mb-3">
            <i class="bi bi-image me-2" aria-hidden="true"></i>No recipe photo
        </asp:Panel>
        <div class="table-responsive">
            <asp:DetailsView ID="dvRecipe" runat="server" AutoGenerateRows="False" CssClass="table" GridLines="None">
                <Fields>
                    <asp:BoundField DataField="recipeID" HeaderText="Recipe ID" />
                    <asp:BoundField DataField="title" HeaderText="Recipe" />
                    <asp:BoundField DataField="description" HeaderText="Description" />
                    <asp:BoundField DataField="displayName" HeaderText="Submitted by" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><span class='<%# AdminTable.StatusBadge(Eval("status")) %>'><%#: Eval("status") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="cuisine" HeaderText="Cuisine" />
                    <asp:BoundField DataField="region" HeaderText="Region" />
                    <asp:TemplateField HeaderText="Difficulty">
                        <ItemTemplate><span class='<%# AdminTable.DifficultyBadge(Eval("difficulty")) %>'><%#: Eval("difficulty") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="preparationTime" HeaderText="Preparation time (minutes)" />
                    <asp:BoundField DataField="estimatedCost" DataFormatString="{0:0.00}" HeaderText="Estimated cost (RM)" />
                    <asp:BoundField DataField="servings" HeaderText="Servings" />
                </Fields>
            </asp:DetailsView>
        </div>
        <asp:Label ID="lblIngredientsHeading" runat="server" CssClass="h6 d-block mt-3" Text="Ingredients" />
        <div class="table-responsive">
            <asp:GridView ID="gvIngredients" runat="server" AutoGenerateColumns="False" AllowSorting="True" OnSorting="gvIngredients_Sorting" CssClass="table" EmptyDataText="No ingredients recorded." GridLines="None">
                <Columns>
                    <asp:BoundField DataField="ingredientName" HeaderText="Ingredient" SortExpression="ingredientName" />
                    <asp:BoundField DataField="quantity" HeaderText="Quantity" SortExpression="quantity" />
                    <asp:BoundField DataField="unit" HeaderText="Unit" SortExpression="unit" />
                </Columns>
            </asp:GridView>
        </div>
        <asp:Label ID="lblStepsHeading" runat="server" CssClass="h6 d-block mt-3" Text="Cooking steps" />
        <div class="table-responsive">
            <asp:GridView ID="gvSteps" runat="server" AutoGenerateColumns="False" AllowSorting="True" OnSorting="gvSteps_Sorting" CssClass="table" EmptyDataText="No cooking steps recorded." GridLines="None">
                <Columns>
                    <asp:BoundField DataField="stepNumber" HeaderText="Step" SortExpression="stepNumber" />
                    <asp:BoundField DataField="instruction" HeaderText="Instruction" SortExpression="instruction" />
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlFilters" runat="server" DefaultButton="btnSearch" CssClass="admin-summary-card mb-4">
        <div class="row g-3">
            <div class="col-12 col-md-8">
                <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch" Text="Recipe title" CssClass="form-label" />
                <asp:TextBox ID="txtSearch" runat="server" ClientIDMode="Static" MaxLength="150" CssClass="form-control" data-auto-filter="true" data-filter-target="btnSearch" />
            </div>
            <div class="col-12 col-md-4">
                <asp:Label ID="lblStatus" runat="server" AssociatedControlID="ddlStatus" Text="Status" CssClass="form-label" />
                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="Filters_Changed">
                    <asp:ListItem Value="" Selected="True">All statuses</asp:ListItem>
                    <asp:ListItem Value="Pending">Pending</asp:ListItem>
                    <asp:ListItem Value="Approved">Approved</asp:ListItem>
                    <asp:ListItem Value="Rejected">Rejected</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <p class="small text-secondary mt-3 mb-0">Search updates after a short pause. Filters apply to both recipe tables. Select a column heading to sort.</p>
        <asp:Button ID="btnSearch" runat="server" ClientIDMode="Static" Text="Apply filters" Style="display:none;" UseSubmitBehavior="False" CausesValidation="False" OnClick="btnSearch_Click" />
    </asp:Panel>

    <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />
    <asp:Label ID="lblRecipeCount" runat="server" CssClass="small text-secondary d-block mb-3" />

    <asp:Panel ID="pnlRecipes" runat="server" CssClass="admin-summary-card">
        <h2 class="h5">Recipe review</h2>
        <p class="small text-secondary">Approve and reject are available only for pending submissions.</p>
        <div class="table-responsive">
            <asp:GridView ID="gvRecipes" runat="server" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" AllowSorting="True" CssClass="table align-middle" GridLines="None" EmptyDataText="No recipes match your filters." OnRowCommand="gvRecipes_RowCommand" OnPageIndexChanging="gvRecipes_PageIndexChanging" OnSorting="gvRecipes_Sorting">
                <Columns>
                    <asp:TemplateField HeaderText="Photo">
                        <ItemTemplate>
                            <asp:Image ID="imgRecipe" runat="server" ImageUrl='<%# Eval("imageUrl") %>' AlternateText='<%# Eval("title") %>' CssClass="recipe-thumb" Visible='<%# !String.IsNullOrEmpty(Convert.ToString(Eval("imageUrl"))) %>' />
                            <asp:Panel ID="pnlNoPhoto" runat="server" CssClass="recipe-placeholder" Visible='<%# String.IsNullOrEmpty(Convert.ToString(Eval("imageUrl"))) %>' role="img" aria-label="No recipe photo">
                                <i class="bi bi-image" aria-hidden="true"></i>
                            </asp:Panel>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="recipeID" HeaderText="ID" SortExpression="recipeID" />
                    <asp:BoundField DataField="title" HeaderText="Recipe" SortExpression="title" />
                    <asp:BoundField DataField="displayName" HeaderText="Submitted by" SortExpression="displayName" />
                    <asp:TemplateField HeaderText="Difficulty" SortExpression="difficultyOrder">
                        <ItemTemplate><span class='<%# AdminTable.DifficultyBadge(Eval("difficulty")) %>'><%#: Eval("difficulty") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status" SortExpression="status">
                        <ItemTemplate><span class='<%# AdminTable.StatusBadge(Eval("status")) %>'><%#: Eval("status") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="submittedLocal" HeaderText="Submitted (MYT)" SortExpression="createdAt" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                    <asp:TemplateField HeaderText="Review">
                        <ItemTemplate>
                            <div class="d-flex flex-wrap gap-2">
                                <asp:LinkButton ID="btnViewRecipe" runat="server" CausesValidation="False" CommandName="ViewRecipe" CssClass="btn btn-sm btn-outline-success" CommandArgument='<%# Eval("recipeID") %>' Text="View" />
                                <asp:LinkButton ID="btnApprove" runat="server" Text="Approve" CausesValidation="False" CssClass="btn btn-sm btn-success" CommandName="ApproveRecipe" CommandArgument='<%# Eval("recipeID") %>' Visible='<%# Convert.ToString(Eval("status")) == "Pending" %>' OnClientClick="return confirm('Approve this recipe for publication?');" />
                                <asp:LinkButton ID="btnReject" runat="server" Text="Reject" CausesValidation="False" CssClass="btn btn-sm btn-outline-danger" CommandName="RejectRecipe" CommandArgument='<%# Eval("recipeID") %>' Visible='<%# Convert.ToString(Eval("status")) == "Pending" %>' OnClientClick="return confirm('Reject this recipe submission?');" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnlAllRecipes" runat="server" CssClass="card p-3 p-md-4 mt-4">
        <asp:Label ID="lblAllRecipes" runat="server" Text="All recipes" CssClass="h5 d-block mb-3" />
        <asp:Label ID="lblDeleteNotice" runat="server" CssClass="text-secondary d-block mb-3" Text="Permanent deletion is available only for recipes without community or learning records." />
        <div class="table-responsive">
            <asp:GridView ID="gvAllRecipes" runat="server" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" AllowSorting="True" CssClass="table align-middle" GridLines="None" EmptyDataText="No recipes match your filters." OnPageIndexChanging="gvAllRecipes_PageIndexChanging" OnRowCommand="gvAllRecipes_RowCommand" OnSorting="gvAllRecipes_Sorting">
                <Columns>
                    <asp:TemplateField HeaderText="Photo">
                        <ItemTemplate>
                            <asp:Image ID="imgRecipe" runat="server" ImageUrl='<%# Eval("imageUrl") %>' AlternateText='<%# Eval("title") %>' CssClass="recipe-thumb" Visible='<%# !String.IsNullOrEmpty(Convert.ToString(Eval("imageUrl"))) %>' />
                            <asp:Panel ID="pnlNoPhoto" runat="server" CssClass="recipe-placeholder" Visible='<%# String.IsNullOrEmpty(Convert.ToString(Eval("imageUrl"))) %>' role="img" aria-label="No recipe photo">
                                <i class="bi bi-image" aria-hidden="true"></i>
                            </asp:Panel>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="recipeID" HeaderText="ID" SortExpression="recipeID" />
                    <asp:BoundField DataField="title" HeaderText="Recipe" SortExpression="title" />
                    <asp:BoundField DataField="displayName" HeaderText="Submitted by" SortExpression="displayName" />
                    <asp:TemplateField HeaderText="Difficulty" SortExpression="difficultyOrder">
                        <ItemTemplate><span class='<%# AdminTable.DifficultyBadge(Eval("difficulty")) %>'><%#: Eval("difficulty") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status" SortExpression="status">
                        <ItemTemplate><span class='<%# AdminTable.StatusBadge(Eval("status")) %>'><%#: Eval("status") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="submittedLocal" HeaderText="Submitted (MYT)" SortExpression="createdAt" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="d-flex flex-wrap gap-2">
                                <asp:LinkButton ID="btnViewRecipe" runat="server" Text="View" CausesValidation="False" CommandName="ViewRecipe" CommandArgument='<%# Eval("recipeID") %>' CssClass="btn btn-sm btn-outline-success" />
                                <asp:LinkButton ID="btnDeleteRecipe" runat="server" Text="Delete" CausesValidation="False" CommandName="RemoveRecipe" CommandArgument='<%# Eval("recipeID") %>' CssClass="btn btn-sm btn-outline-danger" OnClientClick="return confirm('Permanently delete this recipe and its ingredients, steps and category links?');" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>
    <script src="<%= ResolveUrl("~/Scripts/admin-tables.js") %>"></script>
</asp:Content>