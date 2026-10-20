using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Masaking.Learning
{
    public partial class Challenges : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnCheckAnswer_Click(object sender, EventArgs e)
        {
            if (rblAnswers.SelectedIndex == -1)
            {
                lblFeedback.Text = "Please select an answer first.";
                return;
            }

            if (rblAnswers.SelectedValue == "1:1")
            {
                lblFeedback.Text =
                    "Correct! A 1:1 balance of coconut milk to water provides just the right amount of fat for richness while maintaining enough moisture for the rice grains to cook up fluffy and separate.";
            }
            else
            {
                lblFeedback.Text =
                    "Not quite. The correct answer is 1 part coconut milk to 1 part water (1:1)";
            }
        }
    }
}