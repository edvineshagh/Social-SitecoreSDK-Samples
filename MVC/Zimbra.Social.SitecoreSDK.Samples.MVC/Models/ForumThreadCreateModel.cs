﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zimbra.Social.SitecoreSDK.Samples.MVC.Models
{
    public class ForumThreadCreateModel
    {
        public int ForumId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}