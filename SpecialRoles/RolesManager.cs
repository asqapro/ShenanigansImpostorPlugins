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
        void HandleChat(GameCode code, IPlayerChatEvent e);
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
            RegisteredRoles[code].Add(toRegister);
        }

        public void CleanUpRoles(GameCode code)
        {
            RegisteredRoles.Remove(code);
        }

        public void HandleChat(GameCode code, IPlayerChatEvent e)
        {
            foreach(KeyValuePair<GameCode, HashSet<Role>> entry in RegisteredRoles)
            {
                if (entry.Key == code)
                {
                    foreach (var registered in entry.Value)
                    {
                        if (registered._listeners.Contains(ListenerTypes.OnChat))
                        {
                            registered.HandleChat(e);
                        }
                    }
                }
            }
        }
    }
}