using Masaking.Infrastructure;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Masaking.Admin
{
    public partial class ManageCommunity : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            if (!IsPostBack)
            {
                BindCommunity();
            }
        }

        private void BindCommunity()
        {
            const string sql = @" SELECT items.* FROM (SELECT 'Review' AS kind, v.reviewID AS itemID, N'Rating: ' + CONVERT(nvarchar(10), v.rating) + N'/5' + CASE WHEN NULLIF(v.comment, N'') IS NULL THEN N'' ELSE N' - ' + v.comment END AS content, COALESCE(r.title, N'No recipe attached') AS relatedTitle, u.displayName, v.status, v.createdAt, CAST(NULL AS nvarchar(255)) AS image FROM dbo.reviews v INNER JOIN dbo.users u ON u.userID=v.userID LEFT JOIN dbo.recipes r ON r.recipeID=v.recipeID

              UNION ALL

              SELECT 'Post', p.postID, p.content, COALESCE(r.title, N'No recipe attached'), u.displayName, p.status, p.createdAt, p.image FROM dbo.posts p INNER JOIN dbo.users u ON u.userID=p.userID LEFT JOIN dbo.recipes r ON r.recipeID=p.recipeID

              UNION ALL

                    SELECT 'Comment', c.postCommentID, c.content, COALESCE(r.title, N'No recipe attached'), u.displayName, c.status, c.createdAt, CAST(NULL AS nvarchar(255)) FROM dbo.postComments c INNER JOIN dbo.posts p ON p.postID=c.postID INNER JOIN dbo.users u ON u.userID=c.userID LEFT JOIN dbo.recipes r ON r.recipeID=p.recipeID) items
                WHERE (@type='' OR items.kind=@type) AND (@status='' OR items.status=@status) AND ( @search='' OR CHARINDEX(@search, items.content)>0 OR CHARINDEX(@search, items.displayName)>0 OR CHARINDEX(@search, items.relatedTitle)>0 )
                ORDER BY items.createdAt DESC,items.kind,items.itemID DESC;";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@search", SqlDbType.NVarChar, 150).Value = txtSearch.Text.Trim();

                cmd.Parameters.Add("@type", SqlDbType.NVarChar, 20).Value = ddlType.SelectedValue;

                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = ddlStatus.SelectedValue;

                DataTable items = new DataTable();
                adapter.Fill(items);

                items.Columns.Add("imageUrl", typeof(string));
                items.Columns.Add("submittedLocal", typeof(DateTime));

                foreach (DataRow row in items.Rows)
                {

                    row["imageUrl"] = AdminTable.RecipeImage(this, row["image"]);

                    row["submittedLocal"] = row["createdAt"] == DBNull.Value ? DBNull.Value : (object)AdminTable.MalaysiaTime(row["createdAt"]);
                }

                AdminTable.Bind(
                    gvCommunity,
                    items,
                    ViewState,
                    "Community",
                    "createdAt",
                    "DESC");

                lblCount.Text = items.Rows.Count + (items.Rows.Count == 1 ? " item matches" : " items match") + " the filters.";
            }
        }

        protected bool CanApprove(object value)
        {
            string status = Convert.ToString(value);

            return status == "Pending" || status == "Rejected";
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            gvCommunity.PageIndex = 0;
            BindCommunity();
        }

        protected void Filters_Changed(object sender, EventArgs e)
        {
            btnSearch_Click(sender, e);
        }

        protected void gvCommunity_PageIndexChanging(
            object sender,
            GridViewPageEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            gvCommunity.PageIndex = e.NewPageIndex;
            BindCommunity();
        }

        protected void gvCommunity_Sorting(
            object sender,
            GridViewSortEventArgs e)
        {
            if (!Database.RequireAdmin(Response))
            {
                e.Cancel = true;
                return;
            }

            AdminTable.ToggleSort(
                e,
                ViewState,
                "Community",
                "createdAt",
                "DESC",
                "kind",
                "itemID",
                "content",
                "relatedTitle",
                "displayName",
                "status",
                "createdAt");

            gvCommunity.PageIndex = 0;
            BindCommunity();
        }

        protected void gvCommunity_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;

            if (e.CommandName != "ApproveContent" && e.CommandName != "RejectContent" && e.CommandName != "HideContent")
            {
                return;
            }

            string[] args = Convert.ToString(e.CommandArgument).Split(':');

            int itemId;

            if (args.Length != 3 || !Int32.TryParse(args[1], out itemId) || itemId <= 0)
            {
                ShowMessage("Invalid content selection.", "danger");

                return;
            }

            string table;
            string key;

            //table names must come from this fixed list 
            switch (args[0])
            {
                case "Post":
                    table = "posts";
                    key = "postID";
                    break;

                case "Comment":
                    table = "postComments";
                    key = "postCommentID";
                    break;

                case "Review":
                    table = "reviews";
                    key = "reviewID";
                    break;

                default:
                    ShowMessage("Invalid content type.", "danger");
                    return;
            }

            string previousStatus = args[2];
            string newStatus;
            string successMessage;

            if (e.CommandName == "ApproveContent" && CanApprove(previousStatus))
            {
                newStatus = "Approved";
                successMessage = "Content approved.";
            }
            else if (e.CommandName == "RejectContent" && previousStatus == "Pending")
            {
                newStatus = "Rejected";
                successMessage = "Content rejected.";
            }
            else if (e.CommandName == "HideContent" && previousStatus == "Approved")
            {
                newStatus = "Rejected";
                successMessage = "Content hidden from public view.";
            }
            else
            {
                ShowMessage("This action is unavailable for the selected status.", "warning");
                return;
            }

            string sql = "UPDATE dbo." + table + " SET status=@status,updatedAt=@now" + " WHERE " + key + "=@id" + " AND status=@previousStatus;";

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = newStatus;

                cmd.Parameters.Add("@now", SqlDbType.DateTime2).Value = DateTime.UtcNow;

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = itemId;

                cmd.Parameters.Add("@previousStatus", SqlDbType.NVarChar, 20).Value = previousStatus;

                int changed = cmd.ExecuteNonQuery();

                ShowMessage(
                    changed == 1 ? successMessage : "No change made. Another person may have changed this item.",
                    changed == 1 ? "success" : "warning");
            }

            BindCommunity();
        }

        private void ShowMessage(string message, string colour)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "alert alert-" + colour + " d-block";
        }
    }
}