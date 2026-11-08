using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace Masaking
{
    public class Global : HttpApplication
    {
        void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            // Covers pages which read UserId directly as well as RequireUser/RequireAdmin.
            if (Context.Session == null || !(Session["UserId"] is int)) return;
            try
            {
                using (var cn = Masaking.Infrastructure.Database.Open())
                using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT authVersion FROM dbo.users WHERE userID=@id AND isActive=1", cn))
                {
                    cmd.Parameters.AddWithValue("@id", (int)Session["UserId"]);
                    object current = cmd.ExecuteScalar();
                    if (!(Session["AuthVersion"] is int) || !(current is int)
                        || (int)Session["AuthVersion"] != (int)current)
                        Session.Clear();
                }
            }
            catch (System.Data.SqlClient.SqlException)
            {
                Session.Clear();
                System.Diagnostics.Trace.TraceError("Session validation failed. Check database schema and availability.");
            }
        }

        void Application_Start(object sender, EventArgs e)
        {
            Masaking.Infrastructure.Database.EnsureDefaultAdmin();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
