using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading.Tasks;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner.Objects;

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

    public enum RegisterResult
    {
        ServerError,
        AlreadyExists,
        SyntaxError,
        HelpError,
        Success
    }

    public class CommandInfoParser
    {
        public Dictionary<String, CommandInfo> Commands {get; set;}
    }

    public class CommandInfo
    {
        public int Length {get; set;}
        public String Help {get; set;}
        public bool HostOnly {get; set;}
        public bool Enabled {get; set;}
    }

    public class Command
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
                commandList.Commands = new Dictionary<string, CommandInfo>();
                jsonServerError = true;
            }
        }

        private bool reloadCommands()
        {
            try
            {
                commandSyntaxJson = File.ReadAllText(commandsFile);
                commandList = JsonSerializer.Deserialize<CommandInfoParser>(commandSyntaxJson);
            }
            catch
            {
                jsonServerError = true;
            }
            jsonServerError = false;
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
            else if (!match.Groups[2].Success && commandList.Commands[commandName].Length >= 2)
            {
                return ValidateResult.MissingTarget;
            }
            else if (!match.Groups[3].Success && commandList.Commands[commandName].Length == 3)
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

        public Command ParseCommand(String toParse, IClientPlayer sender)
        {
            var parsed = new Command();

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

        public RegisterResult RegisterCommand(String newCommandName, CommandInfo newCommand)
        {
            if (commandList.Commands.ContainsKey(newCommandName))
            {
                return RegisterResult.AlreadyExists;
            }

            String commandPattern = @"(/\w+)";
            var match = Regex.Match(newCommandName, commandPattern);

            if (!match.Groups[1].Success)
            {
                return RegisterResult.SyntaxError;
            }

            bool includesOptions = (newCommand.Length == 3);
            String helpPattern;
            if (includesOptions)
            {
                helpPattern = @"(/\w+)\s+(<(?:\w+\s*)+>)\s+('<.+?>')";
            }
            else
            {
                helpPattern = @"(/\w+)\s+(<(?:\w+\s*)+>)$";
            }

            match = Regex.Match(newCommand.Help, helpPattern);
            if (!match.Success)
            {
                return RegisterResult.HelpError;
            }

            commandList.Commands[newCommandName] = newCommand;

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var registerJson = JsonSerializer.Serialize<CommandInfoParser>(commandList, options);
                File.WriteAllText(commandsFile, registerJson);
            }
            catch
            {
                commandList.Commands.Remove(newCommandName);
                return RegisterResult.ServerError;
            }

            if (!reloadCommands())
            {
                return RegisterResult.ServerError;
            }

            return RegisterResult.Success;
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

    public class CommandManager
    {
        public Dictionary<String, Func<IInnerPlayerControl, Command, ValueTask<String>>> managers = new Dictionary<string, Func<IInnerPlayerControl, Command, ValueTask<string>>>();
    }
}