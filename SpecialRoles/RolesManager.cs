using System;
using System.Collections.Generic;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;
using Roles;
using Roles.Crew;
using Roles.Evil;
using Roles.Neutral;

namespace RolesManager
{
    public interface IManager
    {
        void RegisterRole(IInnerPlayerControl _player, RoleTypes playerRole);
        void HandleEvent(IPlayerChatEvent e);
        void HandleEvent(IPlayerExileEvent e);
        void HandleEvent(IPlayerVotedEvent e);
    }

    public class Manager : IManager
    {
        Dictionary<String, Role> RegisteredRoles;

        public Manager()
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
                    //Change return type to keypair of player : action?
                    await (player.Value.HandlePlayerChat(e));
                }
            }
        }

        public async void HandleEvent(IPlayerExileEvent e)
        {
            foreach(KeyValuePair<String, Role> player in RegisteredRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerExile))
                {
                    await player.Value.HandlePlayerExile(e);
                }
            }
        }

        public async void HandleEvent(IPlayerVotedEvent e)
        {
            foreach(KeyValuePair<String, Role> player in RegisteredRoles)
            {
                if (player.Value._listeners.Contains(ListenerTypes.OnPlayerExile))
                {
                    await player.Value.HandlePlayerVote(e);
                }
            }
        }
    }
}