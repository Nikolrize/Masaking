using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Masaking.Infrastructure
{
    public static class AdminTable
    {
        public static void Bind(GridView grid, DataTable rows, StateBag state,
            string key, string defaultColumn, string defaultDirection)
        {
            string column = state[key + "Column"] as string ?? defaultColumn;
            if (!rows.Columns.Contains(column)) column = defaultColumn;
            string direction = state[key + "Direction"] as string ?? defaultDirection;
            if (direction != "ASC" && direction != "DESC") direction = defaultDirection;

            //each table gets its own view, even when both display the same recipe records
            DataView view = new DataView(rows);
            view.Sort = "[" + column.Replace("]", "]]") + "] " + direction;
            if (grid.AllowPaging)
            {
                int lastPage = rows.Rows.Count == 0 ? 0 : (rows.Rows.Count - 1) / grid.PageSize;
                grid.PageIndex = Math.Max(0, Math.Min(grid.PageIndex, lastPage));
            }
            grid.DataSource = view;
            grid.DataBind();

            if (grid.HeaderRow == null) return;
            grid.HeaderRow.TableSection = TableRowSection.TableHeader;
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                if (String.IsNullOrEmpty(grid.Columns[i].SortExpression)) continue;
                bool selected = grid.Columns[i].SortExpression == column;
                grid.HeaderRow.Cells[i].Attributes["aria-sort"] = !selected ? "none"
                    : direction == "ASC" ? "ascending" : "descending";
                if (selected)
                    grid.HeaderRow.Cells[i].Controls.Add(new LiteralControl(
                        " <i class=\"bi bi-arrow-" + (direction == "ASC" ? "up" : "down")
                        + "-short\" aria-hidden=\"true\"></i>"));
            }
        }

        public static void ToggleSort(GridViewSortEventArgs e, StateBag state,
            string key, string defaultColumn, string defaultDirection, params string[] allowedColumns)
        {
            e.Cancel = true;
            if (Array.IndexOf(allowedColumns, e.SortExpression) < 0) return;
            string previousColumn = state[key + "Column"] as string ?? defaultColumn;
            string previousDirection = state[key + "Direction"] as string ?? defaultDirection;
            state[key + "Column"] = e.SortExpression;
            state[key + "Direction"] = previousColumn == e.SortExpression && previousDirection == "ASC"
                ? "DESC" : "ASC";
        }

        public static void ResetSort(StateBag state, string key)
        {
            state.Remove(key + "Column");
            state.Remove(key + "Direction");
        }

        public static string StatusBadge(object value)
        {
            string status = Convert.ToString(value).Trim().ToLowerInvariant();
            string colour = status == "approved" ? "approved" : status == "pending" ? "pending"
                : status == "rejected" ? "rejected" : "other";
            return "badge rounded-pill status-" + colour;
        }

        public static string DifficultyBadge(object value)
        {
            string difficulty = Convert.ToString(value).Trim().ToLowerInvariant();
            if (difficulty != "easy" && difficulty != "medium" && difficulty != "hard") difficulty = "other";
            return "badge rounded-pill difficulty-" + difficulty;
        }

        public static int DifficultyOrder(object value)
        {
            string difficulty = Convert.ToString(value).Trim().ToLowerInvariant();
            return difficulty == "easy" ? 1 : difficulty == "medium" ? 2 : difficulty == "hard" ? 3 : 4;
        }

        public static DateTime MalaysiaTime(object value)
        {
            DateTime utc = DateTime.SpecifyKind(Convert.ToDateTime(value), DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc,
                TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
        }

        public static string RecipeImage(Page page, object value)
        {
            string path = Convert.ToString(value).Trim();
            if (path.Length == 0) return "";
            try
            {
                path = Uri.UnescapeDataString(path);
                path = path.Replace("/Content/Uploads/Recipes/", "/Content/images/Uploads/Recipes/");
                if (path.StartsWith("/Content/images/", StringComparison.OrdinalIgnoreCase)) path = "~" + path;
                else if (path.StartsWith("Content/images/", StringComparison.OrdinalIgnoreCase)) path = "~/" + path;
                if (!path.StartsWith("~/Content/images/", StringComparison.OrdinalIgnoreCase)
                    || path.Contains("..") || path.Contains("\\")) return "";
                string extension = Path.GetExtension(path).ToLowerInvariant();
                if (extension != ".png" && extension != ".jpg" && extension != ".jpeg"
                    && extension != ".webp" && extension != ".gif" && extension != ".svg") return "";
                return File.Exists(page.Server.MapPath(path)) ? page.ResolveUrl(path) : "";
            }
            catch (ArgumentException) { return ""; }
            catch (HttpException) { return ""; }
            catch (UriFormatException) { return ""; }
        }
    }
}
