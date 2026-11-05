using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Masaking
{
    public partial class Contact : Masaking.Infrastructure.ResponsivePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblHeading.Text = Server.HtmlEncode(Title);
        }
    }
}
