using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;

namespace Masaking.Controls
{
    public partial class CookingStepEditor : System.Web.UI.UserControl
    {
        public class StepRow
        {
            public string instruction { get; set; }
            public string minutes { get; set; }
            public string image { get; set; }
        }

        public void Initialise(DataTable steps = null)
        {
            List<StepRow> rows = new List<StepRow>();

            if (steps != null)
            {
                foreach (DataRow step in steps.Rows)
                {
                    rows.Add(new StepRow
                    {
                        instruction = step["instruction"].ToString(),

                        minutes = Convert.ToString(
                            step["minutes"],
                            CultureInfo.InvariantCulture),

                        image = steps.Columns.Contains("image")
                            ? step["image"].ToString()
                            : ""
                    });
                }
            }

            if (rows.Count == 0)
            {
                rows.Add(new StepRow
                {
                    instruction = "",
                    minutes = "0",
                    image = ""
                });
            }

            JavaScriptSerializer serializer =
                new JavaScriptSerializer();

            stepData.Value = serializer.Serialize(rows);
        }

        public DataTable GetSteps()
        {
            List<StepRow> rows;

            try
            {
                JavaScriptSerializer serializer =
                    new JavaScriptSerializer();

                serializer.MaxJsonLength = 200000;
                serializer.RecursionLimit = 10;

                rows = serializer.Deserialize<List<StepRow>>(
                    stepData.Value);

                if (rows == null || rows.Count > 50)
                {
                    throw new ArgumentException();
                }
            }
            catch (ArgumentException)
            {
                throw new InvalidDataException(
                    "The cooking steps are invalid. Reload the page and try again.");
            }
            catch (InvalidOperationException)
            {
                throw new InvalidDataException(
                    "The cooking steps are invalid. Reload the page and try again.");
            }

            DataTable steps = new DataTable();

            steps.Columns.Add("instruction");
            steps.Columns.Add("minutes");
            steps.Columns.Add("image");

            foreach (StepRow row in rows)
            {
                if (row == null)
                {
                    throw new InvalidDataException(
                        "A cooking step is invalid.");
                }

                steps.Rows.Add(
                    (row.instruction ?? "").Trim(),
                    (row.minutes ?? "").Trim(),
                    (row.image ?? "").Trim());
            }

            return steps;
        }
    }
}