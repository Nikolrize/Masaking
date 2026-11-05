<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageChallenges.aspx.cs" Inherits="Masaking.Admin.ManageChallenges" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Manage Quizzes - Masaking</title>

    <link href="../Content/bootstrap.min.css" rel="stylesheet" />
    <link href="../Content/Site.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <main class="container py-4 py-md-5">

            <asp:HyperLink ID="lnkDashboard" runat="server" NavigateUrl="~/Admin/Dashboard.aspx" Text="Back to Dashboard" CssClass="btn btn-outline-success mb-3" />

            <asp:Label ID="lblTitle" runat="server" Text="Manage Quizzes" CssClass="h1 d-block" />

            <asp:Label ID="lblIntroduction" runat="server" Text="Create a draft quiz, then add questions before publishing." CssClass="text-secondary d-block mb-4" />

            <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />

            <div class="card p-3 p-md-4 mb-4">

                <asp:Label ID="lblEditorHeading" runat="server" Text="New quiz" CssClass="h5 d-block mb-3" />

                <div class="row g-3">

                    <div class="col-12">
                        <asp:Label ID="lblQuizTitle" runat="server" AssociatedControlID="txtTitle" Text="Quiz title" CssClass="form-label" />
                        <asp:TextBox ID="txtTitle" runat="server" MaxLength="150" CssClass="form-control" />
                    </div>
                    <div class="col-12">
                        <asp:Label ID="lblDescription" runat="server" AssociatedControlID="txtDescription" Text="Description" CssClass="form-label" />
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                    </div>
                    <div class="col-12 col-md-6">
                        <asp:Label ID="lblDifficulty" runat="server" AssociatedControlID="ddlDifficulty" Text="Difficulty" CssClass="form-label" />

                        <asp:DropDownList ID="ddlDifficulty" runat="server" CssClass="form-select">
                            <asp:ListItem Value="Easy">Easy</asp:ListItem>
                            <asp:ListItem Value="Medium">Medium</asp:ListItem>
                            <asp:ListItem Value="Hard">Hard</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="col-12 col-md-6">
                        <asp:Label ID="lblMinutes" runat="server" AssociatedControlID="txtMinutes" Text="Time limit in minutes (1–60)" CssClass="form-label" />
                        <asp:TextBox ID="txtMinutes" runat="server" TextMode="Number" Text="5" min="1" max="60" step="1" CssClass="form-control" />
                    </div>
                    <div class="col-12 d-flex flex-wrap gap-2">
                        <asp:Button ID="btnSave" runat="server" Text="Save draft" CssClass="btn btn-success" OnClick="btnSave_Click" />
                        <asp:Button ID="btnNew" runat="server" Text="New quiz" CausesValidation="False" CssClass="btn btn-outline-secondary" OnClick="btnNew_Click" />
                    </div>

                </div>
            </div>

            <div class="card p-3 p-md-4">

                <asp:Label ID="lblListHeading" runat="server" Text="Existing quizzes" CssClass="h5 d-block mb-3" />

                <div class="table-responsive">
                    <asp:GridView ID="gvQuizzes" runat="server" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" CssClass="table align-middle" GridLines="None" EmptyDataText="No quizzes created yet." OnRowCommand="gvQuizzes_RowCommand" OnPageIndexChanging="gvQuizzes_PageIndexChanging">

                        <Columns>
                            <asp:BoundField DataField="challengeID" HeaderText="ID" />
                            <asp:BoundField DataField="title" HeaderText="Quiz" />
                            <asp:BoundField DataField="difficulty" HeaderText="Difficulty" />
                            <asp:BoundField DataField="timeLimit" HeaderText="Seconds" />
                            <asp:CheckBoxField DataField="isActive" HeaderText="Published" />
                            <asp:BoundField DataField="questionCount" HeaderText="Questions" />
                            <asp:BoundField DataField="attemptCount" HeaderText="Attempts" />
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEditQuiz" runat="server" Text="Edit settings" CssClass="btn btn-sm btn-outline-success" CausesValidation="False" CommandName="EditQuiz" CommandArgument='<%# Eval("challengeID") %>' />
                                    &nbsp;<asp:HyperLink ID="lnkQuestions" runat="server" CssClass="btn btn-sm btn-outline-success" NavigateUrl='<%# "QuizQuestions.aspx?id=" + Eval("challengeID") %>' >Questions</asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </main>
    </form>
</body>
</html>