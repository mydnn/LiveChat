// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using DotNetNuke.Collections;
using System.Collections.Generic;
using MyDnn.Modules.Support.LiveChat.Models;
using MyDnn.Modules.Support.LiveChat.ViewModels;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    interface IAgentManager
    {
        int AddAgent(AgentInfo objAgentInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objAgentInfo"></param>
        void UpdateAgent(AgentInfo objAgentInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentID"></param>
        void DeleteAgent(int portalID, int agentID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentID"></param>
        /// <returns></returns>
        AgentInfo GetAgent(int portalID, int agentID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentID"></param>
        /// <returns></returns>
        AgentView GetAgentView(int portalID, int agentID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentUserName"></param>
        /// <returns></returns>
        AgentView GetAgentByUserName(int portalID, string agentUserName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentUserID"></param>
        /// <returns></returns>
        AgentView GetAgentByUserID(int portalID, int agentUserID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<AgentInfo> GetAgents(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<AgentView> GetAgentsFullView(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<AgentViewModel> GetAgentsViewModel(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<int> GetAgentsUserID(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<string> GetAgentsUserName(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        bool IsAgentOnline(int portalID);
    }
}


