# Masaking account, recipe, and community features

These pages use the existing `MasakingDB.mdf` tables. No table or column changes are required. The application connection string uses `|DataDirectory|` to open `App_Data\MasakingDB.mdf`.

## Default administrator

Application startup automatically creates the default administrator:

- Email: `admin@gmail.com`
- Password: `admin@1234`
- Username: `admin_default`
- Display name: `Admin`
- Country: `Malaysia`

Sign in through `/Account/Login.aspx`. Admins are directed to Manage Recipes. No setup key or admin registration is required. The password is stored as a salted PBKDF2 hash. Existing matching admin accounts are preserved, including changed passwords and inactive status. An email or username belonging to another account causes a startup error rather than silently promoting or overwriting that account.

These fixed credentials are for the assignment/demo; change the password before public deployment. Each teammate gets the account when starting the app against their own database.

Public registration always assigns the `Member` role. Existing admins can promote members at `/Admin/ManageUsers.aspx`.

## Password reset

Users enter their email and active matching accounts can proceed directly to choose a new password. This assignment flow does not verify that the person controls the mailbox; deploy a mailbox verification flow before using it for real accounts.

## Moderation

New recipes and edits use `Pending` status. Administrator approval changes them to `Approved`; rejected submissions use `Rejected`. Reviews, community posts, and comments follow the same approval flow through `/Admin/ManageCommunity.aspx`.

