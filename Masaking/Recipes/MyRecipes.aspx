<%@ Page Title="My recipes | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyRecipes.aspx.cs" Inherits="Masaking.Recipes.MyRecipes" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/css/recipes.css" rel="stylesheet" />

    <main class="pb-5">
        <div class="d-flex flex-wrap align-items-end justify-content-between gap-3 mb-4">
            <div>
                <p class="text-success fw-semibold mb-1">YOUR KITCHEN</p>
                <h1 class="fw-bold mb-0">My recipes</h1>
                <p class="text-secondary mb-0">Edit a recipe or check whether it has been approved for the community.</p>
            </div>
            <asp:HyperLink runat="server" ID="lnkShareARecipe" class="btn btn-primary" NavigateUrl="SubmitRecipe.aspx">+ Share a recipe</asp:HyperLink>
        </div>

        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

        <section class="p-0">
            <asp:Repeater ID="rptRecipes" runat="server" OnItemCommand="rptRecipes_ItemCommand" OnItemDataBound="rptRecipes_ItemDataBound">
                <ItemTemplate>
                    <article class="card border rounded-3 shadow-sm overflow-hidden mb-4">
                        <div class="row g-0">
                            <div class="col-md-4">
                                <asp:HyperLink ID="lnkRecipeDetails" runat="server" ClientIDMode="Static">
                                    <asp:Image runat="server" ID="imgDecoration" class="recipe-owned-image" />
                                </asp:HyperLink>
                            </div>
                            <div class="col-md-8 p-3 p-sm-4 d-flex flex-column">
                                <div class="d-flex justify-content-between align-items-start gap-3">
                                    <div>
                                        <span class="small text-success">
                                            <asp:Label ID="lblCuisine" runat="server" /> · <asp:Label ID="lblRegion" runat="server" />
                                        </span>
                                        <h2 class="h4 fw-bold mt-1 mb-2">
                                            <asp:HyperLink ID="lnkRecipeDetails2" runat="server" ClientIDMode="Static" class="text-reset text-decoration-none">
                                                <asp:Label ID="lblTitle" runat="server" />
                                            </asp:HyperLink>
                                        </h2>
                                        <span class="badge bg-light text-dark border rounded-pill px-3 py-2">
                                            <asp:Label ID="lblStatus" runat="server" />
                                        </span>
                                    </div>
                                    <span class="small text-secondary text-nowrap">
                                        <asp:Label ID="lblCreatedAt" runat="server" />
                                    </span>
                                </div>

                                <div class="d-flex flex-wrap gap-3 small text-secondary my-3">
                                    <span>★ <strong><asp:Label ID="lblAverageRating" runat="server" /></strong> (<asp:Label ID="lblReviewCount" runat="server" /> reviews)</span>
                                    <span>◷ Prep: <asp:Label ID="lblPreparationTime" runat="server" /> min</span>
                                    <span>♨ <asp:Label ID="lblDifficulty" runat="server" /></span>
                                    <span>♧ Serves <asp:Label ID="lblServings" runat="server" /></span>
                                    <span><i class="bi bi-currency-dollar cost-icon" aria-hidden="true"></i>RM <asp:Label ID="lblEstimatedCost" runat="server" /></span>
                                </div>

                                <div class="d-flex flex-wrap gap-2 mt-auto">
                                    <asp:HyperLink ID="lnkEditRecipe" runat="server" ClientIDMode="Static" class="btn btn-sm btn-outline-success">
                                        <span aria-hidden="true">✎</span> Edit
                                    </asp:HyperLink>
                                    <asp:HyperLink ID="lnkManageSimulations" runat="server" CssClass="btn btn-sm btn-outline-success" NavigateUrl='<%# "~/Learning/ManageSimulations.aspx?recipeId=" + Eval("recipeID") %>' Visible='<%# Convert.ToString(Eval("status")) == "Approved" %>' Text="Manage simulations" />
                                    <asp:HyperLink ID="lnkManageChallenges" runat="server" CssClass="btn btn-sm btn-outline-success" NavigateUrl='<%# "~/Learning/ManageChallenges.aspx?recipeId=" + Eval("recipeID") %>' Visible='<%# Convert.ToString(Eval("status")) == "Approved" %>' Text="Manage challenges" />
                                    <asp:LinkButton ID="btnDelete" runat="server" class="btn btn-sm btn-outline-danger" CommandName="deleteRecipe" OnClientClick="return confirm('Delete this recipe and its community activity?');">
                                        <span aria-hidden="true">⌫</span> Delete
                                    </asp:LinkButton>
                                    <asp:HyperLink ID="lnkRecipeDetails3" runat="server" ClientIDMode="Static" class="btn btn-sm btn-primary ms-auto">
                                        View recipe
                                    </asp:HyperLink>
                                </div>
                            </div>
                        </div>
                    </article>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" class="card border rounded-3 shadow-sm text-center p-4 py-5">
                <div class="text-success fs-1 mb-2"><i class="bi bi-journal-plus" aria-hidden="true"></i></div>
                <h2 class="h5 fw-bold">Your recipe book is ready</h2>
                <p class="text-secondary">Share the first recipe with the Masaking community.</p>
                <asp:HyperLink runat="server" ID="lnkAddARecipe" class="btn btn-primary" NavigateUrl="SubmitRecipe.aspx">Add a recipe</asp:HyperLink>
            </asp:Panel>
        </section>
    </main>
</asp:Content>