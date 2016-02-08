using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyDnn.Modules.Support.LiveChat.Components.Enums
{
    internal enum LiveChatJoinPermission
    {
        OnlyCurrentAgents,
        OnlyCurrentAgentsAndAdmin,
        AllAgentsInDepartment
    }
}