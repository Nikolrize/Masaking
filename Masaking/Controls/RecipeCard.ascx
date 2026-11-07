<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecipeCard.ascx.cs" Inherits="Masaking.Controls.RecipeCard" %>

<%@ Register Src="~/Controls/UserAvatar.ascx" TagPrefix="uc" TagName="UserAvatar" %>

<asp:HyperLink ID="recipeLink" runat="server"
    CssClass="card h-100 border text-decoration-none text-dark hover-lift">
    <!-- Image -->
    <div class="position-relative">
        <div class="ratio ratio-4x3 bg-dark bg-opacity-25">
            <asp:Image ID="recipeImage" runat="server"
                CssClass="card-img-top object-fit-cover" />
        </div>

        <asp:Label ID="recipeStatus" runat="server"
            Visible="false"
            CssClass="recipe-card-status" />
    </div>

    <!-- Title and Difficulty -->
    <div class="card-body d-flex flex-column gap-2">
        <div class="d-flex justify-content-between align-items-center gap-2">
            <h5 class="card-title recipe-card-title mb-0">
                <asp:Label ID="recipeTitle" runat="server" />
            </h5>
            <asp:Label ID="recipeDifficulty" runat="server" />
        </div>

        <!-- Cuisine and Category -->
        <p class="small text-muted mb-0">
            <asp:Label ID="recipeCuisine" runat="server" />
            &middot;
            <asp:Label ID="recipeCategory" runat="server" />
        </p>

        <!-- Author -->
        <div class="d-flex align-items-center gap-2">
            <uc:UserAvatar ID="authorAvatar" runat="server" />

            <asp:Label ID="recipeAuthor" runat="server"
                CssClass="small text-muted" />
        </div>

        <!-- Prep Time, Budget, and Servings -->
        <span class="pt-3 small text-muted">
            <i class="bi bi-clock me-1"></i>
            <asp:Label ID="recipePreparationTime" runat="server" />
            &middot;
            <i class="bi bi-cash me-1"></i>
            <asp:Label ID="recipeBudget" runat="server" />
            &middot;
            <i class="bi bi-person me-1"></i>
            <asp:Label ID="recipeServings" runat="server" />
        </span>

        <!-- Rating -->
        <div class="mt-auto small">
            <asp:PlaceHolder ID="phRatingStar"
                runat="server" Visible="false">
                <i class="bi bi-star-fill text-warning me-1"></i>
            </asp:PlaceHolder>
            <asp:Label ID="recipeRating" runat="server"
                CssClass="text-muted" />
        </div>
    </div>
</asp:HyperLink>