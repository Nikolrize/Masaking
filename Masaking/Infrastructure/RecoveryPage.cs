using System;
using System.Web;
using System.Web.UI;

namespace Masaking.Infrastructure
{
    public abstract class RecoveryPage : Page
    {
        protected override void OnInit(EventArgs e)
        {
            if (!Request.IsSecureConnection && !Request.IsLocal)
            {
                throw new HttpException(403, "Use HTTPS for password recovery.");
            }

            ViewStateUserKey = Session.SessionID;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Headers["Referrer-Policy"] = "no-referrer";
            Response.Headers["X-Frame-Options"] = "DENY";

            base.OnInit(e);
        }

        protected override void OnPreLoad(EventArgs e)
        {
            if (Session["RecoveryCsrf"] == null)
            {
                Session["RecoveryCsrf"] = Guid.NewGuid().ToString("N");
            }

            bool isPost = string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase);
            string viewStateCsrf = ViewState["RecoveryCsrf"] as string;
            string sessionCsrf = Session["RecoveryCsrf"] as string;

            if (isPost && !string.Equals(viewStateCsrf, sessionCsrf, StringComparison.Ordinal))
            {
                throw new HttpException(400, "This form has expired. Reload the page and try again.");
            }

            ViewState["RecoveryCsrf"] = Session["RecoveryCsrf"];
            base.OnPreLoad(e);
        }
    }
}