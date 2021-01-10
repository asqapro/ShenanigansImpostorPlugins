using System.Collections.Generic;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;
using RolesManager;

namespace Roles
{
    public enum RoleTypes
    {
        Medium
    }

    public enum ListenerTypes
    {
        OnChat
    }

    public abstract class Role
    {
        protected IInnerPlayerControl _player;
        public ICollection<ListenerTypes> _listeners {get; set;}

        public Role(IInnerPlayerControl player)
        {
            _player = player;
        }

        public abstract RoleTypes GetRoleType();
        public abstract int GetTotalAllowed();

        public virtual void HandleChat(IPlayerChatEvent e)
        {
        }
    }
}