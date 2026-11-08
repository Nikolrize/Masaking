using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace Masaking.Infrastructure
{
    //admin and member views share their editor code, but never their access rules
    public abstract class ActivityEditorPage : ResponsivePage
    {
        protected bool MemberEditor
        {
            get { return Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/Learning/", StringComparison.OrdinalIgnoreCase); }
        }

        protected bool IsAdministrator
        {
            get { return String.Equals(Database.UserRole, "Admin", StringComparison.OrdinalIgnoreCase); }
        }

        protected virtual string ActivityKind { get { return null; } }
        protected virtual int ActivityId { get { return 0; } }

        protected override void OnPreInit(EventArgs e)
        {
            if (!RequireEditor()) throw new HttpException(403, "Access denied.");
            base.OnPreInit(e);
            if (!MemberEditor) MasterPageFile = "~/Admin/Admin.Master";
            ViewStateUserKey = Session.SessionID;
        }

        protected bool RequireEditor()
        {
            bool allowed = MemberEditor ? Database.RequireUser(Response) : Database.RequireAdmin(Response);
            if (!allowed) return false;
            if (ActivityKind != null) CheckActivity(ActivityKind, ActivityId);
            return true;
        }

        protected string EditorUrl(string page)
        {
            return (MemberEditor ? "~/Learning/" : "~/Admin/") + page;
        }

        protected void AddOwnerParameters(SqlCommand command)
        {
            command.Parameters.Add("@editorAdmin", SqlDbType.Bit).Value = IsAdministrator;
            command.Parameters.Add("@editorUser", SqlDbType.Int).Value = Database.UserId.Value;
        }

        protected void CheckActivity(string kind, int id)
        {
            using (SqlConnection cn = Database.Open()) CheckActivity(kind, id, cn, null);
        }

        // Call again inside the write transaction. HOLDLOCK keeps ownership stable until commit.
        //call again inside the write transaction HOLDLOCK keeps ownership stable until commit 
        protected void CheckActivity(string kind, int id, SqlConnection cn, SqlTransaction tx)
        {
            string table, key;
            if (kind == "simulation") { table = "simulations"; key = "simulationID"; }
            else if (kind == "challenge") { table = "challenges"; key = "challengeID"; }
            else throw new ArgumentException("Unknown activity type.");
            string sql = "SELECT a." + key + " FROM dbo." + table + " a WITH (UPDLOCK,HOLDLOCK) LEFT JOIN dbo.recipes r WITH (HOLDLOCK) ON r.recipeID=a.recipeID" + " WHERE a." + key + "=@activity AND (@editorAdmin=1 OR r.userID=@editorUser);";
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                AddOwnerParameters(cmd);
                cmd.Parameters.Add("@activity", SqlDbType.Int).Value = id;
                if (cmd.ExecuteScalar() == null) throw new HttpException(403, "This activity is unavailable or belongs to another recipe owner.");
            }
        }

        protected void CheckRecipe(int id, SqlConnection cn, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(@"SELECT recipeID FROM dbo.recipes WITH (UPDLOCK,HOLDLOCK) WHERE recipeID=@recipe AND status='Approved' AND (@editorAdmin=1 OR userID=@editorUser);", cn, tx))
            {
                AddOwnerParameters(cmd);
                cmd.Parameters.Add("@recipe", SqlDbType.Int).Value = id;
                if (cmd.ExecuteScalar() == null) throw new HttpException(403, "Select an approved recipe that you are allowed to manage.");
            }
        }
    }
}
