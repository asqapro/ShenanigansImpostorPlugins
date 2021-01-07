using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Impostor.Api.Net.Inner.Objects;
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
        }

        private async ValueTask ServerMessage(IInnerPlayerControl sender, String message)
        {
            var currentColor = sender.PlayerInfo.ColorId;
            var currentName = sender.PlayerInfo.PlayerName;

            await sender.SetColorAsync(Impostor.Api.Innersloth.Customization.ColorType.White);
            await sender.SetNameAsync("Server");

            await sender.SendChatToPlayerAsync(message, sender);

            await sender.SetColorAsync(currentColor);
            await sender.SetNameAsync(currentName);
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
                ValidatedCommand parsedCommand = parser.ParseCommand(e.Message, e.ClientPlayer);

                String response = "";
                if (parsedCommand.Validation == ValidateResult.Valid)
                {
                    response = await manager.CallManager(parsedCommand, e.ClientPlayer.Character, parsedCommand, e);
                }
                else
                {
                    parser.GetCommandHelp(parsedCommand.CommandName);
                }
                await ServerMessage(e.ClientPlayer.Character, response);
            }
        }
    }
}