// Copyright (c); MyDnn Group. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Data;
using MyDnn.Modules.Support.LiveChat.Models;
using MyDnn.Modules.Support.LiveChats.ViewModels;

namespace MyDnn.Modules.Support.LiveChat.Components
{
    /// <summary>
    /// 
    /// </summary>
    interface IDepartmentManager 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objDepartmentInfo"></param>
        /// <returns></returns>
        int AddDepartment(DepartmentInfo objDepartmentInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objDepartmentInfo"></param>
        void UpdateDepartment(DepartmentInfo objDepartmentInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="departmentID"></param>
        void DeleteDepartment(int portalID, int departmentID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        DepartmentInfo GetDepartment(int portalID, int departmentID);
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<DepartmentInfo> GetDepartments(int portalID);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<DepartmentViewModel> GetDepartmentsViewModel(int portalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        IEnumerable<DepartmentInfo> GetDepartmentsForLiveChat(int portalID);
    }
}