using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
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

        public override async ValueTask<Tuple<String, ResultTypes>> HandlePlayerChat(IPlayerChatEvent e)
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
            return new Tuple<String, ResultTypes>("", ResultTypes.NoAction);
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
            if (ammo < 1)
            {
                await lawResponse("You have no bullets left");
                return;
            }
            await toShoot.SetExiledAsync();
            ammo--;
            if (!toShoot.PlayerInfo.IsImpostor)
            {
                await lawResponse("You shot a crewmate!");
                await _player.SetExiledAsync();
            }
        }

        public override async ValueTask<Tuple<String, ResultTypes>> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.PlayerControl.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
            {
                String commandParsePattern = @"/shoot ((?:\w\s?)+)";
                var parsedCommand = Regex.Match(e.Message, commandParsePattern);
                if (parsedCommand.Success && !_player.PlayerInfo.IsDead)
                {
                    foreach (var player in e.Game.Players)
                    {
                        if (player.Character.PlayerInfo.PlayerName == parsedCommand.Groups[1].Value)
                        {
                            await shootPlayer(player.Character);
                            return new Tuple<String, ResultTypes>(player.Character.PlayerInfo.PlayerName, ResultTypes.KilledPlayer);
                        }
                    }
                }
            }
            return new Tuple<String, ResultTypes>("", ResultTypes.NoAction);
        }
    }

    public class Cop : Role
    {
        private bool usedInvestigate;
        public new static int TotalAllowed = 1;

        public Cop(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnPlayerChat);
            RoleType = RoleTypes.Cop;
            usedInvestigate = false;
        }

        public override async ValueTask<Tuple<String, ResultTypes>> HandlePlayerChat(IPlayerChatEvent e)
        {
            if (e.Message.StartsWith("/") && e.PlayerControl.PlayerInfo.PlayerName == _player.PlayerInfo.PlayerName)
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
                                    await _player.SendChatToPlayerAsync($"{player.Character.PlayerInfo.PlayerName} is an impostor");
                                }
                                else
                                {
                                    await _player.SendChatToPlayerAsync($"{player.Character.PlayerInfo.PlayerName} is a crewmate");
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
            return new Tuple<String, ResultTypes>("", ResultTypes.NoAction);
        }

        public override ValueTask<Tuple<String, ResultTypes>> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            usedInvestigate = false;
            return ValueTask.FromResult(new Tuple<String, ResultTypes>("", ResultTypes.NoAction));
        }
    }
}