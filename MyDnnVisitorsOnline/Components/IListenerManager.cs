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
    interface IListenerManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objListenerInfo"></param>
        /// <returns></returns>
        int AddListener(ListenerInfo objListenerInfo);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objListenerInfo"></param>
        void UpdateListener(ListenerInfo objListenerInfo);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objListenerInfo"></param>
        void DeleteListener(ListenerInfo objListenerInfo);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="listenerID"></param>
        /// <param name="portalID"></param>
        /// <returns></returns>
        ListenerInfo GetListener(int listenerID, int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<ListenerInfo> GetListeners(int portalID);
    }
}


