using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;

namespace RadioPlus.Api
{
    internal static class ExtensionHelper
    {
        internal static bool IsStopThePedRunning { get; private set; }

        internal static void InitExtensions()
        {
            // Since IsLSPDFRPluginRunning uses loops, and we could face potential performance loss
            // If we check it every execution, so we per-determine whether plugins are running to
            // avoid too many loops being called

            // Because if STP does not exist, it will not be exist at the whole execution
            IsStopThePedRunning = IsLSPDFRPluginRunning("StopThePed");
            Game.LogTrivial("Radio+: Stop the ped running: " + IsStopThePedRunning);

            if (IsStopThePedRunning)
            {
                StopThePedEvents.RegisterEvents();
            }
        }

        internal static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                var name = assembly.GetName();
                if (string.Equals(name.Name, Plugin, StringComparison.OrdinalIgnoreCase) && (minversion == null || name.Version.CompareTo(minversion) >= 0))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
