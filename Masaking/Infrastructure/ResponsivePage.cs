using System;
using System.Web;
using System.Web.UI;
using Microsoft.AspNet.FriendlyUrls.Resolvers;

namespace Masaking.Infrastructure
{
    public abstract class ResponsivePage : Page
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            MasterPageFile = WebFormsFriendlyUrlResolver.IsMobileView(new HttpContextWrapper(Context))
                ? "~/Site.Mobile.Master"
                : "~/Site.Master";
        }
    }
}
