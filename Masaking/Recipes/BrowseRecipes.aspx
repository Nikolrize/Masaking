<%@ Page Title="Browse Recipes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BrowseRecipes.aspx.cs" Inherits="Masaking.Recipes.BrowseRecipes" %>

<%@ Register Src="~/Controls/RecipeCard.ascx" TagPrefix="uc" TagName="RecipeCard" %>
<%@ Register Src="~/Controls/RecipeFilters.ascx" TagPrefix="uc" TagName="RecipeFilters" %>

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
                <uc:RecipeFilters ID="recipeFilters" runat="server" OnApplyFilters="Filters_ApplyFilters" />
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
</asp:Content>
