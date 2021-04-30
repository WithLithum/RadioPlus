using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using RadioPlus.Api;
using Rage;

namespace RadioPlus
{
    internal static class HijacksHandler
    {
        internal static void StopThePedManagerThread()
        {
            Game.LogTrivial("Radio+: Successfully started thread StopThePedManager");

            if (ExtensionHelper.IsStopThePedRunning)
            {
                Game.LogTrivialDebug("Radio+: Attaching STP");
                StopThePedEvents.OnTransportCalled += StopThePedEvents_OnTransportCalled;
            }
        }

        private static void StopThePedEvents_OnTransportCalled()
        {
#if DEBUG
            Game.DisplayNotification("Radio+ Intercepting via Report response");
#endif
            GameFiber.StartNew(() =>
            {
                GameFiber.Sleep(4500);
                Functions.PlayScannerAudioUsingPosition("FOR CRIME_OFFICER_REQUESTS_TRANSPORT IN_OR_ON_POSITION INTRO UNITS_RESPOND_CODE_02", Game.LocalPlayer.Character.Position);
            });
        }
    }
}
