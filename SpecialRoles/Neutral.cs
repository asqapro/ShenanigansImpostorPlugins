using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;

namespace Roles.Neutral
{
    public class Jester : Role
    {
        public new static int TotalAllowed = 1;

        public Jester(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnPlayerExile);
            RoleType = RoleTypes.Jester;
        }

        public override async ValueTask<bool> HandlePlayerExile(IPlayerExileEvent e)
        {
            var playerName = _player.PlayerInfo.PlayerName;
            var playerColor = _player.PlayerInfo.ColorId;
            if (e.PlayerControl.PlayerInfo.PlayerName == playerName)
            {
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
            return false;
        }
    }
}