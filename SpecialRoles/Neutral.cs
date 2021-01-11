using System.Threading.Tasks;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;

namespace Roles.Neutral
{
    public class Jester : Role
    {
        public Jester(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnPlayerExile);
            TotalAllowed = 1;
            RoleType = RoleTypes.Jester;
        }

        public override async ValueTask HandlePlayerExile(IPlayerExileEvent e)
        {
            var playerName = _player.PlayerInfo.PlayerName;
            if (e.PlayerControl.PlayerInfo.PlayerName == playerName)
            {
                await e.PlayerControl.SetNameAsync($"{playerName} (Jester)");
                await e.PlayerControl.SendChatAsync("The jester has won!");
            }
        }
    }
}