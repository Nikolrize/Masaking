<%@ Page Title="Reviews and ratings | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reviews.aspx.cs" Inherits="Masaking.Community.Reviews" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/css/recipes.css" rel="stylesheet" />
    <link href="../Content/css/community.css" rel="stylesheet" />
    <link href="../Content/css/community-responsive.css" rel="stylesheet" />

    <main data-community-page data-community-feed data-community-secondary class="position-relative community-reviews">
        <div class="community-page-decor" aria-hidden="true">
            <asp:Image runat="server" ID="imgDecoration" class="community-botanical community-botanical-left" ImageUrl="~/Content/images/community/community-botanical.png" AlternateText="" GenerateEmptyAlternateText="true" decoding="async" />
            <asp:Image runat="server" ID="imgDecoration2" class="community-botanical community-botanical-right" ImageUrl="~/Content/images/community/community-botanical.png" AlternateText="" GenerateEmptyAlternateText="true" decoding="async" />
        </div>

        <header class="community-banner mb-3">
            <div class="community-banner-inner">
                <div class="community-banner-copy">
                    <asp:HyperLink runat="server" ID="lnkBackToCommunity" NavigateUrl="Community.aspx" class="community-back-link">
                        <i class="bi bi-arrow-left community-icon community-back-arrow" aria-hidden="true"></i>
                        <span>Back to Community</span>
                    </asp:HyperLink>
                    <h1 class="mb-2">Reviews and ratings</h1>
                    <p class="mb-0">Your notes help other cooks choose a recipe and make it their own.</p>
                </div>
            </div>
        </header>

        <div class="community-body py-3">
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

            <!-- Review form -->
            <asp:Panel ID="pnlForm" runat="server" class="community-card rounded-3 p-4 mb-4">
                <div class="row g-3 align-items-end">
                    <div class="col-lg-5">
                        <asp:Label runat="server" ID="lblRecipe" class="form-label" AssociatedControlID="ddlRecipe">Recipe</asp:Label>
                        <asp:DropDownList ID="ddlRecipe" runat="server" class="form-select" />
                    </div>
                    <div class="col-lg-2">
                        <asp:Label runat="server" ID="lblRating" class="form-label" AssociatedControlID="ddlRating">Rating</asp:Label>
                        <asp:DropDownList ID="ddlRating" runat="server" class="form-select">
                            <asp:ListItem Value="5">★★★★★</asp:ListItem>
                            <asp:ListItem Value="4">★★★★</asp:ListItem>
                            <asp:ListItem Value="3">★★★</asp:ListItem>
                            <asp:ListItem Value="2">★★</asp:ListItem>
                            <asp:ListItem Value="1">★</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="col-lg-5">
                        <asp:Label runat="server" ID="lblComment" class="form-label" AssociatedControlID="txtComment">Your review</asp:Label>
                        <asp:TextBox ID="txtComment" runat="server" class="form-control" MaxLength="1000" placeholder="Share what worked for you" />
                    </div>
                </div>
                <asp:Button ID="btnSubmit" runat="server" Text="Submit review" class="btn btn-primary mt-3" OnClick="btnSubmit_Click" />
            </asp:Panel>

            <!-- Sign-in prompt -->
            <asp:Panel ID="pnlSignIn" runat="server" Visible="false" class="community-card rounded-3 p-4 mb-4">
                <asp:HyperLink runat="server" ID="lnkSignIn" class="text-brand" NavigateUrl="../Account/Login.aspx">Sign in</asp:HyperLink> to leave a recipe rating.
            </asp:Panel>

            <!-- Review list -->
            <div class="row g-3">
                <asp:Repeater ID="rptReviews" runat="server" OnItemDataBound="rptReviews_ItemDataBound">
                    <ItemTemplate>
                        <div class="col-lg-6">
                            <article class="community-card rounded-3 p-4 h-100">
                                <div class="d-flex justify-content-between gap-2">
                                    <h2 class="h5 page-heading">
                                        <asp:HyperLink ID="lnkRecipeDetails" runat="server" class="text-reset text-decoration-none">
                                            <asp:Label ID="lblTitle" runat="server" />
                                        </asp:HyperLink>
                                    </h2>
                                    <span class="star-rating text-nowrap"><asp:Label ID="lblStars" runat="server" /></span>
                                </div>
                                <p class="small text-brand">By <asp:Label ID="lblDisplayName" runat="server" /> · <asp:Label ID="lblCreatedAt" runat="server" /></p>
                                <p class="mb-0"><asp:Label ID="lblComment2" runat="server" /></p>
                            </article>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" class="community-card rounded-3 p-5 text-center">
                <p class="text-secondary mb-0">Approved recipe reviews will appear here.</p>
            </asp:Panel>
        </div>
    </main>

    <div class="community-floating-actions">
        <button runat="server" ClientIDMode="Static" ID="communityBackToTop" type="button" class="btn btn-light community-back-to-top" aria-label="Back to top" title="Back to top" hidden>
            <i class="bi bi-arrow-up community-icon" aria-hidden="true"></i>
        </button>
    </div>

    <script src="../Scripts/community.js"></script>
</asp:Content>
