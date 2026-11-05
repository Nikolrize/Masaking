using System;
using System.Web.UI;

namespace Masaking.Community
{
    // Preserve saved discussion links now that comments are displayed in the feed.
    public partial class Post : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int postId;
            string value = Request.QueryString["id"] ?? Request.QueryString["post"];
            string destination = "~/Community/Community.aspx";
            if (Int32.TryParse(value, out postId) && postId > 0)
                destination += "?post=" + postId + "#post-" + postId;
            Response.Redirect(destination, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
