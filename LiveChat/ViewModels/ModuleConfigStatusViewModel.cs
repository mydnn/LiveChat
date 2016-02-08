using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace MyDnn.Modules.Support.LiveChat.ViewModels
{
    public class ModuleConfigStatusViewModel
    {
        public int ModuleID { get; set; }
        public int TabID { get; set; }
        public bool VisitorsOnlineEnabled { get; set; }
        public bool LiveChatEnabled { get; set; }
        public bool BasicSettingsUpdated { get; set; }
        public bool WidgetSettingsUpdated { get; set; }
    }
}