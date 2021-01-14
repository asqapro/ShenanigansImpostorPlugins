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
        Jester,
        Hitman,
        VoodooLady
    }

    public enum ListenerTypes
    {
        OnPlayerChat,
        OnPlayerExile,
        OnPlayerVoted
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

        public virtual ValueTask<bool> HandlePlayerChat(IPlayerChatEvent e)
        {
            return ValueTask.FromResult(false);
        }

        public virtual ValueTask<bool> HandlePlayerExile(IPlayerExileEvent e)
        {
            return ValueTask.FromResult(false);
        }

        public virtual ValueTask<bool> HandlePlayerVote(IPlayerVotedEvent e)
        {
            return ValueTask.FromResult(false);
        }
    }
}