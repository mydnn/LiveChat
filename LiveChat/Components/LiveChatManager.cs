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
using DotNetNuke.Framework;
using System.Text;
using System.Data.SqlTypes;
using MyDnn.Modules.Support.LiveChat.ViewModels;
using DotNetNuke.Collections;
using System.Globalization;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    internal class LiveChatManager : ServiceLocator<ILiveChatManager, LiveChatManager>, ILiveChatManager
    {
        protected override Func<ILiveChatManager> GetFactory()
        {
            return () => new LiveChatManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private const string CachePrefix = "LiveChats_";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatInfo"></param>
        /// <returns></returns>
        public int AddLiveChat(LiveChatInfo objLiveChatInfo)
        {
            Requires.NotNull(objLiveChatInfo);
            Requires.PropertyNotNegative(objLiveChatInfo, "PortalID");

            using (IDataContext ctx = DataContext.Instance())
            {
                objLiveChatInfo.LastMessageDate = DateTime.Now;
                var rep = ctx.GetRepository<LiveChatInfo>();
                rep.Insert(objLiveChatInfo);

                DataCache.ClearCache(CachePrefix);

                return objLiveChatInfo.LiveChatID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatInfo"></param>
        public void UpdateLiveChat(LiveChatInfo objLiveChatInfo)
        {
            Requires.NotNull(objLiveChatInfo);
            Requires.PropertyNotNegative(objLiveChatInfo, "PortalID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatInfo>();
                rep.Update(objLiveChatInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        public void DeleteLiveChat(int livechatID)
        {
            Requires.NotNegative("livechatID", livechatID);

            LiveChatInfo objLiveChatInfo = GetLiveChatByID(livechatID);
            Requires.NotNull(objLiveChatInfo);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatInfo>();
                rep.Delete(objLiveChatInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <param name="agentID"></param>
        /// <param name="agentUserID"></param>
        public void StartLiveChat(int livechatID, int agentID, int agentUserID)
        {
            Requires.NotNegative("livechatID", livechatID);
            Requires.NotNegative("agentID", agentID);
            Requires.NotNegative("agentUserID", agentUserID);

            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "MyDnnSupport_LiveChatStart", livechatID, agentID, agentUserID);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <param name="closedByAgent"></param>
        public void CloseLiveChat(int livechatID, bool closedByAgent)
        {
            Requires.NotNegative("livechatID", livechatID);

            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "MyDnnSupport_LiveChatClose", livechatID, closedByAgent);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        public void CloseLeftLiveChats(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "MyDnnSupport_LiveChatCloseLeftChats", portalID);
            }

            DataCache.ClearCache(CachePrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatID"></param>
        /// <returns></returns>
        public LiveChatInfo GetLiveChatByID(int livechatID)
        {
            Requires.NotNegative("livechatID", livechatID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatInfo>();
                return rep.GetById(livechatID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <returns></returns>
        public bool hasVisitorOpenedLiveChat(int portalID, string visitorGUID)
        {
            Requires.NotNegative("portalID", portalID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatInfo>();
                return rep.Find("Where (PortalID = @0 and VisitorGUID = @1 and IsClosed=0)", portalID, visitorGUID).Any();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<LiveChatInfo> GetLiveChats(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatInfo>();
                return rep.Get(portalID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<LiveChatInfo> GetIncomingLiveChats(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            string cacheKey = CachePrefix + "IncomingLiveChats_" + portalID;

            var result = (IEnumerable<LiveChatInfo>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = ctx.ExecuteQuery<LiveChatInfo>(System.Data.CommandType.StoredProcedure, "MyDnnSupport_LiveChatGetIncomingChats", portalID);
                }

                DataCache.SetCache(cacheKey, result);
            }
            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<LiveChatInfo> GetCurrentLiveChats(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            string cacheKey = CachePrefix + "CurrentLiveChats_" + portalID;

            var result = (IEnumerable<LiveChatInfo>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = ctx.ExecuteQuery<LiveChatInfo>(System.Data.CommandType.StoredProcedure, "MyDnnSupport_LiveChatGetCurrentChats", portalID);
                }

                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <returns></returns>
        public LiveChatInfo GetCurrentLiveChatByVisitor(int portalID, string visitorGUID)
        {
            Requires.NotNegative("portalID", portalID);

            using (IDataContext ctx = DataContext.Instance())
            {
                return ctx.ExecuteSingleOrDefault<LiveChatInfo>(System.Data.CommandType.StoredProcedure, "MyDnnSupport_LiveChatGetVisitorCurrentChat", portalID, visitorGUID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="agentUserID"></param>
        /// <returns></returns>
        public IEnumerable<LiveChatInfo> GetCurrentLiveChatsByAgent(int portalID, int agentUserID)
        {
            Requires.NotNegative("portalID", portalID);
            Requires.NotNegative("agentUserID", agentUserID);

            string cacheKey = CachePrefix + "CurrentLiveChats_" + portalID + "_" + agentUserID;

            var result = (IEnumerable<LiveChatInfo>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = ctx.ExecuteQuery<LiveChatInfo>(System.Data.CommandType.StoredProcedure, "MyDnnSupport_LiveChatGetCurrentChats", portalID, agentUserID);
                }

                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }

        public IPagedList<LiveChatViewModel> GetChatHistory(int portalID, int type, string departments, string agents, string visitorEmail, DateTime? fromDate, DateTime? toDate, int rating, bool unread, int pageIndex, int pageSize, int totalCount)
        {
            var condition = new StringBuilder(string.Format("PortalID={0} ", portalID));

            string dbo = DataProvider.Instance().DatabaseOwner;
            string oq = DataProvider.Instance().ObjectQualifier;

            //all | chat | offline
            if (type == 1) //chat
                condition.AppendFormat(" and LiveChatID in (Select LiveChatID From {0}[{1}MyDnnSupport_LiveChatAgents])", dbo, oq);
            else if (type == 2) //offline
                condition.AppendFormat(" and LiveChatID not in (Select LiveChatID From {0}[{1}MyDnnSupport_LiveChatAgents])", dbo, oq);

            if (rating == 1 || rating == 2)
                condition.Append(string.Format(" and Rate = {0}", rating));
            else if (rating == 3)
                condition.Append(" and (Rate = 1 or Rate = 2)");

            //departments
            if (type != 2 && !string.IsNullOrEmpty(departments))
            {
                string departmentsCondition = " and LiveChatID in (Select LiveChatID From {0}[{1}MyDnnSupport_LiveChatDepartments] Where {2})";
                foreach (var dep in departments.Split(','))
                {
                    int value;
                    if (int.TryParse(dep, out value))
                    {
                        departmentsCondition = departmentsCondition.Replace("[OR]", " or {0}");
                        departmentsCondition = string.Format(departmentsCondition, dbo, oq, string.Format("DepartmentID = {0} [OR]", value));
                    }
                }
                departmentsCondition = departmentsCondition.Replace("[OR]", string.Empty);
                condition.Append(departmentsCondition);
            }

            //agents
            if (type != 2 && !string.IsNullOrEmpty(agents))
            {
                string agentsCondition = " and LiveChatID in (Select LiveChatID From {0}[{1}MyDnnSupport_LiveChatAgents] Where {2})";
                foreach (var agent in agents.Split(','))
                {
                    int value;
                    if (int.TryParse(agent, out value))
                    {
                        agentsCondition = agentsCondition.Replace("[OR]", " or {0}");
                        agentsCondition = string.Format(agentsCondition, dbo, oq, string.Format("UserID = {0} [OR]", value));
                    }
                }
                agentsCondition = agentsCondition.Replace("[OR]", string.Empty);
                condition.Append(agentsCondition);
            }

            //visitor email
            if (!string.IsNullOrEmpty(visitorEmail))
                condition.AppendFormat(" and VisitorEmail like N'%{0}%'", visitorEmail);

            //set date culture
            var fromDateVal = fromDate != null ? fromDate.Value.ToString(new CultureInfo("en-US")) : SqlDateTime.MinValue.Value.ToString(new CultureInfo("en-US"));
            var toDateVal = toDate != null ? toDate.Value.ToString(new CultureInfo("en-US")) : SqlDateTime.MaxValue.Value.ToString(new CultureInfo("en-US"));

            //date range
            condition.AppendFormat(" and CreateDate >= '{0}' and CreateDate<= '{1}'", fromDateVal, toDateVal);

            int from = pageIndex * pageSize + 1;
            int to = from + pageSize;

            string query = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY CreateDate desc) AS RowNum, * FROM {0}[{1}MyDnnSupport_LiveChats] Where {2}) AS RowConstrainedResult WHERE RowNum >= {3} AND RowNum < {4} ORDER BY RowNum", dbo, oq, condition.ToString(), from, to);

            var allDepartments = DepartmentManager.Instance.GetDepartments(portalID);
            var allAgents = AgentManager.Instance.GetAgents(portalID);

            using (IDataContext ctx = DataContext.Instance())
            {
                if (pageIndex == 0)
                    totalCount = ctx.ExecuteScalar<int>(System.Data.CommandType.Text, string.Format("Select Count(*) From {0}[{1}MyDnnSupport_LiveChats] Where {2}", dbo, oq, condition));

                var livechats = ctx.ExecuteQuery<LiveChatInfo>(System.Data.CommandType.Text, query);

                var result = livechats.Select(r => new LiveChatViewModel()
                {
                    LiveChatID = r.LiveChatID,
                    Visitor = new LiveChatVisitorViewModel()
                    {
                        UserID = r.VisitorUserID,
                        Avatar = r.VisitorUserID > 0 ? DotNetNuke.Common.Globals.ResolveUrl("~/dnnimagehandler.ashx?mode=profilepic&userid=" + r.VisitorUserID) : string.Empty,
                        DisplayName = r.VisitorName,
                        Email = r.VisitorEmail,
                        IP = r.VisitorIP,
                        UserAgent = r.VisitorUserAgent
                    },
                    Departments = LiveChatDepartmentManager.Instance.GetLiveChatDepartmentsViewModel(portalID, r.LiveChatID),
                    Agents = LiveChatAgentManager.Instance.GetLiveChatAgentsViewModel(portalID, r.LiveChatID),
                    Message = r.VisitorMessage,
                    CreateDate = r.CreateDate
                });

                return new PagedList<LiveChatViewModel>(result, totalCount, pageIndex, pageSize);
            }
        }

        public void PurgeHistory(int[] items)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatInfo>();

                string condition = string.Empty;
                foreach (var livechatID in items)
                {
                    condition = condition.Replace("[OR]", " or ");
                    condition += string.Format(" LiveChatID = {0} [OR]", livechatID);

                }
                condition = condition.Replace("[OR]", string.Empty);

                rep.Delete(string.Format("Where ({0})", condition));

                DataCache.ClearCache(CachePrefix);
            }
        }
    }
}