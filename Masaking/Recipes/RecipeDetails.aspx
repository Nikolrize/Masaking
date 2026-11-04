<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="RecipeDetails.aspx.cs" Inherits="Masaking.Recipes.RecipeDetails" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="/Content/css/recipes.css" rel="stylesheet" />

    <main class="container py-4">
        <!-- Back to Recipes -->
        <asp:HyperLink runat="server"
            NavigateUrl="~/Recipes/BrowseRecipes.aspx"
            CssClass="back-to-recipes mb-4">

            <i class="bi bi-arrow-left" aria-hidden="true"></i>
            Back to recipes
        </asp:HyperLink>

        <asp:Label ID="lblMessage" runat="server"
            Visible="false"
            CssClass="alert alert-warning d-block" />

        <asp:Panel ID="pnlRecipe" runat="server">
            <div class="row g-4 align-items-start">
                <!-- Image -->
                <div class="col-lg-4">
                    <asp:Image ID="recipeImage" runat="server" 
                        CssClass="recipe-detail-cover" />
                </div>

                <div class="col-lg-8">
                    <div class="d-flex justify-content-between align-items-start gap-3 mb-2">
                        <!-- Title -->
                        <h1 class="mb-0">
                            <asp:Label ID="recipeTitle" runat="server" />
                        </h1>

                        <!-- Buttons -->
                        <div class="d-flex align-items-center gap-2">
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
                                CssClass="btn btn-primary text-nowrap">
                                <i class="bi bi-play-circle me-1"></i>
                                Start Learning
                            </asp:HyperLink>
                        </div>
                    </div>

                    <!-- Review -->
                    <p class="mb-4">
                        <asp:PlaceHolder ID="phRatingStar" runat="server" Visible="false">
                            <i class="bi bi-star-fill text-warning me-1"></i>
                        </asp:PlaceHolder>
                        <asp:Label ID="recipeRating" runat="server" />
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
                    <p>
                        <asp:Label ID="recipeDescription" runat="server" />
                    </p>

                </div>
            </div>

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

        </asp:Panel>
    </main>
</asp:Content>
