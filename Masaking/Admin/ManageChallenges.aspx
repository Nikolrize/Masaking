<%@ Page Title="Manage Challenges | Masaking" Language="C#" MasterPageFile="~/Admin/Admin.Master" MaintainScrollPositionOnPostBack="True" AutoEventWireup="true" CodeBehind="ManageChallenges.aspx.cs" Inherits="Masaking.Admin.ManageChallenges" %>

<%@ Import Namespace="Masaking.Infrastructure" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <p class="text-brand fw-semibold">ADMINISTRATION</p>
    <h1 class="page-heading"><asp:Label ID="lblTitle" runat="server" Text="Manage challenges" /></h1>
    <p class="text-secondary"><asp:Label ID="lblIntroduction" runat="server" Text="Create a draft challenge, then add questions before publishing." /></p>

    <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />

    <div class="card p-3 p-md-4 mb-4">

        <asp:Label ID="lblEditorHeading" runat="server" Text="New challenge" CssClass="h5 d-block mb-3" />

        <div class="row g-3">
            <div class="col-12">
                <asp:Label ID="lblRecipe" runat="server" Text="Approved recipe" AssociatedControlID="ddlRecipe" CssClass="form-label" />
                <asp:DropDownList ID="ddlRecipe" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12">
                <asp:Label ID="lblQuizTitle" runat="server" AssociatedControlID="txtTitle" Text="Challenge title" CssClass="form-label" />
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
                <asp:Label ID="lblMinutes" runat="server" AssociatedControlID="txtMinutes" Text="Time limit in minutes (1-60)" CssClass="form-label" />
                <asp:TextBox ID="txtMinutes" runat="server" TextMode="Number" Text="5" min="1" max="60" step="1" CssClass="form-control" />
            </div>
            <div class="col-12 d-flex flex-wrap gap-2">
                <asp:Button ID="btnSave" runat="server" Text="Save draft" CssClass="btn btn-success" OnClick="btnSave_Click" />
                <asp:Button ID="btnNew" runat="server" Text="Reset form" CausesValidation="False" CssClass="btn btn-outline-secondary" OnClick="btnNew_Click" />
            </div>

        </div>
    </div>

    <div class="card p-3 p-md-4">

        <asp:Label ID="lblListHeading" runat="server" Text="Existing challenges" CssClass="h5 d-block mb-3" />

        <asp:Panel ID="pnlFilters" runat="server" DefaultButton="btnSearch" CssClass="mb-4">
            <div class="row g-3">
                <div class="col-12 col-md-8">
                    <asp:Label ID="lblSearch" runat="server" Text="Challenge title" AssociatedControlID="txtSearch" CssClass="form-label" />
                    <asp:TextBox ID="txtSearch" runat="server" ClientIDMode="Static" MaxLength="150" CssClass="form-control" data-auto-filter="true" data-filter-target="btnSearch" />
                </div>
                <div class="col-12 col-md-4">
                    <asp:Label ID="lblPublication" runat="server" Text="Publication" AssociatedControlID="ddlPublication" CssClass="form-label" />
                    <asp:DropDownList ID="ddlPublication" runat="server" AutoPostBack="True" OnSelectedIndexChanged="Filters_Changed" CssClass="form-select">
                        <asp:ListItem Value="">All challenges</asp:ListItem>
                        <asp:ListItem Value="1">Published</asp:ListItem>
                        <asp:ListItem Value="0">Drafts</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <asp:Button ID="btnSearch" runat="server" ClientIDMode="Static" Text="Apply filters" Style="display:none;" CausesValidation="False" UseSubmitBehavior="False" OnClick="btnSearch_Click" />
        </asp:Panel>
        <asp:Label ID="lblCount" runat="server" CssClass="small text-secondary d-block mb-3" role="status" />
        <p class="small text-secondary">Only unpublished challenges without attempts can be edited. Search updates after a short pause.</p>
        <div class="table-responsive">
            <asp:GridView ID="gvQuizzes" runat="server" AutoGenerateColumns="False" AllowSorting="True" OnSorting="gvQuizzes_Sorting" AllowPaging="True" PageSize="10" CssClass="table align-middle" GridLines="None" EmptyDataText="No challenges created yet." OnRowCommand="gvQuizzes_RowCommand" OnPageIndexChanging="gvQuizzes_PageIndexChanging">

                <Columns>
                    <asp:BoundField DataField="challengeID" HeaderText="ID" SortExpression="challengeID" />
                    <asp:BoundField DataField="title" HeaderText="Challenge" SortExpression="title" />
                    <asp:BoundField DataField="recipeTitle" HeaderText="Recipe" />
                    <asp:BoundField DataField="recipeOwner" HeaderText="Recipe owner" SortExpression="recipeOwner" HtmlEncode="True" />
                    <asp:TemplateField HeaderText="Difficulty" SortExpression="difficultyOrder">
                        <ItemTemplate><span class='<%# AdminTable.DifficultyBadge(Eval("difficulty")) %>'><%#: Eval("difficulty") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="minutes" HeaderText="Minutes" SortExpression="timeLimit" DataFormatString="{0:0.#}" />
                    <asp:TemplateField HeaderText="Publication" SortExpression="isActive">
                        <ItemTemplate><span class='<%# Convert.ToBoolean(Eval("isActive")) ? "badge rounded-pill status-approved" : "badge rounded-pill status-pending" %>'><%#: Convert.ToBoolean(Eval("isActive")) ? "Published" : "Draft" %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="questionCount" HeaderText="Questions" SortExpression="questionCount" />
                    <asp:BoundField DataField="attemptCount" HeaderText="Attempts" SortExpression="attemptCount" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="d-flex flex-wrap gap-2">
                            <asp:LinkButton ID="btnEditQuiz" runat="server" Text="Edit settings" Visible='<%# !Convert.ToBoolean(Eval("isActive")) && Convert.ToInt32(Eval("attemptCount")) == 0 %>' CssClass="btn btn-sm btn-outline-primary" CausesValidation="False" CommandName="EditQuiz" CommandArgument='<%# Eval("challengeID") %>' />
                            <asp:HyperLink ID="lnkQuestions" runat="server" CssClass="btn btn-sm btn-outline-info" NavigateUrl='<%# "QuizQuestions.aspx?id=" + Eval("challengeID") %>' >Questions</asp:HyperLink>
                            <asp:LinkButton ID="btnPublish" runat="server" CausesValidation="False" CommandName="PublishQuiz" CssClass="btn btn-sm btn-success" CommandArgument='<%# Eval("challengeID") %>' Visible='<%# !Convert.ToBoolean(Eval("isActive")) %>' OnClientClick="return confirm('Publish this challenge for members?');" >Publish</asp:LinkButton>
                            <asp:LinkButton ID="btnUnpublish" runat="server" CausesValidation="False" CommandName="UnpublishQuiz" CssClass="btn btn-sm btn-outline-secondary" CommandArgument='<%# Eval("challengeID") %>' Visible='<%# Convert.ToBoolean(Eval("isActive")) %>' OnClientClick="return confirm('Unpublish this challenge to prevent new attempts?');">Unpublish</asp:LinkButton>
                                </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <script src="<%= ResolveUrl("~/Scripts/admin-tables.js") %>"></script>
</asp:Content>
