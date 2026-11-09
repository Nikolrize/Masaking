<%@ Page Title="Submit Recipe | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SubmitRecipe.aspx.cs" Inherits="Masaking.Recipes.SubmitRecipe" %>

<%@ Register Src="~/Controls/IngredientEditor.ascx" TagPrefix="uc" TagName="IngredientEditor" %>
<%@ Register Src="~/Controls/CookingStepEditor.ascx" TagPrefix="uc" TagName="CookingStepEditor" %>

<asp:Content ID="BodyContent"
    ContentPlaceHolderID="MainContent" runat="server">

    <link href="../Content/css/recipes.css" rel="stylesheet" />

    <main class="recipe-submit-page mx-auto">

        <!-- Header -->
        <header class="recipe-form-header">
            <asp:HyperLink ID="lnkMyRecipes" runat="server"
                NavigateUrl="~/Recipes/MyRecipes"
                CssClass="back-to-recipes mb-4">
                <i class="bi bi-arrow-left"></i>
                Back to My Recipes
            </asp:HyperLink>

            <h1 id="recipeHeading" runat="server">Share your recipe</h1>
            <p>
                Share the dish you love, list its ingredients,
                and guide other cooks through the method.
            </p>

            <nav class="recipe-form-sections">
                <asp:HyperLink ID="lnk1TheBasics" runat="server"
                    NavigateUrl="#recipeBasics">
                    <span class="detail-step-number">1</span>
                    The basics
                </asp:HyperLink>

                <asp:HyperLink ID="lnk2Photo" runat="server"
                    NavigateUrl="#recipePhoto">
                    <span class="detail-step-number">2</span>
                    Photo
                </asp:HyperLink>

                <asp:HyperLink ID="lnk3Ingredients" runat="server"
                    NavigateUrl="#recipeIngredients">
                    <span class="detail-step-number">3</span>
                    Ingredients
                </asp:HyperLink>

                <asp:HyperLink ID="lnk4Method" runat="server"
                    NavigateUrl="#recipeSteps">
                    <span class="detail-step-number">4</span>
                    Cooking steps
                </asp:HyperLink>
            </nav>
        </header>

        <asp:Label ID="lblMessage" runat="server"
            EnableViewState="false"
            Visible="false"
            role="alert"
            TabIndex="-1" />

        <!-- 1. Recipe details -->
        <section id="recipeBasics"
            class="card shadow-sm recipe-form-section">

            <div class="recipe-section-heading">
                <span class="detail-step-number">1</span>

                <div>
                    <h2>The basics</h2>
                    <p>Help others find the right dish to cook.</p>
                </div>
            </div>

            <div class="row g-4">
                <div class="col-md-8">
                    <asp:Label runat="server"
                        AssociatedControlID="txtTitle"
                        CssClass="form-label">
                        Recipe name
                        <span class="recipe-required">*</span>
                    </asp:Label>

                    <asp:TextBox ID="txtTitle" runat="server"
                        ClientIDMode="Static"
                        CssClass="form-control"
                        MaxLength="150"
                        placeholder="e.g. Nasi Lemak with homemade sambal"
                        required="required" />
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server"
                        AssociatedControlID="ddlDifficulty"
                        CssClass="form-label">
                        Difficulty
                    </asp:Label>

                    <asp:DropDownList ID="ddlDifficulty" runat="server"
                        ClientIDMode="Static"
                        CssClass="form-select">
                        <asp:ListItem Value="Easy">Easy</asp:ListItem>
                        <asp:ListItem Value="Medium">Medium</asp:ListItem>
                        <asp:ListItem Value="Hard">Hard</asp:ListItem>
                    </asp:DropDownList>

                    <p class="form-text">
                        Choose the skill level needed.
                    </p>
                </div>

                <div class="col-12">
                    <asp:Label runat="server"
                        AssociatedControlID="txtDescription"
                        CssClass="form-label">
                        About this dish
                        <span class="recipe-optional">(optional)</span>
                    </asp:Label>

                    <asp:TextBox ID="txtDescription" runat="server"
                        ClientIDMode="Static"
                        CssClass="form-control"
                        TextMode="MultiLine"
                        Rows="4"
                        MaxLength="1000"
                        placeholder="Describe the flavours, share its story, or suggest how to serve it." />

                    <p class="form-text">
                        Keep your description within 1,000 characters.
                    </p>
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server"
                        AssociatedControlID="ddlCategory"
                        CssClass="form-label">
                        Category
                        <span class="recipe-required">*</span>
                    </asp:Label>

                    <asp:DropDownList ID="ddlCategory" runat="server"
                        CssClass="form-select"
                        required="required" />
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server"
                        AssociatedControlID="txtCuisine"
                        CssClass="form-label">
                        Cuisine
                        <span class="recipe-required">*</span>
                    </asp:Label>

                    <asp:TextBox ID="txtCuisine" runat="server"
                        ClientIDMode="Static"
                        CssClass="form-control"
                        MaxLength="80"
                        placeholder="e.g. Malaysian"
                        required="required" />
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server"
                        AssociatedControlID="txtRegion"
                        CssClass="form-label">
                        Region or state
                        <span class="recipe-required">*</span>
                    </asp:Label>

                    <asp:TextBox ID="txtRegion" runat="server"
                        ClientIDMode="Static"
                        CssClass="form-control"
                        MaxLength="80"
                        placeholder="e.g. Penang or Johor"
                        required="required" />
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server"
                        AssociatedControlID="txtServings"
                        CssClass="form-label">
                        Servings
                        <span class="recipe-required">*</span>
                    </asp:Label>

                    <div class="input-group">
                        <asp:TextBox ID="txtServings" runat="server"
                            ClientIDMode="Static"
                            CssClass="form-control"
                            TextMode="Number"
                            placeholder="4"
                            min="1" max="100" step="1"
                            required="required" />

                        <span class="input-group-text">people</span>
                    </div>
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server"
                        AssociatedControlID="txtPrep"
                        CssClass="form-label">
                        Preparation time
                        <span class="recipe-required">*</span>
                    </asp:Label>

                    <div class="input-group">
                        <asp:TextBox ID="txtPrep" runat="server"
                            ClientIDMode="Static"
                            CssClass="form-control"
                            TextMode="Number"
                            placeholder="20"
                            min="0" max="1440" step="1"
                            required="required" />

                        <span class="input-group-text">minutes</span>
                    </div>

                    <p class="form-text">
                        Time spent preparing before cooking.
                    </p>
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server"
                        AssociatedControlID="txtCost"
                        CssClass="form-label">
                        Estimated cost
                        <span class="recipe-required">*</span>
                    </asp:Label>

                    <div class="input-group">
                        <span class="input-group-text">RM</span>

                        <asp:TextBox ID="txtCost" runat="server"
                            ClientIDMode="Static"
                            CssClass="form-control"
                            TextMode="Number"
                            placeholder="10.00"
                            min="0" max="999999.99" step="0.01"
                            required="required" />
                    </div>

                    <p class="form-text">
                        Ingredient cost for all servings.
                    </p>
                </div>
            </div>
        </section>

        <!-- 2. Cover photo -->
        <section id="recipePhoto"
            class="card shadow-sm recipe-form-section">

            <div class="recipe-section-heading">
                <span class="detail-step-number">2</span>

                <div>
                    <h2>A photo of your dish
                        <span class="recipe-optional">(optional)</span>
                    </h2>
                    <p>Show other cooks what the finished dish looks like.</p>
                </div>
            </div>

            <div class="recipe-photo-layout">
                <div class="recipe-device-upload">
                    <i class="bi bi-camera"></i>

                    <asp:Label runat="server"
                        AssociatedControlID="uploadRecipeImage"
                        CssClass="form-label">
                        Choose a photo from your device
                    </asp:Label>

                    <asp:FileUpload ID="uploadRecipeImage" runat="server"
                        ClientIDMode="Static"
                        CssClass="form-control"
                        accept="image/jpeg,image/png,image/gif" />

                    <p class="form-text">
                        JPG, PNG or GIF. Maximum size: 5 MB.
                    </p>

                    <button type="button"
                        id="btnClearRecipePhoto"
                        class="recipe-remove-photo"
                        hidden>
                        Undo photo selection
                    </button>

                    <div id="recipeImageError"
                        class="small text-danger" role="status">
                    </div>

                </div>

                <div class="recipe-photo-preview">
                    <asp:Image ID="imgExistingRecipe" runat="server"
                        ClientIDMode="Static"
                        Visible="false"
                        CssClass="recipe-cover-preview"
                        AlternateText="Current recipe photo" />

                    <asp:Image ID="recipeImagePreview" runat="server"
                        ClientIDMode="Static"
                        CssClass="d-none recipe-cover-preview"
                        AlternateText="Selected recipe photo preview" />

                    <p class="recipe-photo-empty" data-recipe-photo-empty>
                        Your photo preview will appear here.
                   
                    </p>
                </div>
            </div>
        </section>

        <!-- 3. Ingredients -->
        <section id="recipeIngredients"
            class="card shadow-sm recipe-form-section">

            <div class="recipe-section-heading">
                <span class="detail-step-number">3</span>

                <div>
                    <h2>Ingredients</h2>
                    <p>List each ingredient with its quantity and unit.</p>
                </div>
            </div>

            <uc:IngredientEditor ID="ingredientEditor" runat="server" />

            <div hidden>
                <asp:TextBox ID="txtIngredients" runat="server"
                    ClientIDMode="Static"
                    TextMode="MultiLine" />
            </div>
        </section>

        <!-- 4. Cooking steps -->
        <section id="recipeSteps"
            class="card shadow-sm recipe-form-section">

            <div class="recipe-section-heading">
                <span class="detail-step-number">4</span>

                <div>
                    <h2>Cooking steps</h2>
                    <p>Explain each action and how long it takes.</p>
                </div>
            </div>

            <uc:CookingStepEditor ID="stepEditor" runat="server" />

            <div hidden>
                <asp:TextBox ID="txtSteps" runat="server"
                    ClientIDMode="Static"
                    TextMode="MultiLine" />

                <asp:TextBox ID="txtStepImages" runat="server"
                    ClientIDMode="Static"
                    TextMode="MultiLine" />
            </div>
        </section>

        <!-- Save actions -->
        <div class="recipe-submit-actions">
            <p>
                <i class="bi bi-shield-check me-1"></i>
                <asp:Label ID="lblApprovalNotice" runat="server"
                    CssClass="text-reset"
                    Text="Your recipe will appear publicly after approval." />
            </p>

            <div>
                <asp:HyperLink ID="lnkCancel" runat="server"
                    NavigateUrl="~/Recipes/MyRecipes"
                    CssClass="btn btn-outline-secondary">
                    Cancel
                </asp:HyperLink>

                <asp:Button ID="btnSave" runat="server"
                    Text="Submit for review"
                    CssClass="btn btn-primary"
                    formnovalidate="formnovalidate"
                    OnClick="btnSave_Click" />
            </div>
        </div>
    </main>

    <script src="../Scripts/ingredient-editor.js"></script>
    <script src="../Scripts/cooking-step-editor.js"></script>
    <script src="../Scripts/recipe-cover.js"></script>

</asp:Content>
