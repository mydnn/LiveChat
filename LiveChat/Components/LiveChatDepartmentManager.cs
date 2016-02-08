// Copyright (c) MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Framework;
using MyDnn.Modules.Support.LiveChat.Models;
using DotNetNuke.Data;
using MyDnn.Modules.Support.LiveChat.ViewModels;
using DotNetNuke.Common;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    internal class LiveChatDepartmentManager : ServiceLocator<ILiveChatDepartmentManager, LiveChatDepartmentManager>, ILiveChatDepartmentManager
    {
        protected override Func<ILiveChatDepartmentManager> GetFactory()
        {
            return () => new LiveChatDepartmentManager();
        }

        /// <summary>
        /// 
        /// </summary>
        private const string CachePrefix = "LiveChatDepartments_";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLiveChatDepartmentInfo"></param>
        /// <returns></returns>
        public int AddLiveChatDepartment(LiveChatDepartmentInfo objLiveChatDepartmentInfo)
        {
            Requires.NotNull(objLiveChatDepartmentInfo);
            Requires.PropertyNotNegative(objLiveChatDepartmentInfo, "LiveChatID");
            Requires.PropertyNotNegative(objLiveChatDepartmentInfo, "DepartmentID");

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatDepartmentInfo>();
                rep.Insert(objLiveChatDepartmentInfo);

                DataCache.ClearCache(CachePrefix);

                return objLiveChatDepartmentInfo.LiveChatID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="livechatDepartmentID"></param>
        public void DeleteLiveChatDepartment(int livechatDepartmentID)
        {
            Requires.NotNegative("livechatDepartmentID", livechatDepartmentID);

            LiveChatDepartmentInfo objLiveChatDepartmentInfo = GetLiveChatDepartmentByID(livechatDepartmentID);
            Requires.NotNull(objLiveChatDepartmentInfo);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatDepartmentInfo>();
                rep.Delete(objLiveChatDepartmentInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public LiveChatDepartmentInfo GetLiveChatDepartmentByID(int livechatDepartmentID)
        {
            Requires.NotNegative("livechatDepartmentID", livechatDepartmentID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatDepartmentInfo>();
                return rep.GetById(livechatDepartmentID);
            }
        }

        public IEnumerable<LiveChatDepartmentInfo> GetLiveChatDepartments(int livechatID)
        {
            Requires.NotNegative("livechatID", livechatID);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<LiveChatDepartmentInfo>();
                return rep.Get(livechatID);
            }
        }

        public IEnumerable<LiveChatDepartmentViewModel> GetLiveChatDepartmentsViewModel(int portalID, int livechatID)
        {
            Requires.NotNegative("portalID", portalID);
            Requires.NotNegative("livechatID", livechatID);

            string cacheKey = CachePrefix + "ViewModel_" + livechatID;

            var result = (List<LiveChatDepartmentViewModel>)DataCache.GetCache(cacheKey);
            if (result == null)
            {
                result = new List<LiveChatDepartmentViewModel>();
                var livechatDepartments = GetLiveChatDepartments(livechatID);
                var departments = DepartmentManager.Instance.GetDepartments(portalID);
                foreach (LiveChatDepartmentInfo department in livechatDepartments)
                {
                    result.Add(new LiveChatDepartmentViewModel()
                    {
                        DepartmentID = department.DepartmentID,
                        DepartmentName = departments.SingleOrDefault(d => d.DepartmentID == department.DepartmentID).DepartmentName,
                        CreateDate = department.CreateDate
                    });
                }
                DataCache.SetCache(cacheKey, result);
            }
            return result;
        }
    }
}