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
        SyntaxError,
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
        private static readonly Lazy<Handler> lazy =
            new Lazy<Handler>
                (() => new Handler());

        public static Handler Instance { get { return lazy.Value; } }

        private Handler(){}


        private String commandSyntaxJson;
        private CommandInfoParser commandList;
        private String commandsFile = "CommandsSyntax.json";

        private ValidateResult ValidateCommand(String toValidate, IClientPlayer sender)
        {
            String commandParsePattern = @"(/\w+)\s+((?:\w+\s*)+)('.*')*";
            var match = Regex.Match(toValidate, commandParsePattern);

            if (!match.Groups[1].Success || !match.Groups[2].Success)
            {
                return ValidateResult.SyntaxError;
            }

            var commandValue = match.Groups[1].Value.Trim();

            try
            {
                commandSyntaxJson = File.ReadAllText(commandsFile);
                commandList = JsonSerializer.Deserialize<CommandInfoParser>(commandSyntaxJson);
            }
            catch
            {
                return ValidateResult.ServerError;
            }

            Console.WriteLine($"Command value: {commandValue}");
            Console.WriteLine($"Help message: {commandList.Commands[commandValue].Help}");

            if (!commandList.Commands.ContainsKey(commandValue))
            {
                return ValidateResult.DoesNotExist;
            }

            if (!commandList.Commands[commandValue].Enabled)
            {
                return ValidateResult.Disabled;
            }

            if (commandList.Commands[commandValue].HostOnly && !sender.IsHost)
            {
                return ValidateResult.HostOnly;
            }

            if (!match.Groups[3].Success && commandList.Commands[commandValue].Length == 3)
            {
                return ValidateResult.SyntaxError;
            }

            return ValidateResult.Valid;
        }

        public Command ParseCommand(String toParse, IClientPlayer sender)
        {
            var parsed = new Command();
            parsed.Validation = ValidateCommand(toParse, sender);
            if (parsed.Validation != ValidateResult.Valid)
            {
                return parsed;
            }
            String commandParsePattern = @"(/\w+)\s+((?:\w+\s*)+)('.*')*";
            var match = Regex.Match(toParse, commandParsePattern);

            var commandValue = match.Groups[1].Value.Trim();

            parsed.CommandName = commandValue;
            parsed.Target = match.Groups[2].Value.Trim();
            parsed.Options = match.Groups[3].Value.Trim();
            parsed.Help = commandList.Commands[commandValue].Help;

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
    }
}