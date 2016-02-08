// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using DotNetNuke.Framework;
using DotNetNuke.Common;
using DotNetNuke.Data;
using MyDnn.VisitorsOnline.Models;

namespace MyDnn.VisitorsOnline.Components
{
    /// <summary>
    /// 
    /// </summary>
    internal class ListenerManager : ServiceLocator<IListenerManager, ListenerManager>, IListenerManager
    {
        protected override Func<IListenerManager> GetFactory()
        {
            return () => new ListenerManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private const string CachePrefix = "MyDnnListener_";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objListenerInfo"></param>
        /// <returns></returns>
        public int AddListener(ListenerInfo objListenerInfo)
        {
            Requires.NotNull(objListenerInfo);
            Requires.PropertyNotNegative(objListenerInfo, "PortalID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ListenerInfo>();
                rep.Insert(objListenerInfo);
                return objListenerInfo.ListenerID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objListenerInfo"></param>
        public void UpdateListener(ListenerInfo objListenerInfo)
        {
            Requires.NotNull(objListenerInfo);
            Requires.PropertyNotNegative(objListenerInfo, "PortalID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ListenerInfo>();
                rep.Update(objListenerInfo);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objListenerInfo"></param>
        public void DeleteListener(ListenerInfo objListenerInfo)
        {
            Requires.NotNull(objListenerInfo);
            Requires.PropertyNotNegative(objListenerInfo, "ListenerID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ListenerInfo>();
                rep.Update(objListenerInfo);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="listenerID"></param>
        /// <returns></returns>
        public ListenerInfo GetListener(int portalID,int listenerID)
        {
            Requires.NotNegative("listenerID", listenerID);
            Requires.NotNegative("portalID", portalID);

            return GetListeners(portalID).SingleOrDefault(l => l.ListenerID == listenerID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        public IEnumerable<ListenerInfo> GetListeners(int portalID)
        {
            Requires.NotNegative("portalID", portalID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ListenerInfo>();
                return rep.Get(portalID);
            }
        }
    }
}