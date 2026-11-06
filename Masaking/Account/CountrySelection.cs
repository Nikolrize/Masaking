using System.Web.UI.WebControls;

namespace Masaking.Account
{
    internal static class CountrySelection
    {
        private const string Other = "Other";

        private static readonly string[] Countries =
        {
            "Malaysia", "Singapore", "Indonesia", "Thailand", "Brunei", "Philippines",
            "Vietnam", "Cambodia", "Laos", "Myanmar", "China", "India", "Japan", "South Korea",
            "Australia", "New Zealand", "United Kingdom", "United States", "Canada"
        };

        internal static void Populate(DropDownList list)
        {
            list.Items.Add(new ListItem("Select your country", ""));
            foreach (string country in Countries)
                list.Items.Add(country);
            list.Items.Add(Other);
        }

        internal static string Read(DropDownList list, TextBox customCountry)
        {
            return list.SelectedValue == Other ? customCountry.Text.Trim() : list.SelectedValue;
        }

        internal static void SelectSaved(DropDownList list, TextBox customCountry, string savedCountry)
        {
            if (savedCountry != Other && list.Items.FindByValue(savedCountry) != null)
                list.SelectedValue = savedCountry;
            else
            {
                list.SelectedValue = Other;
                customCountry.Text = savedCountry;
            }
        }
    }
}
