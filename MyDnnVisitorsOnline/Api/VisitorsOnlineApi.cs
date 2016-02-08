using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyDnn.VisitorsOnline.Models;
using MyDnn.VisitorsOnline.Components;

namespace MyDnn.VisitorsOnline.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class VisitorsOnlineApi
    {
        /// <summary>
        /// 
        /// </summary>
        public static VisitorsOnlineApi Instance
        {
            get
            {
                return new VisitorsOnlineApi();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<OnlineVisitorInfo> GetVisitorsOnline(int portalID)
        {
            return VisitorOnlineManager.Instance.GetVisitorsOnline(portalID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public OnlineVisitorInfo GetVisitorByUserID(int portalID, int userID)
        {
            return VisitorOnlineManager.Instance.GetVisitorByUserID(portalID, userID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="visitorGUID"></param>
        /// <returns></returns>
        public OnlineVisitorInfo GetVisitorByGUID(int portalID, string visitorGUID)
        {
            return VisitorOnlineManager.Instance.GetVisitorByGUID(portalID, visitorGUID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool IsVisitorOnline(int portalID, int userID)
        {
            return VisitorOnlineManager.Instance.IsVisitorOnline(portalID, userID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objListenerInfo"></param>
        /// <returns></returns>
        public int AddListener(ListenerInfo objListenerInfo)
        {
            return ListenerManager.Instance.AddListener(objListenerInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<ListenerInfo> GetListeners(int portalID)
        {
            return ListenerManager.Instance.GetListeners(portalID);
        }
    }
}