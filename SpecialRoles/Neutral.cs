using System.Threading.Tasks;
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
            TotalAllowed = 1;
            RoleType = RoleTypes.Jester;
        }

        public override async ValueTask<bool> HandlePlayerExile(IPlayerExileEvent e)
        {
            var playerName = _player.PlayerInfo.PlayerName;
            var playerColor = _player.PlayerInfo.ColorId;
            if (e.PlayerControl.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
            {
                foreach (var player in e.Game.Players)
                {
                    if (player.Character != _player)
                    {
                        var currentName = player.Character.PlayerInfo.PlayerName;
                        var currentColor = player.Character.PlayerInfo.ColorId;
                        await player.Character.SetNameAsync($"{playerName} (Jester)");
                        await player.Character.SetColorAsync(currentColor);
                        await player.Character.SendChatToPlayerAsync("The jester has won!", player.Character);
                        await player.Character.SetNameAsync(currentName);
                        await player.Character.SetColorAsync(currentColor);
                    }
                }
                return true;
            }
            return false;
        }
    }
}