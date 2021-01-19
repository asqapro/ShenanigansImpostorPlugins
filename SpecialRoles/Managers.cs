using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner.Objects;
using Roles;
using Roles.Crew;
using Roles.Evil;
using Roles.Neutral;

namespace Managers.Roles
{
    public interface IRolesManager
    {
        void RegisterRole(IClientPlayer parentPlayer, RoleTypes playerRoleType);
        void HandleEvent(IPlayerChatEvent e);
        void HandleEvent(IPlayerExileEvent e);
        void HandleEvent(IPlayerVotedEvent e);
        void HandleEvent(IMeetingStartedEvent e);
        void HandleEvent(IMeetingEndedEvent e);
        void HandleEvent(IPlayerMurderEvent e);
        void HandleEvent(IPlayerMovementEvent e);
    }

    public class RolesManager : IRolesManager
    {
        Dictionary<int, InnerPlayerControlRole> PlayerRoles;

        public RolesManager()
        {
            PlayerRoles = new Dictionary<int, InnerPlayerControlRole>();
        }

        public async void RegisterRole(IClientPlayer parentPlayer, RoleTypes playerRoleType)
        {
            String roleMessage = "";
            InnerPlayerControlRole role;
            switch (playerRoleType)
            {
                case RoleTypes.Crew:
                    role = new Crew(parentPlayer.Character);
                    roleMessage = "You are a crewmate. Finish your tasks to win";
                    break;
                case RoleTypes.Impostor:
                    role = new Impersonator(parentPlayer.Character);
                    roleMessage = "You are an impostor. Kill the crew to win";
                    break; 
                case RoleTypes.Hitman:
                    role = new Hitman(parentPlayer.Character);
                    roleMessage = "You are a hitman. \nYou may silently kill 1 player using /silentkill";
                    break;
                case RoleTypes.Jester:
                    role = new Jester(parentPlayer.Character);
                    roleMessage = "You are a jester. \nTrick the crew into voting you out to win";
                    break;
                case RoleTypes.Medium:
                    role = new Medium(parentPlayer.Character);
                    roleMessage = "You are a medium. \nYou can hear the dead speak";
                    break;
                case RoleTypes.Sheriff:
                    role = new Sheriff(parentPlayer.Character);
                    roleMessage = "You are a sheriff. \nYou may shoot 1 player that you suspect is an impostor, but if you guess wrong, you will also die";
                    break;
                case RoleTypes.VoodooLady:
                    role = new VoodooLady(parentPlayer.Character);
                    roleMessage = "You are a voodoo lady. \nPick a kill word and target using /setkillword";
                    break;
                case RoleTypes.Cop:
                    role = new Cop(parentPlayer.Character);
                    roleMessage = "You are a cop. \nYou can find impostors by using /investigate on players";
                    break;
                case RoleTypes.InsaneCop:
                    role = new InsaneCop(parentPlayer.Character);
                    roleMessage = "You are a cop. \nYou can find impostors by using /investigate on players";
                    break;
                case RoleTypes.ConfusedCop:
                    role = new ConfusedCop(parentPlayer.Character);
                    roleMessage = "You are a cop. \nYou can find impostors by using /investigate on players";
                    break;
                case RoleTypes.Oracle:
                    role = new Oracle(parentPlayer.Character);
                    roleMessage = "You are an oracle. \nWhen you die, you will reveal the role of the last player you picked using /reveal";
                    break;
                case RoleTypes.Lightkeeper:
                    role = new Lightkeeper(parentPlayer.Character);
                    roleMessage = "You are a lightkeeper. \nWhen you die, you will cast the next meeting into darkness";
                    break;
                case RoleTypes.Doctor:
                    role = new Doctor(parentPlayer.Character);
                    roleMessage = "You are a doctor. \nYou may protect 1 player every round with /protect";
                    break;
                case RoleTypes.Arsonist:
                    role = new Arsonist(parentPlayer.Character);
                    roleMessage = "You are an arsonist. \nYou may douse 1 player a round by touching them, then ignite all doused players using /ignite";
                    break;
                default:
                    role = new Crew(parentPlayer.Character);
                    break;
            }

            PlayerRoles[parentPlayer.Client.Id] = role;
            
            var currentName = parentPlayer.Character.PlayerInfo.PlayerName;
            var currentColor = parentPlayer.Character.PlayerInfo.ColorId;
            await parentPlayer.Character.SetNameAsync("Server");
            await parentPlayer.Character.SetColorAsync(Impostor.Api.Innersloth.Customization.ColorType.White);

            await parentPlayer.Character.SendChatToPlayerAsync(roleMessage, parentPlayer.Character);

            await parentPlayer.Character.SetNameAsync(currentName);
            await parentPlayer.Character.SetColorAsync(currentColor);
        }

        private async ValueTask handleKillExile(InnerPlayerControlRole toKill)
        {
            if (toKill.PreventNextDeath)
            {
                await toKill.SendChatToPlayerAsync("You have been protected from death", toKill);
                toKill.PreventNextDeath = false;
                return;
            }
            else
            {
                await toKill.SetExiledAsync();
            }
        }

        private void handleProtectPlayer(InnerPlayerControlRole toProtect)
        {
            toProtect.PreventNextDeath = true;
        }

        private async ValueTask parseResult(HandlerAction handlerResult)
        {
            if (handlerResult.Action == ResultTypes.NoAction)
            {
                return;
            }
            if (handlerResult.AffectedPlayers == null)
            {
                return;
            }
            foreach (var player in handlerResult.AffectedPlayers)
            {
                switch (handlerResult.Action)
                {
                    case ResultTypes.KillExilePlayer:
                        await handleKillExile(PlayerRoles[player]);
                        break;
                    case ResultTypes.ProtectPlayer:
                        handleProtectPlayer(PlayerRoles[player]);
                        break;
                    default:
                        break;
                }
            }
        }

        public async void HandleEvent(IPlayerChatEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerChat))
                {
                    HandlerAction handlerResult = await player.Value.HandlePlayerChat(e);
                    await parseResult(handlerResult);
                }
            }
        }

        public async void HandleEvent(IPlayerExileEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerExile))
                {
                    HandlerAction handlerResult = await player.Value.HandlePlayerExile(e);
                    await parseResult(handlerResult);
                }
            }
        }

        public async void HandleEvent(IPlayerVotedEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerVoted))
                {
                    HandlerAction handlerResult = await player.Value.HandlePlayerVote(e);
                    await parseResult(handlerResult);
                }
            }
        }

        public async void HandleEvent(IMeetingStartedEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnMeetingStarted))
                {
                    HandlerAction handlerResult = await player.Value.HandleMeetingStart(e);
                    await parseResult(handlerResult);
                }
            }
        }

        public async void HandleEvent(IMeetingEndedEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnMeetingEnded))
                {
                    HandlerAction handlerResult = await player.Value.HandleMeetingEnd(e);
                    await parseResult(handlerResult);
                }
            }
        }

        public async void HandleEvent(IPlayerMurderEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerMurder))
                {
                    HandlerAction handlerResult = await player.Value.HandlePlayerMurder(e);
                    await parseResult(handlerResult);
                }
            }
        }

        public async void HandleEvent(IPlayerMovementEvent e)
        {
            if (e.PlayerControl == null || e.ClientPlayer == null)
            {
                return;
            }
            foreach(var player in PlayerRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerMovement))
                {
                    HandlerAction handlerResult = await player.Value.HandlePlayerMovement(e);
                    await parseResult(handlerResult);
                }
            }
        }
    }
}