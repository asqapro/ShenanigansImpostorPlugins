using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Microsoft.Extensions.Logging;
using CommandHandler;

//todo:
// - figure out how to inject _game into whisper and others
// - update GameEeventListeners code to use new class style

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
        private Dictionary<String, Command> pluginCommands = new Dictionary<string, Command>(); 
        private CommandManager manager = CommandManager.Instance;

        public GameEventListener(ILogger<Commands> logger)
        {
            _logger = logger;

            /*var saveCommand = new save(true, false, "/save <filename>", true, true);

            var loadCommand = new load(true, false, "/load <filename>", true, true);

            pluginCommands["/whisper"] = whisperCommand;
            pluginCommands["/kill"] = killCommand;
            pluginCommands["/setname"] = setNameCommand;
            pluginCommands["/save"] = saveCommand;
            pluginCommands["/load"] = loadCommand;
            foreach(var entry in pluginCommands)
            {
                parser.RegisterCommand(entry.Key, entry.Value);
            }*/

            //manager.managers["/whisper"] = handleWhisper;
            //manager.managers["/kill"] = handleKill;
            //manager.managers["/setname"] = handleSetName;
            //manager.managers["/save"] = handleSave;
            //manager.managers["/load"] = handleLoad;
        }

        [EventListener]
        public async ValueTask OnPlayerChat(IPlayerChatEvent e)
        {
            _logger.LogInformation($"{e.PlayerControl.PlayerInfo.PlayerName} said {e.Message}");

            if (_game == null)
            {
                _game = e.Game;
            }
            
            /*if (e.Message.StartsWith("/"))
            {
                ValidatedCommand parsedCommand = parser.ParseCommand(e.Message, e.ClientPlayer);

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
        }*/
    }
}