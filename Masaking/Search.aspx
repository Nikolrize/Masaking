<%@ Page Title="Search | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="Masaking.Search" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/css/search.css?v=1") %>" rel="stylesheet" />
    <main class="container site-search-page">
        <header class="site-search-hero">
        <h1>Search<i class="bi bi-search ms-3" aria-hidden="true"></i></h1>
        <p class="site-search-heading"><asp:Literal ID="litSearchSummary" runat="server" /></p>
        </header>
        <asp:Label ID="lblSearchMessage" runat="server" EnableViewState="false" role="status" />
        <asp:Panel ID="pnlRecipeResults" runat="server" CssClass="site-search-group" Visible="false">
            <h2>Recipes</h2>
            <asp:Repeater ID="rptRecipeResults" runat="server"><ItemTemplate>
                <a class="site-search-result" href='<%# ResolveUrl("~/Recipes/RecipeDetails.aspx?id=" + Eval("recipeID")) %>'><strong><%#: Eval("title") %></strong><p><%#: Eval("cuisine") %> · By <%#: Eval("displayName") %></p></a>
            </ItemTemplate></asp:Repeater>
        </asp:Panel>
        <asp:Panel ID="pnlPostResults" runat="server" CssClass="site-search-group" Visible="false">
            <h2>Community posts</h2>
            <asp:Repeater ID="rptPostResults" runat="server"><ItemTemplate>
                <a class="site-search-result" href='<%# ResolveUrl("~/Community/Post.aspx?id=" + Eval("postID")) %>'><strong><%#: Eval("displayName") %> · @<%#: Eval("username") %></strong><p><%#: Snippet(Eval("content")) %></p></a>
            </ItemTemplate></asp:Repeater>
        </asp:Panel>
    </main>
</asp:Content>
