<%@ Page Title="Cooking Simulations" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Simulations.aspx.cs" Inherits="Masaking.Learning.Simulations" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4 py-md-5">
        <div class="d-flex flex-wrap gap-2 mb-3">
            <asp:HyperLink ID="lnkQuizzes" runat="server" Text="Browse quizzes" NavigateUrl="~/Learning/Challenges.aspx" CssClass="btn btn-outline-success" />
            <asp:HyperLink ID="lnkProgress" runat="server" Text="My progress" NavigateUrl="~/Learning/Progress.aspx" CssClass="btn btn-outline-secondary" />
        </div>
        <h1>Cooking Simulations</h1>
        <p class="text-secondary">Practice the ingredient order at your own pace. There is no time limit.</p>
        <asp:Panel ID="pnlEmpty" runat="server" Visible="False" CssClass="alert alert-info">
            No simulations are published yet. Please check again later.
        </asp:Panel>
        <div class="row g-4">
            <asp:Repeater ID="rptSimulations" runat="server">
                <ItemTemplate>
                    <div class="col-12 col-md-6 col-xl-4">
                        <article class="card p-3 p-md-4 h-100">
                            <h2 class="h5"><%#: Eval("title") %></h2>
                            <p class="small text-secondary">Recipe: <%#: Eval("recipeTitle") %></p>
                            <p><%#: Eval("description") %></p>
                            <p class="small"><%#: Eval("difficulty") %> | <%#: Eval("stepCount") %> steps | Untimed</p>
                            <asp:HyperLink ID="lnkPlay" runat="server" Text='<%# Eval("actionText") %>' CssClass="btn btn-success mt-auto" NavigateUrl='<%# "~/Learning/CookingSimulation.aspx?id=" + Eval("simulationID") %>' />
                        </article>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </main>
</asp:Content>