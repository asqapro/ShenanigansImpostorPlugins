using System;
using System.Threading.Tasks;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;


namespace Roles.Crew
{
    public class Medium : Role
    {
        public Medium(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnChat);
            TotalAllowed = 2;
            RoleType = RoleTypes.Medium;
        }

        private async ValueTask hearDead(IInnerPlayerControl deadSender, IInnerPlayerControl aliveSender, IInnerPlayerControl mediumReceiver, String message)
        {
            var deadName = deadSender.PlayerInfo.PlayerName;
            var deadColor = deadSender.PlayerInfo.ColorId;
            var currentName = aliveSender.PlayerInfo.PlayerName;
            var currentColor = aliveSender.PlayerInfo.ColorId;

            await aliveSender.SetNameAsync($"{deadName} (dead)");
            await aliveSender.SetColorAsync(deadColor);

            await aliveSender.SendChatToPlayerAsync($"[0000ffff]{message}[]", mediumReceiver);

            await aliveSender.SetNameAsync(currentName);
            await aliveSender.SetColorAsync(currentColor);
        }

        public override async void HandleChat(IPlayerChatEvent e)
        {
            if (e.PlayerControl.PlayerInfo.IsDead)
            {
                foreach (var player in e.Game.Players)
                {
                    if (!player.Character.PlayerInfo.IsDead && player.Character.PlayerInfo.PlayerName != _player.PlayerInfo.PlayerName)
                    {
                        await hearDead(e.ClientPlayer.Character, player.Character, _player, e.Message);
                        break;
                    }
                }
            }
        }
    }

    public class Sheriff : Role
    {
        public Sheriff(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnChat);
            TotalAllowed = 1;
            RoleType = RoleTypes.Sheriff;
        }

        public override async void HandleChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/"))
            {
                String[] parseCommand = e.Message.Split(" ", 2);
                if (parseCommand[0] == "/shoot")
                {
                    foreach (var player in e.Game.Players)
                    {
                        if (player.Character.PlayerInfo.PlayerName == parseCommand[1])
                        {
                            await player.Character.SetExiledAsync();
                            if (!player.Character.PlayerInfo.IsImpostor)
                            {
                                await e.PlayerControl.SetExiledAsync();
                            }
                        }
                    }
                }
            }
        }
    }
}