# Masaking account, recipe, and community features

These pages use the existing `MasakingDB.mdf` tables. No table or column changes are required. The application connection string uses `|DataDirectory|` to open `App_Data\MasakingDB.mdf`.

## First administrator

1. In `Web.config`, set `AdminBootstrapKey` to a long, random one-time secret.
2. Open `/Admin/BootstrapAdmin.aspx`. If the `users` table is empty, enter the admin account details, setup key, and a password of at least 8 characters to create the first administrator.
3. If member accounts already exist, sign in as the member to promote, then open `/Admin/BootstrapAdmin.aspx` and provide the setup key and current password.
4. Clear `AdminBootstrapKey` in `Web.config` after setup. Once an active administrator exists, first-admin setup is closed. Existing administrators can promote trusted members later at `/Admin/ManageUsers.aspx`.

Public registration always assigns the `Member` role.

## Password reset

Users enter their email and active matching accounts can proceed directly to choose a new password. This assignment flow does not verify that the person controls the mailbox; deploy a mailbox verification flow before using it for real accounts.

## Moderation

New recipes and edits use `Pending` status. Administrator approval changes them to `Approved`; rejected submissions use `Rejected`. Reviews, community posts, and comments follow the same approval flow through `/Admin/ManageCommunity.aspx`.

