using System;
using System.Collections.Generic;
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
        Dictionary<String, Role> RegisteredRoles;

        public RolesManager()
        {
            RegisteredRoles = new Dictionary<String, Role>();
        }

        public async void RegisterRole(IInnerPlayerControl player, RoleTypes playerRole)
        {
            String roleMessage = "";
            switch (playerRole)
            {
                case RoleTypes.Hitman:
                    RegisteredRoles[player.PlayerInfo.PlayerName] = new Hitman(player);
                    roleMessage = "You are a hitman. \nYou may silenty kill 1 player using /silentkill";
                    break;
                case RoleTypes.Jester:
                    RegisteredRoles[player.PlayerInfo.PlayerName] = new Jester(player);
                    roleMessage = "You are a jester. \nTrick the crew into voting you out to win";
                    break;
                case RoleTypes.Medium:
                    RegisteredRoles[player.PlayerInfo.PlayerName] = new Medium(player);
                    roleMessage = "You are a medium. \nYou can hear the dead speak";
                    break;
                case RoleTypes.Sheriff:
                    RegisteredRoles[player.PlayerInfo.PlayerName] = new Sheriff(player);
                    roleMessage = "You are a sheriff. \nYou may shoot 1 player that you suspect is an impostor, but if you guess wrong, you will also die";
                    break;
                case RoleTypes.VoodooLady:
                    RegisteredRoles[player.PlayerInfo.PlayerName] = new VoodooLady(player);
                    roleMessage = "You are a voodoo lady. \nPick a kill word and target using /setkillword";
                    break;
                case RoleTypes.Cop:
                    RegisteredRoles[player.PlayerInfo.PlayerName] = new Cop(player);
                    roleMessage = "You are a cop. \nYou can find impostors by using /investigate on players";
                    break;
                case RoleTypes.InsaneCop:
                    RegisteredRoles[player.PlayerInfo.PlayerName] = new InsaneCop(player);
                    roleMessage = "You are a cop. \nYou can find impostors by using /investigate on players";
                    break;
                case RoleTypes.ConfusedCop:
                    RegisteredRoles[player.PlayerInfo.PlayerName] = new ConfusedCop(player);
                    roleMessage = "You are a cop. \nYou can find impostors by using /investigate on players";
                    break;
                case RoleTypes.Oracle:
                    RegisteredRoles[player.PlayerInfo.PlayerName] = new Oracle(player);
                    roleMessage = "You are an oracle. \nWhen you die, you will reveal the role of the last player you picked using /reveal";
                    break;
                default:
                    break;
            }
            
            var currentName = player.PlayerInfo.PlayerName;
            var currentColor = player.PlayerInfo.ColorId;
            await player.SetNameAsync("Server");
            await player.SetColorAsync(Impostor.Api.Innersloth.Customization.ColorType.White);

            await player.SendChatToPlayerAsync(roleMessage, player);

            await player.SetNameAsync(currentName);
            await player.SetColorAsync(currentColor);
        }

        public async void HandleEvent(IPlayerChatEvent e)
        {
            foreach(KeyValuePair<String, Role> player in RegisteredRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerChat))
                {
                    Tuple<String, ResultTypes> handlerResult = await (player.Value.HandlePlayerChat(e));
                    if (handlerResult.Item2 == ResultTypes.KilledPlayer)
                    {

                    }
                }
            }
        }

        public async void HandleEvent(IPlayerExileEvent e)
        {
            foreach(KeyValuePair<String, Role> player in RegisteredRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerExile))
                {
                    Tuple<String, ResultTypes> handlerResult = await (player.Value.HandlePlayerExile(e));
                    if (handlerResult.Item2 == ResultTypes.KilledPlayer)
                    {
                        
                    }
                }
            }
        }

        public async void HandleEvent(IPlayerVotedEvent e)
        {
            foreach(KeyValuePair<String, Role> player in RegisteredRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerVoted))
                {
                    Tuple<String, ResultTypes> handlerResult = await (player.Value.HandlePlayerVote(e));
                    if (handlerResult.Item2 == ResultTypes.KilledPlayer)
                    {
                        
                    }
                }
            }
        }

        public async void HandleEvent(IMeetingStartedEvent e)
        {
            foreach(KeyValuePair<String, Role> player in RegisteredRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnMeetingStarted))
                {
                    Tuple<String, ResultTypes> handlerResult = await (player.Value.HandleMeetingStart(e));
                    if (handlerResult.Item2 == ResultTypes.KilledPlayer)
                    {
                        
                    }
                }
            }
        }

        public async void HandleEvent(IMeetingEndedEvent e)
        {
            foreach(KeyValuePair<String, Role> player in RegisteredRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnMeetingEnded))
                {
                    Tuple<String, ResultTypes> handlerResult = await (player.Value.HandleMeetingEnd(e));
                    if (handlerResult.Item2 == ResultTypes.KilledPlayer)
                    {
                        
                    }
                }
            }
        }

        public async void HandleEvent(IPlayerMurderEvent e)
        {
            foreach(KeyValuePair<String, Role> player in RegisteredRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerMurder))
                {
                    Tuple<String, ResultTypes> handlerResult = await (player.Value.HandlePlayerMurder(e));
                    if (handlerResult.Item2 == ResultTypes.KilledPlayer)
                    {
                        
                    }
                }
            }
        }
    }
}