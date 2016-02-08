// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;
using MyDnn.Modules.Support.LiveChat.Components.Enums;

namespace MyDnn.Modules.Support.LiveChat.Models
{
    [TableName("MyDnnSupport_LiveChats")]
    [PrimaryKey("LiveChatID", AutoIncrement = true)]
    [Cacheable("LiveChats_", CacheItemPriority.Normal, 20)]
    [Scope("PortalID")]
    public class LiveChatInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int LiveChatID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PortalID { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string VisitorGUID { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string VisitorName { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string VisitorEmail { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string VisitorMessage { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int VisitorUserID { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string VisitorIP { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string VisitorUserAgent { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateDate { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool IsClosed { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public DateTime CloseDate { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int ClosedBy { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public LiveChatRating Rate { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string VisitorComment { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public DateTime LastMessageDate { get; set; }
    }
}