using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Content.Taxonomy;
using System.Linq;

namespace MyDnn.Modules.Support.LiveChat.App_Start
{
    public class DnnFeatureController : IUpgradeable
    {
        public string UpgradeModule(string version)
        {
            SetUrlFriendly();

            return "Update mydnn support livechat!.";
        }

        private static void SetUrlFriendly()
        {
            HostController.Instance.Update("AUM_DoNotRewriteRegEx", "/signalr", true);
        }
    }
}