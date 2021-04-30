using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using Rage;
using StopThePed.API;

namespace RadioPlus.Api
{
    internal static class StopThePedEvents
    {
        internal delegate void StopThePedEventHandler();
        internal delegate void SinglePedEventHandler(object sender, Ped args);
        internal delegate void SingleVehicleEventHandler(object sender, Vehicle args);

        internal static void RegisterEvents()
        {
            Events.callTransportEvent += () =>
            {
#if true
                Game.DisplayNotification("Radio+ On transport called STP.");
#endif
                OnTransportCalled?.Invoke();
            };
        }

        internal static event StopThePedEventHandler OnTransportCalled;
    }
}
