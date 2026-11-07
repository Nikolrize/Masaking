<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="RecipeDetails.aspx.cs" Inherits="Masaking.Recipes.RecipeDetails" %>

<%@ Register Src="~/Controls/UserAvatar.ascx" TagPrefix="uc" TagName="UserAvatar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="/Content/css/recipes.css" rel="stylesheet" />

    <main class="container py-4">
        <!-- Back to Recipes -->
        <asp:HyperLink ID="lnkBack" runat="server"
            NavigateUrl="~/Recipes/BrowseRecipes"
            CssClass="back-to-recipes mb-4">

            <i class="bi bi-arrow-left"></i>

            <asp:Literal ID="backLinkText" runat="server"
                Mode="Encode" Text="Back to Browse Recipes" />
        </asp:HyperLink>

        <asp:Label ID="lblMessage" runat="server"
            Visible="false"
            CssClass="alert alert-warning d-block" />

        <asp:Panel ID="pnlRecipe" runat="server">
            <section class="card recipe-summary-card p-3 p-md-4">
                <div class="row g-4 align-items-start">
                    <!-- Image -->
                    <div class="col-lg-4">
                        <asp:Image ID="recipeImage" runat="server"
                            CssClass="recipe-detail-cover" />
                    </div>

                    <div class="col-lg-8">
                        <div class="d-flex flex-column flex-md-row justify-content-between align-items-start gap-2 gap-md-3 mb-2">
                            <!-- Title -->
                            <h1 class="mb-0 text-break">
                                <asp:Label ID="recipeTitle" runat="server" />
                            </h1>

                            <!-- Buttons -->
                            <div class="d-flex flex-wrap align-items-center gap-2">
                                <button id="btnFavourite"
                                    runat="server"
                                    type="submit"
                                    class="btn btn-danger"
                                    causesvalidation="false"
                                    onserverclick="btnFavourite_ServerClick">

                                    <i id="favouriteIcon" runat="server" class="bi bi-heart"></i>
                                </button>

                                <asp:HyperLink ID="lnkStartLearning"
                                    runat="server"
                                    NavigateUrl="~/Learning/CookingSimulation"
                                    CssClass="btn btn-primary">
                                <i class="bi bi-play-circle me-1"></i>
                                Start Learning
                                </asp:HyperLink>

                                <asp:PlaceHolder ID="phOwnerActions" runat="server"
                                    Visible="false">

                                    <asp:HyperLink ID="lnkEditRecipe" runat="server"
                                        CssClass="btn recipe-submit-btn">
                                        <i class="bi bi-pencil me-1"></i>
                                        Edit
                                    </asp:HyperLink>

                                    <asp:LinkButton ID="btnDeleteRecipe" runat="server"
                                        CssClass="btn btn-danger"
                                        CausesValidation="false"
                                        OnClick="btnDeleteRecipe_Click"
                                        OnClientClick="return confirm('Delete this recipe? It will no longer be visible on the site.');">
                                        <i class="bi bi-trash me-1"></i>
                                        Delete
                                    </asp:LinkButton>
                                </asp:PlaceHolder>
                            </div>
                        </div>

                        <!-- Review -->
                        <p>
                            <asp:PlaceHolder ID="phRatingStar" runat="server" Visible="false">
                                <i class="bi bi-star-fill text-warning me-1"></i>
                            </asp:PlaceHolder>
                            <asp:Label ID="recipeRating" runat="server" />
                        </p>

                        <!-- Author -->
                        <p>
                            <asp:HyperLink ID="lnkAuthor" runat="server"
                                CssClass="recipe-author-link d-inline-flex align-items-center gap-2">
                                <uc:UserAvatar ID="authorAvatar" runat="server" />
                                <asp:Label ID="recipeAuthor" runat="server" />
                            </asp:HyperLink>
                        </p>

                        <!-- Details -->
                        <div class="row row-cols-1 row-cols-sm-2 g-3 mb-4 text-muted">
                            <div class="col">
                                <i class="bi bi-globe-asia-australia me-2"></i>
                                Cuisine:
                           
                            <asp:Label ID="recipeCuisine" runat="server" />
                            </div>

                            <div class="col">
                                <i class="bi bi-tag me-2"></i>
                                Category:
                           
                            <asp:Label ID="recipeCategory" runat="server" />
                            </div>

                            <div class="col">
                                <i class="bi bi-clock me-2"></i>
                                Preparation:
                            <asp:Label ID="recipeTime" runat="server" />
                            </div>

                            <div class="col">
                                <i class="bi bi-bar-chart me-2"></i>
                                Difficulty:
                            <asp:Label ID="recipeDifficulty" runat="server" />
                            </div>

                            <div class="col">
                                <i class="bi bi-people me-2"></i>
                                Servings:
                            <asp:Label ID="recipeServings" runat="server" />
                            </div>

                            <div class="col">
                                <i class="bi bi-wallet2 me-2"></i>
                                Estimated cost: RM<asp:Label ID="recipeCost" runat="server" />
                            </div>
                        </div>

                        <!-- Description -->
                        <p class="mb-0">
                            <asp:Label ID="recipeDescription" runat="server" />
                        </p>

                    </div>
                </div>
            </section>

            <div class="row g-4 mt-1">
                <!-- Ingredients -->
                <div class="col-lg-4">
                    <section class="card h-100 p-3 p-md-4">
                        <h2 class="h5">Ingredients</h2>

                        <p class="small text-muted">
                            (<asp:Label ID="ingredientServings" runat="server" />
                            servings)
                        </p>

                        <asp:Label ID="lblIngredientsEmpty" runat="server"
                            Visible="false"
                            Text="No ingredients available."
                            CssClass="small text-muted" />

                        <asp:Repeater ID="rptIngredients" runat="server"
                            OnItemDataBound="rptIngredients_ItemDataBound">
                            <ItemTemplate>
                                <div class="detail-ingredient">
                                    <asp:CheckBox ID="ingredientCheck" runat="server"
                                        CssClass="ingredient-check" />

                                    <asp:Label ID="ingredientQuantity" runat="server"
                                        CssClass="small text-muted text-nowrap" />
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </section>
                </div>

                <!-- Cooking steps -->
                <div class="col-lg-8">
                    <section class="card h-100 p-3 p-md-4">
                        <h2 class="h5">Cooking Steps</h2>

                        <asp:Label ID="lblStepsEmpty" runat="server"
                            Visible="false"
                            Text="No cooking steps available."
                            CssClass="small text-muted" />

                        <ol class="list-unstyled">
                            <asp:Repeater ID="rptSteps" runat="server"
                                OnItemDataBound="rptSteps_ItemDataBound">
                                <ItemTemplate>
                                    <li class="detail-step d-flex gap-3">
                                        <asp:Label ID="stepNumber" runat="server"
                                            CssClass="detail-step-number" />

                                        <asp:Image ID="stepImage" runat="server"
                                            CssClass="detail-step-image" />

                                        <div class="flex-grow-1">
                                            <asp:Label ID="stepMinutes" runat="server"
                                                CssClass="small text-muted d-block mb-1" />

                                            <asp:Label ID="stepInstruction" runat="server"
                                                CssClass="detail-step-instruction" />
                                        </div>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ol>
                    </section>
                </div>
            </div>

            <!-- Ratings and Comments -->
            <section class="card recipe-summary-card p-3 p-md-4 mt-4">
                <h2 class="h4 mb-3">Ratings &amp; Reviews</h2>

                <asp:Label ID="lblReviewMessage" runat="server"
                    Visible="false"
                    CssClass="alert alert-info d-block" />

                <asp:Panel ID="pnlReviewForm" runat="server" Visible="false"
                    CssClass="mb-4">

                    <asp:Label runat="server"
                        Text="Your rating"
                        CssClass="form-label d-block" />

                    <asp:RadioButtonList ID="ddlReviewRating" runat="server"
                        RepeatLayout="UnorderedList"
                        CssClass="rating-stars mb-3">
                        <asp:ListItem Value="1">1 star</asp:ListItem>
                        <asp:ListItem Value="2">2 stars</asp:ListItem>
                        <asp:ListItem Value="3">3 stars</asp:ListItem>
                        <asp:ListItem Value="4">4 stars</asp:ListItem>
                        <asp:ListItem Value="5">5 stars</asp:ListItem>
                    </asp:RadioButtonList>

                    <asp:Label runat="server" AssociatedControlID="txtReviewComment"
                        Text="Your comment (optional)" CssClass="form-label d-block" />

                    <asp:TextBox ID="txtReviewComment" runat="server"
                        TextMode="MultiLine" Rows="3"
                        CssClass="form-control mb-2" />

                    <p class="small text-muted">
                        Maximum 1,000 characters. Submitting again updates your review.
                    </p>

                    <asp:Button ID="btnSubmitReview" runat="server"
                        Text="Save review"
                        CssClass="btn btn-primary"
                        CausesValidation="false"
                        OnClick="btnSubmitReview_Click" />
                </asp:Panel>

                <asp:Label ID="lblReviewHint" runat="server"
                    CssClass="text-muted d-block mb-3" />

                <asp:Label ID="lblNoReviews" runat="server"
                    Text="No reviews yet. Be the first to share your experience!"
                    Visible="false" CssClass="text-muted" />

                <asp:Repeater ID="rptReviews" runat="server"
                    OnItemDataBound="rptReviews_ItemDataBound">
                    <ItemTemplate>
                        <article class="py-3 border-top">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <uc:UserAvatar ID="reviewAvatar" runat="server" />

                                <asp:Label ID="reviewName" runat="server"
                                    CssClass="fw-semibold" />

                                <asp:Label ID="reviewDate" runat="server"
                                    CssClass="small text-muted ms-auto" />
                            </div>

                            <asp:Label ID="reviewStars" runat="server"
                                CssClass="text-warning d-block mb-2" />

                            <asp:Label ID="reviewComment" runat="server"
                                CssClass="review-comment d-block" />
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </section>
        </asp:Panel>
    </main>
</asp:Content>
