// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Data;
using MyDnn.Modules.Support.LiveChat.Models;
using DotNetNuke.Common;
using MyDnn.Modules.Support.LiveChat.ViewModels;
using DotNetNuke.Collections;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    interface ILiveChatManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatInfo"></param>
        /// <returns></returns>
        int AddLiveChat(LiveChatInfo objLiveChatInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatInfo"></param>
        void UpdateLiveChat(LiveChatInfo objLiveChatInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        void DeleteLiveChat(int livechatID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <param name="agentID"></param>
        /// <param name="agentUserID"></param>
        void StartLiveChat(int livechatID, int agentID, int agentUserID);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <param name="closedByAgent"></param>
        void CloseLiveChat(int livechatID, bool closedByAgent);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        void CloseLeftLiveChats(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        LiveChatInfo GetLiveChatByID(int livechatID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <returns></returns>
        LiveChatInfo GetCurrentLiveChatByVisitor(int portalID, string visitorGUID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <returns></returns>
        bool hasVisitorOpenedLiveChat(int portalID, string visitorGUID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<LiveChatInfo> GetLiveChats(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<LiveChatInfo> GetCurrentLiveChats(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<LiveChatInfo> GetIncomingLiveChats(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentUserID"></param>
        /// <returns></returns>
        IEnumerable<LiveChatInfo> GetCurrentLiveChatsByAgent(int portalID, int agentUserID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="departments"></param>
        /// <param name="agents"></param>
        /// <param name="visitorEmail"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="rating"></param>
        /// <param name="unread"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<LiveChatViewModel> GetChatHistory(int portalID, int type, string departments, string agents, string visitorEmail, DateTime? fromDate, DateTime? toDate, int rating, bool unread, int pageIndex, int pageSize, int totalCount);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        void PurgeHistory(int[] items);
    }
}