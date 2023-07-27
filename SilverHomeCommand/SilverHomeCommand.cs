using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
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
    public class SilverHomeCommand : RocketPlugin<Config>
    {
        public static SilverHomeCommand Instance { get; private set; }
        public List<ulong> ActiveHomes { get; set; }
        public UnityEngine.Color MessageColor { get; set; }
        public static Config cfg { get; private set; }
        protected override void Load()
        {
            Instance = this;
            cfg = Configuration.Instance;
            MessageColor = (Color)UnturnedChat.GetColorFromHex(cfg.MessageColor);

            if (cfg.CancelOnHarm)
            {
                DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;
                UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
            }
                
            if (cfg.CancelOnMove)
                UnturnedPlayerEvents.OnPlayerUpdatePosition += UnturnedPlayerEvents_OnPlayerUpdatePosition;

            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerRevive += UnturnedPlayerEvents_OnPlayerRevive;

            ActiveHomes = new List<ulong>();
            Rocket.Core.Logging.Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded");
        }

        private void UnturnedPlayerEvents_OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, Steamworks.CSteamID murderer)
        {
            ulong id = (ulong)player.CSteamID;
            if (!ActiveHomes.Contains(id)) return;
            ActiveHomes.Remove(id);
            if (player != null)
                UnturnedChat.Say(player.CSteamID, Translate("CancelHurt"), MessageColor);
        }

        private void UnturnedPlayerEvents_OnPlayerRevive(UnturnedPlayer player, Vector3 position, byte angle)
        {
            bool atBed = false;
            if (BarricadeManager.tryGetBed(player.CSteamID, out var point, out _))
            {
                if (Vector3.Distance(point, player.Position) < 1)
                    atBed = true;
            }
            if (atBed && cfg.PlayEffectOnRespawnBed)
            {
                EffectManager.sendEffect(SilverHomeCommand.cfg.HomeSucceedEffect, SilverHomeCommand.cfg.EffectRadius, position);
            }
            else if (!atBed && cfg.PlayEffectOnRespawnNatural)
            {
                EffectManager.sendEffect(SilverHomeCommand.cfg.HomeSucceedEffect, SilverHomeCommand.cfg.EffectRadius, position);
            }
        }

        private void Events_OnPlayerDisconnected(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            if (ActiveHomes.Contains((ulong)player.CSteamID))
                ActiveHomes.Remove((ulong)player.CSteamID);
        }

        private void UnturnedPlayerEvents_OnPlayerUpdatePosition(Rocket.Unturned.Player.UnturnedPlayer player, UnityEngine.Vector3 position)
        {
            ulong id = (ulong)player.CSteamID;
            if (!ActiveHomes.Contains(id)) return;
            ActiveHomes.Remove(id);
            if (player != null)
                UnturnedChat.Say(player.CSteamID, Translate("CancelMoved"), MessageColor);
        }

        private void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if (!shouldAllow) return;
            if (parameters.player == null) return;
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(parameters.player);
            ulong id = (ulong)player.CSteamID;
            if (!ActiveHomes.Contains(id)) return;
            ActiveHomes.Remove(id);
            if (player != null)
                UnturnedChat.Say(player.CSteamID, Translate("CancelHurt"), MessageColor);
        }

        protected override void Unload()
        {
            DamageTool.damagePlayerRequested -= DamageTool_damagePlayerRequested;
            UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerUpdatePosition -= UnturnedPlayerEvents_OnPlayerUpdatePosition;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerRevive -= UnturnedPlayerEvents_OnPlayerRevive;
            Rocket.Core.Logging.Logger.Log($"{Name} {Assembly.GetName().Version} has been unloaded");
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "StartingTeleport", "Teleporting home in {0} seconds" },
            { "CancelMoved", "You moved, teleportation home cancelled." },
            { "CancelHurt", "You were hurt, teleportation home cancelled." },
            { "FailedVehicle", "You are in a vehicle. Unable to teleport home." },
            { "FailedNoSpace", "There was no space to teleport home." },
            { "NoBed", "You do not have a claimed bed." },
            { "AlreadyTrying", "You are already trying to go home." },
            { "Succeed", "Teleported you home" }
        };
    }
}
