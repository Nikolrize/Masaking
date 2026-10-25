using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Community
{
    public abstract class CommunityPage : Page
    {
        protected Label lblMessage;
        protected const int EditWindowDays = 7;
        protected bool currentUserIsAdministrator, currentUserIsActive;
        protected bool databaseAvailable = true;
        protected string CurrentUserAvatarUrl { get; set; } = Database.DefaultAvatar;
        protected string CurrentDisplayName { get; set; } = "Community member";
        protected string CurrentUsername { get; set; } = "";

        protected override void OnInit(EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            RunDatabaseAction(LoadCurrentUser);
            base.OnLoad(e);
        }

        private void LoadCurrentUser()
        {
            currentUserIsActive = false;
            currentUserIsAdministrator = false;
            CurrentUserAvatarUrl = ResolveUrl(Database.DefaultAvatar);
            if (!Database.UserId.HasValue) return;

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT image, role, displayName, username FROM users WHERE userID=@user AND isActive=1", cn))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Session.Clear();
                        return;
                    }

                    currentUserIsActive = true;
                    CurrentDisplayName = Convert.ToString(reader["displayName"]);
                    CurrentUsername = Convert.ToString(reader["username"]);
                    CurrentUserAvatarUrl = GetAvatarUrl(reader["image"]);
                    string role = Convert.ToString(reader["role"]);
                    currentUserIsAdministrator = String.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                        || String.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        protected string GetAvatarUrl(object image)
        {
            return ResolveUrl(Database.AvatarUrl(image));
        }

        protected string CommunityAvatar(object image, object displayName, string label)
        {
            string name = Convert.ToString(displayName);
            string[] words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = words.Length == 0 ? "M" : words[0].Substring(0, 1) + (words.Length > 1 ? words[words.Length - 1].Substring(0, 1) : "");
            string url = Database.AvatarUrl(image);
            bool hasPhoto = url != Database.DefaultAvatar;
            string markup = "<span class='community-avatar rounded-circle' role='img' aria-label='" + System.Web.HttpUtility.HtmlAttributeEncode(label + ": " + name) + "' data-avatar-photo='" + (hasPhoto ? "true" : "false") + "'><span class='community-avatar-initials' aria-hidden='true'>" + Server.HtmlEncode(initials.ToUpperInvariant()) + "</span>";
            if (hasPhoto) markup += "<img src='" + System.Web.HttpUtility.HtmlAttributeEncode(ResolveUrl(url)) + "' alt='' data-community-avatar-photo='true' />";
            return markup + "</span>";
        }

        private static DateTime AsUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
            return value.ToUniversalTime();
        }

        protected bool CanEdit(DateTime createdAt)
        {
            DateTime createdUtc = AsUtc(createdAt);
            DateTime nowUtc = DateTime.UtcNow;
            return createdUtc <= nowUtc && createdUtc >= nowUtc.AddDays(-EditWindowDays);
        }

        protected string ContentDate(object updatedAt, object createdAt)
        {
            DateTime createdUtc = AsUtc(Convert.ToDateTime(createdAt));
            bool wasEdited = updatedAt != null && updatedAt != DBNull.Value;
            DateTime displayedUtc = wasEdited ? AsUtc(Convert.ToDateTime(updatedAt)) : createdUtc;

            string timestamp = CanEdit(createdUtc)
                ? RelativeAge(displayedUtc)
                : displayedUtc.ToLocalTime().ToString("dd MMM yyyy");

            return (wasEdited ? "Edited · " : "") + timestamp;
        }

        private static string RelativeAge(DateTime utcDate)
        {
            TimeSpan age = DateTime.UtcNow - utcDate;

            if (age.TotalSeconds < 60) return "Just now";

            if (age.TotalMinutes < 60)
            {
                int minutes = Math.Max(1, (int)age.TotalMinutes);
                return minutes == 1 ? "1 min ago" : minutes + " mins ago";
            }

            if (age.TotalHours < 24)
            {
                int hours = Math.Max(1, (int)age.TotalHours);
                return hours == 1 ? "1 hour ago" : hours + " hours ago";
            }

            int days = Math.Max(1, (int)age.TotalDays);
            return days == 1 ? "1 day ago" : days + " days ago";
        }

        protected bool IsPostOwner(object ownerUserId)
        {
            int ownerId;
            return Database.UserId.HasValue
                && ownerUserId != null
                && ownerUserId != DBNull.Value
                && Int32.TryParse(Convert.ToString(ownerUserId), out ownerId)
                && ownerId == Database.UserId.Value;
        }

        protected bool CanDeletePost(object ownerUserId)
        {
            return IsPostOwner(ownerUserId) || currentUserIsAdministrator;
        }

        protected string GetLikeButtonText(object likeCount)
        {
            // Keep the button content in Text so it survives postbacks without nested controls.
            return "<span class=\"community-like-heart\"><i class=\"bi bi-heart community-icon community-heart-outline\" aria-hidden=\"true\"></i><i class=\"bi bi-heart-fill community-icon community-heart-filled\" aria-hidden=\"true\"></i></span>"
                + "<span class=\"small\">" + System.Web.HttpUtility.HtmlEncode(Convert.ToString(likeCount))
                + "</span><span class=\"visually-hidden\">likes</span>";
        }

        protected string PostHeading(object value)
        {
            string content = Convert.ToString(value).Replace("\r\n", "\n");
            int end = content.IndexOf('\n');
            return end > 0 && end <= 140 ? content.Substring(0, end) : "";
        }

        protected string PostBody(object value)
        {
            string content = Convert.ToString(value).Replace("\r\n", "\n");
            string heading = PostHeading(value);
            return heading.Length == 0 ? content : content.Substring(heading.Length).TrimStart('\n');
        }

        protected void rptPosts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "like" && e.CommandName != "unlike") return;
            LinkButton button = e.CommandSource as LinkButton;
            Label error = e.Item.FindControl("lblLikeError") as Label;
            if (button == null || error == null)
            {
                Trace.Warn("Community", "Unable to find the like button or its error label.");
                return;
            }
            if (!databaseAvailable)
            {
                error.Text = "Likes are temporarily unavailable. Please try again later.";
                return;
            }
            if (!RequireCurrentUser()) return;
            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out int postId) || postId <= 0) return;

            try
            {
                // Save and read the real count in one transaction; repeated requests are idempotent.
                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand(@"
                    SET XACT_ABORT ON;
                    BEGIN TRANSACTION;
                    IF EXISTS (
                        SELECT 1 FROM posts p WITH (UPDLOCK, HOLDLOCK)
                        JOIN users u ON u.userID=p.userID
                        WHERE p.postID=@post AND p.status='Approved' AND u.isActive=1)
                    BEGIN
                        IF @like=1
                        BEGIN
                            IF NOT EXISTS (SELECT 1 FROM postLikes WHERE postID=@post AND userID=@user)
                                INSERT INTO postLikes(postID, userID, likedAt)
                                VALUES(@post, @user, SYSUTCDATETIME());
                        END
                        ELSE
                            DELETE FROM postLikes WHERE postID=@post AND userID=@user;

                        SELECT (SELECT COUNT(*) FROM postLikes WHERE postID=@post) AS likeCount,
                            CASE WHEN EXISTS (SELECT 1 FROM postLikes WHERE postID=@post AND userID=@user)
                                THEN 1 ELSE 0 END AS userLiked;
                    END;
                    COMMIT TRANSACTION;", cn))
                {
                    cmd.Parameters.Add("@post", SqlDbType.Int).Value = postId;
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    cmd.Parameters.Add("@like", SqlDbType.Bit).Value = e.CommandName == "like";
                    // Fill drains the complete batch, including COMMIT, before updating the UI.
                    DataTable result = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(result);
                    if (result.Rows.Count == 0)
                    {
                        error.Text = "This post is no longer available.";
                        return;
                    }

                    DataRow row = result.Rows[0];
                    bool liked = Convert.ToInt32(row["userLiked"]) == 1;
                    button.CommandName = liked ? "unlike" : "like";
                    button.ToolTip = liked ? "Unlike post" : "Like post";
                    button.Attributes["aria-label"] = button.ToolTip;
                    button.Attributes["aria-pressed"] = liked ? "true" : "false";
                    button.Text = GetLikeButtonText(row["likeCount"]);
                }
                // Keep the existing repeater intact so only this item's UpdatePanel is replaced.
            }
            catch (SqlException ex)
            {
                Trace.Warn("Community", "Unable to save the like.", ex);
                error.Text = "Your like could not be saved. Please try again.";
            }
        }

        protected string CommunityImageUrl(object value)
        {
            string path = Convert.ToString(value ?? String.Empty).Trim().Replace('\\', '/');
            if (path.Length == 0) return String.Empty;

            if (path.StartsWith("~/", StringComparison.Ordinal)) path = path.Substring(1);
            else if (!path.StartsWith("/", StringComparison.Ordinal)) path = "/" + path;

            const string legacyPrefix = "/Content/Uploads/Community/";
            const string prefix = "/Content/images/Uploads/Community/";

            if (path.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase)) path = prefix + path.Substring(legacyPrefix.Length);
            if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return String.Empty;

            string fileName = path.Substring(path.LastIndexOf('/') + 1);
            string extension = Path.GetExtension(fileName);

            if (!String.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _))
            {
                return String.Empty;
            }
            if (path.Substring(prefix.Length) != fileName) return String.Empty;
            return ResolveUrl("~" + prefix + fileName);
        }

        protected void SafeDeleteUploadedImage(string imagePath)
        {
            if (String.IsNullOrWhiteSpace(imagePath)) return;
            try { ImageUpload.DeleteUploaded(imagePath, "Community"); }
            catch (Exception) { /* Do not replace the original database/upload error with a cleanup error. */ }
        }

        private void RedirectToLogin()
        {
            Response.Redirect("../Account/Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected bool RequireCurrentUser()
        {
            if (currentUserIsActive && Database.UserId.HasValue) return true;
            RedirectToLogin();
            return false;
        }

        protected void Show(string message, bool success)
        {
            lblMessage.CssClass = success ? "alert alert-success d-block mb-3" : "alert alert-danger d-block mb-3";
            lblMessage.Text = Server.HtmlEncode(message);
        }

        protected void RunDatabaseAction(Action action)
        {
            if (!databaseAvailable) return;
            try { action(); }
            catch (SqlException ex)
            {
                databaseAvailable = false;
                Trace.Warn("Community", "Database operation failed.", ex);
                Show("The community is temporarily unavailable. Please try again later.", false);
            }
        }

        protected void Navigate(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected string RecipeImageUrl(object value)
        {
            string path = Convert.ToString(value).Replace('\\', '/');
            Uri uri;
            if (Uri.TryCreate(path, UriKind.Absolute, out uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) return path;
            if (path.StartsWith("~/")) path = path.Substring(1);
            path = path.Replace("/Content/Uploads/Recipes/", "/Content/images/Uploads/Recipes/");
            const string prefix = "/Content/images/Uploads/Recipes/";
            Guid id;
            string file = Path.GetFileName(path);
            return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && path.Substring(prefix.Length) == file
                && file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                && Guid.TryParseExact(Path.GetFileNameWithoutExtension(file), "N", out id) ? ResolveUrl("~" + path) : "";
        }

        protected DataTable ReadPosts(int? postId, string sort)
        {
            string order = sort == "oldest" ? "p.createdAt ASC, p.postID ASC"
                : sort == "popular" ? "likeCount DESC, commentCount DESC, p.postID DESC" : "p.createdAt DESC, p.postID DESC";
            string sql = @"SELECT p.postID, p.content, p.image, p.createdAt, p.updatedAt, p.userID,
                r.recipeID, r.title, r.image AS recipeImage, r.cuisine AS recipeSummary,
                u.displayName, u.username, u.image AS authorImage,
                (SELECT COUNT(*) FROM postComments c JOIN users cu ON cu.userID=c.userID
                    WHERE c.postID=p.postID AND c.status='Approved' AND cu.isActive=1) AS commentCount,
                (SELECT COUNT(*) FROM postLikes pl WHERE pl.postID=p.postID) AS likeCount,
                (SELECT COUNT(*) FROM postLikes pl WHERE pl.postID=p.postID AND pl.userID=@user) AS userLiked
                FROM posts p JOIN users u ON u.userID=p.userID
                LEFT JOIN recipes r ON r.recipeID=p.recipeID AND r.status='Approved'
                WHERE p.status='Approved' AND u.isActive=1 AND (@post IS NULL OR p.postID=@post)
                ORDER BY " + order;
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = (object)Database.UserId ?? DBNull.Value;
                cmd.Parameters.Add("@post", SqlDbType.Int).Value = (object)postId ?? DBNull.Value;
                DataTable data = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(data);
                return data;
            }
        }

        protected void btnDeletePost_Click(object sender, EventArgs e)
        {
            RunDatabaseAction(() =>
            {
                Button button = sender as Button;
                int postId;
                if (!RequireCurrentUser() || button == null || !Int32.TryParse(button.CommandArgument, out postId)) return;
                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand(@"UPDATE posts
                    SET status=CASE WHEN userID=@user THEN 'Deleted' ELSE 'Removed' END
                    WHERE postID=@post AND status='Approved' AND (userID=@user OR @admin=1)", cn))
                {
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    cmd.Parameters.Add("@post", SqlDbType.Int).Value = postId;
                    cmd.Parameters.Add("@admin", SqlDbType.Bit).Value = currentUserIsAdministrator;
                    if (cmd.ExecuteNonQuery() != 1) { Show("This post could not be deleted.", false); return; }
                }
                AfterPostDeleted();
            });
        }

        protected virtual void AfterPostDeleted() { Navigate("Community.aspx"); }
    }
}
