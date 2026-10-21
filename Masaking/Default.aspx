<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Masaking._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="/Content/css/home.css" rel="stylesheet" />

    <main>
        <section class="row align-items-center g-4" aria-labelledby="introSection">
            <div class="col-md-6">
                <h1 id="introTitle">Bring Malaysian flavours to your kitchen</h1>
                <p class="lead">Discover authentic recipes, learn new skills, challenge yourself and be part of our food community.</p>
                <p><a href="/Recipes/BrowseRecipes" class="btn btn-primary btn-md">Explore Recipes &raquo;</a></p>
            </div>
            <div class="col-md-6">
                <img src="/Content/images/home/nasi lemak.png" alt="logo" class="img-fluid">
            </div>
        </section>

        <div class="container p-4 bg-secondary d-flex flex-column gap-4">
            <section aria-labelledby="browseByCategory">
                <h4 id="browseByCategoryTitle">Browse By Category</h4>
                <!-- Replace with actual categories -->
                <div class="row row-cols-2 row-cols-md-3 row-cols-lg-6 g-3">
                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Main%20Dishes"
                            class="category-card tile-main">
                            <i class="bi bi-egg-fried" aria-hidden="true"></i>
                            <span>Main Dishes</span>
                        </a>
                    </div>

                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Desserts"
                            class="category-card tile-desserts">
                            <i class="bi bi-cake2" aria-hidden="true"></i>
                            <span>Desserts</span>
                        </a>
                    </div>

                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Beverages"
                            class="category-card tile-beverages">
                            <i class="bi bi-cup-straw" aria-hidden="true"></i>
                            <span>Beverages</span>
                        </a>
                    </div>

                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Snacks"
                            class="category-card tile-snacks">
                            <i class="bi bi-cookie" aria-hidden="true"></i>
                            <span>Snacks</span>
                        </a>
                    </div>

                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Traditional"
                            class="category-card tile-traditional">
                            <i class="bi bi-basket" aria-hidden="true"></i>
                            <span>Traditional</span>
                        </a>
                    </div>

                    <div class="col">
                        <a href="/Recipes/BrowseRecipes.aspx?category=Others"
                            class="category-card tile-others">
                            <i class="bi bi-three-dots" aria-hidden="true"></i>
                            <span>Others</span>
                        </a>
                    </div>
                </div>
            </section>

            <section aria-labelledby="popularRecipes">
                <h4 id="popularRecipesTitle">Popular Recipes</h4>

                <!-- Replace with actual popular recipe -->
                <div class="card hover-lift border border-subtle" style="max-width: 320px">
                    <img src="/Content/images/home/roti canai.png" class="card-img-top" alt="roti canai">
                    <div class="card-body d-flex flex-column gap-4">
                        <div>
                            <h4 class="card-title">Roti Canai</h4>
                            <p class="card-subtitle">Malaysian &middot; Snack</p>
                        </div>

                        <p class="card-text">
                            <i class="bi bi-star-fill" aria-hidden="true"></i>
                            <span>4.6 (87)</span>
                        </p>
                    </div>
                </div>

            </section>
        </div>
    </main>

</asp:Content>
