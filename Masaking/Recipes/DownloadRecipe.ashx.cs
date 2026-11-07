using Masaking.Infrastructure;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace Masaking.Recipes
{
    public class DownloadRecipe : IHttpHandler, IReadOnlySessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            int recipeId;

            if (!int.TryParse(context.Request.QueryString["id"], out recipeId)
                || recipeId <= 0)
            {
                context.Response.StatusCode = 404;
                context.Response.Write("Recipe not found.");
                return;
            }

            DataTable recipes = RecipeRepository.GetRecipeById(recipeId);

            if (recipes.Rows.Count == 0)
            {
                context.Response.StatusCode = 404;
                context.Response.Write("Recipe not found.");
                return;
            }

            DataRow recipe = recipes.Rows[0];

            // build pdf content
            Document document = new Document();
            document.Info.Title = recipe["title"].ToString();

            document.Styles[StyleNames.Normal].Font.Name = "Arial";
            document.Styles[StyleNames.Normal].Font.Size = 11;

            Section section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = Unit.FromCentimeter(2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);

            Paragraph brand = section.AddParagraph("Masaking");
            brand.Format.Font.Size = 12;
            brand.Format.SpaceAfter = Unit.FromCentimeter(0.5);

            Paragraph title =
                section.AddParagraph(recipe["title"].ToString());

            title.Format.Font.Size = 24;
            title.Format.Font.Bold = true;
            title.Format.Font.Color = Colors.DarkGreen;
            title.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            Paragraph author = section.AddParagraph(
                "By " + recipe["displayName"].ToString());

            author.Format.SpaceAfter = Unit.FromCentimeter(0.5);

            AddPhoto(context, section, recipe["image"], 12);

            section.AddParagraph(recipe["description"].ToString());

            // Recipe facts
            Paragraph facts = section.AddParagraph();
            facts.Format.SpaceBefore = Unit.FromCentimeter(0.5);
            facts.Format.SpaceAfter = Unit.FromCentimeter(0.5);

            facts.AddText("Cuisine: " + recipe["cuisine"]);
            facts.AddLineBreak();

            facts.AddText("Category: " + recipe["categoryName"]);
            facts.AddLineBreak();

            facts.AddText("Preparation time: " + recipe["preparationTime"] + " mins");
            facts.AddLineBreak();

            facts.AddText("Difficulty: " + recipe["difficulty"]);
            facts.AddLineBreak();

            facts.AddText("Servings: " + recipe["servings"]);
            facts.AddLineBreak();

            facts.AddText(
                "Estimated cost: RM" +
                Convert.ToDecimal(recipe["estimatedCost"]).ToString("0.00"));

            // Load ingredients and steps after recipe access has been checked.
            DataSet contents = LoadRecipeContents(recipeId);

            // Ingredients
            Paragraph ingredientHeading = section.AddParagraph("Ingredients");
            ingredientHeading.Format.Font.Size = 16;
            ingredientHeading.Format.Font.Bold = true;
            ingredientHeading.Format.SpaceBefore = Unit.FromCentimeter(0.5);
            ingredientHeading.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            ingredientHeading.Format.KeepWithNext = true;

            foreach (DataRow ingredient in contents.Tables[0].Rows)
            {
                string quantity =
                    Convert.ToDecimal(ingredient["quantity"]).ToString("0.##");

                string optional = Convert.ToBoolean(ingredient["isOptional"])
                    ? " (optional)"
                    : "";

                Paragraph item = section.AddParagraph(
                    "- " + ingredient["ingredientName"] +
                    " — " + quantity + " " + ingredient["unit"] + optional);

                item.Format.SpaceAfter = Unit.FromCentimeter(0.15);
            }

            if (contents.Tables[0].Rows.Count == 0)
            {
                section.AddParagraph("No ingredients listed.");
            }

            // Cooking steps
            Paragraph stepHeading = section.AddParagraph("Cooking Steps");
            stepHeading.Format.Font.Size = 16;
            stepHeading.Format.Font.Bold = true;
            stepHeading.Format.SpaceBefore = Unit.FromCentimeter(0.7);
            stepHeading.Format.SpaceAfter = Unit.FromCentimeter(0.3);
            stepHeading.Format.KeepWithNext = true;

            foreach (DataRow step in contents.Tables[1].Rows)
            {
                Paragraph stepTitle = section.AddParagraph(
                    "Step " + step["stepNumber"] +
                    " · " + step["estimatedMinutes"] + " mins");

                stepTitle.Format.Font.Bold = true;
                stepTitle.Format.KeepWithNext = true;
                stepTitle.Format.SpaceAfter = Unit.FromCentimeter(0.15);

                AddPhoto(context, section, step["image"], 5);

                Paragraph instruction =
                    section.AddParagraph(step["instruction"].ToString());

                instruction.Format.SpaceAfter = Unit.FromCentimeter(0.4);
            }

            if (contents.Tables[1].Rows.Count == 0)
            {
                section.AddParagraph("No cooking steps listed.");
            }

            // convert document into PDF bytes
            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = document;
            renderer.RenderDocument();

            byte[] pdfBytes;

            using (MemoryStream stream = new MemoryStream())
            {
                renderer.PdfDocument.Save(stream, false);
                pdfBytes = stream.ToArray();
            }

            renderer.PdfDocument.Close();

            // send file to browser
            context.Response.Clear();
            context.Response.ContentType = "application/pdf";
            context.Response.AddHeader(
                "Content-Disposition",
                "attachment; filename=\"Masaking-Recipe-" + recipeId + ".pdf\"");

            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.BinaryWrite(pdfBytes);
        }

        public bool IsReusable
        {
            get { return false; }
        }

        private DataSet LoadRecipeContents(int recipeId)
        {
            SqlConnection con = Database.Open();

            try
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT ingredientName, quantity, unit, isOptional
                    FROM dbo.ingredients
                    WHERE recipeID = @recipeID
                    ORDER BY displayOrder, ingredientID;

                    SELECT stepNumber, instruction, image, estimatedMinutes
                    FROM dbo.steps
                    WHERE recipeID = @recipeID
                    ORDER BY stepNumber, stepID;", con);

                cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet contents = new DataSet();
                adapter.Fill(contents);

                return contents;
            }
            finally
            {
                con.Close();
            }
        }

        private void AddPhoto(HttpContext context, Section section, object imageValue, double width)
        {
            string imageUrl = Convert.ToString(imageValue);

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            if (imageUrl.StartsWith("/Content/",
                StringComparison.OrdinalIgnoreCase))
            {
                imageUrl = "~" + imageUrl;
            }

            if (!imageUrl.StartsWith("~/Content/",
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string imagePath = context.Server.MapPath(imageUrl);
            string contentFolder = context.Server.MapPath("~/Content/");

            if (!imagePath.StartsWith(contentFolder,
                StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(imagePath))
            {
                return;
            }

            MigraDoc.DocumentObjectModel.Shapes.Image photo =
                section.AddImage(imagePath);

            photo.LockAspectRatio = true;
            photo.Width = Unit.FromCentimeter(width);

            section.AddParagraph();
        }
    }
}