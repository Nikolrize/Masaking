using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class ManageCommunity : System.Web.UI.Page
    {
        protected Repeater rptQueue;
        protected Panel pnlEmpty;
        protected Label lblMessage;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
            if (!IsPostBack) BindQueue();
        }

        private void BindQueue()
        {
            const string sql = "SELECT 'Review' kind,v.reviewID itemID,COALESCE(v.comment,'Rating: '+CONVERT(nvarchar(1),v.rating)) content,r.title relatedTitle,u.displayName,v.createdAt FROM reviews v JOIN recipes r ON r.recipeID=v.recipeID JOIN users u ON u.userID=v.userID WHERE v.status='Pending' UNION ALL SELECT 'Post',p.postID,p.content,r.title,u.displayName,p.createdAt FROM posts p JOIN recipes r ON r.recipeID=p.recipeID JOIN users u ON u.userID=p.userID WHERE p.status='Pending' UNION ALL SELECT 'Comment',c.postCommentID,c.content,r.title,u.displayName,c.createdAt FROM postComments c JOIN posts p ON p.postID=c.postID JOIN recipes r ON r.recipeID=p.recipeID JOIN users u ON u.userID=c.userID WHERE c.status='Pending' ORDER BY createdAt";
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                rptQueue.DataSource = dt;
                rptQueue.DataBind();
                pnlEmpty.Visible = dt.Rows.Count == 0;
            }
        }

        protected void rptQueue_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
            string[] args = Convert.ToString(e.CommandArgument).Split(':');
            int id;
            if (args.Length != 2 || !Int32.TryParse(args[1], out id) || (e.CommandName != "approve" && e.CommandName != "reject")) return;

            string status = e.CommandName == "approve" ? "Approved" : "Rejected";
            string table = args[0] == "Review" ? "reviews" : args[0] == "Post" ? "posts" : args[0] == "Comment" ? "postComments" : "";
            string key = args[0] == "Review" ? "reviewID" : args[0] == "Post" ? "postID" : args[0] == "Comment" ? "postCommentID" : "";
            if (table == "") return;

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand("UPDATE " + table + " SET status=@status,updatedAt=@now WHERE " + key + "=@id AND status='Pending'", cn))
            {
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@id", id);
                int changed = cmd.ExecuteNonQuery();
                lblMessage.Text = changed == 1 ? "Content " + status.ToLowerInvariant() + "." : "This item has already been reviewed.";
                lblMessage.CssClass = "alert alert-success d-block";
            }
            BindQueue();
        }
    }
}
