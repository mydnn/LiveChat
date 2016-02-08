// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using MyDnn.Modules.Support.LiveChat.Models;
using DotNetNuke.Common;
using MyDnn.Modules.Support.LiveChats.ViewModels;
using DotNetNuke.Framework;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    internal class DepartmentManager : ServiceLocator<IDepartmentManager, DepartmentManager>, IDepartmentManager
    {
        protected override Func<IDepartmentManager> GetFactory()
        {
            return () => new DepartmentManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private const string CachePrefix = "Departments_";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objDepartmentInfo"></param>
        /// <returns></returns>
        public int AddDepartment(DepartmentInfo objDepartmentInfo)
        {
            Requires.NotNull(objDepartmentInfo);
            Requires.PropertyNotNegative(objDepartmentInfo, "PortalID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DepartmentInfo>();
                rep.Insert(objDepartmentInfo);

                DataCache.ClearCache(CachePrefix);

                return objDepartmentInfo.DepartmentID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objDepartmentInfo"></param>
        public void UpdateDepartment(DepartmentInfo objDepartmentInfo)
        {
            Requires.NotNull(objDepartmentInfo);
            Requires.PropertyNotNegative(objDepartmentInfo, "PortalID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DepartmentInfo>();
                rep.Update(objDepartmentInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="departmentID"></param>
        public void DeleteDepartment(int portalID, int departmentID)
        {
            Requires.NotNegative("portalID", portalID);
            Requires.NotNegative("departmentID", departmentID);

            DepartmentInfo objDepartmentInfo = GetDepartment(portalID,departmentID);
            Requires.NotNull(objDepartmentInfo);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DepartmentInfo>();
                rep.Delete(objDepartmentInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        public DepartmentInfo GetDepartment(int portalID, int departmentID)
        {
            Requires.NotNegative("portalID", portalID);
            Requires.NotNegative("departmentID", departmentID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DepartmentInfo>();
                return rep.GetById(departmentID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<DepartmentInfo> GetDepartments(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DepartmentInfo>();
                return rep.Get(portalID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<DepartmentViewModel> GetDepartmentsViewModel(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            string cacheKey = CachePrefix + "DepartmentsViewModel_" + portalID;
            var result = (IEnumerable<DepartmentViewModel>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                var departments = GetDepartments(portalID);
                if (departments != null)
                    result = from dep in departments
                             select new DepartmentViewModel
                             {
                                 DepartmentID = dep.DepartmentID,
                                 DepartmentName = dep.DepartmentName,
                                 Description = dep.Description,
                                 TicketEnabled = dep.TicketEnabled,
                                 LiveChatEnabled = dep.LiveChatEnabled,
                                 Agents = DepartmentAgentManager.Instance.GetDepartmentAgentsViewModel(dep.DepartmentID)
                             };

                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<DepartmentInfo> GetDepartmentsForLiveChat(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            string cacheKey = CachePrefix + "DepartmentsForLiveChat_" + portalID;
            var result = (IEnumerable<DepartmentInfo>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                var departments = GetDepartments(portalID);
                if (departments != null)
                    result = departments.Where(d => d.LiveChatEnabled);

                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }
    }
}