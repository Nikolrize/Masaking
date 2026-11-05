<%@ Page Title="Search | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="Masaking.Search" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link runat="server" href="~/Content/css/search.css?v=1" rel="stylesheet" />
    <main class="container site-search-page">
        <header class="site-search-hero">
        <h1>Search<i class="bi bi-search ms-3" aria-hidden="true"></i></h1>
        <p class="site-search-heading"><asp:Label ID="lblSearchSummary" runat="server" class="d-inline" /></p>
        </header>
        <asp:Label ID="lblSearchMessage" runat="server" EnableViewState="false" role="status" />
        <asp:Panel ID="pnlRecipeResults" runat="server" class="site-search-group" Visible="false">
            <h2>Recipes</h2>
            <asp:Repeater ID="rptRecipeResults" runat="server" OnItemDataBound="rptRecipeResults_ItemDataBound"><ItemTemplate>
                <asp:HyperLink ID="lnkRecipeResult" runat="server" class="site-search-result"><strong><asp:Label ID="lblRecipeTitle" runat="server" class="d-inline" /></strong><p><asp:Label ID="lblCuisine" runat="server" class="d-inline" /> · By <asp:Label ID="lblRecipeAuthor" runat="server" class="d-inline" /></p></asp:HyperLink>
            </ItemTemplate></asp:Repeater>
        </asp:Panel>
        <asp:Panel ID="pnlPostResults" runat="server" class="site-search-group" Visible="false">
            <h2>Community posts</h2>
            <asp:Repeater ID="rptPostResults" runat="server" OnItemDataBound="rptPostResults_ItemDataBound"><ItemTemplate>
                <asp:HyperLink ID="lnkPostResult" runat="server" class="site-search-result"><strong><asp:Label ID="lblPostAuthor" runat="server" class="d-inline" /> · @<asp:Label ID="lblUsername" runat="server" class="d-inline" /></strong><p><asp:Label ID="lblSnippet" runat="server" class="d-inline" /></p></asp:HyperLink>
            </ItemTemplate></asp:Repeater>
        </asp:Panel>
    </main>
</asp:Content>
