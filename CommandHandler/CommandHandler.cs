using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading.Tasks;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Events.Player;

namespace CommandHandler
{
    public enum ValidateResult
    {
        ServerError,
        DoesNotExist,
        Disabled,
        MissingTarget,
        MissingOptions,
        HostOnly,
        Valid
    }

    public class CommandInfoParser
    {
        public Dictionary<String, CommandSchema> Commands {get; set;}
    }

    public class CommandSchema
    {
        public bool HasTarget {get; set;}
        public bool HasOptions {get; set;}
        public String Help {get; set;}
        public bool HostOnly {get; set;}
        public bool Enabled {get; set;}
    }

    public abstract class Command : CommandSchema
    {
        public String Name {get; set;}
        public Command(String _name, bool _hastarget = true, bool _hasoptions = false, String _help = "", bool _hostonly = false, bool _enabled = false)
        {
            Name = _name;
            HasTarget = _hastarget;
            HasOptions = _hasoptions;
            Help = _help;
            HostOnly = _hostonly;
            Enabled = _enabled;
        }

        public abstract ValueTask<String> handle(IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent);
    }

    public class ValidatedCommand
    {
        public String CommandName {get; set;}
        public String Target {get; set;}
        public String Options {get; set;}
        public String Help {get; set;}
        public ValidateResult Validation {get; set;}
    }

    public sealed class CommandParser
    {
        private String commandSyntaxJson;
        private CommandInfoParser commandList;
        private String commandsFile = "CommandsSyntax.json";

        private bool jsonServerError;

        private static readonly Lazy<CommandParser> lazy =
            new Lazy<CommandParser>
                (() => new CommandParser());

        public static CommandParser Instance { get { return lazy.Value; } }

        private CommandParser()
        {
            jsonServerError = false;
            try
            {
                commandSyntaxJson = File.ReadAllText(commandsFile);
                commandList = JsonSerializer.Deserialize<CommandInfoParser>(commandSyntaxJson);
            }
            catch
            {
                commandList = new CommandInfoParser();
                commandList.Commands = new Dictionary<string, CommandSchema>();
                jsonServerError = true;
            }
        }

        private bool reloadCommands()
        {
            jsonServerError = false;
            try
            {
                commandSyntaxJson = File.ReadAllText(commandsFile);
                commandList = JsonSerializer.Deserialize<CommandInfoParser>(commandSyntaxJson);
            }
            catch
            {
                jsonServerError = true;
            }
            return jsonServerError;
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

            if (!commandList.Commands.ContainsKey(commandName))
            {
                return ValidateResult.DoesNotExist;
            }
            else if (!match.Groups[2].Success && commandList.Commands[commandName].HasTarget)
            {
                return ValidateResult.MissingTarget;
            }
            else if (!match.Groups[3].Success && commandList.Commands[commandName].HasOptions)
            {
                return ValidateResult.MissingOptions;
            }
            else if (!commandList.Commands[commandName].Enabled)
            {
                return ValidateResult.Disabled;
            }
            else if (commandList.Commands[commandName].HostOnly && !sender.IsHost)
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

            if (jsonServerError)
            {
                parsed.Validation = ValidateResult.ServerError;
                return parsed;
            }
            
            parsed.Validation = ValidateCommand(toParse, sender);
            if (parsed.Validation == ValidateResult.DoesNotExist)
            {
                return parsed;
            }
            
            String commandParsePattern = @"(/\w+)\s+((?:\w+\s*)+)('.*')*";
            var match = Regex.Match(toParse, commandParsePattern);

            var commandName = match.Groups[1].Value.Trim();
            if (commandName == "")
            {
                commandName = toParse.Split(" ")[0].Trim();
            }

            parsed.CommandName = commandName;
            parsed.Target = match.Groups[2].Value.Trim();
            parsed.Options = match.Groups[3].Value.Trim();
            parsed.Help = commandList.Commands[commandName].Help;

            return parsed;
        }

        public String GetCommandHelp(String commandName)
        {
            if (jsonServerError)
            {
                return "Server experienced error";
            }
            if (!commandList.Commands.ContainsKey(commandName))
            {
                return "Command does not exist";
            }
            return commandList.Commands[commandName].Help;
        }
    }

    public sealed class CommandManager
    {
        private static readonly Lazy<CommandManager> lazy =
            new Lazy<CommandManager>
                (() => new CommandManager());
        public static CommandManager Instance { get { return lazy.Value; } }

        private Dictionary<String, Command> managers = new Dictionary<string, Command>();

        public bool RegisterManager(Command newCommand)
        {
            if (managers.ContainsKey(newCommand.Name))
            {
                return false;
            }
            managers[newCommand.Name] = newCommand;
            return true;
        }

        public async ValueTask<String> CallManager(ValidatedCommand toCall, IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent)
        {
            if (managers.ContainsKey(toCall.CommandName))
            {
                return await managers[toCall.CommandName].handle(sender, parsedCommand, chatEvent);
            }
            return "Command does not have a handler registered";
        }
    }
}