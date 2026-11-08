# Recipe-owned learning activities

Run `003_RecipeActivities.sql` against **MasakingDB** before starting this version on another computer. It is safe to run again. The local development database has already been updated.

The change adds nullable `challenges.recipeID` with a foreign key to `recipes.recipeID`. Existing general challenges are preserved without assigning them to an arbitrary recipe. Update the ERD to show one recipe linked to zero or more challenges; an existing general challenge may have no recipe.

## How to use

- Members open **My recipes**, then select **Manage simulations** or **Manage challenges** on an approved recipe.
- Members can select and manage only recipes they own. An administrator can select any approved recipe.
- Save a draft, add simulation steps or challenge questions, then publish it.
- Published activities must be unpublished before editing. Existing restrictions on content with saved attempts still apply.
- An administrator may link a legacy general challenge to a recipe when editing an eligible draft. Existing attempts are not reassigned.
- Recipes with linked activities cannot be permanently deleted through recipe management. Unpublishing an activity preserves its history.

## Implementation

The four member ASPX views in `Learning` inherit the same compiled editor classes used by their corresponding admin pages. This reuses validation, uploads and publication rules without copying the C# business logic. `ActivityEditorPage` selects the website master for member views and the admin master for admin views. The admin endpoints still require an administrator.

Dropdown and list queries filter by recipe owner. Record actions check ownership separately; write operations repeat this check inside their SQL transaction. `UPDLOCK` and `HOLDLOCK` keep the activity and recipe ownership stable while a change is saved. The user ID comes from the validated session, not the query string. A `recipeId` URL value only preselects an allowed dropdown item.

Member editor styling is scoped to `.activity-editor` in its own stylesheet. Shared CSS and Bootstrap are not modified.

## Verification

The project build and ASP.NET markup precompilation passed. Local HTTP/database checks covered member draft creation, question and step creation, publishing/unpublishing, own-recipe filtering, denied access to another owner's records, denied access to admin endpoints, a stale form after ownership changed, saved-attempt restrictions, administrator access, recipe-card links and mobile HTML rendering. Temporary test accounts and records were removed after testing.

Before submission, manually check the editors in a narrow browser viewport and try a real optional image upload. The automated HTTP checks do not verify visual appearance or image selection in a browser.
