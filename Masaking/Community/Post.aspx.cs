using System;
using System.Web.UI;

namespace Masaking.Community
{
    // Keep existing discussion links working after moving comments into the feed.
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
