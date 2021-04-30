using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;

namespace RadioPlus
{
    internal static class RadioPlusHandler
    {
        private static readonly InitializationFile Configuration = new InitializationFile("plugins\\LSPDFR\\RadioPlus.ini");

        private static bool enableRandomChat;
        private static bool enablePursuitChat;
        private static bool enablePursuitStatus;

        private static int randomChatPrecentage;
        private static int randomChatInterval;

        private static bool isInPursuit;
        private static LHandle pursuit;

        internal static void InitConfig()
        {
            enableRandomChat = Configuration.Read("Main", "EnableRandomChat", true);
            enablePursuitChat = Configuration.Read("Main", "EnablePursuitChat", true);
            enablePursuitStatus = Configuration.Read("Main", "EnablePursuitStatus", false);
            randomChatPrecentage = Configuration.Read("RandomChat", "ChatPrecentage", 55);
            randomChatInterval = Configuration.Read("RandomChat", "ChatInterval", 30);
        }

        internal static void InitThreads()
        {
            Game.LogTrivial("Radio+: Initializing Threads");
            Game.LogTrivial("Radio+: Starting Thread - RandomChatManager");
            GameFiber.StartNew(RandomChatManagerThread);
            Game.LogTrivial("Radio+: Starting Thread - StopThePedManager");
            GameFiber.StartNew(HijacksHandler.StopThePedManagerThread);
            Game.LogTrivial("Radio+: Starting Thread - PursuitUpdateManager");
            GameFiber.StartNew(PursuitUpdateManagerThread);
            Game.LogTrivial("Radio+: Starting Thread - PursuitStatusManager");
            GameFiber.StartNew(PursuitStatusManagerThread, "Radio+ Pursuit Status Manager");

            Events.OnPursuitStarted += Events_OnPursuitStarted;
            Events.OnPursuitEnded += Events_OnPursuitEnded;
        }

        private static void PursuitStatusManagerThread()
        {
            Game.LogTrivial("Radio+: Successfully started thread PursuitStatusManager");

            bool previouslyOnFoot = false;

            while (enablePursuitStatus)
            {
                GameFiber.Yield();
                if (!isInPursuit || Game.IsPaused) continue;

                GameFiber.Sleep(10);

                if (!isInPursuit || pursuit == null || !Functions.IsPursuitStillRunning(pursuit))
                {
                    continue;
                }

                try
                {
                    var peds = Functions.GetPursuitPeds(pursuit);

                    if (peds == null || peds.Length == 0)
                    {
                        continue;
                    }

                    if (!previouslyOnFoot && peds[0].IsOnFoot)
                    {
                        previouslyOnFoot = true;
                        Functions.PlayScannerAudio("ATTENTION_ALL_UNITS TARGET_PREFIX ON_FOOT");
                        continue;
                    }

                    if (peds.Length == 0)
                    {
                        continue;
                    }

                    if (previouslyOnFoot && peds[0].IsInAnyVehicle(false))
                    {
                        previouslyOnFoot = false;
                        Functions.PlayScannerAudio($"ATTENTION_ALL_UNITS TARGET_PREFIX IN_A {peds[0].CurrentVehicle.Model.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Game.LogTrivial("Radio+: Exception caught in PRS");
                    Game.LogTrivial("Radio+ EXC - " + ex.ToString());
                }
            }
        }

        private static void Events_OnPursuitEnded(LHandle handle)
        {
            Game.LogTrivial("Radio+: Detect pursuit ended");
            pursuit = null;
            isInPursuit = false;
        }

        private static void Events_OnPursuitStarted(LHandle handle)
        {
            Game.LogTrivial("Radio+: Detect pursuit started");
            pursuit = handle;
            isInPursuit = true;
        }

        private static void PursuitUpdateManagerThread()
        {
            Game.LogTrivial("Radio+: Successfully started thread PursuitUpdateManager");

            while (enablePursuitChat)
            {
                GameFiber.Sleep(35000);

                if (!isInPursuit || Game.IsPaused) continue;
                if (pursuit == null || !Functions.IsPursuitStillRunning(pursuit))
                {
                    Game.LogTrivial("Radio+: Pursuit was null.");
#if DEBUG
                    Game.DisplayHelp("Radio+ Pursuit Nullify");
#endif
                    pursuit = null;
                    isInPursuit = false;
                    continue;
                }

                GameFiber.Sleep(10);

                try
                {
                    var peds = Functions.GetPursuitPeds(pursuit);

                    if (peds == null || peds.Length == 0 || !peds[0])
                    {
                        Game.LogTrivial("Radio+: Pursuit don't have ped, aborting");
#if DEBUG
                        Game.DisplayHelp("Radio+ Pursuit Ped Nullify");
#endif
                        pursuit = null;
                        isInPursuit = false;
                        continue;
                    }

                    switch (MathHelper.GetRandomInteger(3))
                    {
                        default:
                            Game.LogTrivialDebug("Radio+: Update - 1");
#if DEBUG
                            Game.DisplayHelp("Radio+ Update - 1");
#endif
                            Functions.PlayScannerAudioUsingPosition($"ATTENTION_ALL_UNITS SUSPECT_HEADING_RP {GetHeadingString(peds[0].Heading)} IN_OR_ON_POSITION", peds[0].Position);
                            break;
                        case 1 when peds[0].IsInAnyVehicle(false):
                            Game.LogTrivialDebug("Radio+: Update - 2");
#if DEBUG
                            Game.DisplayHelp("Radio+ Update - 2");
#endif
                            Functions.PlayScannerAudio($"ATTENTION_ALL_UNITS TARGET_PREFIX {GetSpeedString(MathHelper.ConvertMetersPerSecondToMilesPerHour(peds[0].CurrentVehicle.Speed))}");
                            break;
                        case 1 when peds[0].IsOnFoot:
                            Game.LogTrivialDebug("Radio+ Update - 3");
#if DEBUG
                            Game.DisplayHelp("Radio+ Update - 3");
#endif
                            Functions.PlayScannerAudioUsingPosition($"ATTENTION_ALL_UNITS SUSPECT_HEADING_RP {GetHeadingString(peds[0].Heading)} IN_OR_ON_POSITION ON_FOOT", peds[0].Position);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Game.LogTrivial(ex.ToString());
                }
            }
        }

        internal static string GetSpeedString(float speed)
        {
            if (speed > 100f)
            {
                return "DOING_OVER_100_MILES";
            }
            else if (speed == 100f)
            {
                return "DOING_100_MILES";
            }
            else if (speed < 100f && speed >= 90f)
            {
                return "DOING_90_MILES";
            }
            else if (speed < 90f && speed >= 80f)
            {
                return "DOING_80_MILES";
            }
            else if (speed < 80f && speed >= 70f)
            {
                return "DOING_70_MILES";
            }
            else if (speed < 70f && speed >= 60f)
            {
                return "DOING_60_MILES";
            }
            else if (speed < 60f && speed >= 50f)
            {
                return "DOING_50_MILES";
            }
            else
            {
                return "DOING_40_MILES";
            }
        }

        internal static string GetHeadingString(float heading)
        {
            if ((heading > 270f && heading <= 180f) || heading < 90f)
            {
                return "HEADING_NORTH";
            }
            else if (heading >= 90f && heading < 180f)
            {
                return "HEADING_WEST";
            }
            else if (heading >= 180f && heading < 270f)
            {
                return "HEADING_SOUTH";
            }
            else if (heading >= 270f && heading < 360f)
            {
                return "HEADING_EAST";
            }
            else
            {
                return "HEADING_NORTH";
            }
        }

        internal static void RandomChatManagerThread()
        {
            Game.LogTrivial("Radio+: Successfully started thread RandomChatManager");
            int sleep = randomChatInterval * 1000;

            while (enableRandomChat)
            {
                GameFiber.Sleep(sleep);

                if (MathHelper.GetRandomInteger(100) <= randomChatPrecentage && !isInPursuit)
                {
                    Functions.PlayScannerAudio("RANDOMCHAT", true);
                }
            }
        }
    }
}
