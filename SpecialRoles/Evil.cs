using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;

namespace Roles.Evil
{
    public class Hitman : Role
    {
        public new static int TotalAllowed = 1;
        private int ammo;

        public Hitman(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            TotalAllowed = 1;
            RoleType = RoleTypes.Hitman;
            ammo = 1;
        }

        private async ValueTask evilResponse(String message)
        {
            var currentName = _player.PlayerInfo.PlayerName;
            var currentColor = _player.PlayerInfo.ColorId;

            await _player.SetNameAsync("Evil");
            await _player.SetColorAsync(Impostor.Api.Innersloth.Customization.ColorType.Red);

            await _player.SendChatToPlayerAsync(message, _player);

            await _player.SetNameAsync(currentName);
            await _player.SetColorAsync(currentColor);
        }

        private async ValueTask silentKillPlayer(IInnerPlayerControl toShoot)
        {
            await toShoot.SetExiledAsync();
            ammo--;
        }

        public override async ValueTask<bool> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.PlayerControl.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
            {
                String commandParsePattern = @"/silentkill ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success)
                {
                    foreach (var player in e.Game.Players)
                    {
                        if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                        {
                            if (ammo > 0)
                            {
                                await silentKillPlayer(player.Character);
                                return true;
                            }
                            else
                            {
                                await evilResponse("You have no bullets left");
                            }
                            break;
                        }
                    }
                }
            }
            return false;
        }
    }

    public class VoodooLady : Role
    {
        public new static int TotalAllowed = 1;
        private String killWord;
        private String killTarget;

        public VoodooLady(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            TotalAllowed = 1;
            RoleType = RoleTypes.VoodooLady;
            killWord = "";
            killTarget = "";
        }

        private async ValueTask evilResponse(String message)
        {
            var currentName = _player.PlayerInfo.PlayerName;
            var currentColor = _player.PlayerInfo.ColorId;

            await _player.SetNameAsync("Evil");
            await _player.SetColorAsync(Impostor.Api.Innersloth.Customization.ColorType.Red);

            await _player.SendChatToPlayerAsync(message, _player);

            await _player.SetNameAsync(currentName);
            await _player.SetColorAsync(currentColor);
        }

        private async ValueTask silentKillPlayer(IInnerPlayerControl toKill)
        {
            await _player.SendChatToPlayerAsync("I'll have your tongue for that!", toKill);
            await toKill.SetExiledAsync();
        }

        public override async ValueTask<bool> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/"))
            {
                if (e.PlayerControl.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
                {
                    String commandParsePattern = @"/setkillword ((?:\w\s?)+) '(\w+)'";
                    var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                    if (parsedCommand.Success)
                    {
                        if (killWord != "" || killTarget != "")
                        {
                            await evilResponse("Kill target and word have already been set, you cannot change them");
                        }
                        else
                        {
                            killWord = parsedCommand.Groups[1].Value;
                            killTarget = parsedCommand.Groups[2].Value;
                            await evilResponse("Kill target and word have been set");
                        }
                    }
                    else
                    {
                        await evilResponse("Failed to set kill target and / or word");
                    }
                    return false;
                }
            }
            else if (e.Message.Contains(killWord) && e.PlayerControl.PlayerInfo.PlayerName == killTarget)
            {
                await silentKillPlayer(e.PlayerControl);
                return true;
            }
            return false;
        }
    }
}