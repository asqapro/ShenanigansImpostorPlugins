using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Games;
using Microsoft.Extensions.Logging;
using CommandHandler;

namespace Impostor.Plugins.Commands.Handlers
{
    /// <summary>
    ///     A class that listens for two events.
    ///     It may be more but this is just an example.
    ///
    ///     Make sure your class implements <see cref="IEventListener"/>.
    /// </summary>
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<Commands> _logger;
        private IGame _game;
        CommandParser parser = CommandParser.Instance;
        private Dictionary<String, CommandInfo> pluginCommands = new Dictionary<string, CommandInfo>(); 
        private CommandManager manager = CommandManager.Instance;

        public GameEventListener(ILogger<Commands> logger)
        {
            _logger = logger;
            var whisperCommand = new CommandInfo();
            whisperCommand.manualCommandInfo(true, true, "/whisper <target> '<Message>'", false, true);
            var killCommand = new CommandInfo();
            killCommand.manualCommandInfo(true, false, "/kill <target>", true, true);
            var setNameCommand = new CommandInfo();
            setNameCommand.manualCommandInfo(true, false, "/setname <name>", false, true);
            pluginCommands["/whisper"] = whisperCommand;
            pluginCommands["/kill"] = killCommand;
            pluginCommands["/setname"] = setNameCommand;
            foreach(var entry in pluginCommands)
            {
                parser.RegisterCommand(entry.Key, entry.Value);
            }

            manager.managers["/whisper"] = handleWhisper;
            manager.managers["/kill"] = handleKill;
            manager.managers["/setname"] = handleSetName;
        }

        /// <summary>
        ///     An example event listener.
        /// </summary>
        /// <param name="e">
        ///     The event you want to listen for.
        /// </param>
        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            _logger.LogInformation($"Game is starting.");
        }

        [EventListener]
        public void OnGameEnded(IGameEndedEvent e)
        {
            _logger.LogInformation($"Game has ended.");
        }

        [EventListener]
        public void OnPlayerExile(IPlayerExileEvent e)
        {
            _logger.LogInformation($"Player got exiled");
        }

        [EventListener]
        public void OnPlayerDestroyed(IPlayerDestroyedEvent e)
        {
            _logger.LogDebug(e.PlayerControl.PlayerInfo.PlayerName + " destroyed");
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

        private Boolean isAlphaNumeric(string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9\s,]*$");
            return rg.IsMatch(strToCheck);
        }

        private async ValueTask<String> handleWhisper(IInnerPlayerControl sender, Command parsedCommand)
        {
            String whisper = parsedCommand.Options;
            foreach (var player in _game.Players)
            {
                var info = player.Character.PlayerInfo;
                if (info.PlayerName == parsedCommand.Target)
                {
                    var chatWhisper = $"{sender.PlayerInfo.PlayerName} whispers: [ff0000ff]";
                    chatWhisper += parsedCommand.Options + "[]";
                    await ServerMessage(sender, player.Character, chatWhisper);
                    return $"Whisper sent to {parsedCommand.Target}";
                }
            }
            return $"Failed to whisper {parsedCommand.Target}";
        }

        private async ValueTask<String> handleKill(IInnerPlayerControl sender, Command parsedCommand)
        {
            foreach (var player in _game.Players)
            {
                if (player.Character.PlayerInfo.PlayerName == parsedCommand.Target)
                {
                    await player.Character.SetExiledAsync(player);
                    return $"Successfully killed {parsedCommand.Target}";
                }
            }
            return $"Failed to kill {parsedCommand.Target}";
        }

        private async ValueTask<String> handleSetName(IInnerPlayerControl sender, Command parsedCommand)
        {
            var newName = parsedCommand.Target;
            if (!isAlphaNumeric(newName))
            {
                return "New name contained invalid characters. Valid characters are alphanumeric, commas, and spaces";
            }
            foreach (var player in _game.Players)
            {
                if (player.Character.PlayerInfo.PlayerName == newName)
                {
                    return "Another player is already using that name";
                }
            }
            if (_game.GameState != GameStates.NotStarted)
            { 
                return $"You cannot change your name during an active game. Current game state: {_game.GameState}";
            }
            await sender.SetNameAsync(newName);
            return "Succesfully changed name";
        }

        [EventListener]
        public async ValueTask OnPlayerChat(IPlayerChatEvent e)
        {
            _logger.LogInformation($"{e.PlayerControl.PlayerInfo.PlayerName} said {e.Message}");

            if (_game == null)
            {
                _game = e.Game;
            }
            
            if (e.Message.StartsWith("/"))
            {
                String serverResponse = "Command executed successfully";

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
                    serverResponse = "Only the host may use that command";
                }
                else if (parsedCommand.Validation == ValidateResult.MissingTarget)
                {
                    serverResponse = "Missing command target. Proper syntax is: \n" + parsedCommand.Help;
                }
                else if (parsedCommand.Validation == ValidateResult.MissingOptions)
                {
                    serverResponse = "Missing command options. Proper syntax is: \n" + parsedCommand.Help;
                }
                else if (parsedCommand.Validation == ValidateResult.Valid)
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
                serverResponse = "[ff0000ff]" + serverResponse + "[]";
                await ServerMessage(e.ClientPlayer.Character, e.ClientPlayer.Character, serverResponse);
            }
        }
    }
}