<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="ManageRecipes.aspx.cs"
    Inherits="Masaking.Admin.ManageRecipes" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Manage Recipes | Masaking</title>

    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
    <link href="../Content/css/admin.css" rel="stylesheet" />
</head>
<body class="masaking-page">
    <form id="form1" runat="server">
        <main class="container py-5">

            <asp:HyperLink ID="lnkDashboard" runat="server" NavigateUrl="~/Admin/Dashboard.aspx" CssClass="btn btn-outline-success mb-3" Text="Back to Dashboard" />

            <h1>Manage Recipes</h1>
            <p class="text-secondary"> Search submissions and review recipes awaiting approval. </p>
        <asp:Panel ID="pnlDetails" runat="server" CssClass="card p-3 p-md-4 mb-4" Visible="False">
            <asp:Label ID="lblDetailsHeading" runat="server" CssClass="h5 d-block mb-3" Text="Recipe submission"></asp:Label>
            <asp:Button ID="btnCloseDetails" runat="server" CausesValidation="False" CssClass="btn btn-outline-secondary mb-3" Text="Close details" OnClick="btnCloseDetails_Click" />
            <br />
            <div class="table-responsive"> 
                <asp:DetailsView ID="dvRecipe" runat="server" AutoGenerateRows="False" CssClass="table" GridLines="None">
                    <Fields>
                        <asp:BoundField DataField="recipeID" HeaderText="Recipe ID" />
                        <asp:BoundField DataField="title" HeaderText="Recipe" />
                        <asp:BoundField DataField="description" HeaderText="Description" />
                        <asp:BoundField DataField="displayName" HeaderText="Submitted by" />
                        <asp:BoundField HeaderText="Status" DataField="status" />
                        <asp:BoundField DataField="cuisine" HeaderText="Cuisine" />
                        <asp:BoundField DataField="region" HeaderText="Region" />
                        <asp:BoundField DataField="difficulty" HeaderText="Difficulty" />
                        <asp:BoundField DataField="preparationTime" HeaderText="	Preparation time (minutes)" />
                        <asp:BoundField DataField="estimatedCost" DataFormatString="{0:0.00}" HeaderText="Estimated cost (RM)" />
                        <asp:BoundField DataField="servings" HeaderText="Servings" />
                    </Fields>
                </asp:DetailsView>
            </div>
            <asp:Label ID="lblIngredientsHeading" runat="server" CssClass="h6 d-block mt-3" Text="Ingredients"></asp:Label>
            <div class="table-responsive">
                <asp:GridView ID="gvIngredients" runat="server" AutoGenerateColumns="False" CssClass="table" EmptyDataText="No ingredients recorded." GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="ingredientName" HeaderText="Ingredient" />
                        <asp:BoundField DataField="quantity" HeaderText="Quantity" />
                        <asp:BoundField DataField="unit" HeaderText="Unit" />
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Label ID="lblStepsHeading" runat="server" CssClass="h6 d-block mt-3" Text="Cooking steps"></asp:Label>
            <div class="table-responsive">
                <asp:GridView ID="gvSteps" runat="server" AutoGenerateColumns="False" CssClass="table" EmptyDataText="No cooking steps recorded." GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="stepNumber" HeaderText="Step" />
                        <asp:BoundField DataField="instruction" HeaderText="Instruction" />
                    </Columns>
                </asp:GridView>
            </div>
            <br />
        </asp:Panel>

            <asp:Panel ID="pnlFilters" runat="server" CssClass="admin-summary-card mb-4">

                <div class="row g-3 align-items-end">
                    <div class="col-md-6">
                        <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch" Text="Recipe title" CssClass="form-label" />

                        <asp:TextBox ID="txtSearch" runat="server" MaxLength="150" CssClass="form-control" />
                    </div>

                    <div class="col-md-3">
                        <asp:Label ID="lblStatus" runat="server" AssociatedControlID="ddlStatus" Text="Status" CssClass="form-label" />

                        <asp:DropDownList ID="ddlStatus" runat="server"
                            CssClass="form-select">
                            <asp:ListItem Value="Pending">Pending</asp:ListItem>
                            <asp:ListItem Value="Approved">Approved</asp:ListItem>
                            <asp:ListItem Value="Rejected">Rejected</asp:ListItem>
                            <asp:ListItem Value="">All statuses</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-3">
                        <asp:Button ID="btnSearch" runat="server" Text="Apply filters" CssClass="btn btn-success w-100" OnClick="btnSearch_Click" />
                    </div>
                </div>
            </asp:Panel>

            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" role="status" />

            <asp:Panel ID="pnlRecipes" runat="server" CssClass="admin-summary-card">

                <div class="table-responsive">
                    <asp:GridView ID="gvRecipes" runat="server" AutoGenerateColumns="False" CssClass="table align-middle" GridLines="None" EmptyDataText="No recipes match your filters." OnRowCommand="gvRecipes_RowCommand">

                        <Columns>
                            <asp:BoundField DataField="recipeID" HeaderText="ID" />
                            <asp:BoundField DataField="title" HeaderText="Recipe" />
                            <asp:BoundField DataField="displayName" HeaderText="Submitted by" />
                            <asp:BoundField DataField="difficulty" HeaderText="Difficulty" />
                            <asp:BoundField DataField="status" HeaderText="Status" />
                            <asp:BoundField DataField="createdAt" HeaderText="Submitted" DataFormatString="{0:dd MMM yyyy}" />

                            <asp:TemplateField HeaderText="Review">
                                <ItemTemplate>
                                    <div class="d-flex flex-wrap gap-2">
                                        <asp:LinkButton ID="btnViewRecipe" runat="server" CausesValidation="False" CommandName="ViewRecipe" CssClass="btn btn-sm btn-outline-success" CommandArgument='<%# Eval("recipeID") %>'>View</asp:LinkButton>
                                        <asp:LinkButton ID="btnApprove" runat="server" Text="Approve" CssClass="btn btn-sm btn-success" CommandName="ApproveRecipe" CommandArgument='<%# Eval("recipeID") %>' Visible='<%# Convert.ToString(Eval("status")) == "Pending" %>' OnClientClick="return confirm('Approve this recipe for publication?');" />

                                        <asp:LinkButton ID="btnReject" runat="server" Text="Reject" CssClass="btn btn-sm btn-outline-danger" CommandName="RejectRecipe" CommandArgument='<%# Eval("recipeID") %>' Visible='<%# Convert.ToString(Eval("status")) == "Pending" %>' OnClientClick="return confirm('Reject this recipe submission?');"/>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </asp:Panel>
        <asp:Panel ID="pnlAllRecipes" runat="server" CssClass="card p-3 p-md-4 mt-4">
            <asp:Label ID="lblAllRecipes" runat="server" Text="All recipes" CssClass="h5 d-block mb-3"></asp:Label>
            <asp:Label ID="lblDeleteNotice" runat="server" CssClass="text-secondary d-block mb-3" Text="Permanent deletion is available only for recipes without community or learning records."></asp:Label>
            <div class="table-responsive"> 
                <asp:GridView ID="gvAllRecipes" runat="server" AllowPaging="True" PageSize="10" AutoGenerateColumns="False" CssClass="table align-middle" EmptyDataText="No recipes found." GridLines="None"  OnPageIndexChanging="gvAllRecipes_PageIndexChanging" OnRowCommand="gvAllRecipes_RowCommand">
                <Columns>
                    <asp:BoundField DataField="recipeID" HeaderText="ID" />
                    <asp:BoundField DataField="title" HeaderText="Recipe" />
                    <asp:BoundField DataField="displayName" HeaderText="Submitted by" />
                    <asp:BoundField DataField="status" HeaderText="Status" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnViewRecipe" runat="server" CausesValidation="False" CommandArgument='<%# Eval("recipeID") %>' CommandName="ViewRecipe" CssClass="btn btn-sm btn-outline-success">View</asp:LinkButton>
                            <asp:LinkButton ID="btnDeleteRecipe" runat="server" CommandArgument='<%# Eval("recipeID") %>' CommandName="RemoveRecipe" CssClass="btn btn-sm btn-outline-danger" CausesValidation="False" OnClientClick="return confirm('Permanently delete this recipe and its ingredients, steps and category links?');">Delete</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                </asp:GridView>
            </div>
            <br />
        </asp:Panel>
       </main>
    </form>
</body>
</html>