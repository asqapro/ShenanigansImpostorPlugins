using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Net.Inner.Objects;

namespace Roles.Neutral
{
    public class Jester : InnerPlayerControlRole
    {
        public new static int TotalAllowed = 1;
        private int votedJesterCount;
        private int votedSkipCount;
        private bool meetingActive;
        private Dictionary<String, int> votedOtherCount;

        public Jester(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerVoted);
            _listeners.Add(ListenerTypes.OnMeetingStarted);
            _listeners.Add(ListenerTypes.OnMeetingEnded);
            RoleType = RoleTypes.Jester;
            votedJesterCount = 0;
            votedSkipCount = 0;
            meetingActive = false;
            votedOtherCount = new Dictionary<string, int>();
        }

        public override ValueTask<HandlerAction> HandlePlayerVote(IPlayerVotedEvent e)
        {
            if (!meetingActive)
            {
                return ValueTask.FromResult(new HandlerAction());
                //return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
            }
            if(e.VoteType == VoteType.Skip)
            {
                votedSkipCount++;
            }
            else if (e.VotedFor.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
            {
                votedJesterCount++;
            }
            else
            {
                var votedName = e.VotedFor.PlayerInfo.PlayerName;
                if (!votedOtherCount.ContainsKey(votedName))
                {
                    votedOtherCount[votedName] = 0;
                }
                votedOtherCount[votedName]++;
            }
            return ValueTask.FromResult(new HandlerAction());
            //return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
        }

        public override ValueTask<HandlerAction> HandleMeetingStart(IMeetingStartedEvent e)
        {
            meetingActive = true;
            return ValueTask.FromResult(new HandlerAction());
            //return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));

        }

        public override async ValueTask<HandlerAction> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            meetingActive = false;

            var votedJester = true;
            if (votedJesterCount > votedSkipCount)
            {
                foreach (var voteCount in votedOtherCount)
                {
                    if (voteCount.Value >= votedJesterCount)
                    {
                        votedJester = false;
                    }
                    votedOtherCount[voteCount.Key] = 0;
                }
            }
            if (votedSkipCount >= votedJesterCount)
            {
                votedJester = false;
            }
            if (votedJester)
            {
                var playerName = _player.PlayerInfo.PlayerName;
                var playerColor = _player.PlayerInfo.ColorId;
                foreach (var player in e.Game.Players)
                {
                    if (player.Character.PlayerInfo.PlayerName != playerName)
                    {
                        var currentName = player.Character.PlayerInfo.PlayerName;
                        var currentColor = player.Character.PlayerInfo.ColorId;
                        await player.Character.SetNameAsync($"{playerName} (Jester)");
                        await player.Character.SetColorAsync(playerColor);
                        await player.Character.SendChatToPlayerAsync("The jester has won!", player.Character);
                        await player.Character.SetNameAsync(currentName);
                        await player.Character.SetColorAsync(currentColor);
                    }
                }
            }

            votedJesterCount = 0;
            votedSkipCount = 0;
            return new HandlerAction();
            //return new Tuple<String, ResultTypes>("", ResultTypes.NoAction);
        }
    }
}