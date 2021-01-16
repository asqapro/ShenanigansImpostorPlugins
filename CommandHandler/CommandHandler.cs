using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Impostor.Api.Net;
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
        Valid
    }

    public class ValidatedCommand
    {
        public String CommandName {get; set;}
        public String Target {get; set;}
        public String Options {get; set;}
        public String Help {get; set;}
        public ValidateResult Validation {get; set;}
    }

    public abstract class Command
    {
        public bool HasTarget {get; set;}
        public bool HasOptions {get; set;}
        public String Help {get; set;}
        public bool HostOnly {get; set;}
        public bool Enabled {get; set;}
        public String Name {get; set;}

        public Command()
        {
            HasTarget = false;
            HasOptions = false;
            Help = "Unimplemented command";
            HostOnly = true;
            Enabled = false;
            Name = "Unimplemented";
        }

        public abstract void register();

        public abstract ValueTask<String> handle(IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent);
    }

    public interface ICommandManager
    {
        ValidatedCommand ParseCommand(String toParse, IClientPlayer sender);
        String GetCommandHelp(String commandName);
        bool RegisterCommand(Command newCommand);
        ValueTask<String> CallCommand(ValidatedCommand toCall, IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent);

    }

    public class CommandManager : ICommandManager
    {
        private Dictionary<String, Command> managers = new Dictionary<string, Command>();

        public CommandManager()
        {
        }

        private ValidateResult ValidateCommand(String toValidate, IClientPlayer sender)
        {
            String commandParsePattern = @"(/\w+)\s+((?:\w+\s*)+)('.*')*";
            var match = Regex.Match(toValidate, commandParsePattern);

            var commandName = match.Groups[1].Value.Trim();
            if (commandName == "")
            {
                commandName = toValidate.Split(" ")[0].Trim();
            }

            if (!managers.ContainsKey(commandName))
            {
                return ValidateResult.DoesNotExist;
            }
            else if (!match.Groups[2].Success && managers[commandName].HasTarget)
            {
                return ValidateResult.MissingTarget;
            }
            else if (!match.Groups[3].Success && managers[commandName].HasOptions)
            {
                return ValidateResult.MissingOptions;
            }
            else if (!managers[commandName].Enabled)
            {
                return ValidateResult.Disabled;
            }
            else if (managers[commandName].HostOnly && !sender.IsHost)
            {
                return ValidateResult.HostOnly;
            }
            else
            {
                return ValidateResult.Valid;
            }
        }

        public ValidatedCommand ParseCommand(String toParse, IClientPlayer sender)
        {
            var parsed = new ValidatedCommand();
            
            parsed.Validation = ValidateCommand(toParse, sender);
            if (parsed.Validation == ValidateResult.DoesNotExist)
            {
                return parsed;
            }
            
            String commandParsePattern = @"$(/\w+)\s((?:\w+\s*)+)('.*')*^";
            var match = Regex.Match(toParse, commandParsePattern);

            var commandName = match.Groups[1].Value.Trim();
            if (commandName == "")
            {
                commandName = toParse.Split(" ")[0].Trim();
            }

            parsed.CommandName = commandName;
            parsed.Target = match.Groups[2].Value.Trim();
            char[] toTrim = {' ', '\''};
            parsed.Options = match.Groups[3].Value.Trim(toTrim);
            parsed.Help = managers[commandName].Help;

            return parsed;
        }

        public String GetCommandHelp(String commandName)
        {
            if (!managers.ContainsKey(commandName))
            {
                return "Command does not exist";
            }
            return managers[commandName].Help;
        }

        public bool RegisterCommand(Command newCommand)
        {
            if (managers.ContainsKey(newCommand.Name))
            {
                return false;
            }
            newCommand.register();
            managers[newCommand.Name] = newCommand;
            return true;
        }

        public async ValueTask<String> CallCommand(ValidatedCommand toCall, IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent)
        {
            String response = "Command does not have a handler registered";
            if (managers.ContainsKey(toCall.CommandName))
            {
                response = await managers[toCall.CommandName].handle(sender, parsedCommand, chatEvent);
            }
            return response;
        }
    }
}