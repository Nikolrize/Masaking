<%@ Page Title="My Learning Progress" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Progress.aspx.cs" Inherits="Masaking.Learning.Progress" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="py-4 py-md-5">

        <h1>My Learning Progress</h1>
        <p class="text-secondary"> Review your quiz results, cooking practice and unfinished attempts. </p>

        <div class="d-flex flex-wrap gap-2 mb-4">
            <asp:HyperLink ID="lnkQuizzes" runat="server" Text="Browse quizzes" NavigateUrl="~/Learning/Challenges.aspx" CssClass="btn btn-success" />

            <asp:HyperLink ID="lnkSimulation" runat="server" Text="Practice cooking" NavigateUrl="~/Learning/Simulations.aspx" CssClass="btn btn-outline-success" />
        </div>

        <div class="row g-3 mb-4">

            <div class="col-12 col-sm-6 col-xl-3">
                <div class="card p-3 h-100">
                    <h2 class="h6 text-secondary">Completed attempts</h2>
                    <asp:Label ID="lblCompleted" runat="server" CssClass="fs-2 fw-bold d-block" />
                </div>
            </div>
            <div class="col-12 col-sm-6 col-xl-3">
                <div class="card p-3 h-100">
                    <h2 class="h6 text-secondary">Timed-out attempts</h2>
                    <asp:Label ID="lblTimedOut" runat="server" CssClass="fs-2 fw-bold d-block" />
                </div>
            </div>
            <div class="col-12 col-sm-6 col-xl-3">
                <div class="card p-3 h-100">
                    <h2 class="h6 text-secondary">Unfinished attempts</h2>
                    <asp:Label ID="lblUnfinished" runat="server" CssClass="fs-2 fw-bold d-block" />
                </div>
            </div>
            <div class="col-12 col-sm-6 col-xl-3">
                <div class="card p-3 h-100">
                    <h2 class="h6 text-secondary">Best quiz result</h2>
                    <asp:Label ID="lblBest" runat="server" CssClass="fs-2 fw-bold d-block" />
                </div>
            </div>
        </div>
        <div class="card p-3 p-md-4">
            <h2 class="h5">Quiz history</h2>
            <p class="small text-secondary"> Running attempts continue counting down while you are away. Use Save and pause inside the quiz to stop the timer. Dates below are shown in UTC. </p>

            <div class="table-responsive">
                <asp:GridView ID="gvAttempts" runat="server" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" CssClass="table align-middle" GridLines="None" EmptyDataText="You have not started any quizzes yet." OnPageIndexChanging="gvAttempts_PageIndexChanging">

                    <Columns>
                        <asp:BoundField DataField="title" HeaderText="Quiz" />
                        <asp:BoundField DataField="displayStatus" HeaderText="Status" />
                        <asp:BoundField DataField="resultText" HeaderText="Result" />
                        <asp:BoundField DataField="startedAt" HeaderText="Started (UTC)" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                        <asp:BoundField DataField="completedAt" HeaderText="Finished (UTC)" DataFormatString="{0:dd MMM yyyy HH:mm}" NullDisplayText="Not completed" />

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkAttempt" runat="server" Text='<%# Eval("actionText") %>' NavigateUrl='<%# "TakeQuiz.aspx?attempt=" + Eval("attemptID") %>' CssClass="btn btn-sm btn-outline-success" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <p class="small text-secondary mb-0"> Timed out - open to finalise means the time has expired while you were away. Open the attempt to finalise its saved result.
            </p>
        </div>

            <div class="card p-3 p-md-4 mt-4">
            <h2 class="h5">Simulation history</h2>
            <asp:Label ID="lblSimulationSummary" runat="server" CssClass="d-block mb-3" />
            <p class="small text-secondary"> Each ingredient choice is saved. Dates are shown in UTC Restarted runs remain in your history. Unpublished simulations keep their saved records. </p>
            <div class="table-responsive">
                <asp:GridView ID="gvSimulations" runat="server" AutoGenerateColumns="False"
                    AllowPaging="True" PageSize="10" GridLines="None" CssClass="table align-middle"
                    EmptyDataText="You have not started any simulations yet."
                    OnPageIndexChanging="gvSimulations_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="title" HeaderText="Simulation" />
                        <asp:BoundField DataField="displayStatus" HeaderText="Status" />
                        <asp:BoundField DataField="progressText" HeaderText="Steps completed" />
                        <asp:BoundField DataField="mistakes" HeaderText="Incorrect choices" />
                        <asp:BoundField DataField="resultText" HeaderText="Accuracy" />
                        <asp:BoundField DataField="startedAt" HeaderText="Started (UTC)" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                        <asp:BoundField DataField="completedAt" HeaderText="Finished (UTC)" DataFormatString="{0:dd MMM yyyy HH:mm}" NullDisplayText="Not completed" />
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkSimulationAttempt" runat="server" Text="Open" CssClass="btn btn-sm btn-outline-success" Visible='<%# Convert.ToBoolean(Eval("canOpen")) %>' NavigateUrl='<%# "CookingSimulation.aspx?id=" + Eval("simulationID") + "&attempt=" + Eval("simulationProgressID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </main>
</asp:Content>