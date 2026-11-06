using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class ManageUsers : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) {return;}

            if (!IsPostBack)
            {
                BindUsers();
            }
        }

        private void BindUsers()
        {
            const string sql = @"
                SELECT userID, username, displayName, email, role, isActive
                FROM dbo.users
                WHERE (@search = '' OR CHARINDEX(@search, username) > 0 OR CHARINDEX(@search, displayName) > 0 OR CHARINDEX(@search, email) > 0) AND (@role = '' OR role = @role) AND (@active IS NULL OR isActive = @active)
                ORDER BY createdAt DESC, userID DESC;";

            using (SqlConnection connection = Database.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add( "@search", SqlDbType.NVarChar, 255 ).Value = txtSearch.Text.Trim();

                command.Parameters.Add( "@role", SqlDbType.NVarChar, 20 ).Value = ddlRole.SelectedValue;

                object active = DBNull.Value;

                if (ddlStatus.SelectedValue == "1")
                {
                    active = true;
                }
                else if (ddlStatus.SelectedValue == "0")
                {
                    active = false;
                }

                command.Parameters.Add( "@active", SqlDbType.Bit ).Value = active;

                DataTable users = new DataTable();
                adapter.Fill(users);

                gvUsers.DataSource = users;
                gvUsers.DataBind();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response))
            {
                return;
            }

            gvUsers.PageIndex = 0;
            BindUsers();
        }

        protected void gvUsers_PageIndexChanging(
            object sender, GridViewPageEventArgs e)
        {
            if (!Database.RequireAdmin(Response))
            {
                return;
            }

            gvUsers.PageIndex = e.NewPageIndex;
            BindUsers();
        }

        protected void gvUsers_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!Database.RequireAdmin(Response))
            {
                return;
            }

            string sql;
            string successMessage;

            switch (e.CommandName)
            {
                case "ActivateUser":
                    sql = @" UPDATE dbo.users SET isActive = 1
                        WHERE userID = @id AND userID <> @current AND role = 'Member' AND isActive = 0;";
                    successMessage = "Member account activated.";
                    break;

                case "DeactivateUser":
                    sql = @" UPDATE dbo.users SET isActive = 0
                        WHERE userID = @id AND userID <> @current AND role = 'Member' AND isActive = 1;";
                    successMessage = "Member account deactivated.";
                    break;

                case "PromoteUser":
                    sql = @" UPDATE dbo.users SET role = 'Admin'
                        WHERE userID = @id AND userID <> @current AND role = 'Member' AND isActive = 1;";
                    successMessage = "Member promoted to administrator.";
                    break;

                default:
                    return;
            }

            int userId;

            if (!Int32.TryParse(
                Convert.ToString(e.CommandArgument), out userId)
                || userId <= 0)
            {
                lblMessage.Text = "Invalid account selection.";
                lblMessage.CssClass = "alert alert-danger d-block";
                return;
            }

            int changed;

            using (SqlConnection connection = Database.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = userId;

                command.Parameters.Add("@current", SqlDbType.Int).Value = Database.UserId.Value;

                changed = command.ExecuteNonQuery();
            }

            lblMessage.Text = changed == 1 ? successMessage : "No change made. The account may have changed " + "or is protected from this action.";

            lblMessage.CssClass = changed == 1 ? "alert alert-success d-block" : "alert alert-warning d-block";

            gvUsers.PageIndex = 0;
            BindUsers();
        }
    }
}
