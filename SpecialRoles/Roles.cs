using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
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
        VoodooLady,
        Cop
    }

    public enum ListenerTypes
    {
        OnPlayerChat,
        OnPlayerExile,
        OnPlayerVoted,
        OnMeetingEnded
    }

    public enum ResultTypes
    {
        NoAction,
        KilledPlayer
    }

    public abstract class Role
    {
        protected IInnerPlayerControl _player;
        public HashSet<ListenerTypes> _listeners {get; set;}
        public static int TotalAllowed {get; set;}
        public RoleTypes RoleType {get; set;}

        public Tuple<String, ResultTypes> HandlerResult;

        public Role(IInnerPlayerControl player)
        {
            _player = player;
            _listeners = new HashSet<ListenerTypes>();
        }

        public virtual ValueTask<Tuple<String, ResultTypes>> HandlePlayerChat(IPlayerChatEvent e)
        {
            return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
        }

        public virtual ValueTask<Tuple<String, ResultTypes>> HandlePlayerExile(IPlayerExileEvent e)
        {
            return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
        }

        public virtual ValueTask<Tuple<String, ResultTypes>> HandlePlayerVote(IPlayerVotedEvent e)
        {
            return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
        }

        public virtual ValueTask<Tuple<String, ResultTypes>> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
        }
    }
}