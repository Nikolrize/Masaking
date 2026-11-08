<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserAvatar.ascx.cs" Inherits="Masaking.Controls.UserAvatar" %>

<span class="user-avatar">
    <asp:Image ID="avatarImage" runat="server"
        CssClass="user-avatar-photo"
        AlternateText="" Visible="false" />

    <asp:Label ID="avatarInitials" runat="server"
        CssClass="user-avatar-initials" />
</span>