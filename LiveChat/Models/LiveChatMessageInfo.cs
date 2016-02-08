// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.ComponentModel.DataAnnotations;
using MyDnn.Modules.Support.LiveChat.Components.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace MyDnn.Modules.Support.LiveChat.Models
{
    [TableName("MyDnnSupport_LiveChatMessages")]
    [PrimaryKey("MessageID", AutoIncrement = true)]
    [Cacheable("LiveChatMessages_", CacheItemPriority.Normal, 20)]
    [Scope("LiveChatID")]
    public class LiveChatMessageInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int MessageID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LiveChatID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MessageSentBy SentBy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int AgentUserID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Seen { get; set; }
    }
}