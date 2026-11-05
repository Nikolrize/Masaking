using System.Web.UI.HtmlControls;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using Masaking.Infrastructure;

namespace Masaking.Community
{
    public partial class Community : CommunityPage
    {
        protected Repeater rptPosts, rptDeletedPosts;
        protected Panel pnlEmpty, pnlDeletedPosts, pnlAddPost, pnlGuest;
        protected DropDownList ddlSort;
        protected void Page_Load(object sender, EventArgs e)
        {
            pnlAddPost.Visible = currentUserIsActive;
            pnlGuest.Visible = !currentUserIsActive;
            if (!IsPostBack)
                RunDatabaseAction(() =>
                {
                    BindPosts();
                    BindMyDeletedPosts();
                });
        }

        private void BindPosts()
        {
            DataTable data = ReadPosts(null, ddlSort.SelectedValue);
            rptPosts.DataSource = data;
            rptPosts.DataBind();
            pnlEmpty.Visible = data.Rows.Count == 0;
        }

        protected new void rptPosts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "toggleComments")
            {
                base.rptPosts_ItemCommand(source, e);
                return;
            }

            RunCommentAction(e.Item, () =>
            {
                var panel = (Panel)e.Item.FindControl("pnlComments");
                if (panel.Visible)
                    SetCommentsExpanded(e.Item, false);
                else
                    BindComments(e.Item);
            });
        }

        private int CommentPostId(RepeaterItem item)
        {
            var card = (HtmlGenericControl)item.FindControl("postCard");
            int id;
            return Int32.TryParse(card.Attributes["data-post-id"], out id) ? id : 0;
        }

        private RepeaterItem CommentPostItem(Control control)
        {
            while (control != null)
            {
                var item = control as RepeaterItem;
                if (item != null && item.Parent == rptPosts)
                    return item;
                control = control.NamingContainer;
            }

            return null;
        }

        private void SetCommentsExpanded(RepeaterItem item, bool expanded)
        {
            var panel = (Panel)item.FindControl("pnlComments");
            panel.Visible = expanded;
            var button = (LinkButton)item.FindControl("lnkPost");
            button.Attributes["aria-expanded"] = expanded ? "true" : "false";
            button.Attributes["aria-controls"] = panel.ClientID;
        }

        private void CommentMessage(RepeaterItem item, string message, bool success)
        {
            var label = (Label)item.FindControl("lblCommentMessage");
            label.Text = Server.HtmlEncode(message);
            label.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
        }

        private void RunCommentAction(RepeaterItem item, Action action)
        {
            if (item == null)
                return;
            try
            {
                if (!databaseAvailable)
                {
                    SetCommentsExpanded(item, true);
                    CommentMessage(item, "Comments are temporarily unavailable. Please try again later.", false);
                    return;
                }

                action();
            }
            catch (SqlException ex)
            {
                Trace.Warn("Community", "Unable to load or save comments.", ex);
                SetCommentsExpanded(item, true);
                CommentMessage(item, "Comments are temporarily unavailable. Please try again later.", false);
            }
        }

        private void BindComments(RepeaterItem item)
        {
            int postId = CommentPostId(item);
            var sort = (DropDownList)item.FindControl("ddlCommentSort");
            string order = sort.SelectedValue == "oldest" ? "ASC" : "DESC";
            DataTable comments = new DataTable();
            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand(@"SELECT c.postCommentID,c.content,c.createdAt,c.updatedAt,
                c.userID,u.displayName,u.username,u.image AS profilePicture
                FROM postComments c JOIN users u ON u.userID=c.userID
                JOIN posts p ON p.postID=c.postID JOIN users author ON author.userID=p.userID
                WHERE c.postID=@post AND c.status='Approved' AND u.isActive=1
                    AND p.status='Approved' AND author.isActive=1
                ORDER BY c.createdAt " + order + ",c.postCommentID " + order, cn))
            {
                cmd.Parameters.Add("@post", SqlDbType.Int).Value = postId;
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    adapter.Fill(comments);
            }

            var repeater = (Repeater)item.FindControl("rptComments");
            repeater.DataSource = comments;
            repeater.DataBind();
            ((LinkButton)item.FindControl("lnkPost")).Text = comments.Rows.Count.ToString();
            ((Label)item.FindControl("lblThreadCommentCount")).Text = comments.Rows.Count.ToString();
            ((Panel)item.FindControl("pnlNoComments")).Visible = comments.Rows.Count == 0;
            ((Panel)item.FindControl("pnlCommentForm")).Visible = currentUserIsActive;
            ((Label)item.FindControl("lblCurrentUserAvatar")).Text = CommunityAvatar(CurrentUserAvatarUrl, CurrentDisplayName, "Your profile");
            var login = (HyperLink)item.FindControl("lnkCommentLogin");
            login.Visible = !currentUserIsActive;
            login.NavigateUrl = "../Account/Login.aspx?returnUrl=" + Server.UrlEncode(ResolveUrl("~/Community/Community.aspx") + "?post=" + postId);
            SetCommentsExpanded(item, true);
        }

        protected void btnComment_Click(object sender, EventArgs e)
        {
            var item = CommentPostItem((Control)sender);
            RunCommentAction(item, () =>
            {
                if (!RequireCurrentUser())
                    return;
                var input = (TextBox)item.FindControl("txtComment");
                string content = input.Text.Trim();
                if (content.Length == 0 || content.Length > 1000)
                {
                    CommentMessage(item, "Write a comment of 1–1,000 characters before posting.", false);
                    return;
                }

                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO postComments(content,status,createdAt,updatedAt,postID,userID)
                    SELECT @content,'Approved',SYSUTCDATETIME(),NULL,p.postID,@user
                    FROM posts p JOIN users u ON u.userID=p.userID
                    WHERE p.postID=@post AND p.status='Approved' AND u.isActive=1", cn))
                {
                    cmd.Parameters.Add("@content", SqlDbType.NVarChar, 1000).Value = content;
                    cmd.Parameters.Add("@post", SqlDbType.Int).Value = CommentPostId(item);
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        CommentMessage(item, "This post is no longer available for comments.", false);
                        return;
                    }
                }

                input.Text = String.Empty;
                BindComments(item);
                CommentMessage(item, "Your comment has been posted.", true);
            });
        }

        protected void btnUpdateComment_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var item = CommentPostItem(button);
            RunCommentAction(item, () =>
            {
                if (!RequireCurrentUser())
                    return;
                int commentId;
                if (!Int32.TryParse(button.CommandArgument, out commentId))
                    return;
                var editor = (TextBox)button.NamingContainer.FindControl("txtEditComment");
                string content = editor.Text.Trim();
                if (content.Length == 0 || content.Length > 1000)
                {
                    CommentMessage(item, "A comment must contain 1–1,000 characters.", false);
                    return;
                }

                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand(@"UPDATE postComments SET content=@content,updatedAt=SYSUTCDATETIME()
                    WHERE postCommentID=@comment AND postID=@post AND userID=@user AND status='Approved'
                    AND createdAt>=DATEADD(day,-7,SYSUTCDATETIME()) AND createdAt<=SYSUTCDATETIME()
                    AND EXISTS (SELECT 1 FROM posts p JOIN users u ON u.userID=p.userID
                        WHERE p.postID=postComments.postID AND p.status='Approved' AND u.isActive=1)", cn))
                {
                    cmd.Parameters.Add("@content", SqlDbType.NVarChar, 1000).Value = content;
                    cmd.Parameters.Add("@comment", SqlDbType.Int).Value = commentId;
                    cmd.Parameters.Add("@post", SqlDbType.Int).Value = CommentPostId(item);
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    bool updated = cmd.ExecuteNonQuery() == 1;
                    BindComments(item);
                    CommentMessage(item, updated ? "Your comment has been updated." : "You can only edit your own comments within seven days of creation.", updated);
                }
            });
        }

        protected void btnDeleteComment_Click(object sender, EventArgs e)
        {
            var button = (LinkButton)sender;
            var item = CommentPostItem(button);
            RunCommentAction(item, () =>
            {
                if (!RequireCurrentUser())
                    return;
                int commentId;
                if (!Int32.TryParse(button.CommandArgument, out commentId))
                    return;
                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand("UPDATE postComments SET status='Deleted' WHERE postCommentID=@comment AND postID=@post AND userID=@user AND status='Approved'", cn))
                {
                    cmd.Parameters.Add("@comment", SqlDbType.Int).Value = commentId;
                    cmd.Parameters.Add("@post", SqlDbType.Int).Value = CommentPostId(item);
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    bool deleted = cmd.ExecuteNonQuery() == 1;
                    BindComments(item);
                    CommentMessage(item, deleted ? "Your comment has been deleted." : "You may only delete your own comments.", deleted);
                }
            });
        }

        protected override void AfterPostDeleted()
        {
            Show("The post has been removed from the community feed.", true);
            BindPosts();
            BindMyDeletedPosts();
        }

        private void BindMyDeletedPosts()
        {
            if (!Database.UserId.HasValue)
            {
                rptDeletedPosts.DataSource = null;
                rptDeletedPosts.DataBind();
                pnlDeletedPosts.Visible = false;
                return;
            }

            using (SqlConnection cn = Database.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT postID, content, createdAt, updatedAt " + "FROM posts WHERE userID=@user AND status='Deleted' " + "ORDER BY createdAt DESC", cn))
            {
                cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                DataTable data = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(data);
                }

                rptDeletedPosts.DataSource = data;
                rptDeletedPosts.DataBind();
                pnlDeletedPosts.Visible = data.Rows.Count > 0;
            }
        }

        protected void ddlSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            RunDatabaseAction(() =>
            {
                BindPosts();
            });
        }

        protected void rptDeletedPosts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            RunDatabaseAction(() =>
            {
                if (e.CommandName != "restorePost")
                    return;
                if (!RequireCurrentUser())
                    return;
                if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out int postId) || postId <= 0)
                    return;
                using (SqlConnection cn = Database.Open())
                using (SqlCommand cmd = new SqlCommand("UPDATE posts SET status='Approved' WHERE postID=@post AND userID=@user AND status='Deleted' " + "AND (recipeID IS NULL OR EXISTS (SELECT 1 FROM recipes WHERE recipes.recipeID=posts.recipeID AND recipes.status='Approved' AND recipes.userID=@user))", cn))
                {
                    cmd.Parameters.Add("@post", SqlDbType.Int).Value = postId;
                    cmd.Parameters.Add("@user", SqlDbType.Int).Value = Database.UserId.Value;
                    if (cmd.ExecuteNonQuery() != 1)
                        Show("This post could not be restored. Its attached recipe must be one of your approved recipes.", false);
                    else
                        Show("Your post has been restored to the community feed.", true);
                }

                BindPosts();
                BindMyDeletedPosts();
            });
        }

        protected override void OnPreRender(EventArgs e)
        {
            phMemberSection.Visible = !currentUserIsAdministrator;
            base.OnPreRender(e);
        }

        protected void rptPosts_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            var row = (System.Data.DataRowView)e.Item.DataItem;
            var postCard = (HtmlGenericControl)e.Item.FindControl("postCard");
            postCard.Attributes["data-post-id"] = Convert.ToString(row["postID"]);
            postCard.Attributes["data-has-recipe"] = Convert.ToString(row["recipeID"] != DBNull.Value ? "true" : "false");
            var lblAuthorAvatar = (Label)e.Item.FindControl("lblAuthorAvatar");
            lblAuthorAvatar.Text = Convert.ToString(CommunityAvatar(row["authorImage"], row["displayName"], "Post author"));
            var lblDisplayName = (Label)e.Item.FindControl("lblDisplayName");
            lblDisplayName.Text = Server.HtmlEncode(Convert.ToString(row["displayName"]));
            var lblUsername = (Label)e.Item.FindControl("lblUsername");
            lblUsername.Text = Server.HtmlEncode(Convert.ToString(row["username"]));
            var lblContentDate = (Label)e.Item.FindControl("lblContentDate");
            lblContentDate.Text = Server.HtmlEncode(Convert.ToString(ContentDate(row["updatedAt"], row["createdAt"])));
            var phOwnerActions = (PlaceHolder)e.Item.FindControl("phOwnerActions");
            phOwnerActions.Visible = CanDeletePost(row["userID"]);
            var lnkEditPost = (HyperLink)e.Item.FindControl("lnkEditPost");
            lnkEditPost.Visible = IsPostOwner(row["userID"]) && CanEdit(Convert.ToDateTime(row["createdAt"]));
            lnkEditPost.NavigateUrl = Convert.ToString("EditPost.aspx?id=" + row["postID"]);
            var pnlDeletePost = (Panel)e.Item.FindControl("pnlDeletePost");
            pnlDeletePost.Visible = CanDeletePost(row["userID"]);
            var btnDeletePost = (Button)e.Item.FindControl("btnDeletePost");
            btnDeletePost.Text = Convert.ToString(IsPostOwner(row["userID"]) ? "Delete Post" : "Remove Post");
            btnDeletePost.CommandArgument = Convert.ToString(row["postID"]);
            var phMemberSection2 = (PlaceHolder)e.Item.FindControl("phMemberSection2");
            phMemberSection2.Visible = !currentUserIsAdministrator;
            var lblContent = (Label)e.Item.FindControl("lblContent");
            lblContent.Text = Server.HtmlEncode(Convert.ToString(row["content"]));
            var imgPost = (Image)e.Item.FindControl("imgPost");
            imgPost.Visible = !String.IsNullOrWhiteSpace(CommunityImageUrl(row["image"]));
            imgPost.ImageUrl = Convert.ToString(CommunityImageUrl(row["image"]));
            var lnkRecipe = (HyperLink)e.Item.FindControl("lnkRecipe");
            lnkRecipe.Visible = row["recipeID"] != DBNull.Value;
            lnkRecipe.NavigateUrl = Convert.ToString("../Recipes/RecipeDetails.aspx?id=" + row["recipeID"]);
            var imgAttachedRecipe = (Image)e.Item.FindControl("imgAttachedRecipe");
            imgAttachedRecipe.Visible = !String.IsNullOrEmpty(RecipeImageUrl(row["recipeImage"]));
            imgAttachedRecipe.ImageUrl = Convert.ToString(RecipeImageUrl(row["recipeImage"]));
            var lblTitle = (Label)e.Item.FindControl("lblTitle");
            lblTitle.Text = Server.HtmlEncode(Convert.ToString(row["title"]));
            var lblRecipeSummary = (Label)e.Item.FindControl("lblRecipeSummary");
            lblRecipeSummary.Text = Server.HtmlEncode(Convert.ToString(row["recipeSummary"]));
            var btnLike = (LinkButton)e.Item.FindControl("btnLike");
            btnLike.Attributes["data-like-panel"] = Convert.ToString(e.Item.UniqueID + "$upLike");
            btnLike.Attributes["data-like-target"] = Convert.ToString(e.Item.UniqueID + "$btnLike");
            btnLike.CommandName = Convert.ToString(Convert.ToInt32(row["userLiked"]) == 1 ? "unlike" : "like");
            btnLike.CommandArgument = Convert.ToString(row["postID"]);
            btnLike.ToolTip = Convert.ToString(Convert.ToInt32(row["userLiked"]) == 1 ? "Unlike post" : "Like post");
            btnLike.Attributes["aria-label"] = Convert.ToString(Convert.ToInt32(row["userLiked"]) == 1 ? "Unlike post" : "Like post");
            btnLike.Attributes["aria-pressed"] = Convert.ToString(Convert.ToInt32(row["userLiked"]) == 1 ? "true" : "false");
            btnLike.Text = Convert.ToString(GetLikeButtonText(row["likeCount"]));
            var lnkPost = (LinkButton)e.Item.FindControl("lnkPost");
            lnkPost.Text = Convert.ToString(row["commentCount"]);
            int postId = Convert.ToInt32(row["postID"]);
            lnkPost.CommandArgument = postId.ToString();
            SetCommentsExpanded(e.Item, false);
            int requestedPost;
            if (!IsPostBack && Int32.TryParse(Request.QueryString["post"], out requestedPost) && requestedPost == postId)
                BindComments(e.Item);
        }

        protected void rptDeletedPosts_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            var row = (System.Data.DataRowView)e.Item.DataItem;
            var lblContent2 = (Label)e.Item.FindControl("lblContent2");
            lblContent2.Text = Server.HtmlEncode(Convert.ToString(row["content"]));
            var lblContentDate2 = (Label)e.Item.FindControl("lblContentDate2");
            lblContentDate2.Text = Server.HtmlEncode(Convert.ToString(ContentDate(row["updatedAt"], row["createdAt"])));
            var btnRestorePost = (Button)e.Item.FindControl("btnRestorePost");
            btnRestorePost.CommandArgument = Convert.ToString(row["postID"]);
        }

        protected void rptComments_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            var row = (System.Data.DataRowView)e.Item.DataItem;
            var commentRow = (HtmlGenericControl)e.Item.FindControl("commentRow");
            commentRow.Attributes["data-comment-created"] = Convert.ToDateTime(row["createdAt"])
                .ToString("yyyy-MM-ddTHH:mm:ss.fffffff", System.Globalization.CultureInfo.InvariantCulture);
            commentRow.Attributes["data-comment-id"] = Convert.ToString(row["postCommentID"], System.Globalization.CultureInfo.InvariantCulture);
            var pnlEditComment = (Panel)e.Item.FindControl("pnlEditComment");
            var lblAuthorAvatar2 = (Label)e.Item.FindControl("lblAuthorAvatar2");
            lblAuthorAvatar2.Text = Convert.ToString(CommunityAvatar(row["profilePicture"], row["displayName"], "Comment author"));
            var lblDisplayName2 = (Label)e.Item.FindControl("lblDisplayName2");
            lblDisplayName2.Text = Server.HtmlEncode(Convert.ToString(row["displayName"]));
            var lblUsername2 = (Label)e.Item.FindControl("lblUsername2");
            lblUsername2.Text = Server.HtmlEncode(Convert.ToString(row["username"]));
            var lblContentDate2 = (Label)e.Item.FindControl("lblContentDate2");
            lblContentDate2.Text = Server.HtmlEncode(Convert.ToString(ContentDate(row["updatedAt"], row["createdAt"])));
            var phOwnerActions3 = (PlaceHolder)e.Item.FindControl("phOwnerActions3");
            phOwnerActions3.Visible = IsPostOwner(row["userID"]);
            var phOwnerActions4 = (PlaceHolder)e.Item.FindControl("phOwnerActions4");
            phOwnerActions4.Visible = CanEdit(Convert.ToDateTime(row["createdAt"]));
            var btnToggleSection = (HtmlButton)e.Item.FindControl("btnToggleSection");
            btnToggleSection.Attributes["data-bs-target"] = "#" + pnlEditComment.ClientID;
            var btnDeleteComment = (LinkButton)e.Item.FindControl("btnDeleteComment");
            btnDeleteComment.CommandArgument = Convert.ToString(row["postCommentID"]);
            var lblContent3 = (Label)e.Item.FindControl("lblContent3");
            lblContent3.Text = Server.HtmlEncode(Convert.ToString(row["content"]));
            var txtEditComment = (TextBox)e.Item.FindControl("txtEditComment");
            txtEditComment.Text = Convert.ToString(row["content"]);
            var btnUpdateComment = (Button)e.Item.FindControl("btnUpdateComment");
            btnUpdateComment.CommandArgument = Convert.ToString(row["postCommentID"]);
            var btnToggleSection2 = (HtmlButton)e.Item.FindControl("btnToggleSection2");
            btnToggleSection2.Attributes["data-bs-target"] = "#" + pnlEditComment.ClientID;
        }
    }
}
