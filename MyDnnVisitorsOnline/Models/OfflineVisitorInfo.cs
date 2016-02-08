// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace MyDnn.VisitorsOnline.Models
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class OfflineVisitorInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string VisitorGUID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
    }
}