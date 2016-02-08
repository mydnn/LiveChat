// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using DotNetNuke.Framework;
using DotNetNuke.Common;
using DotNetNuke.Data;
using MyDnn.VisitorsOnline.Models;

namespace MyDnn.VisitorsOnline.Components
{
    /// <summary>
    /// 
    /// </summary>
    internal class RoleOnlineManager : ServiceLocator<IRoleOnlineManager, RoleOnlineManager>, IRoleOnlineManager
    {
        protected override Func<IRoleOnlineManager> GetFactory()
        {
            return () => new RoleOnlineManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private const string CachePrefix = "MyDnnRoleOnline_";

        /// <summary>
        /// 
        /// </summary>
        public string DatabaseOwner
        {
            get
            {
                return DataProvider.Instance().DatabaseOwner;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ObjectQualifier
        {
            get
            {
                return DataProvider.Instance().ObjectQualifier;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objOnlineRoleInfo"></param>
        public void AddRoleOnline(OnlineRoleInfo objOnlineRoleInfo)
        {
            Requires.NotNull(objOnlineRoleInfo);
            Requires.PropertyNotNegative(objOnlineRoleInfo, "PortalID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OnlineRoleInfo>();
                rep.Insert(objOnlineRoleInfo);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objOnlineRoleInfo"></param>
        public void DeleteRoleOnline(OnlineRoleInfo objOnlineRoleInfo)
        {
            Requires.NotNull(objOnlineRoleInfo);
            Requires.PropertyNotNegative(objOnlineRoleInfo, "PortalID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OnlineRoleInfo>();
                rep.Delete(objOnlineRoleInfo);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public OnlineRoleInfo GetRoleOnline(int portalID, string roleName)
        {
            Requires.NotNegative("portalID", portalID);

            return GetRolesOnline(portalID).SingleOrDefault(l => l.PortalID == portalID && l.RoleName == roleName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool IsRoleOnline(int portalID, string roleName)
        {
            Requires.NotNegative("portalID", portalID);

            return GetRolesOnline(portalID).Where(l => l.PortalID == portalID && l.RoleName == roleName).Any();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<OnlineRoleInfo> GetRolesOnline(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OnlineRoleInfo>();
                return rep.Get(portalID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IEnumerable<string> GetSuperUserRoles(int portalID, int userID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var roles = ctx.ExecuteQuery<int>(System.Data.CommandType.Text, string.Format("Select Distinct [RoleID] From {0}[{1}UserRoles] Where [UserID]={2}", DatabaseOwner, ObjectQualifier, userID));
                return DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalID).Join(roles, r => r.RoleID, ur => ur, (r, ur) => r.RoleName);
            }
        }
    }
}