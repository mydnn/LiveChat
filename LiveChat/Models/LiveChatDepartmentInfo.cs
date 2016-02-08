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
    [TableName("MyDnnSupport_LiveChatDepartments")]
    [PrimaryKey("LiveChatDepartmentID", AutoIncrement = true)]
    [Cacheable("LiveChatDepartments_", CacheItemPriority.Normal, 20)]
    [Scope("LiveChatID")]
    public class LiveChatDepartmentInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int LiveChatDepartmentID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LiveChatID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int DepartmentID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}