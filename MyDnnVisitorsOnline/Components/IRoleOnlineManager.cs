// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using DotNetNuke.Collections;
using System.Collections.Generic;
using MyDnn.VisitorsOnline.Models;

namespace MyDnn.VisitorsOnline.Components
{
    /// <summary>
    /// 
    /// </summary>
    interface IRoleOnlineManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objOnlineRoleInfo"></param>
        void AddRoleOnline(OnlineRoleInfo objOnlineRoleInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objOnlineRoleInfo"></param>
        void DeleteRoleOnline(OnlineRoleInfo objOnlineRoleInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        OnlineRoleInfo GetRoleOnline(int portalID, string roleName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        bool IsRoleOnline(int portalID, string roleName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<OnlineRoleInfo> GetRolesOnline(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        IEnumerable<string> GetSuperUserRoles(int portalID, int userID);
    }
}


