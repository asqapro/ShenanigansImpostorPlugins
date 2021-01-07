﻿using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CommandHandler;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;

namespace PlayerToPlayerCommands
{
    public class whisper : Command
    {
        public whisper(bool _hastarget, bool _hasoptions, String _help, bool _hostonly, bool _enabled) : base(_hastarget, _hasoptions, _help, _hostonly, _enabled)
        {
        }

        private async ValueTask sendWhisper(IInnerPlayerControl sender, IInnerPlayerControl receiver, String message)
        {
            var currentName = sender.PlayerInfo.PlayerName;
            await sender.SetNameAsync($"{currentName} (whispering)");
            await sender.SendChatToPlayerAsync($"[ff0000ff]{message}[]", receiver);
            await sender.SetNameAsync(currentName);
        }

        public override async ValueTask<String> handle(IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent)
        {
            String whisper = parsedCommand.Options;
            foreach (var player in chatEvent.Game.Players)
            {
                var info = player.Character.PlayerInfo;
                if (info.PlayerName == parsedCommand.Target)
                {
                    await sendWhisper(sender, player.Character, whisper);
                    return $"Whisper sent to {parsedCommand.Target}";
                }
            }
            return $"Failed to whisper {parsedCommand.Target}";
        }
    }

    public class kill : Command
    {
        public kill(bool _hastarget, bool _hasoptions, String _help, bool _hostonly, bool _enabled) : base(_hastarget, _hasoptions, _help, _hostonly, _enabled)
        {
        }

        public override async ValueTask<String> handle(IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent)
        {
            foreach (var player in chatEvent.Game.Players)
            {
                if (player.Character.PlayerInfo.PlayerName == parsedCommand.Target)
                {
                    await player.Character.SetExiledAsync(player);
                    return $"Successfully killed {parsedCommand.Target}";
                }
            }
            return $"Failed to kill {parsedCommand.Target}";
        }
    }

    public class setname : Command
    {
        public setname(bool _hastarget, bool _hasoptions, String _help, bool _hostonly, bool _enabled) : base(_hastarget, _hasoptions, _help, _hostonly, _enabled)
        {
        }

        private Boolean isAlphaNumeric(string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9\s,]*$");
            return rg.IsMatch(strToCheck);
        }

        public override async ValueTask<String> handle(IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent)
        {
            var newName = parsedCommand.Target;
            if (!isAlphaNumeric(newName))
            {
                return "New name contained invalid characters. Valid characters are alphanumeric, commas, and spaces";
            }
            foreach (var player in chatEvent.Game.Players)
            {
                if (player.Character.PlayerInfo.PlayerName == newName)
                {
                    return "Another player is already using that name";
                }
            }
            if (chatEvent.Game.GameState != GameStates.NotStarted)
            { 
                return $"You cannot change your name during an active game. Current game state: {chatEvent.Game.GameState}";
            }
            await sender.SetNameAsync(newName);
            return "Succesfully changed name";
        }
    }

    public class handler
    {
        private CommandManager manager = CommandManager.Instance;
        public handler()
        {
            var whisperCommand = new whisper(true, true, "/whisper <target> '<Message>'", false, true);
            var killCommand = new kill(true, false, "/kill <target>", true, true);
            var setNameCommand = new setname(true, false, "/setname <name>", false, true);
            manager.
    }
}