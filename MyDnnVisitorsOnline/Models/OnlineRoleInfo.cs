// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace MyDnn.VisitorsOnline.Models
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [TableName("MyDnnVisitorsOnline_RolesOnline")]
    [PrimaryKey("ItemID", AutoIncrement = true)]
    [Cacheable("MyDnnRolesOnline", CacheItemPriority.Default, 20)]
    [Scope("PortalID")]
    public class OnlineRoleInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int ItemID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PortalID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime OnlineDate { get; set; }
    }
}