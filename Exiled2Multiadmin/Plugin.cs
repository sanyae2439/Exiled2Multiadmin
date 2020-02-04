using System.Collections.Generic;
using System.Net;
using EXILED;
using Harmony;
using MEC;
using UnityEngine;

namespace Exiled2Multiadmin
{
    public class Exiled2Multiadmin : Plugin
    {
        public override string getName { get; } = "Exiled2Multiadmin";
        public static readonly string Version = "1.0a";
        private EventHandler eventHandler;
        private HarmonyInstance harmony;

        public override void OnEnable()
        {
            Plugin.Info("[OnEnable] Enabled.");

            ServerConsole.AddLog($"ServerMod - Version {EventPlugin.Version.Major}.{EventPlugin.Version.Minor}.{EventPlugin.Version.Patch}-EXILED");
            ServerConsole.AddLog($"Player connect: ");

            try
            {
                eventHandler = new EventHandler();
                Events.RoundRestartEvent += eventHandler.OnRoundRestart;
                Events.RemoteAdminCommandEvent += eventHandler.OnRACommand;

                harmony = HarmonyInstance.Create("com.sanyae2439." + this.getName);
                harmony.PatchAll();
            }
            catch(System.Exception e)
            {
                Plugin.Error($"[OnEnable] Error:{e}");
            }
        }
        public override void OnDisable() 
        {
            harmony.UnpatchAll();

            Events.RoundRestartEvent -= eventHandler.OnRoundRestart;
            Events.RemoteAdminCommandEvent -= eventHandler.OnRACommand;
            eventHandler = null;

            Plugin.Info("[OnDisable] Disabled.");
        }
        public override void OnReload()
        {
            Plugin.Info("[OnReload] Reloaded.");
        }
    }

    public class EventHandler
    {
        public void OnRoundRestart()
        {
            ServerConsole.AddLog("Round restarting");
        }

        public void OnRACommand(ref RACommandEvent ev)
        {
            if(ev.Command.ToUpper() == "RECONNECTRS")
            {
                ev.Allow = false;
                PlayerManager.localPlayer.GetComponent<PlayerStats>()?.Roundrestart();
                Timing.RunCoroutine(this._Quit(), Segment.Update);
                ev.Sender.RaReply("Exiled2Multiadmin#RECONNECTRS...", true,true,string.Empty);
            }
        }

        private IEnumerator<float> _Quit()
        {
            yield return Timing.WaitForSeconds(1f);
            Application.Quit();
            yield break;
        }
    }

    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerConnect))]
    public class ConnectPatch
    {
        public static void Prefix()
        {
            ServerConsole.AddLog($"Player connect: ");
        }
    }

    [HarmonyPatch(typeof(CustomNetworkManager),nameof(CustomNetworkManager.OnServerDisconnect))]
    public class DisconnectPatch
    {
        public static void Prefix()
        {
            ServerConsole.AddLog("Player disconnect: ");
        }
    }
}
