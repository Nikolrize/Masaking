<%@ Page Title="Learn & Improve" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Masaking.Learning.LearningHome" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/css/learning.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="learning-page py-4 py-md-5">
        <div class="d-flex align-items-center justify-content-between gap-3 mb-4">
            <div>
                <p class="intro-label mb-2">YOUR COOKING JOURNEY</p>
                <h1 class="mb-2">Learn &amp; Improve</h1>
                <p class="text-secondary mb-0">Try our interactive cooking simulations and quiz challenges.</p>
            </div>
            <div class="heading-icon d-none d-sm-flex" aria-hidden="true">
                <i class="bi bi-egg-fried"></i>
            </div>
        </div>

        <div class="row g-4">
            <div class="col-12 col-lg-7">
                <section class="card p-3 p-md-4 h-100" aria-labelledby="simulationHeading">
                    <div class="d-flex align-items-start justify-content-between gap-3 mb-4">
                        <div class="d-flex gap-3">
                            <span class="section-icon" aria-hidden="true"><i class="bi bi-egg-fried"></i></span>
                            <div>
                                <h2 id="simulationHeading" class="h5 mb-1">Cooking Simulation</h2>
                                <p class="small text-secondary mb-0">Learn by doing, step by step. No time limit.</p>
                            </div>
                        </div>
                        <asp:HyperLink ID="lnkAllSimulations" runat="server" Text="View all"
                            NavigateUrl="~/Learning/Simulations.aspx" CssClass="browse-link small" />
                    </div>

                    <asp:Panel ID="pnlNoSimulations" runat="server" Visible="False" CssClass="empty-message">
                        <i class="bi bi-egg-fried fs-2 d-block mb-2" aria-hidden="true"></i>
                        <p class="fw-bold mb-1">Your next cooking adventure is on its way.</p>
                        <p class="small text-secondary mb-0">Published simulations will appear here when they are ready.</p>
                    </asp:Panel>

                    <asp:Repeater ID="rptSimulations" runat="server">
                        <ItemTemplate>
                            <article class="simulation-preview">
                                <div class="d-flex align-items-center gap-3 mb-3">
                                    <asp:Image ID="imgRecipe" runat="server" ImageUrl='<%# Eval("imageUrl") %>' AlternateText='<%# Eval("title") %>' CssClass="preview-image" Visible='<%# !String.IsNullOrEmpty(Convert.ToString(Eval("imageUrl"))) %>' />
                                    <asp:Panel ID="pnlImagePlaceholder" runat="server" CssClass="preview-placeholder" Visible='<%# String.IsNullOrEmpty(Convert.ToString(Eval("imageUrl"))) %>' aria-hidden="true"><i class="bi bi-egg-fried"></i></asp:Panel>
                                    <div class="preview-title">
                                        <h3 class="h6 fw-bold mb-2"><%#: Eval("title") %></h3>
                                        <div class="d-flex flex-wrap gap-2 small text-secondary">
                                            <span><%#: Eval("difficulty") %></span>
                                            <span aria-hidden="true">|</span>
                                            <span><%#: Eval("stepCount") %> steps</span>
                                            <span aria-hidden="true">|</span>
                                            <span>Untimed</span>
                                        </div>
                                    </div>
                                </div>
                                <div class="progress mb-2" role="progressbar" aria-label="Simulation steps completed"
                                    aria-valuemin="0" aria-valuemax="100" aria-valuenow='<%# Eval("percentage") %>'>
                                    <div class="progress-bar" style='<%# "width:" + Eval("percentage") + "%;" %>'></div>
                                </div>
                                <div class="d-flex justify-content-between gap-2 small text-secondary mb-3">
                                    <span><%#: Eval("progressText") %></span><span><%#: Eval("percentage") %>%</span>
                                </div>
                                <p class="small text-secondary mb-3"><%#: Eval("description") %></p>
                                <div class="preview-note mb-3">
                                    <p class="small fw-bold mb-1"><%#: Eval("instructionHeading") %></p>
                                    <p class="small mb-0"><%#: Eval("instructionPreview") %></p>
                                </div>
                                <p class="small text-secondary mb-3">
                                    <span class="fw-bold">Ingredients:</span> <%#: Eval("ingredientPreview") %>
                                </p>
                                <asp:HyperLink ID="lnkSimulation" runat="server" Text='<%# Eval("actionText") %>' NavigateUrl='<%# "~/Learning/CookingSimulation.aspx?id=" + Eval("simulationID") %>' CssClass="btn btn-primary w-100" />
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </section>
            </div>

            <div class="col-12 col-lg-5">
                <div class="d-flex flex-column gap-4">
                    <section class="card p-3 p-md-4" aria-labelledby="challengeHeading">
                        <div class="d-flex align-items-start justify-content-between gap-3 mb-3">
                            <div class="d-flex gap-3">
                                <span class="section-icon" aria-hidden="true"><i class="bi bi-stopwatch"></i></span>
                                <div>
                                    <h2 id="challengeHeading" class="h5 mb-1">Quiz Challenge</h2>
                                    <p class="small text-secondary mb-0">Test what you know about cooking.</p>
                                </div>
                            </div>
                            <asp:HyperLink ID="lnkAllQuizzes" runat="server" Text="View all"
                                NavigateUrl="~/Learning/Challenges.aspx" CssClass="browse-link small" />
                        </div>
                        <asp:Panel ID="pnlNoChallenges" runat="server" Visible="False" CssClass="empty-message">
                            <p class="fw-bold mb-1">More challenges are coming.</p>
                            <p class="small text-secondary mb-0">Check back when a quiz has been published.</p>
                        </asp:Panel>
                        <asp:Repeater ID="rptChallenge" runat="server">
                            <ItemTemplate>
                                <article>
                                    <div class="challenge-time d-flex justify-content-center align-items-center gap-3 mb-3">
                                        <i class="bi bi-stopwatch" aria-hidden="true"></i>
                                        <div><p class="time-value mb-0"><%#: Eval("timeText") %></p>
                                            <p class="small mb-0">Time limit</p></div>
                                    </div>
                                    <h3 class="h6 fw-bold"><%#: Eval("title") %></h3>
                                    <p class="small text-secondary"><%#: Eval("description") %></p>
                                    <div class="d-flex flex-wrap gap-2 mb-3">
                                        <span class="detail-tag"><%#: Eval("difficulty") %></span>
                                        <span class="detail-tag"><%#: Eval("questionCount") %> questions</span>
                                    </div>
                                    <asp:HyperLink ID="lnkChallenge" runat="server" Text="Open challenge"
                                        NavigateUrl='<%# "~/Learning/TakeQuiz.aspx?id=" + Eval("challengeID") %>'
                                        CssClass="btn challenge-button w-100" />
                                    <p class="small text-secondary mt-2 mb-0">The timer starts when you resume the quiz.</p>
                                </article>
                            </ItemTemplate>
                        </asp:Repeater>
                    </section>

                    <section class="card p-3 p-md-4" aria-labelledby="progressHeading">
                        <div class="d-flex justify-content-between align-items-start gap-3 mb-3">
                            <div><h2 id="progressHeading" class="h5 mb-1">Your Progress</h2>
                                <p class="small text-secondary mb-0">Keep building your cooking confidence.</p></div>
                            <i class="bi bi-trophy progress-icon" aria-hidden="true"></i>
                        </div>
                        <div class="d-flex justify-content-between gap-2 small mb-2">
                            <span>Simulations completed</span>
                            <asp:Label ID="lblSimulationProgress" runat="server" CssClass="fw-bold" />
                        </div>
                        <div id="simulationProgressTrack" runat="server" class="progress mb-3"
                            role="progressbar" aria-label="Published simulations completed" aria-valuemin="0" aria-valuemax="100">
                            <div id="simulationProgressBar" runat="server" class="progress-bar"></div>
                        </div>
                        <div class="d-flex justify-content-between gap-2 small mb-2">
                            <span>Quizzes completed</span>
                            <asp:Label ID="lblQuizProgress" runat="server" CssClass="fw-bold" />
                        </div>
                        <div id="quizProgressTrack" runat="server" class="progress mb-3" role="progressbar" aria-label="Published quizzes completed" aria-valuemin="0" aria-valuemax="100">
                            <div id="quizProgressBar" runat="server" class="progress-bar"></div>
                        </div>
                        <p class="small text-secondary mb-3">
                            <asp:Label ID="lblSavedAttempts" runat="server" CssClass="fw-bold" /> unfinished attempts in your history.
                            The bars count each currently published activity once.
                        </p>
                        <asp:HyperLink ID="lnkProgress" runat="server" Text="View my progress" NavigateUrl="~/Learning/Progress.aspx" CssClass="btn btn-outline-success w-100" />
                    </section>
                </div>
            </div>
        </div>
    </main>
</asp:Content>
