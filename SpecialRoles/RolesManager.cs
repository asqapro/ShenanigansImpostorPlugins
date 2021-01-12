using System;
using System.Collections.Generic;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Roles;

namespace RolesManager
{
    public interface IManager
    {
        void RegisterRole(GameCode code, Role toRegister);
        void HandlePlayerChat(GameCode code, IPlayerChatEvent e);
        void HandlePlayerExile(GameCode code, IPlayerExileEvent e);
        void CleanUpRoles(GameCode code);
    }

    public class Manager : IManager
    {
        Dictionary<GameCode, HashSet<Role>> RegisteredRoles;

        public Manager()
        {
            RegisteredRoles = new Dictionary<GameCode, HashSet<Role>>();
        }

        public void RegisterRole(GameCode code, Role toRegister)
        {
            if (!RegisteredRoles.ContainsKey(code))
            {
                RegisteredRoles[code] = new HashSet<Role>();
            }
            RegisteredRoles[code].Add(toRegister);
        }

        public void CleanUpRoles(GameCode code)
        {
            RegisteredRoles.Remove(code);
        }

        public async void HandlePlayerChat(GameCode code, IPlayerChatEvent e)
        {
            if (!RegisteredRoles.ContainsKey(code))
            {
                return;
            }
            foreach (var registered in RegisteredRoles[code])
            {
                if (registered._listeners.Contains(ListenerTypes.OnPlayerChat))
                {
                    await registered.HandlePlayerChat(e);
                }
            }
        }

        public async void HandlePlayerExile(GameCode code, IPlayerExileEvent e)
        {
            if (!RegisteredRoles.ContainsKey(code))
            {
                return;
            }
            foreach (var registered in RegisteredRoles[code])
            {
                if (registered._listeners.Contains(ListenerTypes.OnPlayerExile))
                {
                    await registered.HandlePlayerExile(e);
                }
            }
        }
    }
}