<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Reviews.aspx.cs" Inherits="Masaking.Community.Reviews" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <link href="<%= ResolveUrl("~/Content/bootstrap-icons.min.css?v=1") %>" rel="stylesheet" />
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Reviews and ratings | Masaking</title>
    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
    <link href="../Content/css/recipes.css" rel="stylesheet" />
    <link href="../Content/css/community.css?v=20261024-39" rel="stylesheet" />
    <link href="../Content/css/community-responsive.css?v=2" rel="stylesheet" />
    <link href="../Content/css/mobile-artwork.css?v=2" rel="stylesheet" />
    <link href="../Content/css/back-links.css?v=2" rel="stylesheet" />
</head>
<body class="masaking-page">
    <form id="form1" runat="server">

        <nav class="navbar masaking-nav">
            <div class="container">
                <a class="navbar-brand" href="../Default.aspx">
                    <img class="brand-mark" src="../Content/images/Masaking%20Logo.png" alt="Masaking" />
                </a>
                <div class="navbar-nav ms-auto">
                    <a class="nav-link" href="../Recipes/BrowseRecipes.aspx">Recipes</a>
                    <a class="nav-link" href="Community.aspx">Community</a>
                    <a class="nav-link" href="../Account/Profile.aspx">My profile</a>
                </div>
            </div>
        </nav>

        <main class="community-view community-feed-view community-secondary-view community-reviews">
    <div class="community-page-decor" aria-hidden="true">
        <img class="community-botanical community-botanical-left" src="<%= ResolveUrl("~/Content/images/community/community-botanical.png") %>" alt="" width="1024" height="1536" decoding="async" />
        <img class="community-botanical community-botanical-right" src="<%= ResolveUrl("~/Content/images/community/community-botanical.png") %>" alt="" width="1024" height="1536" decoding="async" />
    </div>
            <header class="community-banner mb-3">
                <div class="community-banner-inner container">
                    <div class="community-banner-copy">
                        <a href="Community.aspx" class="community-back-link"><i class="bi bi-arrow-left community-icon community-back-arrow" aria-hidden="true"></i><span>Back to Community</span></a>
                        <h1 class="fw-bold mb-2">Reviews and ratings</h1>
                        <p class="mb-0">Your notes help other cooks choose a recipe and make it their own.</p>
                    </div>
                </div>
            </header>
            <div class="community-body container">
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

            <%-- Review form --%>
            <asp:Panel ID="pnlForm" runat="server" CssClass="community-card rounded-3 p-4 mb-4">
                <div class="row g-3 align-items-end">
                    <div class="col-lg-5">
                        <label class="form-label" for="ddlRecipe">Recipe</label>
                        <asp:DropDownList ID="ddlRecipe" runat="server" CssClass="form-select" />
                    </div>

                    <div class="col-lg-2">
                        <label class="form-label" for="ddlRating">Rating</label>
                        <asp:DropDownList ID="ddlRating" runat="server" CssClass="form-select">
                            <asp:ListItem Value="5">★★★★★</asp:ListItem>
                            <asp:ListItem Value="4">★★★★</asp:ListItem>
                            <asp:ListItem Value="3">★★★</asp:ListItem>
                            <asp:ListItem Value="2">★★</asp:ListItem>
                            <asp:ListItem Value="1">★</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="col-lg-5">
                        <label class="form-label" for="txtComment">Your review</label>
                        <asp:TextBox ID="txtComment" runat="server" CssClass="form-control"
                            MaxLength="1000" placeholder="Share what worked for you" />
                    </div>
                </div>

                <asp:Button ID="btnSubmit" runat="server" Text="Submit review"
                    CssClass="btn btn-primary mt-3" OnClick="btnSubmit_Click" />
            </asp:Panel>

            <%-- Sign-in prompt --%>
            <asp:Panel ID="pnlSignIn" runat="server" Visible="false" CssClass="community-card rounded-3 p-4 mb-4">
                <a class="text-brand" href="../Account/Login.aspx">Sign in</a> to leave a recipe rating.
            </asp:Panel>

            <%-- Review list --%>
            <div class="row g-3">
                <asp:Repeater ID="rptReviews" runat="server">
                    <ItemTemplate>
                        <div class="col-lg-6">
                            <article class="community-card rounded-3 p-4 h-100">
                                <div class="d-flex justify-content-between gap-2">
                                    <h2 class="h5 page-heading">
                                        <a class="text-reset text-decoration-none"
                                           href='<%# "../Recipes/RecipeDetails.aspx?id=" + Eval("recipeID") %>'><%#: Eval("title") %></a>
                                    </h2>
                                    <span class="star-rating text-nowrap"><%#: Eval("stars") %></span>
                                </div>
                                <p class="small text-brand">By <%#: Eval("displayName") %> · <%#: Eval("createdAt", "{0:dd MMM yyyy}") %></p>
                                <p class="mb-0"><%#: Eval("comment") %></p>
                            </article>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="community-card rounded-3 p-5 text-center">
                <p class="text-secondary mb-0">Approved recipe reviews will appear here.</p>
            </asp:Panel>
        </div>
        </main>
<div class="community-floating-actions"><button id="communityBackToTop" type="button" class="btn btn-light community-back-to-top" aria-label="Back to top" title="Back to top" hidden><i class="bi bi-arrow-up community-icon" aria-hidden="true"></i></button></div>

    </form>

    <script src="../Scripts/bootstrap.bundle.min.js"></script>
    <script src="../Scripts/community.js?v=20261024-39"></script>
</body>
</html>
