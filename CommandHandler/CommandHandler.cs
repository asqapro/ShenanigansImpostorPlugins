using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Events.Player;

namespace CommandHandler
{
    public enum ValidateResult
    {
        DoesNotExist,
        Disabled,
        MissingTarget,
        MissingOptions,
        HostOnly,
        CommandError,
        Valid
    }

    public abstract class Command
    {
        protected String commandParsePattern;
        public String Help {get; set;}
        public bool HostOnly {get; set;}
        public bool Enabled {get; set;}
        public String Name {get; set;}

        public Command()
        {
            commandParsePattern = @"^\w+ ((?:\w+\s?)+) '(.*)'$";
        }

        public abstract ValueTask<ValidateResult> Handle(IPlayerChatEvent chatEvent);

        protected async ValueTask ServerMessage(IInnerPlayerControl sender, String message)
        {
            var currentColor = sender.PlayerInfo.ColorId;
            var currentName = sender.PlayerInfo.PlayerName;

            await sender.SetColorAsync(Impostor.Api.Innersloth.Customization.ColorType.White);
            await sender.SetNameAsync("Server");

            await sender.SendChatToPlayerAsync(message, sender);

            await sender.SetColorAsync(currentColor);
            await sender.SetNameAsync(currentName);
        }
    }

    public interface ICommandManager
    {
        ValueTask<ValidateResult> Handle(IPlayerChatEvent chatEvent);
        bool RegisterCommand(Command newCommand);
    }

    public class CommandManager : ICommandManager
    {
        private Dictionary<String, Command> managers = new Dictionary<string, Command>();

        public CommandManager()
        {
        }

        public bool RegisterCommand(Command newCommand)
        {
            if (managers.ContainsKey(newCommand.Name))
            {
                return false;
            }
            managers[newCommand.Name] = newCommand;
            return true;
        }

        public async ValueTask<ValidateResult> Handle(IPlayerChatEvent chatEvent)
        {
            String commandPattern = @"^(/\w+)\s?";
            var match = Regex.Match(chatEvent.Message, commandPattern);

            var commandName = match.Groups[1].Value.Trim();
            if (!managers.ContainsKey(commandName))
            {
                return ValidateResult.DoesNotExist;
            }

            if (!managers[commandName].Enabled)
            {
                return ValidateResult.Disabled;
            }

            if (managers[commandName].HostOnly && !chatEvent.ClientPlayer.IsHost)
            {
                return ValidateResult.HostOnly;
            }

            return await managers[commandName].Handle(chatEvent);
        }

        public String GetCommandHelp(String chat)
        {
            String commandPattern = @"^(/\w+)\s?";
            var match = Regex.Match(chat, commandPattern);
            var commandName = match.Groups[1].Value.Trim();
            if (!managers.ContainsKey(commandName))
            {
                return "Command does not exist";
            }
            return managers[commandName].Help;
        }
    }
}