// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Portals;

namespace MyDnn.VisitorsOnline.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceController : DnnApiController
    {
        #region WebApi Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage DetectVisitorsOnline()
        {
            var visitorsOnlineEnabled = bool.Parse(PortalController.GetPortalSetting("MyDnnEnableVisitorsOnline", PortalSettings.PortalId, "false"));

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                RootUrl = DotNetNuke.Common.Globals.ResolveUrl("~/"),
                VisitorsOnlineEnabled = visitorsOnlineEnabled,
                PortalID = PortalSettings.PortalId,
            });
        }

        #endregion
    }
}