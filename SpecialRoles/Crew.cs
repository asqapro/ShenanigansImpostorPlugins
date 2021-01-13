using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;

namespace Roles.Crew
{
    public class Medium : Role
    {
        public new static int TotalAllowed = 2;

        public Medium(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
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

        public override async ValueTask<bool> HandlePlayerChat(IPlayerChatEvent e)
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
                return true;
            }
            return false;
        }
    }

    public class Sheriff : Role
    {
        private int ammo;
        public new static int TotalAllowed = 1;

        public Sheriff(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            RoleType = RoleTypes.Sheriff;
            ammo = 1;
        }

        private async ValueTask lawResponse(String message)
        {
            var currentName = _player.PlayerInfo.PlayerName;
            var currentColor = _player.PlayerInfo.ColorId;

            await _player.SetNameAsync("The Law");
            await _player.SetColorAsync(Impostor.Api.Innersloth.Customization.ColorType.Red);

            await _player.SendChatToPlayerAsync(message, _player);

            await _player.SetNameAsync(currentName);
            await _player.SetColorAsync(currentColor);
        }

        private async ValueTask shootPlayer(IInnerPlayerControl toShoot)
        {
            await toShoot.SetExiledAsync();
            ammo--;
            if (!toShoot.PlayerInfo.IsImpostor)
            {
                await lawResponse("You shot a crewmate!");
                await _player.SetExiledAsync();
            }
        }

        public override async ValueTask<bool> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.PlayerControl.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
            {
                String commandParsePattern = @"/shoot ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success)
                {
                    foreach (var player in e.Game.Players)
                    {
                        if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                        {
                            if (ammo > 0)
                            {
                                await shootPlayer(player.Character);
                                return true;
                            }
                            else
                            {
                                await lawResponse("You have no bullets left");
                            }
                            break;
                        }
                    }
                }
            }
            return false;
        }
    }
}