<%@ Page Title="Share a recipe | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SubmitRecipe.aspx.cs" Inherits="Masaking.Recipes.SubmitRecipe" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/css/recipes.css" rel="stylesheet" />

    <main class="recipe-submit-page mx-auto" data-recipe-form>
        <header class="recipe-form-header">
            <asp:HyperLink runat="server" ID="lnkMyRecipes" class="recipe-form-back" NavigateUrl="MyRecipes.aspx">
                <i class="bi bi-arrow-left" aria-hidden="true"></i> <span>My recipes</span>
            </asp:HyperLink>
            <p class="recipe-form-eyebrow">FROM YOUR KITCHEN</p>
            <h1 id="recipeHeading" runat="server" class="fw-bold">Share your recipe</h1>
            <p>Tell us about your dish, list what goes into it, and walk other cooks through each step.</p>
            
            <nav class="recipe-form-sections" aria-label="Recipe form sections">
                <asp:HyperLink runat="server" ID="lnk1TheBasics" NavigateUrl="#recipeBasics"><span>1</span> The basics</asp:HyperLink>
                <asp:HyperLink runat="server" ID="lnk2Photo" NavigateUrl="#recipePhoto"><span>2</span> Photo</asp:HyperLink>
                <asp:HyperLink runat="server" ID="lnk3Ingredients" NavigateUrl="#recipeIngredients"><span>3</span> Ingredients</asp:HyperLink>
                <asp:HyperLink runat="server" ID="lnk4Method" NavigateUrl="#recipeSteps"><span>4</span> Method</asp:HyperLink>
            </nav>
        </header>

        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" role="status" />

        <section id="recipeBasics" class="card border rounded-3 shadow-sm recipe-form-section">
            <div class="recipe-section-heading">
                <span class="recipe-section-number">1</span>
                <div>
                    <h2>The basics</h2>
                    <p>A few details to help others decide what to cook.</p>
                </div>
            </div>

            <div class="row g-3">
                <div class="col-md-8">
                    <asp:Label runat="server" AssociatedControlID="txtTitle" class="form-label">Recipe name <span class="recipe-required">*</span></asp:Label>
                    <asp:TextBox ID="txtTitle" ClientIDMode="Static" runat="server" class="form-control" MaxLength="150" placeholder="e.g. Nasi Lemak with homemade sambal" required="required" />
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="ddlDifficulty" class="form-label">Difficulty</asp:Label>
                    <asp:DropDownList ID="ddlDifficulty" ClientIDMode="Static" runat="server" class="form-select">
                        <asp:ListItem>Easy</asp:ListItem>
                        <asp:ListItem>Medium</asp:ListItem>
                        <asp:ListItem>Hard</asp:ListItem>
                    </asp:DropDownList>
                    <p class="form-text">Easy: beginner-friendly · Hard: more practice needed.</p>
                </div>

                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtDescription" class="form-label">About this dish <span class="recipe-optional">(optional)</span></asp:Label>
                    <asp:TextBox ID="txtDescription" ClientIDMode="Static" runat="server" class="form-control" TextMode="MultiLine" Rows="3" MaxLength="1000" placeholder="What makes your recipe special? Describe its flavour or share a serving suggestion." />
                </div>

                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="txtCuisine" class="form-label">Cuisine <span class="recipe-required">*</span></asp:Label>
                    <asp:TextBox ID="txtCuisine" ClientIDMode="Static" runat="server" class="form-control" Text="Malaysian" MaxLength="80" placeholder="e.g. Malaysian, Chinese, Indian" required="required" />
                </div>

                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="txtRegion" class="form-label">Region or state <span class="recipe-required">*</span></asp:Label>
                    <asp:TextBox ID="txtRegion" ClientIDMode="Static" runat="server" class="form-control" Text="Malaysia" MaxLength="80" placeholder="e.g. Penang or Johor" required="required" />
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="txtServings" class="form-label">Servings <span class="recipe-required">*</span></asp:Label>
                    <div class="input-group">
                        <asp:TextBox ID="txtServings" ClientIDMode="Static" runat="server" class="form-control" TextMode="Number" Text="4" min="1" max="100" required="required" />
                        <span class="input-group-text">people</span>
                    </div>
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="txtPrep" class="form-label">Preparation time <span class="recipe-required">*</span></asp:Label>
                    <div class="input-group">
                        <asp:TextBox ID="txtPrep" ClientIDMode="Static" runat="server" class="form-control" TextMode="Number" Text="20" min="0" max="1440" required="required" />
                        <span class="input-group-text">minutes</span>
                    </div>
                    <p class="form-text">Time spent preparing ingredients before cooking.</p>
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="txtCost" class="form-label">Estimated ingredient cost <span class="recipe-required">*</span></asp:Label>
                    <div class="input-group">
                        <span class="input-group-text">RM</span>
                        <asp:TextBox ID="txtCost" ClientIDMode="Static" runat="server" class="form-control" TextMode="Number" Text="10.00" min="0" max="999999.99" step="0.01" required="required" />
                    </div>
                    <p class="form-text">Total cost for all the servings above.</p>
                </div>
            </div>
        </section>

        <section id="recipePhoto" class="card border rounded-3 shadow-sm recipe-form-section">
            <div class="recipe-section-heading">
                <span class="recipe-section-number">2</span>
                <div>
                    <h2>A photo of your dish <span class="recipe-optional">(optional)</span></h2>
                    <p>A clear photo helps other cooks picture the finished recipe.</p>
                </div>
            </div>

            <div class="recipe-photo-layout">
                <div class="recipe-device-upload">
                    <i class="bi bi-camera" aria-hidden="true"></i>
                    <asp:Label runat="server" AssociatedControlID="uploadRecipeImage" class="form-label">Choose a photo from your device</asp:Label>
                    <asp:FileUpload ID="uploadRecipeImage" ClientIDMode="Static" runat="server" class="form-control" accept="image/jpeg,image/png,image/gif" data-image-preview="recipeImagePreview" data-image-error="recipeImageError" />
                    <p class="form-text">JPG, PNG or GIF · Up to 5 MB.</p>
                    <div id="recipeImageError" class="small text-danger" role="status"></div>
                </div>

                <div class="recipe-photo-preview">
                    <asp:Image ID="imgExistingRecipe" ClientIDMode="Static" runat="server" Visible="false" class="recipe-cover-preview" AlternateText="Current recipe photo" />
                    <asp:Image runat="server" ClientIDMode="Static" ID="recipeImagePreview" class="d-none recipe-cover-preview" AlternateText="Selected recipe photo preview" />
                    <p class="recipe-photo-empty" data-recipe-photo-empty>Your photo preview will appear here.</p>
                </div>
            </div>
        </section>

        <section id="recipeIngredients" class="card border rounded-3 shadow-sm recipe-form-section">
            <div class="recipe-section-heading">
                <span class="recipe-section-number">3</span>
                <div>
                    <h2>Ingredients</h2>
                    <p>Add each ingredient separately, including how much to use.</p>
                </div>
            </div>

            <p class="recipe-entry-example"><i class="bi bi-info-circle" aria-hidden="true"></i>For example: <strong>Rice</strong> · <strong>300</strong> · <strong>g</strong></p>
            
            <div id="recipeIngredientRows" class="recipe-ingredient-rows" hidden></div>

            <button runat="server" id="btnToggleSection" type="button" class="btn btn-outline-success recipe-add-row" data-add-ingredient hidden>
                <i class="bi bi-plus-lg" aria-hidden="true"></i>Add ingredient
            </button>

            <div data-ingredient-fallback>
                <asp:Label runat="server" AssociatedControlID="txtIngredients" class="form-label">Ingredients</asp:Label>
                <asp:TextBox ID="txtIngredients" ClientIDMode="Static" runat="server" class="form-control" TextMode="MultiLine" Rows="6" placeholder="Rice | 300 | g&#10;Coconut milk | 200 | ml" />
                <p class="form-text">One ingredient per line: name | quantity | unit.</p>
            </div>

            <datalist id="recipeUnits">
                <option value="g" />
                <option value="kg" />
                <option value="ml" />
                <option value="l" />
                <option value="tsp" />
                <option value="tbsp" />
                <option value="cup" />
                <option value="piece" />
                <option value="clove" />
                <option value="pinch" />
            </datalist>
        </section>

        <section id="recipeSteps" class="card border rounded-3 shadow-sm recipe-form-section">
            <div class="recipe-section-heading">
                <span class="recipe-section-number">4</span>
                <div>
                    <h2>Cooking method</h2>
                    <p>Break the method into clear, numbered steps. Add a photo to any step if it helps.</p>
                </div>
            </div>

            <div id="recipeStepRows" class="recipe-step-rows" hidden></div>

            <button runat="server" id="btnToggleSection2" type="button" class="btn btn-outline-success recipe-add-row" data-add-step hidden>
                <i class="bi bi-plus-lg" aria-hidden="true"></i>Add step
            </button>

            <div data-step-fallback>
                <asp:Label runat="server" AssociatedControlID="txtSteps" class="form-label">Cooking steps</asp:Label>
                <asp:TextBox ID="txtSteps" ClientIDMode="Static" runat="server" class="form-control" TextMode="MultiLine" Rows="6" placeholder="Rinse the rice until the water runs clear.&#10;Cook the rice with coconut milk and a pinch of salt." />
                <p class="form-text">Put one cooking instruction on each line.</p>
            </div>

            <div hidden>
                <asp:TextBox ID="txtStepImages" ClientIDMode="Static" runat="server" TextMode="MultiLine" />
            </div>
        </section>

        <div class="recipe-submit-actions">
            <p><i class="bi bi-shield-check" aria-hidden="true"></i> <asp:Label ID="lblApprovalNotice" runat="server" class="text-reset" Text="New recipes need administrator approval before they appear publicly." /></p>
            <div>
                <asp:HyperLink runat="server" ID="lnkCancel" class="btn btn-outline-secondary" NavigateUrl="MyRecipes.aspx">Cancel</asp:HyperLink>
                <asp:Button ID="btnSave" runat="server" Text="Submit for review" class="btn btn-primary" OnClick="btnSave_Click" />
            </div>
        </div>
    </main>

    <template id="recipeIngredientTemplate">
        <div class="recipe-ingredient-row">
            <div>
                <asp:Label runat="server" AssociatedControlID="ingredientNameTemplate">Ingredient</asp:Label>
                <asp:TextBox ID="ingredientNameTemplate" runat="server" class="form-control" data-name="true" MaxLength="50" placeholder="e.g. Rice" required="required" />
            </div>
            <div>
                <asp:Label runat="server" AssociatedControlID="ingredientQuantityTemplate">Quantity</asp:Label>
                <asp:TextBox ID="ingredientQuantityTemplate" runat="server" class="form-control" data-quantity="true" TextMode="Number" min="0" max="99999999.99" step="0.01" placeholder="300" required="required" />
            </div>
            <div>
                <asp:Label runat="server" AssociatedControlID="ingredientUnitTemplate">Unit</asp:Label>
                <asp:TextBox ID="ingredientUnitTemplate" runat="server" class="form-control" data-unit="true" MaxLength="30" list="recipeUnits" placeholder="g" required="required" />
            </div>
            <button id="btnRemoveIngredientTemplate" runat="server" type="button" class="recipe-remove-row" aria-label="Remove ingredient">
                <i class="bi bi-trash" aria-hidden="true"></i>
            </button>
        </div>
    </template>

    <template id="recipeStepTemplate">
        <div class="recipe-step-card">
            <div class="recipe-step-title">
                <strong data-step-number></strong>
                <button id="btnRemoveStepTemplate" runat="server" type="button" class="recipe-remove-row" data-remove-step="true">
                    <i class="bi bi-trash" aria-hidden="true"></i>
                </button>
            </div>
            <asp:Label runat="server" AssociatedControlID="stepInstructionTemplate">What should the cook do?</asp:Label>
            <asp:TextBox ID="stepInstructionTemplate" runat="server" class="form-control" TextMode="MultiLine" Rows="3" MaxLength="2000" placeholder="e.g. Rinse the rice until the water runs clear, then drain." required="required" />
            
            <div class="recipe-step-photo">
                <asp:Label runat="server" AssociatedControlID="stepPhotoTemplate"><i class="bi bi-camera" aria-hidden="true"></i> Step photo <span>(optional)</span></asp:Label>
                <asp:FileUpload ID="stepPhotoTemplate" runat="server" class="form-control" accept="image/jpeg,image/png,image/gif" />
                <span class="form-text">JPG, PNG or GIF · Up to 5 MB.</span>
                <p class="small text-danger" data-step-error role="status"></p>
                <asp:Image ID="imgStepPreviewTemplate" runat="server" class="recipe-step-preview" AlternateText="Step photo preview" hidden="hidden" />
                <button id="btnRemoveStepPhotoTemplate" runat="server" type="button" class="recipe-remove-photo" hidden>Remove photo</button>
            </div>
        </div>
    </template>

    <script src="../Scripts/form-helpers.js"></script>
    <script src="../Scripts/recipe-form.js"></script>
</asp:Content>
