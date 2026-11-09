<%@ Page Title="My Learning Progress | Masaking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" MaintainScrollPositionOnPostBack="True" CodeBehind="Progress.aspx.cs" Inherits="Masaking.Learning.Progress" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/css/learning.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="learning-page progress-page py-4 py-md-5">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
            <div>
                <p class="intro-label mb-2">YOUR LEARNING JOURNEY</p>
                <h1>My learning progress</h1>
                <p class="text-secondary mb-0">Review your challenge results, cooking practice and saved attempts.</p>
            </div>
            <div class="d-flex flex-wrap gap-2">
                <asp:HyperLink ID="lnkQuizzes" runat="server" Text="Browse challenges" NavigateUrl="~/Learning/Challenges.aspx" CssClass="btn btn-success" />
                <asp:HyperLink ID="lnkSimulation" runat="server" Text="Practise cooking" NavigateUrl="~/Learning/Simulations.aspx" CssClass="btn btn-outline-success" />
            </div>
        </div>

        <section aria-labelledby="challengeSummaryHeading" class="mb-4">
            <h2 id="challengeSummaryHeading" class="h5 mb-3">Challenge overview</h2>
            <div class="row g-3">
                <div class="col-12 col-sm-6 col-xl-4">
                    <div class="card summary-card p-3 h-100">
                        <i class="bi bi-patch-check summary-icon" aria-hidden="true"></i>
                        <h3 class="h6 mt-2">Challenges completed</h3>
                        <asp:Label ID="lblChallengesCompleted" runat="server" CssClass="fs-2 fw-bold d-block" />
                        <p class="small text-secondary mb-0">Different challenges submitted at least once. Repeats count once; timeouts are excluded.</p>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-xl-4">
                    <div class="card summary-card p-3 h-100">
                        <i class="bi bi-flag summary-icon" aria-hidden="true"></i>
                        <h3 class="h6 mt-2">Finished attempts</h3>
                        <asp:Label ID="lblFinished" runat="server" CssClass="fs-2 fw-bold d-block" />
                        <p class="small text-secondary mb-0">Submitted plus time-expired attempts. Repeated attempts count separately.</p>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-xl-4">
                    <div class="card summary-card p-3 h-100">
                        <i class="bi bi-send-check summary-icon" aria-hidden="true"></i>
                        <h3 class="h6 mt-2">Submitted attempts</h3>
                        <asp:Label ID="lblCompleted" runat="server" CssClass="fs-2 fw-bold d-block" />
                        <p class="small text-secondary mb-0">Attempts you finished using Submit challenge.</p>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-xl-4">
                    <div class="card summary-card p-3 h-100">
                        <i class="bi bi-hourglass-bottom summary-icon" aria-hidden="true"></i>
                        <h3 class="h6 mt-2">Time-expired attempts</h3>
                        <asp:Label ID="lblTimedOut" runat="server" CssClass="fs-2 fw-bold d-block" />
                        <p class="small text-secondary mb-0">Includes expired attempts awaiting result finalisation.</p>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-xl-4">
                    <div class="card summary-card p-3 h-100">
                        <i class="bi bi-pause-circle summary-icon" aria-hidden="true"></i>
                        <h3 class="h6 mt-2">Unfinished attempts</h3>
                        <asp:Label ID="lblUnfinished" runat="server" CssClass="fs-2 fw-bold d-block" />
                        <p class="small text-secondary mb-0">Paused attempts and running attempts with time left.</p>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-xl-4">
                    <div class="card summary-card p-3 h-100">
                        <i class="bi bi-trophy summary-icon" aria-hidden="true"></i>
                        <h3 class="h6 mt-2">Best saved result</h3>
                        <asp:Label ID="lblBest" runat="server" CssClass="fs-2 fw-bold d-block" />
                        <p class="small text-secondary mb-0">Highest percentage from a finalised submitted or time-expired attempt.</p>
                    </div>
                </div>
            </div>
        </section>

        <asp:Label ID="lblPendingSummary" runat="server" Visible="False" CssClass="alert alert-warning d-block mb-4" role="status" />

        <section class="card p-3 p-md-4" aria-labelledby="challengeHistoryHeading">
            <h2 id="challengeHistoryHeading" class="h5">Challenge history</h2>
            <p class="small text-secondary">Running attempts keep counting down while you are away. Use Save and pause inside the challenge to stop the timer.</p>
            <div class="table-responsive">
                <asp:GridView ID="gvAttempts" runat="server" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" CssClass="table align-middle" GridLines="None" EmptyDataText="You have not started any challenges yet." OnPageIndexChanging="gvAttempts_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="title" HeaderText="Challenge"><ItemStyle CssClass="history-title" /></asp:BoundField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate><span class='<%# StatusClass(Eval("historyStatus")) %>'><%#: Eval("displayStatus") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="resultText" HeaderText="Result" />
                        <asp:BoundField DataField="startedLocal" HeaderText="Started (MYT)" DataFormatString="{0:dd MMM yyyy HH:mm}" NullDisplayText="Not recorded" />
                        <asp:BoundField DataField="finishedText" HeaderText="Result saved (MYT)" />
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkAttempt" runat="server" Text='<%# Eval("actionText") %>' NavigateUrl='<%# "TakeQuiz.aspx?attempt=" + Eval("attemptID") %>' CssClass="btn btn-sm btn-outline-success" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <p class="small text-secondary mb-0">All dates use Malaysian time (MYT, UTC+8). Result saved shows when your result was finalised; this may be later than timer expiry if you were away. Unfinished attempts have no completion date.</p>
        </section>

        <section class="card p-3 p-md-4 mt-4" aria-labelledby="simulationHistoryHeading">
            <h2 id="simulationHistoryHeading" class="h5">Simulation history</h2>
            <asp:Label ID="lblSimulationSummary" runat="server" CssClass="d-block mb-3" />
            <p class="small text-secondary">Each ingredient choice is saved. Restarted runs stay in your history. Unpublished simulations retain their records but cannot currently be opened. Dates use Malaysian time.</p>
            <div class="table-responsive">
                <asp:GridView ID="gvSimulations" runat="server" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" GridLines="None" CssClass="table align-middle" EmptyDataText="You have not started any simulations yet." OnPageIndexChanging="gvSimulations_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="title" HeaderText="Simulation"><ItemStyle CssClass="history-title" /></asp:BoundField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate><span class='<%# StatusClass(Eval("status")) %>'><%#: Eval("displayStatus") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="progressText" HeaderText="Steps completed" />
                        <asp:BoundField DataField="mistakes" HeaderText="Incorrect choices" />
                        <asp:BoundField DataField="resultText" HeaderText="Accuracy" />
                        <asp:BoundField DataField="startedLocal" HeaderText="Started (MYT)" DataFormatString="{0:dd MMM yyyy HH:mm}" NullDisplayText="Not recorded" />
                        <asp:BoundField DataField="finishedText" HeaderText="Finished (MYT)" />
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkSimulationAttempt" runat="server" Text='<%# Eval("actionText") %>' CssClass="btn btn-sm btn-outline-success" Visible='<%# Convert.ToBoolean(Eval("canOpen")) %>' NavigateUrl='<%# "CookingSimulation.aspx?id=" + Eval("simulationID") + "&attempt=" + Eval("simulationProgressID") %>' />
                                <asp:Label ID="lblUnavailable" runat="server" Text='<%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("availabilityText"))) %>' CssClass="small text-secondary" Visible='<%# !Convert.ToBoolean(Eval("canOpen")) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>
