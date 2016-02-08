// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Common;
using MyDnn.Modules.Support.LiveChat.Models;
using MyDnn.Modules.Support.LiveChat.ViewModels;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    interface ILiveChatAgentManager {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatAgentInfo"></param>
        /// <returns></returns>
        int AddLiveChatAgent(LiveChatAgentInfo objLiveChatAgentInfo);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatAgentInfo"></param>
        void UpdateLiveChatAgent(LiveChatAgentInfo objLiveChatAgentInfo);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatAgentID"></param>
        void DeleteLiveChatAgent(int livechatAgentID);
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatAgentID"></param>
        /// <returns></returns>
        LiveChatAgentInfo GetLiveChatAgentByID(int livechatAgentID);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        IEnumerable<LiveChatAgentInfo> GetLiveChatAgents(int livechatID);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        IEnumerable<LiveChatAgentViewModel> GetLiveChatAgentsViewModel(int portalID, int livechatID);
    }
}