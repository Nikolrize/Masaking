<%@ Page Title="My recipes | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyRecipes.aspx.cs" Inherits="Masaking.Recipes.MyRecipes" %>

<%@ Register Src="~/Controls/RecipeCard.ascx" TagPrefix="uc" TagName="RecipeCard" %>
<%@ Register Src="~/Controls/RecipeFilters.ascx" TagPrefix="uc" TagName="RecipeFilters" %>

<asp:Content ID="BodyContent"
    ContentPlaceHolderID="MainContent" runat="server">

    <link href="/Content/css/recipes.css" rel="stylesheet" />

    <main class="container py-4 recipe-browse-page">
        <h1>My Recipes</h1>
        <p class="lead">
            Your creations and favourite recipes, all in one place.
       
        </p>

        <nav class="my-recipe-tabs d-flex flex-wrap gap-2 mb-4">
            <asp:LinkButton ID="tabAll" runat="server"
                Text="All" CommandArgument="All"
                CausesValidation="false"
                OnCommand="RecipeTab_Command" />

            <asp:LinkButton ID="tabApproved" runat="server"
                Text="Approved" CommandArgument="Approved"
                CausesValidation="false" OnCommand="RecipeTab_Command" />

            <asp:LinkButton ID="tabPending" runat="server"
                Text="Pending" CommandArgument="Pending"
                CausesValidation="false" OnCommand="RecipeTab_Command" />

            <asp:LinkButton ID="tabRejected" runat="server"
                Text="Rejected" CommandArgument="Rejected"
                CausesValidation="false" OnCommand="RecipeTab_Command" />

            <asp:LinkButton ID="tabFavourites" runat="server"
                Text="Favourites" CommandArgument="Favourites"
                CausesValidation="false" OnCommand="RecipeTab_Command" />
        </nav>

        <asp:Panel runat="server" DefaultButton="btnSearch" CssClass="input-group mb-4">
            <asp:TextBox ID="txtSearch" runat="server"
                CssClass="form-control"
                placeholder="Search this collection..." />

            <asp:Button ID="btnSearch" runat="server"
                Text="Search" CssClass="btn btn-primary"
                CausesValidation="false"
                OnClick="ReloadRecipes" />
        </asp:Panel>

        <div class="row g-4">
            <aside class="col-lg-3">
                <uc:RecipeFilters ID="recipeFilters" runat="server"
                    IncludeOwnRecipes="true"
                    OnApplyFilters="Filters_ApplyFilters" />
            </aside>

            <section class="col-lg-9">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-3">

                    <div>
                        <h4 class="mb-1">
                            <asp:Label ID="collectionTitle" runat="server" />
                        </h4>
                        <asp:Label ID="recipeCount" runat="server"
                            CssClass="small text-muted" />
                    </div>

                    <div class="d-flex align-items-center gap-2">
                        <asp:Label runat="server"
                            AssociatedControlID="ddlSort"
                            Text="Sort by"
                            CssClass="small text-muted text-nowrap" />

                        <asp:DropDownList ID="ddlSort" runat="server"
                            CssClass="form-select w-auto"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ReloadRecipes">

                            <asp:ListItem Value="nameAsc">Name: A-Z</asp:ListItem>
                            <asp:ListItem Value="nameDesc">Name: Z-A</asp:ListItem>
                            <asp:ListItem Value="newest">Newest</asp:ListItem>
                            <asp:ListItem Value="rating">Highest rated</asp:ListItem>
                            <asp:ListItem Value="reviews">Most reviewed</asp:ListItem>
                            <asp:ListItem Value="cost">Lowest cost</asp:ListItem>
                            <asp:ListItem Value="time">Quickest</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>



                <asp:Panel ID="pnlEmpty" runat="server" Visible="false"
                    CssClass="card p-4 text-center text-muted">
                    No recipes found in this collection.
       
                </asp:Panel>

                <div class="row row-cols-1 row-cols-sm-2 row-cols-xl-3 g-3">
                    <asp:Repeater ID="rptRecipes" runat="server"
                        OnItemDataBound="rptRecipes_ItemDataBound">

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
