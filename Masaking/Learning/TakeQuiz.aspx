<%@ Page Title="Cooking Challenge" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" MaintainScrollPositionOnPostBack="True" CodeBehind="TakeQuiz.aspx.cs" Inherits="Masaking.Learning.TakeQuiz" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/css/learning.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="learning-page challenge-play py-4 py-md-5">
        <asp:HyperLink ID="lnkBack" runat="server" Text="Back to challenges" NavigateUrl="~/Learning/Challenges.aspx" CssClass="btn btn-outline-success mb-4" />
        <p class="intro-label mb-2">COOKING KNOWLEDGE CHALLENGE</p>
        <h1 class="mb-4"><asp:Label ID="lblTitle" runat="server" Text="Cooking challenge" /></h1>
        <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />

        <asp:Panel ID="pnlStart" runat="server" CssClass="card p-3 p-md-4" Visible="False">
            <asp:Label ID="lblDescription" runat="server" CssClass="challenge-description d-block mb-4" />
            <div class="d-flex flex-wrap gap-3 mb-4">
                <asp:Label ID="lblTimeLimit" runat="server" CssClass="detail-tag" />
                <asp:Label ID="lblQuestionCount" runat="server" CssClass="detail-tag" />
            </div>
            <p class="text-secondary">Prepare your attempt, then select Resume challenge when you are ready. Save and pause stops the timer; leaving the page while it is running does not.</p>
            <asp:Button ID="btnStart" runat="server" Text="Start / reopen attempt" CssClass="btn btn-success" OnClick="btnStart_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlAttempt" runat="server" CssClass="card p-3 p-md-4" Visible="False">
            <div class="d-flex flex-wrap align-items-center justify-content-between gap-2 mb-4">
                <asp:Label ID="lblAttempt" runat="server" CssClass="small text-secondary" />
                <asp:Label ID="lblStatus" runat="server" CssClass="attempt-status" />
            </div>

            <asp:Panel ID="pnlTimer" runat="server" CssClass="mb-4">
                <div id="challengeTimer" class="timer-panel"
                    data-running="<%= TimerRunning ? "true" : "false" %>"
                    data-seconds="<%= CountdownSeconds %>"
                    data-total="<%= TotalSeconds %>">
                    <div class="d-flex align-items-center gap-2 mb-2">
                        <i class="bi bi-hourglass-split" aria-hidden="true"></i>
                        <asp:Label ID="lblRemaining" runat="server" ClientIDMode="Static" CssClass="fw-bold" />
                    </div>
                    <div class="progress">
                        <div id="timeBar" class="progress-bar" role="progressbar" aria-label="Time remaining"
                            aria-valuemin="0" aria-valuemax="<%= TotalSeconds %>"
                            aria-valuenow="<%= CountdownSeconds %>" style="width:<%= RemainingPercentage %>%"></div>
                    </div>
                    <p id="timerWarning" class="timer-warning small fw-semibold mt-2 mb-0" role="alert" hidden>Only 10 seconds remain. Finish your answer or submit the challenge.</p>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlPaused" runat="server" Visible="False">
                <div class="pause-message mb-4">
                    <i class="bi bi-pause-circle fs-2" aria-hidden="true"></i>
                    <h2 class="h5 mt-2">Ready when you are</h2>
                    <p class="text-secondary mb-0">Your attempt is paused and the timer is stopped. Resume to view the questions and continue.</p>
                </div>
                <asp:Button ID="btnResume" runat="server" Text="Resume challenge" CssClass="btn btn-success" OnClick="btnResume_Click" />
            </asp:Panel>

            <asp:Panel ID="pnlQuestion" runat="server" Visible="False">
                <asp:Label ID="lblQuestionNumber" runat="server" CssClass="text-secondary small d-block mb-2" />
                <h2 class="h4 mb-4"><asp:Label ID="lblQuestion" runat="server" /></h2>

                <asp:RadioButtonList ID="rblAnswers" runat="server" ClientIDMode="Static"
                    RepeatLayout="UnorderedList" CssClass="answer-options mb-4" aria-label="Choose an answer"
                    AutoPostBack="True" OnSelectedIndexChanged="rblAnswers_SelectedIndexChanged">
                    <asp:ListItem Value="A">Option A</asp:ListItem>
                    <asp:ListItem Value="B">Option B</asp:ListItem>
                    <asp:ListItem Value="C">Option C</asp:ListItem>
                    <asp:ListItem Value="D">Option D</asp:ListItem>
                </asp:RadioButtonList>

                <div class="challenge-actions d-flex flex-wrap gap-2">
                    <asp:LinkButton ID="btnPrevious" runat="server" CssClass="btn btn-outline-secondary"
                        OnClick="btnPrevious_Click" CausesValidation="False" aria-label="Previous question">
                        <i class="bi bi-arrow-left" aria-hidden="true"></i><span class="ms-2">Previous</span>
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnNext" runat="server" CssClass="btn btn-outline-success"
                        OnClick="btnNext_Click" CausesValidation="False" aria-label="Next question">
                        <span class="me-2">Next</span><i class="bi bi-arrow-right" aria-hidden="true"></i>
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnPause" runat="server" CssClass="btn btn-outline-secondary"
                        OnClick="btnPause_Click" CausesValidation="False" aria-label="Save answer and pause challenge">
                        <i class="bi bi-pause-circle" aria-hidden="true"></i><span class="ms-2">Save and pause</span>
                    </asp:LinkButton>
                    <asp:Button ID="btnSubmit" runat="server" Text="Submit challenge" CssClass="btn btn-success"
                        OnClientClick="return confirm('Submit your challenge? You cannot change answers afterwards.');" OnClick="btnSubmit_Click" />
                </div>

                <div class="save-note text-secondary mt-4">
                    <asp:Label ID="lblSaveState" runat="server" ClientIDMode="Static" role="status" CssClass="d-block mb-1" />
                    <p class="mb-0">Each selected answer is saved when the page finishes updating. Unanswered questions count as incorrect when submitted.</p>
                </div>
                <asp:Button ID="btnTimeout" runat="server" Text="Finish expired challenge" ClientIDMode="Static"
                    UseSubmitBehavior="False" Style="display:none;" OnClick="btnTimeout_Click" />
            </asp:Panel>

            <asp:Panel ID="pnlResult" runat="server" Visible="False">
                <i class="bi bi-check2-circle result-icon" aria-hidden="true"></i>
                <h2 class="h4 mt-2">Challenge result</h2>
                <asp:Label ID="lblResult" runat="server" CssClass="fs-3 fw-bold d-block mb-2" />
                <asp:Label ID="lblTimeUsed" runat="server" CssClass="text-secondary d-block mb-4" />
                <asp:HyperLink ID="lnkMoreQuizzes" runat="server" Text="Browse challenges" NavigateUrl="~/Learning/Challenges.aspx" CssClass="btn btn-outline-success" />
            </asp:Panel>
        </asp:Panel>
    </main>
    <script src="<%= ResolveUrl("~/Scripts/challenge.js") %>"></script>
</asp:Content>
