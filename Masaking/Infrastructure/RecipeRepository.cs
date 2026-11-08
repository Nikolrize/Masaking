using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Masaking.Infrastructure
{
    public class RecipeRepository
    {
        private const string RecipeBaseSql = @"
            SELECT
                r.recipeID,
                r.userID,
                r.title,
                r.description,
                r.cuisine,
                r.region,
                r.difficulty,
                r.preparationTime,
                r.estimatedCost,
                r.servings,
                r.image,
                c.categoryName,
                u.displayName,
                u.image AS authorImage,
                r.status,
                CAST(
                    CASE WHEN EXISTS (
                        SELECT 1
                        FROM dbo.favourites AS saved
                        WHERE saved.recipeID = r.recipeID
                          AND saved.userID = @currentUserID
                    )
                    THEN 1 ELSE 0 END
                AS bit) AS isFavourited,
                COALESCE(ratings.reviewCount, 0) AS reviewCount,
                ratings.averageRating
            FROM dbo.recipes AS r
            INNER JOIN dbo.users AS u
                ON u.userID = r.userID
            LEFT JOIN dbo.recipesCategories AS rc
                ON rc.recipeID = r.recipeID
            LEFT JOIN dbo.categories AS c
                ON c.categoryID = rc.categoryID
            LEFT JOIN (
                SELECT
                    recipeID,
                    COUNT(*) AS reviewCount,
                    AVG(CAST(rating AS decimal(10, 2))) AS averageRating
                FROM dbo.reviews
                WHERE status = 'Approved'
                GROUP BY recipeID
            ) AS ratings
                ON ratings.recipeID = r.recipeID
        ";

        private const string RecipeSelectSql = RecipeBaseSql + " WHERE r.status = 'Approved' ";

        public static DataTable GetPopularRecipes()
        {
            SqlConnection con = Database.Open();
            DataTable recipes = new DataTable();

            try
            {
                SqlCommand cmd = new SqlCommand(RecipeSelectSql + @"
                    ORDER BY
                        reviewCount DESC,
                        ratings.averageRating DESC,
                        r.createdAt DESC,
                        r.recipeID DESC
                    OFFSET 0 ROWS FETCH NEXT 4 ROWS ONLY;", con);

                cmd.Parameters.Add("@currentUserID", SqlDbType.Int).Value =
                    Database.UserId.HasValue
                        ? (object)Database.UserId.Value
                        : DBNull.Value;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(recipes);
            }
            finally
            {
                con.Close();
            }

            return recipes;
        }

        public static DataTable GetRecipeById(int recipeId)
        {
            SqlConnection con = Database.Open();
            DataTable recipes = new DataTable();

            try
            {
                SqlCommand cmd = new SqlCommand(RecipeBaseSql + @"
                    WHERE r.recipeID = @recipeID
                      AND (
                          r.status = 'Approved'
                          OR (
                              r.userID = @viewerID
                              AND r.status IN ('Pending', 'Rejected')
                          )
                      );", con);

                cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;
                cmd.Parameters.Add("@viewerID", SqlDbType.Int).Value =
                    Database.UserId.HasValue
                        ? (object)Database.UserId.Value
                        : DBNull.Value;
                cmd.Parameters.Add("@currentUserID", SqlDbType.Int).Value =
                    Database.UserId.HasValue
                        ? (object)Database.UserId.Value
                        : DBNull.Value;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(recipes);
            }
            finally
            {
                con.Close();
            }

            return recipes;
        }

        public static DataTable GetRecipesByFilters(
            string search,
            string difficulty,
            int maxCost,
            int maxTime,
            string cuisine,
            string region,
            int minServings,
            List<int> categoryIds,
            string sort,
            string collection = null)


        {
            string condition = "r.status = 'Approved'";

            if (collection != null)
            {
                if (!Database.UserId.HasValue)
                {
                    throw new InvalidOperationException("Sign in to view your recipes.");
                }

                switch (collection)
                {
                    case "Approved":
                    case "Pending":
                    case "Rejected":
                        condition = "r.userID = @userID AND r.status = @status";
                        break;

                    case "Favourites":
                        condition = @"
                            r.status = 'Approved'
                            AND r.userID <> @userID
                            AND EXISTS (
                                SELECT 1
                                FROM dbo.favourites AS f
                                WHERE f.recipeID = r.recipeID
                                  AND f.userID = @userID
                            )";
                        break;

                    case "All":
                        condition = @"
                            (
                                (
                                    r.userID = @userID
                                    AND r.status IN ('Approved', 'Pending', 'Rejected')
                                )
                                OR (
                                    r.userID <> @userID
                                    AND r.status = 'Approved'
                                    AND EXISTS (
                                        SELECT 1
                                        FROM dbo.favourites AS f
                                        WHERE f.recipeID = r.recipeID
                                          AND f.userID = @userID
                                    )
                                )
                            )";
                        break;

                    default:
                        throw new ArgumentException("Invalid recipe collection.");
                }
            }

            SqlConnection con = Database.Open();
            DataTable recipes = new DataTable();

            try
            {
                SqlCommand cmd = new SqlCommand(
                    RecipeBaseSql + " WHERE " + condition + @"
                    AND (@search = ''
                        OR r.title LIKE '%' + @search + '%')
                    AND (@difficulty = ''
                        OR r.difficulty = @difficulty)
                    AND (@maxCost = 0
                        OR r.estimatedCost <= @maxCost)
                    AND (@maxTime = 0
                        OR r.preparationTime <= @maxTime)
                    AND (@cuisine = ''
                        OR r.cuisine = @cuisine)
                    AND (@region = ''
                        OR r.region = @region)
                    AND (@minServings = 0
                        OR r.servings >= @minServings)", con);

                if (collection != null)
                {
                    cmd.Parameters.Add("@userID", SqlDbType.Int).Value = Database.UserId.Value;
                    cmd.Parameters.Add("@status", SqlDbType.NVarChar, 30).Value = collection;
                }

                cmd.Parameters.Add("@search", SqlDbType.NVarChar, -1).Value = search;
                cmd.Parameters.Add("@difficulty", SqlDbType.NVarChar, 30).Value = difficulty;
                cmd.Parameters.Add("@maxCost", SqlDbType.Decimal).Value = maxCost;
                cmd.Parameters.Add("@maxTime", SqlDbType.Int).Value = maxTime;
                cmd.Parameters.Add("@cuisine", SqlDbType.NVarChar, -1).Value = cuisine;
                cmd.Parameters.Add("@region", SqlDbType.NVarChar, -1).Value = region;
                cmd.Parameters.Add("@minServings", SqlDbType.Int).Value = minServings;
                cmd.Parameters.Add("@currentUserID", SqlDbType.Int).Value =
                    Database.UserId.HasValue
                        ? (object)Database.UserId.Value
                        : DBNull.Value;

                List<string> categoryParameters = new List<string>();

                foreach (int categoryId in categoryIds)
                {
                    string parameterName = "@category" + categoryParameters.Count;
                    categoryParameters.Add(parameterName);
                    cmd.Parameters.Add(parameterName, SqlDbType.Int).Value = categoryId;
                }

                if (categoryParameters.Count > 0)
                {
                    cmd.CommandText += " AND c.categoryID IN (" + string.Join(",", categoryParameters) + ")";
                }

                cmd.CommandText += " ORDER BY " + GetSortOrder(sort) + ", r.recipeID ASC;";

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(recipes);
            }
            finally
            {
                con.Close();
            }

            return recipes;
        }

        public static int DeleteOwnedRecipe(int recipeId)
        {
            if (!Database.UserId.HasValue)
            {
                throw new InvalidOperationException("Sign in before deleting a recipe.");
            }

            SqlConnection con = Database.Open();

            try
            {
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE dbo.recipes
                    SET status = 'Deleted',
                        updatedAt = SYSUTCDATETIME()
                    WHERE recipeID = @recipeID
                      AND userID = @userID
                      AND status IN ('Approved', 'Pending', 'Rejected');", con);

                cmd.Parameters.Add("@recipeID", SqlDbType.Int).Value = recipeId;
                cmd.Parameters.Add("@userID", SqlDbType.Int).Value = Database.UserId.Value;

                return cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }

        private static string GetSortOrder(string sort)
        {
            switch (sort)
            {
                case "nameDesc":
                    return "r.title DESC";

                case "newest":
                    return "r.createdAt DESC";

                case "rating":
                    return "ratings.averageRating DESC, reviewCount DESC";

                case "reviews":
                    return "reviewCount DESC";

                case "cost":
                    return "r.estimatedCost ASC";

                case "time":
                    return "r.preparationTime ASC";

                default:
                    return "r.title ASC";
            }
        }
    }
}