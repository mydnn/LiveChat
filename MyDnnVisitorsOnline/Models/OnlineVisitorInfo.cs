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
    [TableName("MyDnnVisitorsOnline")]
    [PrimaryKey("VisitorID", AutoIncrement = true)]
    [Scope("PortalID")]
    public class OnlineVisitorInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int VisitorID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VisitorGUID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PortalID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime OnlineDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LastURL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ReferrerURL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalConnections { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Connections { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Ping { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PingDate { get; set; }
    }
}