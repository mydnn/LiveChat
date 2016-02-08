using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;
using MyDnn.Modules.Support.LiveChat.Models;

namespace MyDnn.Modules.Support.LiveChats.ViewModels
{
    public class DepartmentViewModel
    {
        public DepartmentViewModel()
        {
        }

        public DepartmentViewModel(DepartmentInfo info)
        {
            if (info == null) return;
            DepartmentID = info.DepartmentID;
            DepartmentName = info.DepartmentName;
            Description = info.Description;
        }

        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public bool TicketEnabled { get; set; }
        public bool LiveChatEnabled { get; set; }

        public IEnumerable<DepartmentAgentViewModel> Agents { get; set; }
    }
}