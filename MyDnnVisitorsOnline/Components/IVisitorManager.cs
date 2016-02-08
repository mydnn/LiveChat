// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using DotNetNuke.Collections;
using System.Collections.Generic;
using MyDnn.VisitorsOnline.Models;

namespace MyDnn.VisitorsOnline.Components
{
    /// <summary>
    /// 
    /// </summary>
    interface IVisitorOnlineManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objOnlineVisitorInfo"></param>
        /// <param name="connectionID"></param>
        void AddVisitorOnline(OnlineVisitorInfo objOnlineVisitorInfo, string connectionID);

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
        void UpdateVisitorOnline(int portalID, string visitorGUID, string userName, string displayName, string email, string lastUrl, string connectionID, bool isUpdateInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visitorGUID"></param>
        void Ping(string visitorGUID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionId"></param>
        void DeleteConnection(string connectionId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<OfflineVisitorInfo> PurgeVisitorsOnline(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        OnlineVisitorInfo GetVisitorByUserID(int portalID, int userID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <returns></returns>
        OnlineVisitorInfo GetVisitorByGUID(int portalID, string visitorGUID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        bool IsVisitorOnline(int portalID, int userid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<OnlineVisitorInfo> GetVisitorsOnline(int portalID);
    }
}


