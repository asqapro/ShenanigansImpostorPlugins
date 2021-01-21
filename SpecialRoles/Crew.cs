using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Innersloth.Customization;

namespace Roles.Crew
{
    public class Crew : InnerPlayerControlRole
    {
        public Crew(IInnerPlayerControl parent) : base(parent)
        {
            RoleType = RoleTypes.Crew;
        }
    }

    public class Medium : InnerPlayerControlRole
    {
        public new static int TotalAllowed = 2;

        public Medium(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            RoleType = RoleTypes.Medium;
        }

        private async ValueTask hearDead(IInnerPlayerControl deadSender, IInnerPlayerControl mediumReceiver, String message)
        {
            var deadName = deadSender.PlayerInfo.PlayerName;
            var deadColor = deadSender.PlayerInfo.ColorId;
            var currentName = mediumReceiver.PlayerInfo.PlayerName;
            var currentColor = mediumReceiver.PlayerInfo.ColorId;

            await mediumReceiver.SetNameAsync($"{deadName} (dead)");
            await mediumReceiver.SetColorAsync(deadColor);

            await mediumReceiver.SendChatToPlayerAsync($"[0000ffff]{message}[]", mediumReceiver);

            await mediumReceiver.SetNameAsync(currentName);
            await mediumReceiver.SetColorAsync(currentColor);
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.PlayerControl.PlayerInfo.IsDead && e.ClientPlayer.Client.Id != _player.OwnerId)
            {
                await hearDead(e.ClientPlayer.Character, _player, e.Message);
            }
            return new HandlerAction(ResultTypes.NoAction);
        }
    }

    public class Sheriff : InnerPlayerControlRole
    {
        private int ammo { get; set; }
        public new static int TotalAllowed = 1;

        public Sheriff(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            RoleType = RoleTypes.Sheriff;
            ammo = 1;
        }

        private async ValueTask<List<int>> shootPlayer(IInnerPlayerControl toShoot)
        {
            var toKill = new List<int>();
            if (ammo < 1)
            {
                await _player.SendChatToPlayerAsync("You have no bullets left", _player);
                return toKill;
            }
            ammo--;
            if (!toShoot.PlayerInfo.IsImpostor)
            {
                await _player.SendChatToPlayerAsync("You shot a crewmate! You've killed yourself out of guilt", _player);
                toKill.Add(_player.OwnerId);
            }
            else
            {
                toKill.Add(toShoot.OwnerId);
            }
            return toKill;
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.ClientPlayer.Client.Id == _player.OwnerId)
            {
                String commandParsePattern = @"/shoot ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    foreach (var player in e.Game.Players)
                    {
                        if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                        {
                            var toKill = await shootPlayer(player.Character);
                            if (toKill.Count > 0)
                            {
                                return new HandlerAction(ResultTypes.KillExile, toKill);
                            }
                            return new HandlerAction(ResultTypes.NoAction);
                        }
                    }
                    await _player.SendChatToPlayerAsync("Could not find a player with that name", _player);
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }
    }

    public class Deputy : InnerPlayerControlRole
    {
        private bool usedInvestigate { get; set; }
        public new static int TotalAllowed = 1;

        public Deputy(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            _listeners.Add(ListenerTypes.OnMeetingEnded);
            RoleType = RoleTypes.Deputy;
            usedInvestigate = false;
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.ClientPlayer.Client.Id == _player.OwnerId)
            {
                String commandParsePattern = @"/investigate ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    if (!usedInvestigate)
                    {
                        foreach (var player in e.Game.Players)
                        {
                            if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                            {
                                if (player.Character.PlayerInfo.IsImpostor)
                                {
                                    await _player.SendChatToPlayerAsync($"{player.Character.PlayerInfo.PlayerName} is an impostor", _player);
                                }
                                else
                                {
                                    await _player.SendChatToPlayerAsync($"{player.Character.PlayerInfo.PlayerName} is a crewmate", _player);
                                }
                                break;
                            }
                        }
                        usedInvestigate = true;
                    }
                    else
                    {
                        await _player.SendChatToPlayerAsync("You may only investigate 1 player every meeting");
                    }
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }

        public override ValueTask<HandlerAction> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            usedInvestigate = false;
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }
    }

    public class InsaneDeputy : InnerPlayerControlRole
    {
        private bool usedInvestigate { get; set; }
        public new static int TotalAllowed = 1;

        public InsaneDeputy(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            _listeners.Add(ListenerTypes.OnMeetingEnded);
            RoleType = RoleTypes.InsaneDeputy;
            usedInvestigate = false;
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.ClientPlayer.Client.Id == _player.OwnerId)
            {
                String commandParsePattern = @"/investigate ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    if (!usedInvestigate)
                    {
                        foreach (var player in e.Game.Players)
                        {
                            if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                            {
                                if (player.Character.PlayerInfo.IsImpostor)
                                {
                                    await _player.SendChatToPlayerAsync($"{player.Character.PlayerInfo.PlayerName} is a crewmate", _player);
                                }
                                else
                                {
                                    await _player.SendChatToPlayerAsync($"{player.Character.PlayerInfo.PlayerName} is an impostor", _player);
                                }
                                break;
                            }
                        }
                        usedInvestigate = true;
                    }
                    else
                    {
                        await _player.SendChatToPlayerAsync("You may only investigate 1 player every meeting", _player);
                    }
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }

        public override ValueTask<HandlerAction> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            usedInvestigate = false;
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }
    }
    public class ConfusedDeputy : InnerPlayerControlRole
    {
        private bool usedInvestigate { get; set; }
        public new static int TotalAllowed = 1;

        public ConfusedDeputy(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            _listeners.Add(ListenerTypes.OnMeetingEnded);
            RoleType = RoleTypes.ConfusedDeputy;
            usedInvestigate = false;
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.ClientPlayer.Client.Id == _player.OwnerId)
            {
                String commandParsePattern = @"/investigate ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    if (!usedInvestigate)
                    {
                        foreach (var player in e.Game.Players)
                        {
                            if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                            {
                                Random rng = new Random();
                                if (player.Character.PlayerInfo.IsImpostor && rng.Next(1, 3) == 2)
                                {
                                    await _player.SendChatToPlayerAsync($"{player.Character.PlayerInfo.PlayerName} is an impostor", _player);
                                }
                                else
                                {
                                    await _player.SendChatToPlayerAsync($"{player.Character.PlayerInfo.PlayerName} is a crewmate", _player);
                                }
                                break;
                            }
                        }
                        usedInvestigate = true;
                    }
                    else
                    {
                        await _player.SendChatToPlayerAsync("You may only investigate 1 player every meeting", _player);
                    }
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }

        public override ValueTask<HandlerAction> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            usedInvestigate = false;
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }
    }

    public class Oracle : InnerPlayerControlRole
    {
        private bool usedReveal { get; set; }
        private IInnerPlayerControl toReveal { get; set; }
        public new static int TotalAllowed = 1;

        public Oracle(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            _listeners.Add(ListenerTypes.OnPlayerExile);
            _listeners.Add(ListenerTypes.OnPlayerMurder);
            _listeners.Add(ListenerTypes.OnMeetingEnded);
            RoleType = RoleTypes.Oracle;
            usedReveal = false;
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.ClientPlayer.Client.Id == _player.OwnerId)
            {
                String commandParsePattern = @"/reveal ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    if (!usedReveal)
                    {
                        foreach (var player in e.Game.Players)
                        {
                            if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                            {
                                toReveal = player.Character;
                                break;
                            }
                        }
                        usedReveal = true;
                    }
                    else
                    {
                        await _player.SendChatToPlayerAsync("You may only pick 1 player every meeting");
                    }
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }

        public override ValueTask<HandlerAction> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            usedReveal = false;
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        private async ValueTask revealOnDeath(IEnumerable<IClientPlayer> players)
        {
            var playerName = _player.PlayerInfo.PlayerName;
            var playerColor = _player.PlayerInfo.ColorId;

            var revealMessage = "";
            if (toReveal.PlayerInfo.IsImpostor)
            {
                revealMessage = $"{toReveal.PlayerInfo.PlayerName} was an impostor";
            }
            else
            {
                revealMessage = $"{toReveal.PlayerInfo.PlayerName} was a crewmate";
            }

            foreach (var player in players)
            {
                if (player.Client.Id != _player.OwnerId && !player.Character.PlayerInfo.IsDead)
                {
                    var currentName = player.Character.PlayerInfo.PlayerName;
                    var currentColor = player.Character.PlayerInfo.ColorId;

                    await player.Character.SetNameAsync($"{playerName} (Oracle)");
                    await player.Character.SetColorAsync(playerColor);

                    await player.Character.SendChatAsync(revealMessage);

                    await player.Character.SetNameAsync(currentName);
                    await player.Character.SetColorAsync(currentColor);
                    break;
                }
            }
        }

        public override async ValueTask<HandlerAction> HandlePlayerExile(IPlayerExileEvent e)
        {
            if (e.ClientPlayer.Client.Id == _player.OwnerId && toReveal != null)
            {
                await revealOnDeath(e.Game.Players);
            }
            return new HandlerAction(ResultTypes.NoAction);
        }

        public override async ValueTask<HandlerAction> HandlePlayerMurder(IPlayerMurderEvent e)
        {
            if (e.Victim.OwnerId == _player.OwnerId && toReveal != null)
            {
                await revealOnDeath(e.Game.Players);
            }
            return new HandlerAction(ResultTypes.NoAction);
        }
    }

    public class Lightkeeper : InnerPlayerControlRole
    {
        private bool extinguishLight { get; set; }
        private bool lightExtinguished { get; set; }
        private Dictionary<int, String> originalPlayerNames { get; set; }
        private Dictionary<int, byte> originalPlayerColors { get; set; }
        public new static int TotalAllowed = 1;

        public Lightkeeper(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerExile);
            _listeners.Add(ListenerTypes.OnPlayerMurder);
            _listeners.Add(ListenerTypes.OnMeetingStarted);
            _listeners.Add(ListenerTypes.OnMeetingEnded);
            RoleType = RoleTypes.Lightkeeper;
            extinguishLight = false;
            lightExtinguished = false;
            originalPlayerNames = new Dictionary<int, String>();
            originalPlayerColors = new Dictionary<int, byte>();
        }

        public override ValueTask<HandlerAction> HandlePlayerMurder(IPlayerMurderEvent e)
        {
            if (e.Victim.OwnerId == _player.OwnerId)
            {
                extinguishLight = true;
            }
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        public override ValueTask<HandlerAction> HandlePlayerExile(IPlayerExileEvent e)
        {
            if (e.ClientPlayer.Client.Id == _player.OwnerId)
            {
                extinguishLight = true;
            }
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        public override async ValueTask<HandlerAction> HandleMeetingStart(IMeetingStartedEvent e)
        {
            if (extinguishLight)
            {
                lightExtinguished = true;
                saveSettings(e);
                e.Game.Options.AnonymousVotes = true;
                await e.Game.SyncSettingsAsync();
                foreach (var player in e.Game.Players)
                {
                    originalPlayerNames[player.Client.Id] = player.Character.PlayerInfo.PlayerName;
                    originalPlayerColors[player.Client.Id] = player.Character.PlayerInfo.ColorId;
                    await player.Character.SetNameAsync("");
                    await player.Character.SetColorAsync(ColorType.Black);
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }

        public override async ValueTask<HandlerAction> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            if (lightExtinguished)
            {
                loadSettings(e);
                foreach (var player in e.Game.Players)
                {
                    var origPlayerName = originalPlayerNames[player.Client.Id];
                    var origPlayerColor = originalPlayerColors[player.Client.Id];
                    await player.Character.SetNameAsync(origPlayerName);
                    await player.Character.SetColorAsync(origPlayerColor);
                }
                extinguishLight = false;
                lightExtinguished = false;
            }
            return new HandlerAction(ResultTypes.NoAction);
        }
    }

    public class Doctor : InnerPlayerControlRole
    {
        private bool usedProtect { get; set; }
        private int toProtect { get; set; }
        public new static int TotalAllowed = 1;

        public Doctor(IInnerPlayerControl parent) : base(parent)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            _listeners.Add(ListenerTypes.OnMeetingEnded);
            RoleType = RoleTypes.Doctor;
            usedProtect = false;
        }

        public override async ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.ClientPlayer.Client.Id == _player.OwnerId)
            {
                String commandParsePattern = @"/protect ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    if (!usedProtect)
                    {
                        foreach (var player in e.Game.Players)
                        {
                            if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                            {
                                usedProtect = true;
                                toProtect = player.Client.Id;
                                await _player.SendChatToPlayerAsync($"{player.Character.PlayerInfo.PlayerName} will be protected once during the next meeting", _player);
                                return new HandlerAction(ResultTypes.NoAction);
                            }
                        }
                    }
                    else
                    {
                        await _player.SendChatToPlayerAsync("You may only protect 1 player every meeting");
                    }
                }
            }
            return new HandlerAction(ResultTypes.NoAction);
        }

        public override ValueTask<HandlerAction> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            if (usedProtect)
            {
                usedProtect = false;
                return ValueTask.FromResult(new HandlerAction(ResultTypes.Protect, new List<int> {toProtect}));
            }
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }
    }
}