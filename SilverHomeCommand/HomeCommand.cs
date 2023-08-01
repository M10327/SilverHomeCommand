using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SilverHomeCommand
{
    public class HomeCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Home";

        public string Help => "Teleports the player home";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "home" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer p = caller as UnturnedPlayer;
            ulong id = (ulong)p.CSteamID;
            Rocket.Core.Logging.Logger.Log($"Attempting to send {p.DisplayName} home");
            if (SilverHomeCommand.Instance.ActiveHomes.Contains((ulong)p.CSteamID))
            {
                UnturnedChat.Say(caller, SilverHomeCommand.Instance.Translate("AlreadyTrying"), SilverHomeCommand.Instance.MessageColor);
                return;
            }
            if (p.IsInVehicle)
            {
                UnturnedChat.Say(caller, SilverHomeCommand.Instance.Translate("FailedVehicle"), SilverHomeCommand.Instance.MessageColor);
                return;
            }
            if (!BarricadeManager.tryGetBed(p.CSteamID, out var point, out _))
            {
                UnturnedChat.Say(caller, SilverHomeCommand.Instance.Translate("NoBed"), SilverHomeCommand.Instance.MessageColor);
                return;
            }
            var cfg = SilverHomeCommand.cfg;
            if (cfg.HomeDelay < 1 || p.HasPermission(cfg.BypassDelayPermission))
            {
                GoHomeInstant(p, point);
            }
            else
            {
                StartGoHome(p, point, cfg.HomeDelay);
            }
        }

        public async void StartGoHome(UnturnedPlayer p, Vector3 point, int delay)
        {
            UnturnedChat.Say(p, SilverHomeCommand.Instance.Translate("StartingTeleport", delay.ToString()), SilverHomeCommand.Instance.MessageColor);
            SilverHomeCommand.Instance.ActiveHomes.Add((ulong)p.CSteamID);
            await Task.Delay(1000 * delay);
            if (SilverHomeCommand.Instance.ActiveHomes.Contains((ulong)p.CSteamID))
            {
                GoHomeInstant(p, point);
            }
        }

        public async void GoHomeInstant(UnturnedPlayer p, Vector3 point)
        {
            SilverHomeCommand.Instance.ActiveHomes.Remove((ulong)p.CSteamID);
            point.y += 0.5f;
            p.Teleport(point, 0);
            await Task.Delay(20);
            if (Vector3.Distance(p.Position, point) < 1)
            {
                UnturnedChat.Say(p, SilverHomeCommand.Instance.Translate("Succeed"), SilverHomeCommand.Instance.MessageColor);
                EffectManager.sendEffect(SilverHomeCommand.cfg.HomeSucceedEffect, SilverHomeCommand.cfg.EffectRadius, point);
            }
            else
            {
                UnturnedChat.Say(p, SilverHomeCommand.Instance.Translate("FailedNoSpace"), SilverHomeCommand.Instance.MessageColor);
            }
        }
    }
}
