using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;
using Microsoft.Extensions.Logging;
using CommandHandler;
using PlayerToPlayerCommands;
using GameOptionsSaverLoader;

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
        private Dictionary<String, Command> pluginCommands = new Dictionary<string, Command>(); 
        private CommandManager manager;

        public GameEventListener(ILogger<Commands> logger)
        {
            _logger = logger; 
            manager = new CommandManager();
            var PtPCommandHandler = new PlayerToPlayerCommandsHandler(manager);
            var SaverLoaderHandler = new GameOptionsSaverLoaderHandler(manager); 
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

            if (e.Message.StartsWith("/"))
            {
                ValidatedCommand parsedCommand = manager.ParseCommand(e.Message, e.ClientPlayer);

                String response = "";
                if (parsedCommand.Validation == ValidateResult.Valid)
                {
                    response = await manager.CallCommand(parsedCommand, e.ClientPlayer.Character, parsedCommand, e);
                }
                else if (parsedCommand.Validation == ValidateResult.HostOnly)
                {
                    response = "Only the host may call that command";
                }
                else if (parsedCommand.Validation == ValidateResult.Disabled)
                {
                    response = "Command is disabled";
                }
                else if (parsedCommand.Validation == ValidateResult.DoesNotExist)
                {
                    response = "Command does not exist";
                }
                else if (parsedCommand.Validation == ValidateResult.MissingTarget)
                {
                    response = $"Command was missing target. \nRefer to help: {parsedCommand.Help}";
                }
                else if (parsedCommand.Validation == ValidateResult.MissingOptions)
                {
                    response = $"Command was missing options. \nRefer to help: {parsedCommand.Help}";
                }
                else
                {
                    response = $"Something very unexpected happened. Tell {e.Game.Host.Character.PlayerInfo.PlayerName}";
                }
                response = $"[ff0000ff]{response}[]";
                await ServerMessage(e.ClientPlayer.Character, response);
            }
        }
    }
}