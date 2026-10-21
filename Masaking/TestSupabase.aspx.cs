using Npgsql;
using System;

namespace Masaking
{
    public partial class TestSupabase : System.Web.UI.Page
    {
        protected void btnTestConnection_Click(object sender, EventArgs e)
        {
            try
            {
                NpgsqlConnection con = DbConnection.GetConnection();
                con.Open();

                NpgsqlCommand cmd = new NpgsqlCommand(
                    "SELECT current_database()",
                    con
                );

                string database = Convert.ToString(cmd.ExecuteScalar());

                lblResult.Text = "Connected successfully to: " + database;

                cmd.Dispose();
                con.Close();
            }
            catch (Exception)
            {
                lblResult.Text = "Connection failed. Check your configuration.";
            }
        }
    }
}
