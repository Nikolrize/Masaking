using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Community
{
    public partial class Post : CommunityPage
    {
        protected Repeater rptPosts, rptComments;
        protected Panel pnlThread, pnlNoComments, pnlCommentForm, pnlUnavailable;
        protected TextBox txtComment;
        protected HyperLink lnkCommentLogin;
        protected Literal litCommentCount;
        protected DropDownList ddlCommentSort;
        private int ThreadId { get { int id; return Int32.TryParse(Request.QueryString["id"] ?? Request.QueryString["post"], out id) && id > 0 ? id : 0; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) RunDatabaseAction(() => BindThread(ThreadId));
            pnlCommentForm.Visible = pnlThread.Visible && currentUserIsActive;
        }

        private void BindThread(int postId)
        {
            DataTable posts = postId > 0 ? ReadPosts(postId, "newest") : new DataTable();
            bool found = posts.Rows.Count == 1;
            pnlThread.Visible = found;
            pnlUnavailable.Visible = !found;
            pnlCommentForm.Visible = found && currentUserIsActive;
            lnkCommentLogin.Visible = found && !currentUserIsActive;
            lnkCommentLogin.NavigateUrl = "../Account/Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl);
            rptPosts.DataSource = posts;
            rptPosts.DataBind();
            if (!found) return;
            string order = ddlCommentSort.SelectedValue == "oldest" ? "ASC" : "DESC";
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT c.postCommentID, c.content, c.createdAt, c.updatedAt,
                c.userID, u.displayName, u.username, u.image AS profilePicture
                FROM postComments c JOIN users u ON u.userID=c.userID
                WHERE c.postID=@id AND c.status='Approved' AND u.isActive=1
                ORDER BY c.createdAt " + order + ", c.postCommentID " + order, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = postId;
                DataTable comments = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(comments);
                rptComments.DataSource = comments;
                rptComments.DataBind();
                litCommentCount.Text = comments.Rows.Count.ToString();
                pnlNoComments.Visible = comments.Rows.Count == 0;
            }
        }

        protected void ddlCommentSort_SelectedIndexChanged(object sender, EventArgs e)
        { RunDatabaseAction(() => BindThread(ThreadId)); }

        protected void btnComment_Click(object sender, EventArgs e)
        {
            RunDatabaseAction(() =>
            {
                if (!RequireCurrentUser()) return;

                string content = (txtComment.Text ?? String.Empty).Trim();
                if (ThreadId <= 0 || content.Length == 0 || content.Length > 1000)
                {
                    Show("Write a comment of 1–1,000 characters before posting.", false);
                    return;
                }

                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO postComments (content, status, createdAt, updatedAt, postID, userID) " +
                    "SELECT @content, 'Approved', @created, NULL, p.postID, @user " +
                    "FROM posts p JOIN users u ON u.userID=p.userID " +
                    "WHERE p.postID=@post AND p.status='Approved' AND u.isActive=1", cn))
                {
                    cmd.Parameters.Add("@content", SqlDbType.NVarChar, 1000).Value = content;
                    cmd.Parameters.Add("@created", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    cmd.Parameters.Add("@post", SqlDbType.Int).Value = ThreadId;

                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        Show("This post is no longer available for comments.", false);
                        BindThread(ThreadId);
                        return;
                    }
                }

                txtComment.Text = String.Empty;
                Show("Your comment has been posted.", true);
                BindThread(ThreadId);
            });
        }

        protected void btnUpdateComment_Click(object sender, EventArgs e)
        {
            RunDatabaseAction(() =>
            {
                Button button = sender as Button;
                if (!RequireCurrentUser()) return;
                if (button == null || !Int32.TryParse(button.CommandArgument, out int commentId) || commentId <= 0 || ThreadId <= 0) return;

                TextBox editor = button.NamingContainer.FindControl("txtEditComment") as TextBox;
                string content = editor == null ? String.Empty : (editor.Text ?? String.Empty).Trim();

                if (content.Length == 0 || content.Length > 1000)
                {
                    Show("A comment must contain 1–1,000 characters.", false);
                    return;
                }

                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE postComments SET content=@content, updatedAt=SYSUTCDATETIME() " +
                    "WHERE postCommentID=@comment AND postID=@post AND userID=@user AND status='Approved' " +
                    "AND EXISTS (SELECT 1 FROM posts p JOIN users u ON u.userID=p.userID WHERE p.postID=postComments.postID AND p.status='Approved' AND u.isActive=1) " +
                    "AND createdAt>=DATEADD(day,-7,SYSUTCDATETIME()) AND createdAt<=SYSUTCDATETIME()", cn))
                {
                    cmd.Parameters.Add("@content", SqlDbType.NVarChar, 1000).Value = content;
                    cmd.Parameters.Add("@comment", SqlDbType.Int).Value = commentId;
                    cmd.Parameters.Add("@post", SqlDbType.Int).Value = ThreadId;
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;

                    if (cmd.ExecuteNonQuery() != 1) Show("This comment can no longer be edited. Comments can only be edited within seven days of creation.", false);
                    else Show("Your comment has been updated.", true);
                }
                if (ThreadId > 0) BindThread(ThreadId);
            });
        }

        protected void btnDeleteComment_Click(object sender, EventArgs e)
        {
            RunDatabaseAction(() =>
            {
                var button = sender as LinkButton;
                if (!RequireCurrentUser()) return;
                if (button == null || !Int32.TryParse(button.CommandArgument, out int commentId) || commentId <= 0 || ThreadId <= 0) return;

                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE postComments SET status='Deleted' WHERE postCommentID=@comment AND postID=@post AND userID=@user AND status='Approved'", cn))
                {
                    cmd.Parameters.Add("@comment", SqlDbType.Int).Value = commentId;
                    cmd.Parameters.Add("@post", SqlDbType.Int).Value = ThreadId;
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;

                    if (cmd.ExecuteNonQuery() != 1) Show("The comment could not be deleted. You may only delete your own comments.", false);
                    else Show("Your comment has been deleted.", true);
                }
                if (ThreadId > 0) BindThread(ThreadId);
            });
        }

    }
}
