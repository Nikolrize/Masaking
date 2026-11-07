using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;

namespace Masaking.Controls
{
    public partial class IngredientEditor : System.Web.UI.UserControl
    {
        public class IngredientRow
        {
            public string name { get; set; }
            public string quantity { get; set; }
            public string unit { get; set; }
        }

        public void Initialise(DataTable ingredients = null)
        {
            List<IngredientRow> rows = new List<IngredientRow>();

            if (ingredients != null)
            {
                foreach (DataRow ingredient in ingredients.Rows)
                {
                    rows.Add(new IngredientRow
                    {
                        name = ingredient["name"].ToString(),

                        quantity = Convert.ToString(
                            ingredient["quantity"],
                            CultureInfo.InvariantCulture),

                        unit = ingredient["unit"].ToString()
                    });
                }
            }

            if (rows.Count == 0)
            {
                rows.Add(new IngredientRow
                {
                    name = "",
                    quantity = "",
                    unit = ""
                });
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            ingredientData.Value = serializer.Serialize(rows);
        }

        public DataTable GetIngredients()
        {
            List<IngredientRow> rows;

            try
            {
                JavaScriptSerializer serializer =
                    new JavaScriptSerializer();

                serializer.MaxJsonLength = 100000;
                serializer.RecursionLimit = 10;

                rows = serializer.Deserialize<List<IngredientRow>>(
                    ingredientData.Value);

                if (rows == null || rows.Count > 100)
                {
                    throw new ArgumentException();
                }
            }
            catch (ArgumentException)
            {
                throw new InvalidDataException(
                    "The ingredient data is invalid. Reload the page and try again.");
            }
            catch (InvalidOperationException)
            {
                throw new InvalidDataException(
                    "The ingredient data is invalid. Reload the page and try again.");
            }

            DataTable ingredients = new DataTable();

            ingredients.Columns.Add("name");
            ingredients.Columns.Add("quantity");
            ingredients.Columns.Add("unit");

            foreach (IngredientRow row in rows)
            {
                if (row == null)
                {
                    throw new InvalidDataException(
                        "An ingredient row is invalid.");
                }

                ingredients.Rows.Add(
                    (row.name ?? "").Trim(),
                    (row.quantity ?? "").Trim(),
                    (row.unit ?? "").Trim());
            }

            return ingredients;
        }
    }
}