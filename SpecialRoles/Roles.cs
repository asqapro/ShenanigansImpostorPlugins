using System.Collections.Generic;
using System.Threading.Tasks;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;

namespace Roles
{
    public enum RoleTypes
    {
        Crew,
        Impostor,
        Medium,
        Sheriff,
        Jester
    }

    public enum ListenerTypes
    {
        OnPlayerChat,
        OnPlayerExile,
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

        public async virtual ValueTask HandlePlayerChat(IPlayerChatEvent e)
        {
        }

        public async virtual ValueTask HandlePlayerExile(IPlayerExileEvent e)
        {
        }
    }
}