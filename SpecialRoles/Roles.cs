using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Innersloth;

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
        Cop,
        InsaneCop,
        ConfusedCop,
        Oracle,
        Lightkeeper,
    }

    public enum ListenerTypes
    {
        OnPlayerChat,
        OnPlayerExile,
        OnPlayerVoted,
        OnMeetingStarted,
        OnMeetingEnded,
        OnPlayerMurder
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
        protected MemoryStream preEditOptionsStream;
        protected BinaryWriter preEditOptionsWriter;

        public Role(IInnerPlayerControl player)
        {
            _player = player;
            _listeners = new HashSet<ListenerTypes>();
        }

        protected void saveSettings(IGameEvent e)
        {
            preEditOptionsStream = new MemoryStream();
            preEditOptionsWriter = new BinaryWriter(preEditOptionsStream);
            e.Game.Options.Serialize(preEditOptionsWriter, GameOptionsData.LatestVersion);
        }

        protected void loadSettings(IGameEvent e)
        {
            if (preEditOptionsStream == null)
            {
                return;
            }
            byte[] gameOptions = preEditOptionsStream.GetBuffer();
            var memory = new ReadOnlyMemory<byte>(gameOptions);

            e.Game.Options.Deserialize(memory);
            e.Game.SyncSettingsAsync();
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

        public virtual ValueTask<Tuple<String, ResultTypes>> HandleMeetingStart(IMeetingStartedEvent e)
        {
            return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
        }

        public virtual ValueTask<Tuple<String, ResultTypes>> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
        }

        public virtual ValueTask<Tuple<String, ResultTypes>> HandlePlayerMurder(IPlayerMurderEvent e)
        {
            return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
        }
    }
}