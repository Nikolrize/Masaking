<%@ Page Title="Manage Users" Language="C#" MasterPageFile="~/Admin/Admin.Master" MaintainScrollPositionOnPostBack="True" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="Masaking.Admin.ManageUsers" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <p class="text-brand fw-semibold">ADMINISTRATION</p>
    <h1 class="page-heading"><asp:Label ID="lblTitle" runat="server" Text="Manage users" /></h1>
    <p class="text-secondary"><asp:Label ID="lblIntroduction" runat="server" Text="Find accounts and manage member access. Administrator accounts are protected on this page." /></p>

    <asp:Panel ID="pnlFilters" runat="server" DefaultButton="btnSearch" CssClass="card p-3 p-md-4 mb-4">
        <div class="row g-3">
            <div class="col-12 col-lg-6">
                <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch" Text="Name or email" CssClass="form-label" />
                <asp:TextBox ID="txtSearch" runat="server" ClientIDMode="Static" MaxLength="255" CssClass="form-control" data-auto-filter="true" data-filter-target="btnSearch" />
            </div>
            <div class="col-12 col-sm-6 col-lg-3">
                <asp:Label ID="lblRole" runat="server" AssociatedControlID="ddlRole" Text="Role" CssClass="form-label" />
                <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="Filters_Changed">
                    <asp:ListItem Value="">All roles</asp:ListItem>
                    <asp:ListItem Value="Member">Member</asp:ListItem>
                    <asp:ListItem Value="Admin">Admin</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-12 col-sm-6 col-lg-3">
                <asp:Label ID="lblStatus" runat="server" AssociatedControlID="ddlStatus" Text="Account status" CssClass="form-label" />
                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="Filters_Changed">
                    <asp:ListItem Value="">All accounts</asp:ListItem>
                    <asp:ListItem Value="1">Active</asp:ListItem>
                    <asp:ListItem Value="0">Inactive</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <p class="small text-secondary mt-3 mb-0">Search updates after a short pause. Select a column heading to sort.</p>
        <asp:Button ID="btnSearch" runat="server" ClientIDMode="Static" Text="Apply filters" Style="display:none;" UseSubmitBehavior="False" CausesValidation="False" OnClick="btnSearch_Click" />
    </asp:Panel>

    <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />
    <asp:Label ID="lblUserCount" runat="server" CssClass="small text-secondary d-block mb-3" />
    <div class="card p-3 p-md-4">
        <div class="table-responsive">
            <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" AllowSorting="True" CssClass="table align-middle" GridLines="None" EmptyDataText="No accounts match your filters." OnRowCommand="gvUsers_RowCommand" OnPageIndexChanging="gvUsers_PageIndexChanging" OnSorting="gvUsers_Sorting">
                <Columns>
                    <asp:BoundField DataField="userID" HeaderText="ID" SortExpression="userID" />
                    <asp:BoundField DataField="username" HeaderText="Username" SortExpression="username" />
                    <asp:BoundField DataField="displayName" HeaderText="Display name" SortExpression="displayName" />
                    <asp:BoundField DataField="email" HeaderText="Email" SortExpression="email" />
                    <asp:TemplateField HeaderText="Role" SortExpression="role">
                        <ItemTemplate>
                            <span class='<%# Convert.ToString(Eval("role")) == "Admin" ? "badge rounded-pill role-admin" : "badge rounded-pill role-member" %>'><%#: Eval("role") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Account status" SortExpression="isActive">
                        <ItemTemplate>
                            <span class='<%# Convert.ToBoolean(Eval("isActive")) ? "badge rounded-pill status-approved" : "badge rounded-pill status-rejected" %>'>
                                <i class='<%# Convert.ToBoolean(Eval("isActive")) ? "bi bi-check-circle me-1" : "bi bi-x-circle me-1" %>' aria-hidden="true"></i><%# Convert.ToBoolean(Eval("isActive")) ? "Active" : "Inactive" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="d-flex flex-wrap gap-2">
                                <asp:LinkButton ID="btnActivate" runat="server" Text="Activate" CausesValidation="False" CssClass="btn btn-sm btn-outline-success" CommandName="ActivateUser" CommandArgument='<%# Eval("userID") %>' Visible='<%# Convert.ToString(Eval("role")) == "Member" && !Convert.ToBoolean(Eval("isActive")) %>' OnClientClick="return confirm('Activate this member account?');" />
                                <asp:LinkButton ID="btnDeactivate" runat="server" Text="Deactivate" CausesValidation="False" CssClass="btn btn-sm btn-outline-danger" CommandName="DeactivateUser" CommandArgument='<%# Eval("userID") %>' Visible='<%# Convert.ToString(Eval("role")) == "Member" && Convert.ToBoolean(Eval("isActive")) %>' OnClientClick="return confirm('Deactivate this member account?');" />
                                <asp:LinkButton ID="btnPromote" runat="server" Text="Make admin" CausesValidation="False" CssClass="btn btn-sm btn-outline-secondary" CommandName="PromoteUser" CommandArgument='<%# Eval("userID") %>' Visible='<%# Convert.ToString(Eval("role")) == "Member" && Convert.ToBoolean(Eval("isActive")) %>' OnClientClick="return confirm('Give this member administrator access?');" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>
    <script src="<%= ResolveUrl("~/Scripts/admin-tables.js") %>"></script>
</asp:Content>