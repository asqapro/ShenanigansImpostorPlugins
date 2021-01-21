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
        private int ammo { get; set; }

        public Hitman(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            RoleType = RoleTypes.Hitman;
            ammo = 1;
        }

        private async ValueTask<List<int>> silentKillPlayer(IInnerPlayerControl toShoot)
        {
            var toKill = new List<int>();
            if (ammo < 1)
            {
                await _player.SendChatToPlayerAsync("You have no bullets left", _player);
                return toKill;
            }
            ammo--;
            toKill.Add(toShoot.OwnerId);
            return toKill;
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.ClientPlayer.Client.Id == _player.OwnerId)
            {
                String commandParsePattern = @"/silentkill ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    foreach (var player in e.Game.Players)
                    {
                        if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                        {
                            var toKill = await silentKillPlayer(player.Character);
                            if (toKill.Count > 0)
                            {
                                return new HandlerAction(ResultTypes.KillExile, toKill);
                            }
                            return new HandlerAction(ResultTypes.NoAction);
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
        private String killWord { get; set; }
        private int killTarget { get; set; }
        private bool targetKilled { get; set; }

        public VoodooLady(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            RoleType = RoleTypes.VoodooLady;
            killWord = "";
            killTarget = -3;
            targetKilled = false;
        }

        private async ValueTask voodooPlayer(IInnerPlayerControl toKill)
        {
            var currentName = _player.PlayerInfo.PlayerName;
            await _player.SetNameAsync($"{currentName} (Voodoo Lady)");
            await _player.SendChatToPlayerAsync("I'll have your tongue for that!", toKill);
            await _player.SetNameAsync(currentName);
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.ClientPlayer.Client.Id == _player.OwnerId)
            {
                String commandParsePattern = @"/setkillword (\w+) '((?:\w\s?)+)'";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    if (killWord != "" && killTarget != -3)
                    {
                        await _player.SendChatToPlayerAsync("Kill target and word have already been set, you cannot change them", _player);
                        return new HandlerAction(ResultTypes.NoAction);
                    }
                    bool foundPlayer = false;
                    foreach (var player in e.Game.Players)
                    {
                        if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[2].Value)
                        {
                            killTarget = player.Client.Id;
                            foundPlayer = true;
                            break;
                        }
                    }
                    if (!foundPlayer)
                    {
                        await _player.SendChatToPlayerAsync("Kill target is a not a player in this game", _player);
                        return new HandlerAction(ResultTypes.NoAction);
                    }
                    killWord = parsedCommand.Groups[1].Value;
                    await _player.SendChatToPlayerAsync("Kill target and word have been set", _player);
                }
                return new HandlerAction(ResultTypes.NoAction);
            }
            String[] checkWords = e.Message.Split(" ");
            if ((checkWords.Contains(killWord) || checkWords.Contains($"'{killWord}'")) && e.PlayerControl.OwnerId == killTarget)
            {
                if (!targetKilled)
                {
                    await voodooPlayer(e.PlayerControl);
                    targetKilled = true;
                    return new HandlerAction(ResultTypes.KillExile, new List<int> {e.ClientPlayer.Client.Id});
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }
    }

    public class Arsonist : InnerPlayerControlRole
    {
        public new static int TotalAllowed = 1;
        private ICollection<IInnerPlayerControl> dousedPlayers { get; set; }
        private bool dousedPlayer { get; set; }
        private int dousedCountdown { get; set; }
        private int maxDoused { get; set; }

        public Arsonist(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            _listeners.Add(ListenerTypes.OnPlayerMovement);
            _listeners.Add(ListenerTypes.OnMeetingStarted);
            RoleType = RoleTypes.Arsonist;
            dousedPlayers = new List<IInnerPlayerControl>();
            dousedPlayer = false;
            dousedCountdown = 3;
            maxDoused = 2;
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.ClientPlayer.Client.Id == _player.OwnerId)
            {
                String commandParsePattern = @"/ignite";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    List<int> toKill = new List<int>();
                    foreach (var doused in dousedPlayers)
                    {
                        toKill.Add(doused.OwnerId);
                        await _player.SendChatToPlayerAsync("You have been ignited", doused);
                    }
                    dousedPlayers.Clear();
                    await _player.SendChatToPlayerAsync("You have ignited your targets", _player);
                    return new HandlerAction(ResultTypes.KillExile, toKill);
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }

        public override ValueTask<HandlerAction> HandlePlayerMovement(IPlayerMovementEvent e)
        {
            if (dousedPlayer || dousedPlayers.Count >= maxDoused)
            {
                return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
            }
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
                if (distance < 0.6 && !otherPlayer.Character.PlayerInfo.IsImpostor)
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
            if (dousedCountdown == 1 && !_player.PlayerInfo.IsDead)
            {
                _player.SendChatToPlayerAsync("The gas will wear off next round. Ignite this round, or lose the chance", _player);
            }
            else if (dousedCountdown == 0 && !_player.PlayerInfo.IsDead && dousedPlayers.Count() > 0)
            {
                dousedPlayers.Clear();
                 _player.SendChatToPlayerAsync("The gas has worn off");
            }
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }
    }
}