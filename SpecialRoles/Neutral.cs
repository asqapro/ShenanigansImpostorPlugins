using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Net.Inner.Objects;

namespace Roles.Neutral
{
    public class Jester : Role
    {
        public new static int TotalAllowed = 1;
        private int votedJesterCount;
        private int votedSkipCount;
        private Dictionary<String, int> votedOtherCount;

        public Jester(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnPlayerVoted);
            _listeners.Add(ListenerTypes.OnMeetingEnded);
            RoleType = RoleTypes.Jester;
            votedJesterCount = 0;
            votedSkipCount = 0;
            votedOtherCount = new Dictionary<string, int>();
        }

        public override ValueTask<Tuple<String, ResultTypes>> HandlePlayerVote(IPlayerVotedEvent e)
        {
            if (e.VotedFor.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
            {
                votedJesterCount++;
            }
            else if(e.VoteType == VoteType.Skip)
            {
                votedSkipCount++;
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
            return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
        }

        public override async ValueTask<Tuple<String, ResultTypes>> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            var jesterVoted = true;
            if (votedJesterCount > votedSkipCount)
            {
                foreach (var voteCount in votedOtherCount)
                {
                    if (voteCount.Value > votedJesterCount)
                    {
                        jesterVoted = false;
                    }
                    votedOtherCount[voteCount.Key] = 0;
                }
            }
            if (votedSkipCount > votedJesterCount)
            {
                jesterVoted = false;
            }
            if (jesterVoted)
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
            return new Tuple<String, ResultTypes>("", ResultTypes.NoAction);
        }
    }
}