<%@ Page Title="Browse Recipes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BrowseRecipes.aspx.cs" Inherits="Masaking.Recipes.BrowseRecipes" %>

<%@ Register Src="~/Controls/RecipeCard.ascx" TagPrefix="uc" TagName="RecipeCard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="/Content/css/recipes.css" rel="stylesheet" />

    <main class="container py-4 recipe-browse-page">
        <h1>Browse Recipes</h1>
        <p class="lead">Find your next favourite Malaysian dish.</p>

        <!-- Search -->
        <div class="input-group mb-5">

            <asp:TextBox ID="txtSearch"
                runat="server"
                CssClass="form-control"
                placeholder="Search recipes..." />

            <asp:Button ID="btnSearch"
                runat="server"
                CssClass="btn btn-primary"
                Text="Search"
                OnClick="btnSearch_Click" />
        </div>

        <div class="row g-4">
            <!-- Filters -->
            <aside class="col-lg-3">
                <div class="card p-3 border border-subtle">
                    <h2 class="h5">Category</h2>

                    <!-- Categories -->
                    <asp:CheckBoxList ID="cblCategories" runat="server" CssClass="category-filters" />

                    <hr />

                    <!-- Cuisine abd Region -->
                    <h2 class="h5">Cuisine</h2>
                    <asp:DropDownList ID="ddlCuisine" runat="server" CssClass="form-select" />

                    <h2 class="h5 mt-3">Region</h2>
                    <asp:DropDownList ID="ddlRegion" runat="server" CssClass="form-select" />

                    <!-- Difficulty -->
                    <h2 class="h5 mt-3">Difficulty</h2>
                    <asp:RadioButtonList ID="rblDifficulty" runat="server"
                        RepeatLayout="Flow"
                        CssClass="filter-options">
                        <asp:ListItem Value="" Selected="True">All</asp:ListItem>
                        <asp:ListItem Value="Easy">Easy</asp:ListItem>
                        <asp:ListItem Value="Medium">Medium</asp:ListItem>
                        <asp:ListItem Value="Hard">Hard</asp:ListItem>
                    </asp:RadioButtonList>

                    <!-- Max Budget and Prep Time -->
                    <h2 class="h5 mt-3">Maximum budget</h2>
                    <span id="budgetValue" class="small text-muted">Any budget</span>
                    <asp:TextBox ID="rngBudget" runat="server"
                        TextMode="Range"
                        Text="0"
                        min="0"
                        step="1"
                        CssClass="form-range" />

                    <h2 class="h5 mt-3">Maximum preparation time</h2>
                    <span id="timeValue" class="small text-muted">Any duration</span>
                    <asp:TextBox ID="rngTime" runat="server"
                        TextMode="Range"
                        Text="0"
                        min="0"
                        step="1"
                        CssClass="form-range" />

                    <!-- Min Serving -->
                    <h2 class="h5 mt-3">Minimum servings</h2>
                    <asp:TextBox ID="txtServings" runat="server"
                        TextMode="Number"
                        Text="0"
                        min="0"
                        step="1"
                        CssClass="form-control" />
                    <p class="form-text">Enter 0 for any number of servings.</p>

                    <!-- Error and Submit -->
                    <asp:Label ID="lblFilterError" runat="server"
                        Visible="false"
                        CssClass="text-danger small d-block" />

                    <asp:Button ID="btnApplyFilters"
                        runat="server"
                        Text="Apply filters"
                        CssClass="btn btn-primary mt-3"
                        OnClick="btnApplyFilters_Click" />
                </div>
            </aside>

            <!-- Recipe results -->
            <section class="col-lg-9">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h4 class="mb-1">All Recipes</h4>
                        <asp:Label ID="recipeCount" runat="server" CssClass="small text-muted" />
                    </div>

                    <div class="d-flex align-items-center gap-2">
                        <asp:Label runat="server"
                            AssociatedControlID="ddlSort"
                            Text="Sort by"
                            CssClass="small text-muted text-nowrap" />

                        <asp:DropDownList ID="ddlSort" runat="server"
                            CssClass="form-select w-auto"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlSort_SelectedIndexChanged">
                            <asp:ListItem Value="nameAsc">Name: A–Z</asp:ListItem>
                            <asp:ListItem Value="nameDesc">Name: Z–A</asp:ListItem>
                            <asp:ListItem Value="newest">Newest</asp:ListItem>
                            <asp:ListItem Value="rating">Highest rated</asp:ListItem>
                            <asp:ListItem Value="reviews">Most reviewed</asp:ListItem>
                            <asp:ListItem Value="cost">Lowest cost</asp:ListItem>
                            <asp:ListItem Value="time">Quickest</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                </div>

                <div class="row row-cols-1 row-cols-sm-2 row-cols-xl-3 g-3">
                    <asp:Repeater ID="rptRecipes" runat="server" OnItemDataBound="rptRecipes_ItemDataBound">
                        <ItemTemplate>
                            <div class="col">
                                <uc:RecipeCard ID="recipeCard" runat="server" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>
        </div>
    </main>

    <script>
        (function () {

            const budgetSlider = document.getElementById("<%= rngBudget.ClientID %>");
            const timeSlider = document.getElementById("<%= rngTime.ClientID %>");

            const budgetDisplay = document.getElementById("budgetValue");
            const timeDisplay = document.getElementById("timeValue");

            function updateBudget() {
                const text = budgetSlider.value === "0"
                    ? "Any budget"
                    : "Up to RM" + budgetSlider.value;

                budgetDisplay.textContent = text;
            }

            function updateTime() {
                const text = timeSlider.value === "0"
                    ? "Any duration"
                    : "Up to " + timeSlider.value + " minutes";

                timeDisplay.textContent = text;
            }

            budgetSlider.addEventListener("input", updateBudget);
            timeSlider.addEventListener("input", updateTime);

            updateBudget();
            updateTime();
        })();
    </script>
</asp:Content>
