using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyDnn.Modules.Support.LiveChat.ViewModels
{
    public class LiveChatDepartmentViewModel
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string AgentOnlineStatus { get; set; }
        public DateTime CreateDate { get; set; }
    }
}