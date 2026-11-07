<%@ Page Title="Cooking Challenges" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" MaintainScrollPositionOnPostBack="True" CodeBehind="Challenges.aspx.cs" Inherits="Masaking.Learning.Challenges" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/css/learning.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="learning-page challenge-list py-4 py-md-5">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
            <div>
                <p class="intro-label mb-2">TEST YOUR COOKING KNOWLEDGE</p>
                <h1 class="mb-2">Cooking challenges</h1>
                <p class="text-secondary mb-0">Choose a timed question challenge, or return to a saved attempt.</p>
            </div>
            <div class="d-flex flex-wrap gap-2">
                <asp:HyperLink ID="lnkProgress" runat="server" Text="My progress" NavigateUrl="~/Learning/Progress.aspx" CssClass="btn btn-outline-success" />
                <asp:HyperLink ID="lnkSimulations" runat="server" Text="Cooking simulations" NavigateUrl="~/Learning/Simulations.aspx" CssClass="btn btn-outline-success" />
            </div>
        </div>
        <asp:Panel ID="pnlFilters" runat="server" DefaultButton="btnSearch" CssClass="card p-3 p-md-4 mb-4">
            <div class="row g-3 align-items-end">
                <div class="col-12 col-md-6">
                    <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch" Text="Challenge title" CssClass="form-label" />
                    <asp:TextBox ID="txtSearch" runat="server" MaxLength="150" CssClass="form-control" />
                </div>
                <div class="col-12 col-md-3">
                    <asp:Label ID="lblDifficulty" runat="server" AssociatedControlID="ddlDifficulty" Text="Difficulty" CssClass="form-label" />
                    <asp:DropDownList ID="ddlDifficulty" runat="server" CssClass="form-select">
                        <asp:ListItem Value="">All difficulties</asp:ListItem>
                        <asp:ListItem Value="Easy">Easy</asp:ListItem>
                        <asp:ListItem Value="Medium">Medium</asp:ListItem>
                        <asp:ListItem Value="Hard">Hard</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-12 col-md-3">
                    <asp:Button ID="btnSearch" runat="server" Text="Find challenges" CssClass="btn btn-success w-100" OnClick="btnSearch_Click" />
                </div>
            </div>
        </asp:Panel>
        <asp:Label ID="lblCount" runat="server" CssClass="small text-secondary d-block mb-3" role="status" />
        <asp:ListView ID="lvChallenges" runat="server" OnPagePropertiesChanging="lvChallenges_PagePropertiesChanging">
            <LayoutTemplate>
                <div class="row g-4"><asp:PlaceHolder ID="itemPlaceholder" runat="server" /></div>
            </LayoutTemplate>
            <ItemTemplate>
                <div class="col-12 col-md-6 col-xl-4">
                    <article class="card h-100 p-3 p-md-4">
                        <div class="d-flex justify-content-between align-items-center gap-2 mb-3">
                            <span class="challenge-symbol" aria-hidden="true"><i class="bi bi-patch-question"></i></span>
                            <span class='<%# DifficultyClass(Eval("difficulty")) %>'><%#: Eval("difficulty") %></span>
                        </div>
                        <h2 class="h5"><%#: Eval("title") %></h2>
                        <p class="text-secondary challenge-description"><%#: Eval("description") %></p>
                        <div class="d-flex flex-wrap gap-2 mb-3">
                            <span class="detail-tag"><i class="bi bi-stopwatch me-1" aria-hidden="true"></i><%#: Eval("minutes", "{0:0.#}") %> min</span>
                            <span class="detail-tag"><i class="bi bi-list-check me-1" aria-hidden="true"></i><%#: Eval("questionCount") %> questions</span>
                        </div>
                        <p class="small text-secondary mb-2">Attempts: <strong><%#: Eval("attemptCount") %></strong> &middot; Completed: <strong><%#: Eval("completedCount") %></strong></p>
                        <p class="small mb-3"><%#: SavedAttemptText(Eval("savedStatus")) %></p>
                        <asp:HyperLink ID="lnkStartChallenge" runat="server" CssClass="btn challenge-button w-100 mt-auto" Text='<%# ChallengeButtonText(Eval("savedAttemptID"), Eval("attemptCount")) %>' NavigateUrl='<%# ChallengeUrl(Eval("challengeID"), Eval("savedAttemptID")) %>' />
                    </article>
                </div>
            </ItemTemplate>
            <EmptyDataTemplate>
                <div class="empty-message">
                    <i class="bi bi-search fs-3 d-block mb-2" aria-hidden="true"></i>
                    No published challenges match your filters. Try another title or difficulty.
                </div>
            </EmptyDataTemplate>
        </asp:ListView>
        <asp:DataPager ID="pagerChallenges" runat="server" PagedControlID="lvChallenges" PageSize="6" class="challenge-pages d-flex flex-wrap justify-content-center gap-2 mt-4" aria-label="Challenge pages">
            <Fields>
                <asp:NextPreviousPagerField ShowFirstPageButton="False" ShowLastPageButton="False" ShowNextPageButton="False" PreviousPageText="Previous" ButtonCssClass="btn btn-outline-success" />
                <asp:NumericPagerField ButtonCount="5" NumericButtonCssClass="btn btn-outline-success" CurrentPageLabelCssClass="btn btn-success" NextPreviousButtonCssClass="btn btn-outline-success" />
                <asp:NextPreviousPagerField ShowFirstPageButton="False" ShowLastPageButton="False" ShowPreviousPageButton="False" NextPageText="Next" ButtonCssClass="btn btn-outline-success" />
            </Fields>
        </asp:DataPager>
        <p class="small text-secondary mt-4 mb-0">Opening a challenge does not start its timer. Select Resume challenge on the attempt page when you are ready. Save and pause stops the timer.</p>
    </main>
</asp:Content>
