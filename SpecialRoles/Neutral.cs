using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;

namespace Roles.Neutral
{
    public class Jester : Role
    {
        public Jester(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnChat);
            TotalAllowed = 1;
            RoleType = RoleTypes.Jester;
        }

        public override async void HandleExile(IPlayerExileEvent e)
        {
            var playerName = _player.PlayerInfo.PlayerName;
            if (e.PlayerControl.PlayerInfo.PlayerName == playerName)
            {
                await e.PlayerControl.SendChatAsync("The jester has won!");
                await e.PlayerControl.SetNameAsync($"{playerName} (Jester)");
            }
        }
    }
}