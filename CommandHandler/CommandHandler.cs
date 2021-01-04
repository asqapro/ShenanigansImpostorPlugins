using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using Impostor.Api.Net;

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

    public sealed class Handler
    {
        private String commandSyntaxJson;
        private CommandInfoParser commandList;
        private String commandsFile = "CommandsSyntax.json";

        private bool validateServerError;

        private static readonly Lazy<Handler> lazy =
            new Lazy<Handler>
                (() => new Handler());

        public static Handler Instance { get { return lazy.Value; } }

        private Handler()
        {
            try
            {
                commandSyntaxJson = File.ReadAllText(commandsFile);
                commandList = JsonSerializer.Deserialize<CommandInfoParser>(commandSyntaxJson);
            }
            catch
            {
                validateServerError = true;
            }
            validateServerError = false;
        }

        private ValidateResult ValidateCommand(String toValidate, IClientPlayer sender)
        {
            String commandParsePattern = @"(/\w+)\s+((?:\w+\s*)+)('.*')*";
            var match = Regex.Match(toValidate, commandParsePattern);

            var commandName = match.Groups[1].Value.Trim();
            if (commandName == "")
            {
                commandName = toValidate.Trim();
            }

            if (!commandList.Commands.ContainsKey(commandName))
            {
                return ValidateResult.DoesNotExist;
            }
            else if (!commandList.Commands[commandName].Enabled)
            {
                return ValidateResult.Disabled;
            }
            else if (commandList.Commands[commandName].HostOnly && !sender.IsHost)
            {
                return ValidateResult.HostOnly;
            }
            else if (!match.Groups[2].Success && commandList.Commands[commandName].Length >= 2)
            {
                return ValidateResult.MissingTarget;
            }
            else if (!match.Groups[3].Success && commandList.Commands[commandName].Length == 3)
            {
                return ValidateResult.MissingOptions;
            }
            else
            {
                return ValidateResult.Valid;
            }
        }

        public Command ParseCommand(String toParse, IClientPlayer sender)
        {
            var parsed = new Command();

            if (validateServerError)
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
                commandName = toParse.Trim();
            }

            parsed.CommandName = commandName;
            parsed.Target = match.Groups[2].Value.Trim();
            parsed.Options = match.Groups[3].Value.Trim();
            parsed.Help = commandList.Commands[commandName].Help;

            return parsed;
        }

        public RegisterResult RegisterCommand(String commandName, bool includesOptions, String helpMessage, bool hostOnly)
        {
            if (commandList.Commands.ContainsKey(commandName))
            {
                return RegisterResult.AlreadyExists;
            }

            String commandPattern = @"(/\w+)";
            var match = Regex.Match(commandName, commandPattern);

            if (!match.Groups[1].Success)
            {
                return RegisterResult.SyntaxError;
            }

            String helpPattern;
            if (includesOptions)
            {
                helpPattern = @"(/\w+)\s+(<(?:\w+\s*)+>)\s+('<.+?>')";
            }
            else
            {
                helpPattern = @"(/\w+)\s+(<(?:\w+\s*)+>)$";
            }

            match = Regex.Match(helpMessage, helpPattern);
            if (!match.Success)
            {
                return RegisterResult.HelpError;
            }

            var toRegister = new CommandInfo();
            toRegister.Length = includesOptions ? 3 : 2;
            toRegister.Help = helpMessage;
            toRegister.HostOnly = hostOnly;
            toRegister.Enabled = true;

            commandList.Commands[commandName] = toRegister;

            try
            {
                var registerJson = JsonSerializer.Serialize<CommandInfoParser>(commandList);
                File.WriteAllText(commandsFile, registerJson);
            }
            catch
            {
                commandList.Commands.Remove(commandName);
                return RegisterResult.ServerError;
            }

            return RegisterResult.Success;
        }

        public String GetCommandHelp(String commandName)
        {
            if (validateServerError)
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
}