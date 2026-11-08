using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Masaking.Account;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class ManageUsers : System.Web.UI.Page
    {
        protected override void OnPreInit(EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
            base.OnPreInit(e);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CountrySelection.Populate(ddlCreateCountry);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) {return;}

            if (!IsPostBack)
            {
                BindUsers();
            }
        }

        protected void btnCreateUser_Click(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            string name = txtCreateName.Text.Trim();
            string username = txtCreateUsername.Text.Trim();
            string email = txtCreateEmail.Text.Trim();
            string country = CountrySelection.Read(ddlCreateCountry, txtCreateOtherCountry);
            string password = txtCreatePassword.Text;

            if (name.Length == 0 || name.Length > 80
                || !Regex.IsMatch(username, @"\A[A-Za-z0-9_.-]{3,40}\z")
                || String.IsNullOrWhiteSpace(country) || country.Length > 80)
            {
                ShowCreateMessage("Enter a display name within 80 characters, a username with 3-40 letters, numbers, dots, dashes or underscores, and a country within 80 characters.", false);
                return;
            }

            try
            {
                if (email.Length == 0 || email.Length > 254
                    || !String.Equals(new MailAddress(email).Address, email, StringComparison.OrdinalIgnoreCase))
                    throw new FormatException();
            }
            catch (FormatException)
            {
                ShowCreateMessage("Enter a valid email address within 254 characters.", false);
                return;
            }

            if (password.Length < 8 || password.Length > 64 || password != txtCreateConfirm.Text)
            {
                ShowCreateMessage("Enter matching passwords between 8 and 64 characters.", false);
                return;
            }

            const string sql = @" IF EXISTS (SELECT 1 FROM dbo.users WITH (UPDLOCK,HOLDLOCK) WHERE username=@username OR email=@email) SELECT 0; ELSE BEGIN INSERT INTO dbo.users (username,email,password,displayName,role,country,image,createdAt,isActive,authVersion) VALUES (@username,@email,@password,@name,'Member',@country,@image,@now,1,0); SELECT 1; END;";

            try
            {
                int created;
                using (SqlConnection cn = Database.Open())
                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                {
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 40).Value = username;
                    cmd.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email;
                    cmd.Parameters.Add("@password", SqlDbType.NVarChar, 255).Value = Database.HashPassword(password);
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 80).Value = name;
                    cmd.Parameters.Add("@country", SqlDbType.NVarChar, 80).Value = country;
                    cmd.Parameters.Add("@image", SqlDbType.NVarChar, 255).Value = Database.DefaultAvatar;
                    cmd.Parameters.Add("@now", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                    created = Convert.ToInt32(cmd.ExecuteScalar());
                    tx.Commit();
                }

                if (created != 1)
                {
                    ShowCreateMessage("That username or email is already registered.", false);
                    return;
                }

                ClearCreateForm();
                txtSearch.Text = "";
                ddlRole.SelectedValue = "";
                ddlStatus.SelectedValue = "";
                AdminTable.ResetSort(ViewState, "Users");
                gvUsers.PageIndex = 0;
                BindUsers();
                ShowCreateMessage("Member account created. You are still signed in as the administrator.", true);
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                ShowCreateMessage("That username or email is already registered.", false);
            }
            catch (SqlException ex)
            {
                Trace.Warn("Manage users", "Account creation failed.", ex);
                ShowCreateMessage("The account could not be created. Check the details and try again.", false);
            }
        }

        protected void btnClearCreate_Click(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
            ClearCreateForm();
        }

        private void ClearCreateForm()
        {
            txtCreateName.Text = txtCreateUsername.Text = txtCreateEmail.Text = "";
            txtCreateOtherCountry.Text = txtCreatePassword.Text = txtCreateConfirm.Text = "";
            ddlCreateCountry.SelectedIndex = 0;
        }

        private void ShowCreateMessage(string message, bool success)
        {
            lblCreateMessage.Text = message;
            lblCreateMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
        }

        private void BindUsers()
        {
            const string sql = @"SELECT userID, username, displayName, email, role, isActive, createdAt FROM dbo.users WHERE (@search = '' OR CHARINDEX(@search, username) > 0 OR CHARINDEX(@search, displayName) > 0 OR CHARINDEX(@search, email) > 0) AND (@role = '' OR role = @role) AND (@active IS NULL OR isActive = @active) ORDER BY createdAt DESC, userID DESC;";

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

                AdminTable.Bind(gvUsers, users, ViewState, "Users", "createdAt", "DESC");
                lblUserCount.Text = users.Rows.Count
                    + (users.Rows.Count == 1 ? " account matches" : " accounts match") + " the filters.";
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

        protected void Filters_Changed(object sender, EventArgs e)
        {
            btnSearch_Click(sender, e);
        }

        protected void gvUsers_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) { e.Cancel = true; return; }
            AdminTable.ToggleSort(e, ViewState, "Users", "createdAt", "DESC",
                "userID", "username", "displayName", "email", "role", "isActive");
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
