// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace MyDnn.Modules.Support.LiveChat.Models
{
    /// <summary>
    /// 
    /// </summary>
    [TableName("vw_MyDnnSupport_Agents")]
    [PrimaryKey("AgentID", AutoIncrement = false)]
    [Scope("PortalID")]
    public class AgentView
    {
        /// <summary>
        /// 
        /// </summary>
        public int AgentID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PortalID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int CreateByUser { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }
    }
}