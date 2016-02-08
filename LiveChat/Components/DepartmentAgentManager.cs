// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;
using MyDnn.Modules.Support.LiveChat.Models;
using MyDnn.Modules.Support.LiveChats.ViewModels;
using MyDnn.VisitorsOnline.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    internal class DepartmentAgentManager : ServiceLocator<IDepartmentAgentManager, DepartmentAgentManager>, IDepartmentAgentManager
    {
        protected override Func<IDepartmentAgentManager> GetFactory()
        {
            return () => new DepartmentAgentManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private const string CachePrefix = "DepartmentAgents_";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objDepartmentAgentInfo"></param>
        public void AddDepartmentAgent(DepartmentAgentInfo objDepartmentAgentInfo)
        {
            Requires.NotNull(objDepartmentAgentInfo);
            Requires.PropertyNotNegative(objDepartmentAgentInfo, "DepartmentID");
            Requires.PropertyNotNegative(objDepartmentAgentInfo, "AgentID");
            Requires.PropertyNotNegative(objDepartmentAgentInfo, "UserID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DepartmentAgentInfo>();
                rep.Insert(objDepartmentAgentInfo);
            }

            DataCache.ClearCache();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objDepartmentAgentInfo"></param>
        public void UpdateDepartmentAgent(DepartmentAgentInfo objDepartmentAgentInfo)
        {
            Requires.PropertyNotNegative(objDepartmentAgentInfo, "DepartmentID");
            Requires.PropertyNotNegative(objDepartmentAgentInfo, "AgentID");
            Requires.PropertyNotNegative(objDepartmentAgentInfo, "UserID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DepartmentAgentInfo>();
                rep.Update(objDepartmentAgentInfo);
            }

            DataCache.ClearCache();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DepartmentAgentID"></param>
        public void DeleteDepartmentAgent(int departmentID, int departmentAgentID)
        {
            Requires.NotNegative("departmentAgentID", departmentAgentID);

            DepartmentAgentInfo objDepartmentAgentInfo = GetDepartmentAgent(departmentID, departmentAgentID);
            Requires.NotNull(objDepartmentAgentInfo);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DepartmentAgentInfo>();
                rep.Delete(objDepartmentAgentInfo);
            }

            DataCache.ClearCache();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agentID"></param>
        public void DeleteAgentDepartments(int agentID)
        {
            Requires.NotNegative("agentID", agentID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DepartmentAgentInfo>();
                rep.Delete("Where ([AgentID]=@0)", agentID);

                DataCache.ClearCache(CachePrefix);
            }

            DataCache.ClearCache();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        public bool IsAgentOnlineByDepartment(int portalID, int departmentID)
        {
            var agents = GetDepartmentAgents(departmentID);
            var usersOnline = VisitorsOnlineApi.Instance.GetVisitorsOnline(portalID);
            bool isAgentOnline = false;
            if (usersOnline != null && agents != null)
            {
                isAgentOnline = usersOnline.Any(u => agents.Select(a => a.UserID).Contains(u.UserID));
            }
            return isAgentOnline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DepartmentAgentID"></param>
        /// <returns></returns>
        public DepartmentAgentInfo GetDepartmentAgent(int departmentID, int departmentAgentID)
        {
            Requires.NotNegative("departmentID", departmentID);
            Requires.NotNegative("departmentAgentID", departmentAgentID);

            return GetDepartmentAgents(departmentID).SingleOrDefault(da => da.DepartmentAgentID == departmentAgentID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        public IEnumerable<DepartmentAgentInfo> GetDepartmentAgents(int departmentID)
        {
            Requires.NotNegative("departmentID", departmentID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DepartmentAgentInfo>();
                return rep.Get(departmentID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        public IEnumerable<DepartmentAgentViewModel> GetDepartmentAgentsViewModel(int departmentID)
        {
            Requires.NotNegative("departmentID", departmentID);

            string cacheKey = CachePrefix + "DepartmentAgentsViewModel_" + departmentID;

            var result = (IEnumerable<DepartmentAgentViewModel>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                var departmentAgents = GetDepartmentAgents(departmentID);
                if (departmentAgents != null)
                    result = from agent in departmentAgents
                             select new DepartmentAgentViewModel
                             {
                                 DepartmentID = agent.DepartmentID,
                                 AgentID = agent.AgentID,
                                 UserID = agent.UserID,
                                 DisplayName = Common.Common.Instance.GetUserDisplayName(agent.UserID)
                             };

                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        public IEnumerable<DepartmentAgentInfo> GetAgentDepartments(int agentID)
        {
            Requires.NotNegative("agentID", agentID);

            string cacheKey = "LSAgentDepartments_" + agentID;

            var result = (IEnumerable<DepartmentAgentInfo>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    var rep = ctx.GetRepository<DepartmentAgentInfo>();
                    result= rep.Find("Where (AgentID=@0)", agentID);
                }
                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentID"></param>
        /// <returns></returns>
        public IEnumerable<string> GetNamesOfDepartments(int portalID, int agentID)
        {
            Requires.NotNegative("portalID", portalID);
            Requires.NotNegative("agentID", agentID);

            string cacheKey = CachePrefix + "AgentDepartmentNames" + agentID;

            var result = (IEnumerable<string>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                var agentDepartments = GetAgentDepartments(agentID);
                var departments = DepartmentManager.Instance.GetDepartments(portalID);
                if (agentDepartments != null && departments != null)
                    result = from agentdep in agentDepartments
                             join department in departments on agentdep.DepartmentID equals department.DepartmentID
                             select department.DepartmentName;

                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }
    }
}