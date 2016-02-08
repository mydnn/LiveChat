// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.ComponentModel.DataAnnotations;
using MyDnn.VisitorsOnline.Components.Enums;
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
    [TableName("MyDnnVisitorsOnline_Listeners")]
    [PrimaryKey("ListenerID", AutoIncrement = true)]
    [Cacheable("MyDnnListener", CacheItemPriority.Default, 20)]
    [Scope("PortalID")]
    public class ListenerInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int ListenerID { get; set; }

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
        public LoginState LoginState { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string InvokeScript { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CreatedByModuleName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedOnDate { get; set; }
    }
}