using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;

namespace RadioPlus
{
    public class Main : Plugin
    {
        public override void Finally()
        {
        }

        public override void Initialize()
        {
            Game.LogTrivial("Radio+: Called initialize");
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
        }

        private void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                Api.ExtensionHelper.InitExtensions();
                RadioPlusHandler.InitConfig();
                RadioPlusHandler.InitThreads();
            }

            AppDomain.CurrentDomain.AssemblyResolve += LSPDFRResolveEventHandler;
            Functions.OnOnDutyStateChanged -= Functions_OnOnDutyStateChanged;
        }

        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                if (args.Name.Equals(assembly.GetName().Name, StringComparison.OrdinalIgnoreCase))
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}
