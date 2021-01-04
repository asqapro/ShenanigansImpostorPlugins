using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Messages;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Inner.Objects;
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
        private readonly IMessageWriterProvider _provider;

        public GameEventListener(ILogger<Commands> logger, IMessageWriterProvider provider)
        {
            _logger = logger;
            _provider = provider;
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

        public static Boolean isAlphaNumeric(string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9\s,]*$");
            return rg.IsMatch(strToCheck);
        }

        [EventListener]
        public async ValueTask OnPlayerChat(IPlayerChatEvent e)
        {
            _logger.LogInformation($"{e.PlayerControl.PlayerInfo.PlayerName} said {e.Message}");

            var handler = Handler.Instance;
            
            if (e.Message.StartsWith("/"))
            {
                String serverResponse = "Command executed successfully";

                Command parsedCommand = handler.ParseCommand(e.Message, e.ClientPlayer);

                if (parsedCommand.Validation == ValidateResult.ServerError)
                {
                    serverResponse = "Server experienced an error. Inform the host <" + e.Game.Host.Character.PlayerInfo.PlayerName + ">";
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
                    serverResponse = "Missing command target. Proper syntax is: " + parsedCommand.Help;
                }
                else if (parsedCommand.Validation == ValidateResult.MissingOptions)
                {
                    serverResponse = "Missing command options. Proper syntax is: " + parsedCommand.Help;
                }   
                else if (parsedCommand.Validation == ValidateResult.Valid)
                {
                    if (parsedCommand.CommandName == "/whisper")
                    {
                        var whisperSent = false;
                        String whisper = parsedCommand.Options;
                        foreach (var player in e.Game.Players)
                        {
                            var info = player.Character.PlayerInfo;
                            if (info.PlayerName == parsedCommand.Target)
                            {
                                var chatWhisper = e.ClientPlayer.Character.PlayerInfo.PlayerName + " whispers: [ff0000ff]";
                                chatWhisper += parsedCommand.Options + "[]";
                                await ServerMessage(e.ClientPlayer.Character, player.Character, chatWhisper);
                                serverResponse = "Whisper sent to " + parsedCommand.Target;
                                whisperSent = true;
                                break;
                            }
                        }
                        if (!whisperSent)
                        {
                            serverResponse = "Failed to whisper " + parsedCommand.Target;
                        }
                    }
                    else if (parsedCommand.CommandName == "/kill")
                    {
                        foreach (var player in e.Game.Players)
                        {
                            if (player.Character.PlayerInfo.PlayerName == parsedCommand.Target)
                            {
                                await player.Character.SetExiledAsync(player);
                            }
                        }
                    }
                    else if (parsedCommand.CommandName == "/setname")
                    {
                        var newName = parsedCommand.Target;
                        var canSetName = true;
                        if (!isAlphaNumeric(newName))
                        {
                            canSetName = false;
                            serverResponse = "New name contained invalid characters. Valid characters are alphanumeric, commas, and spaces";
                        }
                        foreach (var player in e.Game.Players)
                        {
                            if (player.Character.PlayerInfo.PlayerName == newName)
                            {
                                canSetName = false;
                                serverResponse = "Another player is already using that name";
                                break;
                            }
                        }
                        if (e.Game.GameState != GameStates.NotStarted)
                        { 
                            canSetName = false;
                            serverResponse = "You cannot change your name during an active game. Current game state: " + e.Game.GameState;
                        }
                        if (canSetName)
                        {
                            await e.ClientPlayer.Character.SetNameAsync(newName);
                        }
                    }
                    else
                    {
                        serverResponse = "Invalid command or syntax";
                    }
                }
                serverResponse = "[ff0000ff]" + serverResponse + "[]";
                await ServerMessage(e.ClientPlayer.Character, e.ClientPlayer.Character, serverResponse);
            }
        }
    }
}