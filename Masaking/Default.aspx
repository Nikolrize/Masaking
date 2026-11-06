<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Masaking._Default" %>

<%@ Register Src="~/Controls/RecipeCard.ascx" TagPrefix="uc" TagName="RecipeCard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="/Content/css/home.css" rel="stylesheet" />
    <link href="/Content/css/recipes.css" rel="stylesheet" />

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

                <!-- Categories -->
                <div class="row row-cols-2 row-cols-md-3 row-cols-lg-6 g-3">
                    <asp:Repeater ID="rptCategories" runat="server"
                        OnItemDataBound="rptCategories_ItemDataBound">
                        <ItemTemplate>
                            <div class="col">
                                <asp:HyperLink ID="categoryLink" runat="server">
                                    <i id="categoryIcon" runat="server"></i>
                                    <asp:Label ID="categoryName" runat="server" />
                                </asp:HyperLink>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>

            <!-- Recipes -->
            <section class="mb-2">
                <h4 class="mb-3">Popular Recipes</h4>

                <asp:Label ID="lblNoPopularRecipes" runat="server"
                    Visible="false"
                    Text="No recipes available yet."
                    CssClass="text-muted" />

                <div class="row row-cols-1 row-cols-sm-2 row-cols-xl-4 g-3">
                    <asp:Repeater ID="rptPopularRecipes" runat="server"
                        OnItemDataBound="rptPopularRecipes_ItemDataBound">
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

</asp:Content>
