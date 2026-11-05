using System.Data;
using System.Data.SqlClient;

namespace Masaking.Infrastructure
{
    public class CategoryRepository
    {
        public static DataTable GetActiveCategories(bool alphabetical)
        {
            SqlConnection con = Database.Open();
            DataTable categories = new DataTable();

            try
            {
                string orderBy = alphabetical
                    ? "categoryName"
                    : "categoryID";

                SqlCommand cmd = new SqlCommand(@"
                    SELECT categoryID, categoryName
                    FROM dbo.categories
                    WHERE isActive = 1
                    ORDER BY " + orderBy + ";", con);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(categories);
            }
            finally
            {
                con.Close();
            }

            return categories;
        }
    }
}