using System;
using Masaking.Infrastructure;

namespace Masaking.Admin
{
    public partial class Dashboard : Masaking.Infrastructure.ResponsivePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.RequireAdmin(Response)) return;
        }
    }
}
