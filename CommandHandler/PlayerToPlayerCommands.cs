using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using CommandHandler;

namespace PlayerToPlayerCommands
{
    public class whisper : Command
    {
        public whisper() : base()
        {
        }

        public override void register()
        {
            HasTarget = true;
            HasOptions = true;
            Help = "/w <target> '<Message>'";
            HostOnly = false;
            Enabled = true;
            Name = "/w";
        }

        private async ValueTask sendWhisper(IInnerPlayerControl sender, IInnerPlayerControl receiver, String message)
        {
            var currentName = sender.PlayerInfo.PlayerName;
            await sender.SetNameAsync($"{currentName} (whispering)");
            await sender.SendChatToPlayerAsync(message, receiver);
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
        public kill() : base()
        {
        }

        public override void register()
        {
            HasTarget = true;
            HasOptions = false;
            Help = "/kill <target>";
            HostOnly = true;
            Enabled = true;
            Name = "/kill";
        }

        public override async ValueTask<String> handle(IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent)
        {
            foreach (var player in chatEvent.Game.Players)
            {
                if (player.Character.PlayerInfo.PlayerName == parsedCommand.Target)
                {
                    await player.Character.SetExiledAsync();
                    return $"Successfully killed {parsedCommand.Target}";
                }
            }
            return $"Failed to kill {parsedCommand.Target}";
        }
    }

    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength); 
        }
    }

    public class setname : Command
    {
        public setname() : base()
        {
        }

        public override void register()
        {
            HasTarget = true;
            HasOptions = false;
            Help = "/setname <newname>";
            HostOnly = false;
            Enabled = true;
            Name = "/setname";
        }

        private Boolean isAlphaNumeric(string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9\s,]*$");
            return rg.IsMatch(strToCheck);
        }

        public override async ValueTask<String> handle(IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent)
        {
            var newName = parsedCommand.Target.Truncate(25);
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

    public class PlayerToPlayerCommandsHandler
    {
        private ICommandManager manager;
        public PlayerToPlayerCommandsHandler(ICommandManager manager)
        {
            var whisperCommand = new whisper();
            var killCommand = new kill();
            var setnameCommand = new setname();
            manager.RegisterCommand(whisperCommand);
            manager.RegisterCommand(killCommand);
            manager.RegisterCommand(setnameCommand);
        }
    }
}