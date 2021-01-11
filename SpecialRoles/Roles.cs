using System.Collections.Generic;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;

namespace Roles
{
    public enum RoleTypes
    {
        Medium,
        Sheriff,
        Jester
    }

    public enum ListenerTypes
    {
        OnChat,
        OnExile
    }

    public abstract class Role
    {
        protected IInnerPlayerControl _player;
        public HashSet<ListenerTypes> _listeners {get; set;}
        public static int TotalAllowed {get; set;}
        public RoleTypes RoleType {get; set;}

        public Role(IInnerPlayerControl player)
        {
            _player = player;
            _listeners = new HashSet<ListenerTypes>();
        }

        public virtual void HandleChat(IPlayerChatEvent e)
        {
        }

        public virtual void HandleExile(IPlayerExileEvent e)
        {
        }
    }
}