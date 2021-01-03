using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using Impostor.Api.Net;

namespace CommandsBase
{
    public class CommandParser
    {
        public Dictionary<String, Command> Commands {get; set;}
        public Dictionary<String, bool> Enabled {get; set;}
    }

    public class Command
    {
        public int Length {get; set;}
        public String Help {get; set;}
        public bool Hostonly {get; set;}
        public String Message {get; set;}
    }

    public class CommandHandler
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
            AlreadyExists,
            SyntaxError,
            Success
        }

        public ValidateResult ValidateCommand(String toValidate, IClientPlayer sender)
        {
            String commandParsePattern = @"(/\w+)\s+((?:\w+\s*)+)('.*')*";
            var match = Regex.Match(toValidate, commandParsePattern);

            if (!match.Groups[1].Success || !match.Groups[2].Success)
            {
                return ValidateResult.SyntaxError;
            }

            var commandValue = match.Groups[1].Value;

            var commandsFile = "CommandList.json";
            CommandParser commandList;

            try
            {
                var commandSyntaxJson = File.ReadAllText(commandsFile);
                commandList = JsonSerializer.Deserialize<CommandParser>(commandSyntaxJson);
            }
            catch
            {
                return ValidateResult.ServerError;
            }

            if (!commandList.Commands.ContainsKey(commandValue))
            {
                return ValidateResult.DoesNotExist;
            }

            if (!commandList.Enabled[commandValue])
            {
                return ValidateResult.Disabled;
            }

            if (commandList.Commands[commandValue].Hostonly && !sender.IsHost)
            {
                return ValidateResult.HostOnly;
            }

            if (!match.Groups[3].Success && commandList.Commands[commandValue].Length == 3)
            {
                return ValidateResult.SyntaxError;
            }

            return ValidateResult.Valid;
        }

        protected RegisterResult RegisterCommand()
        {
            return RegisterResult.Success;
        }
    }
}