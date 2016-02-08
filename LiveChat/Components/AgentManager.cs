// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;
using MyDnn.Modules.Support.LiveChat.Models;
using MyDnn.Modules.Support.LiveChat.ViewModels;
using MyDnn.VisitorsOnline.Components;
using DotNetNuke.Common;
using MyDnn.VisitorsOnline.Api;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    internal class AgentManager : ServiceLocator<IAgentManager, AgentManager>, IAgentManager
    {
        protected override Func<IAgentManager> GetFactory()
        {
            return () => new AgentManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private const string CachePrefix = "Agents_";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objAgentInfo"></param>
        /// <returns></returns>
        public int AddAgent(AgentInfo objAgentInfo)
        {
            Requires.NotNull(objAgentInfo);
            Requires.PropertyNotNegative(objAgentInfo, "PortalID");
            Requires.PropertyNotNegative(objAgentInfo, "UserID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgentInfo>();
                rep.Insert(objAgentInfo);

                DataCache.ClearCache(CachePrefix);

                return objAgentInfo.AgentID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objAgentInfo"></param>
        public void UpdateAgent(AgentInfo objAgentInfo)
        {
            Requires.NotNull(objAgentInfo);
            Requires.PropertyNotNegative(objAgentInfo, "PortalID");
            Requires.PropertyNotNegative(objAgentInfo, "UserID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgentInfo>();
                rep.Update(objAgentInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentID"></param>
        public void DeleteAgent(int portalID, int agentID)
        {
            Requires.NotNegative("portalID", portalID);
            Requires.NotNegative("agentID", agentID);

            AgentInfo objAgentInfo = GetAgent(portalID, agentID);
            Requires.NotNull(objAgentInfo);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgentInfo>();
                rep.Delete(objAgentInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentID"></param>
        /// <returns></returns>
        public AgentInfo GetAgent(int portalID, int agentID)
        {
            Requires.NotNegative("portalID", portalID);
            Requires.NotNegative("agentID", agentID);

            return GetAgents(portalID).SingleOrDefault(a => a.AgentID == agentID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentID"></param>
        /// <returns></returns>
        public AgentView GetAgentView(int portalID, int agentID)
        {
            Requires.NotNegative("portalID", portalID);
            Requires.NotNegative("agentID", agentID);

            return GetAgentsFullView(portalID).SingleOrDefault(a => a.AgentID == agentID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentUserName"></param>
        /// <returns></returns>
        public AgentView GetAgentByUserName(int portalID, string agentUserName)
        {
            Requires.NotNegative("portalID", portalID);

            var agents = GetAgentsFullView(portalID);
            return agents.FirstOrDefault(a => a.Enabled && a.UserName == agentUserName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentUserID"></param>
        /// <returns></returns>
        public AgentView GetAgentByUserID(int portalID, int agentUserID)
        {
            Requires.NotNegative("portalID", portalID);
            Requires.NotNegative("agentUserID", agentUserID);

            var agents = GetAgentsFullView(portalID);
            return agents.FirstOrDefault(a => a.Enabled && a.UserID == agentUserID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<AgentInfo> GetAgents(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgentInfo>();
                return rep.Get(portalID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<AgentView> GetAgentsFullView(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgentView>();
                return rep.Get(portalID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<AgentViewModel> GetAgentsViewModel(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            string cacheKey = CachePrefix + "AgentsViewModel_" + portalID;

            var result = (IEnumerable<AgentViewModel>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                var agents = GetAgentsFullView(portalID);
                if (agents != null)
                    result = from agent in agents
                             select new AgentViewModel
                             {
                                 AgentID = agent.AgentID,
                                 UserID = agent.UserID,
                                 DisplayName = agent.DisplayName,
                                 Email = agent.Email,
                                 Enabled = agent.Enabled,
                                 Departments = DepartmentAgentManager.Instance.GetNamesOfDepartments(portalID, agent.AgentID)
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
        public IEnumerable<int> GetAgentsUserID(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            string cacheKey = CachePrefix + "AgentsUserID_" + portalID;

            var result = (IEnumerable<int>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                var agents = GetAgents(portalID);
                if (agents != null)
                    result = agents.Where(a => a.Enabled).Select(a => a.UserID);

                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<string> GetAgentsUserName(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            string cacheKey = CachePrefix + "AgentsUserName_" + portalID;

            var result = (IEnumerable<string>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                var agents = GetAgentsFullView(portalID);
                if (agents != null)
                    result = agents.Where(a => a.Enabled).Select(a => a.UserName);

                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public bool IsAgentOnline(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            var usersOnline = VisitorsOnlineApi.Instance.GetVisitorsOnline(portalID);

            var agents = GetAgentsUserID(portalID);

            return usersOnline.Any(u => agents.Contains(u.UserID));
        }
    }
}