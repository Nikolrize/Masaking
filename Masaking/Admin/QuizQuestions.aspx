<%@ Page Title="Quiz Questions" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="QuizQuestions.aspx.cs" Inherits="Masaking.Admin.QuizQuestions" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/Admin/ManageChallenges.aspx" Text="Back to quizzes" CssClass="btn btn-outline-success mb-3" />

    <asp:Label ID="lblHeading" runat="server" Text="Quiz questions" CssClass="h1 d-block mb-3" />

    <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />

    <div class="card p-3 p-md-4 mb-4">
        <div class="row g-3">

            <div class="col-12 col-md-4">
                <asp:Label ID="lblNumber" runat="server" AssociatedControlID="txtNumber" Text="Question number" CssClass="form-label" />
                <asp:TextBox ID="txtNumber" runat="server" TextMode="Number" Text="1" min="1" max="100" step="1" CssClass="form-control" />
            </div>
            <div class="col-12">
                <asp:Label ID="lblQuestion" runat="server" AssociatedControlID="txtQuestion" Text="Question" CssClass="form-label" />
                <asp:TextBox ID="txtQuestion" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>
            <div class="col-12 col-md-6">
                <asp:Label ID="lblA" runat="server" AssociatedControlID="txtA" Text="Option A" CssClass="form-label" />
                <asp:TextBox ID="txtA" runat="server" MaxLength="500" CssClass="form-control" />
            </div>
            <div class="col-12 col-md-6">
                <asp:Label ID="lblB" runat="server" AssociatedControlID="txtB" Text="Option B" CssClass="form-label" />
                <asp:TextBox ID="txtB" runat="server" MaxLength="500" CssClass="form-control" />
            </div>
            <div class="col-12 col-md-6">
                <asp:Label ID="lblC" runat="server" AssociatedControlID="txtC" Text="Option C" CssClass="form-label" />
                <asp:TextBox ID="txtC" runat="server" MaxLength="500" CssClass="form-control" />
            </div>
            <div class="col-12 col-md-6">
                <asp:Label ID="lblD" runat="server" AssociatedControlID="txtD" Text="Option D" CssClass="form-label" />

                <asp:TextBox ID="txtD" runat="server" MaxLength="500" CssClass="form-control" />
            </div>

            <div class="col-12 col-md-4">
                <asp:Label ID="lblCorrect" runat="server" AssociatedControlID="ddlCorrect" Text="Correct answer" CssClass="form-label" />

                <asp:DropDownList ID="ddlCorrect" runat="server"
                    CssClass="form-select">
                    <asp:ListItem Value="A">A</asp:ListItem>
                    <asp:ListItem Value="B">B</asp:ListItem>
                    <asp:ListItem Value="C">C</asp:ListItem>
                    <asp:ListItem Value="D">D</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="col-12">
                <asp:Label ID="lblExplanation" runat="server" AssociatedControlID="txtExplanation" Text="Explanation" CssClass="form-label" />
                <asp:TextBox ID="txtExplanation" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
            </div>

            <div class="col-12 d-flex flex-wrap gap-2">
                <asp:Button ID="btnSave" runat="server" Text="Save question" CssClass="btn btn-success" OnClick="btnSave_Click" />
                <asp:Button ID="btnNew" runat="server" Text="New question" CausesValidation="False" CssClass="btn btn-outline-secondary" OnClick="btnNew_Click" />
            </div>
        </div>
    </div>

    <div class="card p-3 p-md-4">
        <h2 class="h5 mb-3">Existing questions</h2>

        <div class="table-responsive">
            <asp:GridView ID="gvQuestions" runat="server" AutoGenerateColumns="False" CssClass="table align-middle" GridLines="None" EmptyDataText="No questions added yet." OnRowCommand="gvQuestions_RowCommand">

                <Columns>
                    <asp:BoundField DataField="questionNumber" HeaderText="Number" />
                    <asp:BoundField DataField="questionText" HeaderText="Question" />
                    <asp:BoundField DataField="correctOption" HeaderText="Correct answer" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                                <div class="d-flex flex-wrap gap-2">
                            <asp:LinkButton ID="btnEditQuestion" runat="server" Text="Edit" CssClass="btn btn-sm btn-outline-success" CausesValidation="False" CommandName="EditQuestion" CommandArgument='<%# Eval("questionID") %>' />
                            <asp:Button ID="btnDeleteQuestion" runat="server" CausesValidation="False" CommandName="RemoveQuestion" CssClass="btn btn-sm btn-outline-danger" OnClientClick="return confirm('Delete this question? You may need to renumber the remaining questions.');" CommandArgument='<%# Eval("questionID") %>' Text="Delete" />
                                </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

</asp:Content>
