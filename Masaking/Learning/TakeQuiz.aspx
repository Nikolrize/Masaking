<%@ Page Title="Take Quiz" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TakeQuiz.aspx.cs" Inherits="Masaking.Learning.TakeQuiz" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4 py-md-5">

        <asp:HyperLink ID="lnkBack" runat="server" Text="Back to quizzes" NavigateUrl="~/Learning/Challenges.aspx" CssClass="btn btn-outline-success mb-3" />

        <asp:Label ID="lblTitle" runat="server" Text="Cooking Quiz" CssClass="h1 d-block mb-3" />
        <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />

        <asp:Panel ID="pnlStart" runat="server" CssClass="card p-3 p-md-4 mb-4" Visible="False">
            <asp:Label ID="lblDescription" runat="server" CssClass="d-block mb-3" />
            <asp:Label ID="lblTimeLimit" runat="server" CssClass="d-block mb-2" />
            <asp:Label ID="lblQuestionCount" runat="server" CssClass="d-block mb-3" />

            <p class="text-secondary"> Start or reopen your saved attempt, then select Resume quiz to begin. Use Save and pause to stop the timer.</p>

            <asp:Button ID="btnStart" runat="server" Text="Start / reopen attempt" CssClass="btn btn-success" OnClick="btnStart_Click" />

        </asp:Panel>

        <asp:Panel ID="pnlAttempt" runat="server" CssClass="card p-3 p-md-4" Visible="False">

            <asp:Label ID="lblAttempt" runat="server" CssClass="small text-secondary d-block mb-2" />
            <asp:Label ID="lblStatus" runat="server" CssClass="d-block mb-2" />
            <asp:Label ID="lblRemaining" runat="server" ClientIDMode="Static" CssClass="fw-bold d-block mb-3" />
            <asp:Panel ID="pnlPaused" runat="server" Visible="False">
                <p>Your quiz is paused. Resume when you are ready.</p>
                <asp:Button ID="btnResume" runat="server" Text="Resume quiz" CssClass="btn btn-success" OnClick="btnResume_Click" />
            </asp:Panel>

            <asp:Panel ID="pnlQuestion" runat="server" Visible="False">
                <asp:Label ID="lblQuestionNumber" runat="server" CssClass="text-secondary d-block mb-2" />
                <asp:Label ID="lblQuestion" runat="server" CssClass="h5 d-block mb-3" />

                <asp:RadioButtonList ID="rblAnswers" runat="server" CssClass="mb-3" AutoPostBack="True" OnSelectedIndexChanged="rblAnswers_SelectedIndexChanged">
                    <asp:ListItem Value="A">Option A</asp:ListItem>
                    <asp:ListItem Value="B">Option B</asp:ListItem>
                    <asp:ListItem Value="C">Option C</asp:ListItem>
                    <asp:ListItem Value="D">Option D</asp:ListItem>
                </asp:RadioButtonList>

                <p class="small text-secondary"> Each selected answer is saved when the page finishes updating. Unanswered questions count as incorrect when submitted.
                </p>

                <div class="d-flex flex-wrap gap-2">
                    <asp:Button ID="btnPrevious" runat="server" Text="Previous" CssClass="btn btn-outline-secondary" OnClick="btnPrevious_Click" />

                    <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="btn btn-outline-success" OnClick="btnNext_Click" />

                    <asp:Button ID="btnPause" runat="server" Text="Save and pause" CssClass="btn btn-outline-secondary" OnClick="btnPause_Click" />

                    <asp:Button ID="btnSubmit" runat="server" Text="Submit quiz" CssClass="btn btn-success" OnClientClick="return confirm('Submit your quiz? You cannot change answers afterwards.');" OnClick="btnSubmit_Click" />
                </div>

                <asp:Button ID="btnTimeout" runat="server" Text="Finish expired quiz" ClientIDMode="Static" UseSubmitBehavior="False" style="display: none;" OnClick="btnTimeout_Click" />
            </asp:Panel>

            <asp:Panel ID="pnlResult" runat="server" Visible="False">
                <h2 class="h5">Quiz result</h2>

                <asp:Label ID="lblResult" runat="server" CssClass="fs-4 d-block mb-3" />

                <asp:HyperLink ID="lnkMoreQuizzes" runat="server" Text="Browse quizzes" NavigateUrl="~/Learning/Challenges.aspx" CssClass="btn btn-outline-success" />
            </asp:Panel>
        </asp:Panel>
    </main>

    <script>
    if (<%= TimerRunning ? "true" : "false" %>) {
        const remainingSeconds = <%= CountdownSeconds %>;
        const deadline = Date.now() + remainingSeconds * 1000;

        const timerLabel = document.getElementById("lblRemaining");
        let timeoutSent = false;

        function updateCountdown() {
            const seconds = Math.max(0,Math.ceil((deadline - Date.now()) / 1000));

            const minutes = Math.floor(seconds / 60);
            const remainder = seconds % 60;

            timerLabel.textContent ="Time remaining: " + minutes + ":" + String(remainder).padStart(2, "0");

            if (seconds === 0 && !timeoutSent) {
                timeoutSent = true;
                clearInterval(timerInterval);
                document.getElementById("btnTimeout").click();
            }
        }

        const timerInterval = setInterval(updateCountdown, 250);
        updateCountdown();
    }
    </script>
</asp:Content>
