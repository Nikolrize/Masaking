<%@ Page Title="Cooking Quizzes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Challenges.aspx.cs" Inherits="Masaking.Learning.Challenges" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="py-4 py-md-5">

        <asp:HyperLink ID="lnkHome" runat="server" NavigateUrl="~/Default.aspx" Text="Back to home" CssClass="btn btn-outline-success mb-3" />

        &nbsp;<asp:HyperLink ID="lnkProgress" runat="server" CssClass="btn btn-outline-success mb-3" NavigateUrl="~/Learning/Progress.aspx">My Progress</asp:HyperLink>
    <asp:HyperLink ID="lnkSimulations" runat="server" CssClass="btn btn-outline-success mb-3" NavigateUrl="~/Learning/Simulations.aspx">Cooking simulations</asp:HyperLink>
&nbsp;<asp:Label ID="lblTitle" runat="server" Text="Cooking Quizzes" CssClass="h1 d-block" />

        <asp:Label ID="lblIntroduction" runat="server" Text="Test your cooking knowledge with timed quizzes." CssClass="text-secondary d-block mb-4" />

        <div class="card p-3 p-md-4 mb-4">
            <div class="row g-3 align-items-end">
                <div class="col-12 col-md-6">
                    <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch" Text="Quiz title" CssClass="form-label" />

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
                    <asp:Button ID="btnSearch" runat="server" Text="Find quizzes" CssClass="btn btn-success w-100" OnClick="btnSearch_Click" />
                </div>

            </div>
        </div>

        <div class="card p-3 p-md-4">
            <div class="table-responsive">

                <asp:GridView ID="gvQuizzes" runat="server" AutoGenerateColumns="False" AllowPaging="True" CssClass="table align-middle" GridLines="None" EmptyDataText="No published quizzes match your filters." OnPageIndexChanging="gvQuizzes_PageIndexChanging">

                    <Columns>
                        <asp:BoundField DataField="title" HeaderText="Quiz" />
                        <asp:BoundField DataField="description" HeaderText="Description" />
                        <asp:BoundField DataField="difficulty" HeaderText="Difficulty" />
                        <asp:BoundField DataField="minutes" HeaderText="Time limit (minutes)" DataFormatString="{0:0.#}" />
                        <asp:BoundField DataField="questionCount" HeaderText="Questions" />
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkStartQuiz" runat="server" Text="Start / reopen" CssClass="btn btn-success" NavigateUrl='<%# "TakeQuiz.aspx?id=" + Eval("challengeID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </main>

</asp:Content>
