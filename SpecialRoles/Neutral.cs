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
        private int votedJesterCount { get; set; }
        private int votedSkipCount { get; set; }
        private bool meetingActive { get; set; }
        private Dictionary<int, int> votedOtherCount { get; set; }

        public Jester(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerVoted);
            _listeners.Add(ListenerTypes.OnMeetingStarted);
            _listeners.Add(ListenerTypes.OnMeetingEnded);
            RoleType = RoleTypes.Jester;
            votedJesterCount = 0;
            votedSkipCount = 0;
            meetingActive = false;
            votedOtherCount = new Dictionary<int, int>();
        }

        public override ValueTask<HandlerAction> HandlePlayerVote(IPlayerVotedEvent e)
        {
            if (!meetingActive)
            {
                return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
            }
            if(e.VoteType == VoteType.Skip)
            {
                votedSkipCount++;
            }
            else if (e.VotedFor.OwnerId == _player.OwnerId)
            {
                votedJesterCount++;
            }
            else
            {
                var votedFor = e.VotedFor.OwnerId;
                if (!votedOtherCount.ContainsKey(votedFor))
                {
                    votedOtherCount[votedFor] = 0;
                }
                votedOtherCount[votedFor]++;
            }
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        public override ValueTask<HandlerAction> HandleMeetingStart(IMeetingStartedEvent e)
        {
            meetingActive = true;
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
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
                    if (player.Client.Id != _player.OwnerId)
                    {
                        var currentName = player.Character.PlayerInfo.PlayerName;
                        var currentColor = player.Character.PlayerInfo.ColorId;

                        await player.Character.SetNameAsync($"{playerName} (Jester)");
                        await player.Character.SetColorAsync(playerColor);

                        await player.Character.SendChatAsync("The jester has won!");

                        await player.Character.SetNameAsync(currentName);
                        await player.Character.SetColorAsync(currentColor);
                        break;
                    }
                }
            }

            votedJesterCount = 0;
            votedSkipCount = 0;
            return new HandlerAction(ResultTypes.NoAction);
        }
    }
}