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
            commandParsePattern = @"^(?:/w|/whisper) ((?:\w+\s?)+) '(.*)'$";
            Help = "(/w | /whisper) <target> '<Message>'";
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

        public async override ValueTask<ValidateResult> Handle(IPlayerChatEvent chatEvent)
        {
            var match = Regex.Match(chatEvent.Message, commandParsePattern);

            if (!match.Groups[1].Success)
            {
                return ValidateResult.MissingTarget;
            }
            else if(!match.Groups[2].Success)
            {
                return ValidateResult.MissingOptions;
            }

            var target = match.Groups[1].Value;
            var whisper = match.Groups[2].Value;
            
            var whisperSent = false;

            foreach (var player in chatEvent.Game.Players)
            {
                var info = player.Character.PlayerInfo;
                if (info.PlayerName == target)
                {
                    await sendWhisper(chatEvent.PlayerControl, player.Character, whisper);
                    whisperSent = true;
                }
            }

            if (!whisperSent)
            {
                await ServerMessage(chatEvent.PlayerControl, $"Failed to whisper {target}");
                return ValidateResult.CommandError;
            }

            return ValidateResult.Valid;
        }
    }

    public class kill : Command
    {
        public kill() : base()
        {
            commandParsePattern = @"^/kill ((?:\w+\s?)+)$";
            Help = "/kill <target>";
            HostOnly = true;
            Enabled = true;
            Name = "/kill";
        }

        public async override ValueTask<ValidateResult> Handle(IPlayerChatEvent chatEvent)
        {
            var match = Regex.Match(chatEvent.Message, commandParsePattern);

            if (!match.Groups[1].Success)
            {
                return ValidateResult.MissingTarget;
            }

            var target = match.Groups[1].Value;

            var killedPlayer = false;

            foreach (var player in chatEvent.Game.Players)
            {
                if (player.Character.PlayerInfo.PlayerName == target)
                {
                    await player.Character.SetExiledAsync();
                    killedPlayer = true;
                }
            }

            if (!killedPlayer)
            {
                await ServerMessage(chatEvent.PlayerControl, $"Failed to kill {target}");
                return ValidateResult.CommandError;
            }

            return ValidateResult.Valid;
        }
    }

    public class setname : Command
    {
        public setname() : base()
        {
            commandParsePattern = @"^/setname ((?:\w+\s?)+)$";
            Help = "/setname <newname>";
            HostOnly = false;
            Enabled = true;
            Name = "/setname";
        }

        public async override ValueTask<ValidateResult> Handle(IPlayerChatEvent chatEvent)
        {
            var match = Regex.Match(chatEvent.Message, commandParsePattern);

            if (!match.Groups[1].Success)
            {
                return ValidateResult.MissingTarget;
            }

            var target = match.Groups[1].Value;

            //Truncate new name to 20 characters max
            var maxNameLength = 20;
            var newName = target.Length <= maxNameLength ? target : target.Substring(0, maxNameLength);

            bool canSetName = true;
            foreach (var player in chatEvent.Game.Players)
            {
                if (player.Character.PlayerInfo.PlayerName == newName)
                {
                    await ServerMessage(chatEvent.PlayerControl, "Another player is already using that name");
                    canSetName = false;
                }
            }
            if (chatEvent.Game.GameState != GameStates.NotStarted)
            { 
                await ServerMessage(chatEvent.PlayerControl, $"You cannot change your name during an active game. Current game state: {chatEvent.Game.GameState}");
                canSetName = false;
            }

            if (canSetName)
            {
                await chatEvent.PlayerControl.SetNameAsync(newName);
            }
            else
            {
                return ValidateResult.CommandError;
            }

            return ValidateResult.Valid;
        }
    }
}