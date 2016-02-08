// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;
using MyDnn.VisitorsOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyDnn.VisitorsOnline.Components
{
    /// <summary>
    /// 
    /// </summary>
    internal class VisitorOnlineManager : ServiceLocator<IVisitorOnlineManager, VisitorOnlineManager>, IVisitorOnlineManager
    {
        protected override Func<IVisitorOnlineManager> GetFactory()
        {
            return () => new VisitorOnlineManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private const string CachePrefix = "VisitorsOnline_";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objOnlineVisitorInfo"></param>
        /// <param name="connectionID"></param>
        public void AddVisitorOnline(OnlineVisitorInfo objOnlineVisitorInfo, string connectionID)
        {
            Requires.NotNull(objOnlineVisitorInfo);

            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "MyDnnVisitorsOnline_Add", objOnlineVisitorInfo.PortalID,
                    objOnlineVisitorInfo.VisitorGUID,
                    objOnlineVisitorInfo.UserName,
                    objOnlineVisitorInfo.DisplayName,
                    objOnlineVisitorInfo.Email,
                    objOnlineVisitorInfo.OnlineDate,
                    objOnlineVisitorInfo.IP,
                    objOnlineVisitorInfo.UserAgent,
                    objOnlineVisitorInfo.LastURL,
                    objOnlineVisitorInfo.ReferrerURL,
                    connectionID);
            }

            string cacheKey = CachePrefix + objOnlineVisitorInfo.PortalID;
            DataCache.ClearCache(cacheKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <param name="userName"></param>
        /// <param name="displayName"></param>
        /// <param name="email"></param>
        /// <param name="lastUrl"></param>
        /// <param name="connectionID"></param>
        /// <param name="isUpdateInfo"></param>
        public void UpdateVisitorOnline(int portalID, string visitorGUID, string userName, string displayName, string email, string lastUrl, string connectionID, bool isUpdateInfo)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "MyDnnVisitorsOnline_Update", portalID, visitorGUID, userName, displayName, email, lastUrl, connectionID, isUpdateInfo);
            }

            string cacheKey = CachePrefix + portalID;
            DataCache.ClearCache(cacheKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitorGUID"></param>
        public void Ping(string visitorGUID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "MyDnnVisitorsOnline_Ping", visitorGUID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionId"></param>
        public void DeleteConnection(string connectionId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "MyDnnVisitorsOnline_DeleteConnection", connectionId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<OfflineVisitorInfo> PurgeVisitorsOnline(int portalID)
        {
            IEnumerable<OfflineVisitorInfo> result;
            using (IDataContext ctx = DataContext.Instance())
            {
                result = ctx.ExecuteQuery<OfflineVisitorInfo>(System.Data.CommandType.StoredProcedure, "MyDnnVisitorsOnline_PurgeVisitorsOnline");

                string cacheKey = CachePrefix + portalID;
                DataCache.ClearCache(cacheKey);

                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public OnlineVisitorInfo GetVisitorByUserID(int portalID, int userID)
        {
            var visitors = GetVisitorsOnline(portalID);
            return visitors.SingleOrDefault(u => u.UserID == userID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <returns></returns>
        public OnlineVisitorInfo GetVisitorByGUID(int portalID, string visitorGUID)
        {
            var visitors = GetVisitorsOnline(portalID);
            return visitors.SingleOrDefault(u => u.VisitorGUID == visitorGUID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool IsVisitorOnline(int portalID, int userID)
        {
            var visitors = GetVisitorsOnline(portalID);
            return (visitors != null ? visitors.Any(v => v.UserID == userID) : false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<OnlineVisitorInfo> GetVisitorsOnline(int portalID)
        {
            string cacheKey = CachePrefix + portalID;
            var result = DataCache.GetCache<IEnumerable<OnlineVisitorInfo>>(cacheKey);
            if (result == null)
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    result = ctx.ExecuteQuery<OnlineVisitorInfo>(System.Data.CommandType.StoredProcedure, "MyDnnVisitorsOnline_GetAll", portalID);
                    DataCache.SetCache(cacheKey, result);
                }
            }
            return result;
        }
    }
}