using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Net.Inner.Objects;

namespace Roles.Evil
{
    public class Impersonator : InnerPlayerControlRole
    {
        public Impersonator(IInnerPlayerControl parent) : base(parent)
        {
            RoleType = RoleTypes.Impostor;
        }
    }

    public class Hitman : InnerPlayerControlRole
    {
        public new static int TotalAllowed = 1;
        private int ammo;

        public Hitman(IInnerPlayerControl parent) : base(parent)
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

        private void silentKillPlayer(IInnerPlayerControl toShoot)
        {
            ammo--;
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
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
                                silentKillPlayer(player.Character);
                                return new HandlerAction(ResultTypes.KillExilePlayer, new List<int> {player.Client.Id});
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
            return new HandlerAction(ResultTypes.NoAction);
        }
    }

    public class VoodooLady : InnerPlayerControlRole
    {
        public new static int TotalAllowed = 1;
        private String killWord;
        private String killTarget;
        private bool targetKilled;

        public VoodooLady(IInnerPlayerControl parent) : base(parent)
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
            await _player.SetNameAsync(currentName);
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
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
                            return new HandlerAction(ResultTypes.NoAction);
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
                            return new HandlerAction(ResultTypes.NoAction);
                        }
                        killWord = parsedCommand.Groups[1].Value;
                        killTarget = parsedCommand.Groups[2].Value;
                        await evilResponse("Kill target and word have been set");
                    }
                    return new HandlerAction(ResultTypes.NoAction);
                }
            }
            String[] checkWords = e.Message.Split(" ");
            if ((checkWords.Contains(killWord) || checkWords.Contains($"'{killWord}'")) && e.PlayerControl.PlayerInfo.PlayerName == killTarget)
            {
                if (!targetKilled)
                {
                    await silentKillPlayer(e.PlayerControl);
                    targetKilled = true;
                    return new HandlerAction(ResultTypes.KillExilePlayer, new List<int> {e.ClientPlayer.Client.Id});
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }
    }

    public class Arsonist : InnerPlayerControlRole
    {
        public new static int TotalAllowed = 1;
        private ICollection<IInnerPlayerControl> dousedPlayers;
        private bool dousedPlayer;

        public Arsonist(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            _listeners.Add(ListenerTypes.OnPlayerMovement);
            _listeners.Add(ListenerTypes.OnMeetingStarted);
            RoleType = RoleTypes.Arsonist;
            dousedPlayers = new List<IInnerPlayerControl>();
            dousedPlayer = false;
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/"))
            {
                if (e.PlayerControl.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
                {
                    String commandParsePattern = @"/ignite";
                    var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                    if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                    {
                        List<int> toKill = new List<int>();
                        foreach (var doused in dousedPlayers)
                        {
                            Console.WriteLine($"Adding ID to douse: {doused.OwnerId}");
                            toKill.Add(doused.OwnerId);
                            await _player.SendChatToPlayerAsync("You have been ignited", doused);
                        }
                        await _player.SendChatToPlayerAsync("You have ignited your targets");
                        return new HandlerAction(ResultTypes.KillExilePlayer, toKill);
                    }
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }

        public override ValueTask<HandlerAction> HandlePlayerMovement(IPlayerMovementEvent e)
        {
            foreach (var otherPlayer in e.Game.Players)
            {
                if (otherPlayer == null)
                {
                    continue;
                }
                if (otherPlayer.Client.Id == _player.OwnerId)
                {
                    continue;
                }
                var distance = Vector2.Distance(_player.NetworkTransform.Position, otherPlayer.Character.NetworkTransform.Position);
                if (distance < 0.6 && !dousedPlayer && dousedPlayers.Count < 3)
                {
                    dousedPlayers.Add(otherPlayer.Character);
                    dousedPlayer = true;
                }
            }
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        public override ValueTask<HandlerAction> HandleMeetingStart(IMeetingStartedEvent e)
        {
            dousedPlayer = false;
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }
    }
}