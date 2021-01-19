using System;
using System.Collections.Generic;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Net.Inner.Objects;
using Roles;
using Roles.Crew;
using Roles.Evil;
using Roles.Neutral;

namespace Managers.Roles
{
    public interface IRolesManager
    {
        void RegisterRole(IInnerPlayerControl _player, RoleTypes playerRole);
        void HandleEvent(IPlayerChatEvent e);
        void HandleEvent(IPlayerExileEvent e);
        void HandleEvent(IPlayerVotedEvent e);
        void HandleEvent(IMeetingStartedEvent e);
        void HandleEvent(IMeetingEndedEvent e);
        void HandleEvent(IPlayerMurderEvent e);
    }

    public class RolesManager : IRolesManager
    {
        ICollection<InnerPlayerControlRole> PlayerRoles;

        public RolesManager()
        {
            PlayerRoles = new List<InnerPlayerControlRole>();
        }

        public async void RegisterRole(IInnerPlayerControl parentPlayerControl, RoleTypes playerRoleType)
        {
            String roleMessage = "";
            InnerPlayerControlRole role = new Crew(parentPlayerControl);
            switch (playerRoleType)
            {
                case RoleTypes.Crew:
                    role = new Crew(parentPlayerControl);
                    roleMessage = "You are a crewmate. Finish your tasks to win";
                    break;
                case RoleTypes.Impostor:
                    role = new Impersonator(parentPlayerControl);
                    roleMessage = "You are an impostor. Kill the crew to win";
                    break; 
                case RoleTypes.Hitman:
                    role = new Hitman(parentPlayerControl);
                    roleMessage = "You are a hitman. \nYou may silently kill 1 player using /silentkill";
                    break;
                case RoleTypes.Jester:
                    role = new Jester(parentPlayerControl);
                    roleMessage = "You are a jester. \nTrick the crew into voting you out to win";
                    break;
                case RoleTypes.Medium:
                    role = new Medium(parentPlayerControl);
                    roleMessage = "You are a medium. \nYou can hear the dead speak";
                    break;
                case RoleTypes.Sheriff:
                    role = new Sheriff(parentPlayerControl);
                    roleMessage = "You are a sheriff. \nYou may shoot 1 player that you suspect is an impostor, but if you guess wrong, you will also die";
                    break;
                case RoleTypes.VoodooLady:
                    role = new VoodooLady(parentPlayerControl);
                    roleMessage = "You are a voodoo lady. \nPick a kill word and target using /setkillword";
                    break;
                case RoleTypes.Cop:
                    role = new Cop(parentPlayerControl);
                    roleMessage = "You are a cop. \nYou can find impostors by using /investigate on players";
                    break;
                case RoleTypes.InsaneCop:
                    role = new InsaneCop(parentPlayerControl);
                    roleMessage = "You are a cop. \nYou can find impostors by using /investigate on players";
                    break;
                case RoleTypes.ConfusedCop:
                    role = new ConfusedCop(parentPlayerControl);
                    roleMessage = "You are a cop. \nYou can find impostors by using /investigate on players";
                    break;
                case RoleTypes.Oracle:
                    role = new Oracle(parentPlayerControl);
                    roleMessage = "You are an oracle. \nWhen you die, you will reveal the role of the last player you picked using /reveal";
                    break;
                case RoleTypes.Lightkeeper:
                    role = new Lightkeeper(parentPlayerControl);
                    roleMessage = "You are a lightkeeper. \nWhen you die, you will cast the next meeting into darkness";
                    break;
                default:
                    break;
            }

            PlayerRoles.Add(role);
            
            var currentName = parentPlayerControl.PlayerInfo.PlayerName;
            var currentColor = parentPlayerControl.PlayerInfo.ColorId;
            await parentPlayerControl.SetNameAsync("Server");
            await parentPlayerControl.SetColorAsync(Impostor.Api.Innersloth.Customization.ColorType.White);

            await parentPlayerControl.SendChatToPlayerAsync(roleMessage, parentPlayerControl);

            await parentPlayerControl.SetNameAsync(currentName);
            await parentPlayerControl.SetColorAsync(currentColor);
        }

        public async void HandleEvent(IPlayerChatEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player._listeners.Contains(ListenerTypes.OnPlayerChat))
                {
                    HandlerAction handlerResult = await (player.HandlePlayerChat(e));
                    if (handlerResult.Action == ResultTypes.KilledPlayer)
                    {

                    }
                }
            }
        }

        public async void HandleEvent(IPlayerExileEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player._listeners.Contains(ListenerTypes.OnPlayerExile))
                {
                    HandlerAction handlerResult = await (player.HandlePlayerExile(e));
                    if (handlerResult.Action == ResultTypes.KilledPlayer)
                    {
                        
                    }
                }
            }
        }

        public async void HandleEvent(IPlayerVotedEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player._listeners.Contains(ListenerTypes.OnPlayerVoted))
                {
                    HandlerAction handlerResult = await (player.HandlePlayerVote(e));
                    if (handlerResult.Action == ResultTypes.KilledPlayer)
                    {
                        
                    }
                }
            }
        }

        public async void HandleEvent(IMeetingStartedEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player._listeners.Contains(ListenerTypes.OnMeetingStarted))
                {
                    HandlerAction handlerResult = await (player.HandleMeetingStart(e));
                    if (handlerResult.Action == ResultTypes.KilledPlayer)
                    {
                        
                    }
                }
            }
        }

        public async void HandleEvent(IMeetingEndedEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player._listeners.Contains(ListenerTypes.OnMeetingEnded))
                {
                    HandlerAction handlerResult = await (player.HandleMeetingEnd(e));
                    if (handlerResult.Action == ResultTypes.KilledPlayer)
                    {
                        
                    }
                }
            }
        }

        public async void HandleEvent(IPlayerMurderEvent e)
        {
            foreach(var player in PlayerRoles)
            {
                if (player._listeners.Contains(ListenerTypes.OnPlayerMurder))
                {
                    HandlerAction handlerResult = await (player.HandlePlayerMurder(e));
                    if (handlerResult.Action == ResultTypes.KilledPlayer)
                    {
                        
                    }
                }
            }
        }
    }
}