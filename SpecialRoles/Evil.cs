using System;
using System.Linq;
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

        public override async ValueTask<Tuple<String, ResultTypes>> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.PlayerControl.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
            {
                String commandParsePattern = @"/silentkill ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    foreach (var player in e.Game.Players)
                    {
                        if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                        {
                            if (ammo > 0)
                            {
                                await silentKillPlayer(player.Character);
                                return new Tuple<String, ResultTypes>(player.Character.PlayerInfo.PlayerName, ResultTypes.KilledPlayer);
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
            return new Tuple<String, ResultTypes>("", ResultTypes.NoAction);
        }
    }

    public class VoodooLady : Role
    {
        public new static int TotalAllowed = 1;
        private String killWord;
        private String killTarget;
        private bool targetKilled;

        public VoodooLady(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            RoleType = RoleTypes.VoodooLady;
            killWord = "";
            killTarget = "";
            targetKilled = false;
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
            var currentName = _player.PlayerInfo.PlayerName;
            await _player.SetNameAsync($"{currentName} (Voodoo Lady)");
            await _player.SendChatToPlayerAsync("I'll have your tongue for that!", toKill);
            await toKill.SetExiledAsync();
            await _player.SetNameAsync(currentName);
        }

        public override async ValueTask<Tuple<String, ResultTypes>> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/"))
            {
                if (e.PlayerControl.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
                {
                    String commandParsePattern = @"/setkillword (\w+) '((?:\w\s?)+)'";
                    var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                    if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                    {
                        if (killWord != "" || killTarget != "")
                        {
                            await evilResponse("Kill target and word have already been set, you cannot change them");
                            return new Tuple<String, ResultTypes>("", ResultTypes.NoAction);
                        }
                        bool foundPlayer = false;
                        foreach (var player in e.Game.Players)
                        {
                            if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[2].Value)
                            {
                                foundPlayer = true;
                                break;
                            }
                        }
                        if (!foundPlayer)
                        {
                            await evilResponse("Kill target is a not a player in this game");
                            return new Tuple<String, ResultTypes>("", ResultTypes.NoAction);
                        }
                        killWord = parsedCommand.Groups[1].Value;
                        killTarget = parsedCommand.Groups[2].Value;
                        await evilResponse("Kill target and word have been set");
                    }
                    return new Tuple<String, ResultTypes>("", ResultTypes.NoAction);
                }
            }
            String[] checkWords = e.Message.Split(" ");
            if ((checkWords.Contains(killWord) || checkWords.Contains($"'{killWord}'")) && e.PlayerControl.PlayerInfo.PlayerName == killTarget)
            {
                if (!targetKilled)
                {
                    await silentKillPlayer(e.PlayerControl);
                    targetKilled = true;
                    return new Tuple<String, ResultTypes>(e.PlayerControl.PlayerInfo.PlayerName, ResultTypes.KilledPlayer);
                }
            }
            return new Tuple<String, ResultTypes>("", ResultTypes.NoAction);
        }
    }
}