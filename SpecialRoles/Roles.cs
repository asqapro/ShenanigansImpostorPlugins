using System.Collections.Generic;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;

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
        protected ICollection<ListenerTypes> _listeners;
        protected IInnerPlayerControl _player;

        public Role(IInnerPlayerControl player)
        {
            _player = player;
        }

        public abstract RoleTypes GetRoleType();
        public abstract ICollection<ListenerTypes> GetListenerTypes();
        public abstract int GetTotalAllowed();

        public virtual void HandleChat(IPlayerChatEvent e)
        {
        }
    }
}