using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Games;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using CommandHandler;

namespace Impostor.Plugins.GameOptionsSaverLoader.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<GameOptionsSaverLoader> _logger;
        private IGame _game;
        CommandParser parser = CommandParser.Instance;
        private Dictionary<String, CommandInfo> pluginCommands = new Dictionary<string, CommandInfo>(); 
        private CommandManager manager;

        public GameEventListener(ILogger<GameOptionsSaverLoader> logger)
        {
            _logger = logger;
            var saveCommand = new CommandInfo();
            saveCommand.manualCommandInfo(true, false, "/save <filename.bin>", true, true);
            var loadCommand = new CommandInfo();
            loadCommand.manualCommandInfo(true, false, "/load <filename.bin>", true, true);
            pluginCommands["/save"] = saveCommand;
            pluginCommands["/load"] = loadCommand;
            foreach(var entry in pluginCommands)
            {
                RegisterResult result = parser.RegisterCommand(entry.Key, entry.Value);
                if (result == RegisterResult.AlreadyExists)
                {
                    _logger.LogError($"Command already exists: {entry.Key}");
                }
                else if (result == RegisterResult.HelpError)
                {
                    _logger.LogError($"Error with help: {entry.Key}");
                }
                else if (result == RegisterResult.ServerError)
                {
                    _logger.LogError($"Server error occured: {entry.Key}");
                }
                else if (result == RegisterResult.SyntaxError)
                {
                    _logger.LogError($"Syntax error occured: {entry.Key}");
                }
                else if (result == RegisterResult.Success)
                {
                    _logger.LogError($"Successfully registered command: {entry.Key}");
                }
            }

            manager = new CommandManager();
            manager.managers["/save"] = handleSave;
            manager.managers["/load"] = handleLoad;
        }

        private ValueTask<String> handleSave(IInnerPlayerControl sender, Command parsedCommand)
        {
            String success = "";
            using (BinaryWriter configWriter = new BinaryWriter(File.Open(parsedCommand.Target, FileMode.Create)))
            {
                _game.Options.Serialize(configWriter, 3 /*or maybe e.Game.Version, but the options are 1, 2, or 3*/);
                success = $"Saving game config file: {parsedCommand.Target}";
            }
            if (!File.Exists(parsedCommand.Target))
            {
                success = $"Failed to save game config: {parsedCommand.Target}";
            }
            return ValueTask.FromResult(success);
        }

        private async ValueTask<String> handleLoad(IInnerPlayerControl sender, Command parsedCommand)
        {
            if (File.Exists(parsedCommand.Target))
            {
                byte[] gameOptions = File.ReadAllBytes(parsedCommand.Target);
                var memory = new ReadOnlyMemory<byte>(gameOptions);
                _game.Options.Deserialize(memory);
                await _game.SyncSettingsAsync();
                return $"Successfully loaded game config file: {parsedCommand.Target}";
            }
            else
            {
                return $"Failed to load game config: {parsedCommand.Target}";
            }
        }

        [EventListener]
        public async ValueTask OnPlayerChat(IPlayerChatEvent e)
        {
            if (_game == null)
            {
                _game = e.Game;
            }
            if(e.Message.StartsWith("/"))
            {
                String serverResponse = "Command executed successfully";
                if(e.Game.GameState != GameStates.Started)
                {
                    Command parsedCommand = parser.ParseCommand(e.Message, e.ClientPlayer);

                    if (parsedCommand.Validation == ValidateResult.ServerError)
                    {
                        serverResponse = "Server experienced an error. Inform the host: \n<" + e.Game.Host.Character.PlayerInfo.PlayerName + ">";
                    }
                    else if (parsedCommand.Validation == ValidateResult.DoesNotExist)
                    {
                        serverResponse = "Command does not exist";
                    }
                    else if (parsedCommand.Validation == ValidateResult.HostOnly)
                    {
                        serverResponse = $"Must be game host to save file: {parsedCommand.Target}";
                    }
                    else if (parsedCommand.Validation == ValidateResult.MissingTarget)
                    {
                        serverResponse = "Missing command target. Proper syntax is: \n" + parsedCommand.Help;
                    }
                    else if (parsedCommand.Validation == ValidateResult.MissingOptions)
                    {
                        serverResponse = "Missing command options. Proper syntax is: \n" + parsedCommand.Help;
                    }
                    else if(!isAlphaNumeric(parsedCommand.Target))
                    {
                        serverResponse = "Filename must not contain spaces or special characters";
                    }
                    else
                    {
                        if (!manager.managers.ContainsKey(parsedCommand.CommandName))
                        {
                            serverResponse = "Invalid command or syntax";
                        }
                        else
                        {
                            serverResponse = await manager.managers[parsedCommand.CommandName](e.PlayerControl, parsedCommand);
                        }
                    }
                }
                else
                {
                    //Broadcast error message that "Cannot use command during game"
                    serverResponse = "Command not allowed during active game.";
                }
                serverResponse = "[ff0000ff]" + serverResponse + "[]";
                await ServerMessage(e.ClientPlayer.Character, e.ClientPlayer.Character, serverResponse);
            }
        }

        private async ValueTask ServerMessage(IInnerPlayerControl sender, IInnerPlayerControl receiver, String message)
        {
            var currentColor = sender.PlayerInfo.ColorId;
            var currentName = sender.PlayerInfo.PlayerName;

            await sender.SetColorAsync(Impostor.Api.Innersloth.Customization.ColorType.White);
            await sender.SetNameAsync("Server");

            await sender.SendChatToPlayerAsync(message, receiver);

            await sender.SetColorAsync(currentColor);
            await sender.SetNameAsync(currentName);
        }

        public static Boolean isAlphaNumeric(string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9\.]*$");
            return rg.IsMatch(strToCheck);
        }
    }
}