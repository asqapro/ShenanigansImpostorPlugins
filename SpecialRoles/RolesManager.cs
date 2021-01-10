using System;
using System.Collections.Generic;
using Impostor.Api.Events.Player;
using Roles;

namespace RolesManager
{
    public interface IManager
    {
        bool RegisterRole(Role toRegister);
    }

    public class Manager : IManager
    {
        ICollection<Role> RegisteredRoles;
        IDictionary<RoleTypes, int> RegisteredRolesCount;

        public Manager()
        {
            RegisteredRoles = new List<Role>();
            RegisteredRolesCount = new Dictionary<RoleTypes, int>();
            foreach (RoleTypes RoleType in Enum.GetValues(typeof(RoleTypes)))
            {
                RegisteredRolesCount[RoleType] = 0;
            }
        }
        public bool RegisterRole(Role toRegister)
        {
            RoleTypes type = toRegister.GetRoleType();
            if (RegisteredRolesCount[type] <= toRegister.GetTotalAllowed())
            {
                RegisteredRoles.Add(toRegister);
                RegisteredRolesCount[type]++;
                return true;
            }
            return false;
        }

        public void HandleChat(IPlayerChatEvent e)
        {
            foreach (var registered in RegisteredRoles)
            {
                registered.HandleChat(e);
            }
        }
    }
}