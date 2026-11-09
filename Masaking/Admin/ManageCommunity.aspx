<%@ Page Title="Community moderation | Masaking" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" MaintainScrollPositionOnPostBack="True" CodeBehind="ManageCommunity.aspx.cs" Inherits="Masaking.Admin.ManageCommunity" %>

<%@ Import Namespace="Masaking.Infrastructure" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <p class="text-brand fw-semibold">ADMINISTRATION</p>
    <h1 class="page-heading">Community moderation</h1>
    <p class="text-secondary"> Review community posts, comments and recipe ratings. </p>

    <asp:Panel ID="pnlFilters" runat="server" DefaultButton="btnSearch" CssClass="card p-3 p-md-4 mb-4">

        <div class="row g-3">

            <div class="col-12 col-md-6">
                <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch" Text="Search content, author or recipe" CssClass="form-label" />
                <asp:TextBox ID="txtSearch" runat="server" ClientIDMode="Static" MaxLength="150" CssClass="form-control" data-auto-filter="true" data-filter-target="btnSearch" />
            </div>

            <div class="col-12 col-md-3">
                <asp:Label ID="lblType" runat="server" AssociatedControlID="ddlType" Text="Content type" CssClass="form-label" />
                <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="Filters_Changed">

                    <asp:ListItem Value="">All types</asp:ListItem>
                    <asp:ListItem Value="Post">Posts</asp:ListItem>
                    <asp:ListItem Value="Comment">Comments</asp:ListItem>
                    <asp:ListItem Value="Review">Recipe reviews</asp:ListItem>

                </asp:DropDownList>
            </div>

            <div class="col-12 col-md-3">
                <asp:Label ID="lblStatus" runat="server" AssociatedControlID="ddlStatus" Text="Status" CssClass="form-label" />

                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="Filters_Changed">

                    <asp:ListItem Value="">All statuses</asp:ListItem>
                    <asp:ListItem Value="Pending">Pending</asp:ListItem>
                    <asp:ListItem Value="Approved">Approved</asp:ListItem>
                    <asp:ListItem Value="Rejected">Rejected</asp:ListItem>
                    <asp:ListItem Value="Deleted">Deleted by user</asp:ListItem>
                    <asp:ListItem Value="Removed">Removed</asp:ListItem>

                </asp:DropDownList>
            </div>

        </div>

        <p class="small text-secondary mt-3 mb-0"> Search updates after a short pause. Select a column heading to sort. </p>

        <asp:Button ID="btnSearch" runat="server" ClientIDMode="Static" Text="Apply filters" Style="display:none;" UseSubmitBehavior="False" CausesValidation="False" OnClick="btnSearch_Click" />

    </asp:Panel>

    <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />

    <asp:Label ID="lblCount" runat="server" CssClass="small text-secondary d-block mb-3" role="status" />

    <section class="card p-3 p-md-4">

        <p class="small text-secondary"> Hide removes approved content from public view. Deleted and removed items are shown for reference.
        </p>

        <div class="table-responsive">

            <asp:GridView ID="gvCommunity" runat="server" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" AllowSorting="True" GridLines="None" CssClass="table align-middle" EmptyDataText="No community content matches your filters." OnPageIndexChanging="gvCommunity_PageIndexChanging" OnSorting="gvCommunity_Sorting" OnRowCommand="gvCommunity_RowCommand">

                <Columns>

                    <asp:BoundField DataField="kind" HeaderText="Type" SortExpression="kind" />
                    <asp:BoundField DataField="itemID" HeaderText="ID" SortExpression="itemID" />
                    <asp:TemplateField HeaderText="Photo">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkPhoto" runat="server" NavigateUrl='<%# Eval("imageUrl") %>' Visible='<%# !String.IsNullOrEmpty(Convert.ToString(Eval("imageUrl"))) %>' Target="_blank" rel="noopener">

                                <asp:Image ID="imgPhoto" runat="server" ImageUrl='<%# Eval("imageUrl") %>' AlternateText="Post attachment. Open full image." CssClass="recipe-thumb" />
                            </asp:HyperLink>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="content" HeaderText="Content" SortExpression="content">
                        <ItemStyle CssClass="community-content" />
                    </asp:BoundField>

                    <asp:BoundField DataField="relatedTitle" HeaderText="Related recipe" SortExpression="relatedTitle" />

                    <asp:BoundField DataField="displayName" HeaderText="Submitted by" SortExpression="displayName" />

                    <asp:TemplateField HeaderText="Status" SortExpression="status">
                        <ItemTemplate>
                            <span class='<%# AdminTable.StatusBadge(Eval("status")) %>'> <%#: Eval("status") %> </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="submittedLocal" HeaderText="Submitted (MYT)" SortExpression="createdAt" DataFormatString="{0:dd MMM yyyy HH:mm}" />

                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="d-flex flex-wrap gap-2">
                                <asp:LinkButton ID="btnApprove" runat="server" Text="Approve" CausesValidation="False" CssClass="btn btn-sm btn-success" CommandName="ApproveContent" CommandArgument='<%# Eval("kind") + ":" + Eval("itemID") + ":" + Eval("status") %>' Visible='<%# CanApprove(Eval("status")) %>' OnClientClick="return confirm('Approve this community content?');" />

                                <asp:LinkButton ID="btnReject" runat="server" Text="Reject" CausesValidation="False" CssClass="btn btn-sm btn-outline-danger" CommandName="RejectContent" CommandArgument='<%# Eval("kind") + ":" + Eval("itemID") + ":" + Eval("status") %>' Visible='<%# Convert.ToString(Eval("status")) == "Pending" %>' OnClientClick="return confirm('Reject this community content?');" />

                                <asp:LinkButton ID="btnHide" runat="server" Text="Hide" CausesValidation="False" CssClass="btn btn-sm btn-outline-danger" CommandName="HideContent" CommandArgument='<%# Eval("kind") + ":" + Eval("itemID") + ":" + Eval("status") %>' Visible='<%# Convert.ToString(Eval("status")) == "Approved" %>' OnClientClick="return confirm('Hide this content from public view?');" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </section>
    <script src="<%= ResolveUrl("~/Scripts/admin-tables.js") %>"></script>
</asp:Content>