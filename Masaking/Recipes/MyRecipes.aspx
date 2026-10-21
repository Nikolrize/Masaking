<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MyRecipes.aspx.cs" Inherits="Masaking.Recipes.MyRecipes" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>My recipes | Masaking</title>
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
</head>
<body class="masaking-page">
    <form id="form1" runat="server">

        <nav class="navbar navbar-expand-lg masaking-nav">
            <div class="container">
                <a class="navbar-brand" href="../Default.aspx">
                    <img class="brand-mark" src="../Content/images/Masaking%20Logo.png" alt="Masaking" />
                </a>
                <div class="navbar-nav ms-auto">
                    <a class="nav-link" href="BrowseRecipes.aspx">Browse recipes</a>
                    <a class="nav-link" href="../Account/Profile.aspx">My profile</a>
                    <a class="nav-link" href="../Community/Community.aspx">Community</a>
                </div>
            </div>
        </nav>

        <main class="container py-5">
            <div class="d-flex flex-wrap align-items-end justify-content-between gap-3 mb-4">
                <div>
                    <p class="text-brand fw-semibold mb-1">YOUR KITCHEN</p>
                    <h1 class="page-heading mb-0">My recipes</h1>
                    <p class="text-secondary mb-0">Edit a recipe or check whether it has been approved for the community.</p>
                </div>
                <a class="btn btn-primary" href="SubmitRecipe.aspx">+ Share a recipe</a>
            </div>

            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

            <section class="p-0">
                <asp:Repeater ID="rptRecipes" runat="server" OnItemCommand="rptRecipes_ItemCommand">
                    <ItemTemplate>
                        <article class="masaking-card recipe-owned-card overflow-hidden mb-4">
                            <div class="row g-0">
                                <div class="col-md-4"><a href='<%# "RecipeDetails.aspx?id=" + Eval("recipeID") %>'><img class="recipe-owned-image" src='<%# OwnedImageUrl(Eval("image")) %>' alt='<%#: Eval("title") %>' /></a></div>
                                <div class="col-md-8 p-4 p-lg-4 d-flex flex-column">
                                    <div class="d-flex justify-content-between align-items-start gap-3"><div><span class="small text-brand"><%#: Eval("cuisine") %> · <%#: Eval("region") %></span><h2 class="h4 page-heading mt-1 mb-2"><a class="text-reset text-decoration-none" href='<%# "RecipeDetails.aspx?id=" + Eval("recipeID") %>'><%#: Eval("title") %></a></h2><span class="badge badge-pending rounded-pill px-3 py-2"><%#: Eval("status") %></span></div><span class="small text-secondary text-nowrap"><%#: Eval("createdAt", "{0:dd MMM yyyy}") %></span></div>
                                    <div class="d-flex flex-wrap gap-3 small text-secondary my-3"><span>★ <strong><%#: Eval("averageRating", "{0:0.0}") %></strong> (<%#: Eval("reviewCount") %> reviews)</span><span>◷ Prep: <%#: Eval("preparationTime") %> min</span><span>♨ <%#: Eval("difficulty") %></span><span>♧ Serves <%#: Eval("servings") %></span><span><svg class="cost-icon" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="M15.5 8.5c-.6-.7-1.5-1-2.8-1-1.4 0-2.4.7-2.4 1.8 0 3 5.6 1.2 5.6 4.2 0 1.2-1.1 2-2.8 2-1.3 0-2.4-.4-3.1-1.2M12.5 6v12"/></svg> RM <%#: Eval("estimatedCost", "{0:0.00}") %></span></div>
                                    <div class="d-flex gap-2 mt-auto"><a class="btn btn-sm btn-outline-success" href='<%# "SubmitRecipe.aspx?id=" + Eval("recipeID") %>'><span aria-hidden="true">✎</span> Edit</a><asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger" CommandName="deleteRecipe" CommandArgument='<%# Eval("recipeID") %>' OnClientClick="return confirm('Delete this recipe and its community activity?');"><span aria-hidden="true">⌫</span> Delete</asp:LinkButton><a class="btn btn-sm btn-primary ms-auto" href='<%# "RecipeDetails.aspx?id=" + Eval("recipeID") %>'>View recipe</a></div>
                                </div>
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="masaking-card text-center py-5">
                    <div class="auth-ornament fs-1">✿</div>
                    <h2 class="h5 page-heading">Your recipe book is ready</h2>
                    <p class="text-secondary">Share the first recipe with the Masaking community.</p>
                    <a class="btn btn-primary" href="SubmitRecipe.aspx">Add a recipe</a>
                </asp:Panel>
            </section>
        </main>

    </form>

    <script src="../Scripts/bootstrap.bundle.min.js"></script>
</body>
</html>
