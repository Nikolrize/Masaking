<%@ Page Title="Manage Users" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="Masaking.Admin.ManageUsers" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HyperLink ID="lnkDashboard" runat="server" NavigateUrl="~/Admin/Dashboard.aspx" Text="Back to Dashboard" CssClass="btn btn-outline-success mb-3" />

    <asp:Label ID="lblTitle" runat="server" Text="Manage Users" CssClass="h1 d-block" />

    <asp:Label ID="lblIntroduction" runat="server" Text="Find accounts and manage member access. Administrator accounts are protected on this page." CssClass="text-secondary d-block mb-4" />

    <div class="card p-3 p-md-4 mb-4">
        <div class="row g-3 align-items-end">

            <div class="col-12 col-lg-4">
                <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch" Text="Name or email" CssClass="form-label" />

                <asp:TextBox ID="txtSearch" runat="server" MaxLength="255" CssClass="form-control" />
            </div>

            <div class="col-12 col-sm-6 col-lg-3">
                <asp:Label ID="lblRole" runat="server" AssociatedControlID="ddlRole" Text="Role" CssClass="form-label" />

                <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select">
                    <asp:ListItem Value="">All roles</asp:ListItem>
                    <asp:ListItem Value="Member">Member</asp:ListItem>
                    <asp:ListItem Value="Admin">Admin</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="col-12 col-sm-6 col-lg-3">
                <asp:Label ID="lblStatus" runat="server" AssociatedControlID="ddlStatus" Text="Account status" CssClass="form-label" />

                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                    <asp:ListItem Value="">All accounts</asp:ListItem>
                    <asp:ListItem Value="1">Active</asp:ListItem>
                    <asp:ListItem Value="0">Inactive</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="col-12 col-lg-2">
                <asp:Button ID="btnSearch" runat="server" Text="Apply filters" CssClass="btn btn-success w-100" OnClick="btnSearch_Click" />
            </div>

        </div>
    </div>

    <asp:Label ID="lblMessage" runat="server" EnableViewState="False" role="status" />

    <div class="card p-3 p-md-4">
        <div class="table-responsive">

            <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" CssClass="table align-middle" GridLines="None" EmptyDataText="No accounts match your filters." OnRowCommand="gvUsers_RowCommand" OnPageIndexChanging="gvUsers_PageIndexChanging">

                <Columns>
                    <asp:BoundField DataField="userID" HeaderText="ID" />

                    <asp:BoundField DataField="username" HeaderText="Username" />

                    <asp:BoundField DataField="displayName" HeaderText="Display name" />

                    <asp:BoundField DataField="email" HeaderText="Email" />

                    <asp:BoundField DataField="role" HeaderText="Role" />

                    <asp:CheckBoxField DataField="isActive" HeaderText="Active" />

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

</asp:Content>
