using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using Masaking.Infrastructure;

namespace Masaking.Account
{
    public abstract class ProfilePage : Page
    {
        protected string ProfileName = "Community member";
        protected string ProfileUsername = "";
        protected string ProfileEmail = "";
        protected string ProfileCountry = "";
        protected string ProfileImage = Database.DefaultAvatar;
        protected int RecipeCount;
        protected int PostCount;

        protected string AvatarDisplayUrl
        {
            get { return ProfileImage; }
        }

        protected bool IsAdministrator
        {
            get
            {
                return String.Equals(Database.UserRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                       String.Equals(Database.UserRole, "Administrator", StringComparison.OrdinalIgnoreCase);
            }
        }

        protected string Initials
        {
            get
            {
                string[] words = ProfileName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return words.Length == 0 ? "M" : (words[0].Substring(0, 1) + (words.Length > 1 ? words[words.Length - 1].Substring(0, 1) : "")).ToUpperInvariant();
            }
        }

        protected string H(object value)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(value));
        }

        protected string Icon(string name, string extra = "")
        {
            return "<i class='bi bi-" + name + " profile-icon " + extra + "' aria-hidden='true'></i>";
        }

        protected string ContactIcon(string name)
        {
            return Icon(name);
        }

        protected override void OnInit(EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
            base.OnInit(e);
        }

        protected void LoadAccount()
        {
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT username, displayName, email, country, image,
                (SELECT COUNT(*) FROM recipes WHERE userID=@id) AS recipeCount,
                (SELECT COUNT(*) FROM posts WHERE userID=@id AND status='Approved') AS postCount
                FROM users WHERE userID=@id AND isActive=1", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = Database.UserId.Value;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return;
                    ProfileName = Convert.ToString(reader["displayName"]);
                    ProfileUsername = Convert.ToString(reader["username"]);
                    ProfileEmail = Convert.ToString(reader["email"]);
                    ProfileCountry = Convert.ToString(reader["country"]);
                    ProfileImage = Database.AvatarUrl(reader["image"]);
                    Session["UserImage"] = ProfileImage;
                    RecipeCount = Convert.ToInt32(reader["recipeCount"]);
                    PostCount = Convert.ToInt32(reader["postCount"]);
                }
            }
        }

        protected DataTable UserRecipes()
        {
            return Read(@"SELECT TOP (12) r.recipeID, r.title, r.cuisine, r.preparationTime, r.image, r.status,
                (SELECT COUNT(*) FROM favourites f WHERE f.recipeID=r.recipeID) AS saves
                FROM recipes r WHERE r.userID=@id ORDER BY r.createdAt DESC, r.recipeID DESC");
        }

        protected DataTable UserActivity()
        {
            return Read(@"SELECT TOP (30) activity.* FROM (
                SELECT p.postID, p.content, p.createdAt, 'post' AS kind, r.image AS image, r.title AS recipeTitle
                FROM posts p LEFT JOIN recipes r ON r.recipeID=p.recipeID
                WHERE p.userID=@id AND p.status='Approved'
                UNION ALL
                SELECT p.postID, c.content, c.createdAt, 'comment' AS kind, r.image AS image, r.title AS recipeTitle
                FROM postComments c JOIN posts p ON p.postID=c.postID JOIN users u ON u.userID=p.userID
                LEFT JOIN recipes r ON r.recipeID=p.recipeID
                WHERE c.userID=@id AND c.status='Approved' AND p.status='Approved' AND u.isActive=1
                ) activity ORDER BY activity.createdAt DESC, activity.postID DESC");
        }

        private DataTable Read(string sql)
        {
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = Database.UserId.Value;
                DataTable table = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(table);
                return table;
            }
        }

        protected string Picture(object value)
        {
            string path = Convert.ToString(value).Replace('\\', '/');
            Uri uri;
            if (Uri.TryCreate(path, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)) return path;
            if (path.StartsWith("~/", StringComparison.Ordinal)) path = path.Substring(1);
            path = path.Replace("/Content/Uploads/Recipes/", "/Content/images/Uploads/Recipes/");
            const string prefix = "/Content/images/Uploads/Recipes/";
            Guid id;
            string file = Path.GetFileName(path);
            return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && path.Substring(prefix.Length) == file && file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && Guid.TryParseExact(Path.GetFileNameWithoutExtension(file), "N", out id) ? ResolveUrl("~" + path) : "";
        }

        protected string RecipeUrl(object id, object status)
        {
            return Convert.ToString(status) == "Approved" ? "../Recipes/RecipeDetails.aspx?id=" + Convert.ToInt32(id) : "../Recipes/MyRecipes.aspx";
        }

        protected string ActivityText(object kind)
        {
            return Convert.ToString(kind) == "post" ? "You posted a new discussion" : "You commented on a discussion";
        }

        protected string Excerpt(object value)
        {
            string text = Convert.ToString(value).Replace("\r", " ").Replace("\n", " ").Trim();
            return text.Length > 110 ? text.Substring(0, 107) + "…" : text;
        }

        protected string Age(object value)
        {
            DateTime date = DateTime.SpecifyKind(Convert.ToDateTime(value), DateTimeKind.Utc);
            TimeSpan age = DateTime.UtcNow - date;
            if (age.TotalMinutes < 1) return "Just now";
            if (age.TotalHours < 1) return Math.Max(1, (int)age.TotalMinutes) + " min ago";
            if (age.TotalDays < 1) return Math.Max(1, (int)age.TotalHours) + " hr ago";
            if (age.TotalDays < 7) return Math.Max(1, (int)age.TotalDays) + " days ago";
            return date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }
    }
}