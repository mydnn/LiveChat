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
    [TableName("MyDnnSupport_DepartmentAgents")]
    [PrimaryKey("DepartmentAgentID", AutoIncrement = true)]
    [Cacheable("DepartmentAgents_", CacheItemPriority.Default, 20)]
    [Scope("DepartmentID")]
    public class DepartmentAgentInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int DepartmentAgentID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int DepartmentID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int AgentID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UserID { get; set; }
    }
}