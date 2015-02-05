﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telligent.Evolution.Extensibility.Rest.Version1;

public partial class Forums_ForumThreadView : System.Web.UI.UserControl
{
    private RestHost host;

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        rptReplies.ItemDataBound += rptReplies_ItemDataBound;
    }

    void rptReplies_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            return;

        dynamic reply = (dynamic)e.Item.DataItem;
        var lblAuthorName = e.Item.FindControl("lblAuthorName") as Label;
        var imgAuthorAvatar = e.Item.FindControl("imgAuthorAvatar") as Image;
        var lblPostDate = e.Item.FindControl("lblPostDate") as Label;
        var litBody = e.Item.FindControl("litBody") as Literal;
        lblAuthorName.Text = reply.Author.DisplayName;
        imgAuthorAvatar.ImageUrl = reply.Author.AvatarUrl;
        litBody.Text = string.IsNullOrEmpty(reply.Body) ? "" : reply.Body;
        if (reply.Date != null)
            lblPostDate.Text = DateTime.Parse(reply.Date.ToString()).ToString();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        litError.Visible = false;
        try
        {
            //If you loaded multiple hosts, like say for each website, replace the string with a retrieval method of your choice.
            //Example, if you loaded by site name you could get the current site name.
            host = Host.Get("default");

            int threadId;
            if (Request["t"] == null || !Int32.TryParse(Request["t"], out threadId))
            {
                throw new ArgumentException("Thread Id was missing or invalid");
            }

            var endpointThreadShow = string.Format("forums/threads/{0}.json", threadId);
            dynamic responseThreadShow = host.GetToDynamic(2, endpointThreadShow);
            litThreadSubject.Text = responseThreadShow.Thread.Subject;
            lblThreadAuthorName.Text = responseThreadShow.Thread.Author.DisplayName;
            imgThreadAuthorAvatar.ImageUrl = responseThreadShow.Thread.Author.AvatarUrl;
            lblThreadPostDate.Text = responseThreadShow.Thread.LatestPostDate;
            litThreadBody.Text = responseThreadShow.Thread.Body;

            var options = new NameValueCollection();
            options.Add("PageSize", "50");
            options.Add("PageIndex", "0");
            options.Add("SortBy", "PostDate");
            options.Add("SortOrder", "Descending");

            var endpointForumShow = string.Format("forums/{0}.json", responseThreadShow.Thread.ForumId);
            dynamic responseForumShow = host.GetToDynamic(2, endpointForumShow);

            lnkForum.Text = responseForumShow.Forum.Name;
            lnkForum.NavigateUrl = string.Format("/community/forum?f={0}", responseForumShow.Forum.Id);

            var endpoint = string.Format("forums/{0}/threads/{1}/replies.json?{2}", responseForumShow.Forum.Id, threadId,
                String.Join("&", options.AllKeys.Select(a => a + "=" + HttpUtility.UrlEncode(options[a]))));
            dynamic response = host.GetToDynamic(2, endpoint);
            if (response.Errors.Count > 0)
                throw new Exception(response.Errors[0].Message.ToString());
            rptReplies.DataSource = response.Replies;
            rptReplies.DataBind();
        }
        catch (Exception ex)
        {
            litError.Text = ex.Message + "<br/><pre>" + ex.StackTrace + "</pre>";
            litError.Visible = true;
        }
    }
}