// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace MyDnn.Modules.Support.LiveChat.Models
{
    [TableName("MyDnnSupport_LiveChatAgents")]
    [PrimaryKey("LiveChatAgentID", AutoIncrement = true)]
    [Cacheable("LiveChatAgents_", CacheItemPriority.Normal, 20)]
    [Scope("LiveChatID")]
    public class LiveChatAgentInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int LiveChatAgentID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LiveChatID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int AgentID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime JoinDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool hasLeft { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime LeftDate { get; set; }
    }
}