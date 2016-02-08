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
using DotNetNuke.Common;
using MyDnn.Modules.Support.LiveChat.ViewModels;
using MyDnn.VisitorsOnline.Api;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    internal class LiveChatAgentManager : ServiceLocator<ILiveChatAgentManager, LiveChatAgentManager>, ILiveChatAgentManager
    {
        protected override Func<ILiveChatAgentManager> GetFactory()
        {
            return () => new LiveChatAgentManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private const string CachePrefix = "LiveChatAgents_";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatAgentInfo"></param>
        /// <returns></returns>
        public int AddLiveChatAgent(LiveChatAgentInfo objLiveChatAgentInfo)
        {
            objLiveChatAgentInfo.LeftDate = DateTime.MaxValue;

            Requires.NotNull(objLiveChatAgentInfo);
            Requires.PropertyNotNegative(objLiveChatAgentInfo, "LiveChatID");
            Requires.PropertyNotNegative(objLiveChatAgentInfo, "UserID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatAgentInfo>();
                rep.Insert(objLiveChatAgentInfo);

                DataCache.ClearCache(CachePrefix);

                return objLiveChatAgentInfo.LiveChatID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatAgentInfo"></param>
        public void UpdateLiveChatAgent(LiveChatAgentInfo objLiveChatAgentInfo)
        {
            Requires.NotNull(objLiveChatAgentInfo);
            Requires.PropertyNotNegative(objLiveChatAgentInfo, "LiveChatID");
            Requires.PropertyNotNegative(objLiveChatAgentInfo, "UserID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatAgentInfo>();
                rep.Update(objLiveChatAgentInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatAgentID"></param>
        public void DeleteLiveChatAgent(int livechatAgentID)
        {
            Requires.NotNegative("livechatAgentID", livechatAgentID);

            LiveChatAgentInfo objLiveChatAgentInfo = GetLiveChatAgentByID(livechatAgentID);
            Requires.NotNull(objLiveChatAgentInfo);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatAgentInfo>();
                rep.Delete(objLiveChatAgentInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatAgentID"></param>
        /// <returns></returns>
        public LiveChatAgentInfo GetLiveChatAgentByID(int livechatAgentID)
        {
            Requires.NotNegative("livechatAgentID", livechatAgentID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatAgentInfo>();
                return rep.GetById(livechatAgentID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        public IEnumerable<LiveChatAgentInfo> GetLiveChatAgents(int livechatID)
        {
            Requires.NotNegative("livechatID", livechatID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatAgentInfo>();
                return rep.Get(livechatID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        public IEnumerable<LiveChatAgentViewModel> GetLiveChatAgentsViewModel(int portalID, int livechatID)
        {
            Requires.NotNegative("portalID", portalID);
            Requires.NotNegative("livechatID", livechatID);

            string cacheKey = CachePrefix + "ViewModel_" + livechatID;
            var result = (List<LiveChatAgentViewModel>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                result = new List<LiveChatAgentViewModel>();

                var agents = GetLiveChatAgents(livechatID);
                foreach (LiveChatAgentInfo agent in agents)
                {
                    var objAgentView = AgentManager.Instance.GetAgentByUserID(portalID, agent.UserID);
                    result.Add(new LiveChatAgentViewModel()
                    {
                        AgentID = objAgentView.AgentID,
                        UserID = objAgentView.UserID,
                        DisplayName = objAgentView.DisplayName,
                        Email = objAgentView.Email,
                        Avatar = DotNetNuke.Common.Globals.ResolveUrl("~/dnnimagehandler.ashx?mode=profilepic&userid=" + objAgentView.UserID),
                        JoinDate = agent.JoinDate,
                        LeftDate = agent.LeftDate,
                        IsOnline = VisitorsOnlineApi.Instance.IsVisitorOnline(portalID, objAgentView.UserID)
                    });
                }

                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }
    }
}