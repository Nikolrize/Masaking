<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Masaking._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="/Content/css/home.css?v=2" rel="stylesheet" />

    <main>
        <section class="row align-items-center g-4">
            <div class="col-md-6">
                <h1>Bring Malaysian flavours to your kitchen</h1>
                <p class="lead">Discover authentic recipes, learn new skills, challenge yourself and be part of our food community.</p>
                <asp:Button ID="btnBrowseRecipes"
                    runat="server"
                    CssClass="btn btn-primary"
                    Text="Explore Recipes" PostBackUrl="~/Recipes/BrowseRecipes.aspx" />
            </div>
            <div class="col-md-6">
                <img src="/Content/images/home/nasi lemak.png" alt="logo" class="img-fluid">
            </div>
        </section>

        <div class="container p-4 d-flex flex-column gap-4" style="background-color: var(--color-surface, #fffdf7);">
            <section>
                <h4 class="mb-3">Browse By Category</h4>

                <!-- Replace with actual categories -->
                <div class="row row-cols-2 row-cols-md-3 row-cols-lg-6 g-3">
                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Main%20Dishes"
                            class="category-card tile-main">
                            <i class="bi bi-egg-fried"></i>
                            <span>Main Dishes</span>
                        </a>
                    </div>

                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Desserts"
                            class="category-card tile-desserts">
                            <i class="bi bi-cake2"></i>
                            <span>Desserts</span>
                        </a>
                    </div>

                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Beverages"
                            class="category-card tile-beverages">
                            <i class="bi bi-cup-straw"></i>
                            <span>Beverages</span>
                        </a>
                    </div>

                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Snacks"
                            class="category-card tile-snacks">
                            <i class="bi bi-cookie"></i>
                            <span>Snacks</span>
                        </a>
                    </div>

                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Traditional"
                            class="category-card tile-traditional">
                            <i class="bi bi-basket"></i>
                            <span>Traditional</span>
                        </a>
                    </div>

                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Others"
                            class="category-card tile-others">
                            <i class="bi bi-three-dots"></i>
                            <span>Others</span>
                        </a>
                    </div>
                </div>
            </section>

            <section class="col-lg-9 mb-2">
                <h4 class="mb-3">Popular Recipes</h4>

                <div class="row row-cols-1 row-cols-sm-2 row-cols-xl-3 g-3">
                    <div class="col">
                        <a href="RecipeDetails.aspx?id=1"
                            class="card border border-subtle text-decoration-none text-dark hover-lift">
                            <img src="/Content/images/home/roti canai.png"
                                class="card-img-top"
                                alt="Roti canai served with curry" />

                            <div class="card-body">
                                <h5 class="card-title">Roti Canai</h5>
                                <p class="card-text">Malaysian · Snack</p>
                                <span class="text-warning">
                                    <i class="bi bi-star-fill"></i>
                                    4.6 (87)
                                </span>
                            </div>
                        </a>
                    </div>

                    <!-- Add more recipe cards here -->
                </div>
            </section>
        </div>
    </main>

</asp:Content>
