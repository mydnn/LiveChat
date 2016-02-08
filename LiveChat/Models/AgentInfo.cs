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
    /// <summary>
    /// 
    /// </summary>
    [TableName("MyDnnSupport_Agents")]
    [PrimaryKey("AgentID", AutoIncrement = true)]
    [Cacheable("Agents_", CacheItemPriority.Default, 20)]
    [Scope("PortalID")]
    public class AgentInfo
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
        public DateTime CreateDate { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int CreateByUser { get; set; }
    }
}