using System;
using System.Data;
using System.Data.SqlClient;

namespace Masaking.Infrastructure
{
    public static class SimulationStructure
    {
        public static DataTable ReadSteps(SqlConnection cn, SqlTransaction tx, int simulationId)
        {
            const string sql = @"SELECT st.stepNumber,st.ingredientID,st.instruction,st.actionType,
                st.simulationStageID,i.ingredientName,i.recipeID,
                COALESCE(NULLIF(st.image,''),i.image,'') AS ingredientImage,
                g.stageNumber,g.title AS stageTitle,g.instructions AS stageInstructions,g.cookwareImage AS stageImage,
                (SELECT COUNT(*) FROM dbo.simulationStages stages WHERE stages.simulationID=st.simulationID) AS stageCount
                FROM dbo.simulationSteps st LEFT JOIN dbo.ingredients i ON i.ingredientID=st.ingredientID
                LEFT JOIN dbo.simulationStages g ON g.simulationStageID=st.simulationStageID AND g.simulationID=st.simulationID
                WHERE st.simulationID=@id ORDER BY st.stepNumber;";
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = simulationId;
                DataTable steps = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(steps);
                return steps;
            }
        }

        public static bool IsValid(DataTable steps, int recipeId, out string message)
        {
            message = "";
            if (steps == null || steps.Rows.Count == 0 || steps.Rows.Count > 100)
            {
                message = "Add between 1 and 100 simulation steps.";
                return false;
            }
            int stageCount = Convert.ToInt32(steps.Rows[0]["stageCount"]);
            int previousStage = 0;
            int ingredientChoices = 0;
            for (int index = 0; index < steps.Rows.Count; index++)
            {
                DataRow step = steps.Rows[index];
                if (Convert.ToInt32(step["stepNumber"]) != index + 1
                    || String.IsNullOrWhiteSpace(Convert.ToString(step["instruction"]))
                    || Convert.ToString(step["instruction"]).Length > 1000)
                {
                    message = "Number steps consecutively from 1 and give each step an instruction within 1,000 characters.";
                    return false;
                }
                string type = Convert.ToString(step["actionType"]);
                if (type == "Ingredient") ingredientChoices++;
                if ((type != "Ingredient" && type != "Action")
                    || (type == "Ingredient" && (step["recipeID"] == DBNull.Value
                        || Convert.ToInt32(step["recipeID"]) != recipeId))
                    || (type == "Action" && step["ingredientID"] != DBNull.Value))
                {
                    message = "Ingredient steps must use the linked recipe. Cooking actions must not select an ingredient.";
                    return false;
                }
                if (stageCount > 0)
                {
                    if (step["stageNumber"] == DBNull.Value
                        || String.IsNullOrWhiteSpace(Convert.ToString(step["stageTitle"])))
                    {
                        message = "Assign every step to a stage in this simulation.";
                        return false;
                    }
                    int number = Convert.ToInt32(step["stageNumber"]);
                    if (number < 1 || number > 20 || number < previousStage || number > previousStage + 1)
                    {
                        message = "Stages must run in order from 1, without gaps or returning to an earlier stage.";
                        return false;
                    }
                    previousStage = number;
                }
                else if (step["simulationStageID"] != DBNull.Value)
                {
                    message = "A step refers to an unavailable stage.";
                    return false;
                }
            }
            if (ingredientChoices == 0)
            {
                message = "Add at least one ingredient-choice step to the simulation.";
                return false;
            }
            if (previousStage != stageCount)
            {
                message = "Give every stage at least one step and number the stages consecutively from 1.";
                return false;
            }
            return true;
        }
    }
}
