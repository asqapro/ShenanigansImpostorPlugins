using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Games;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Impostor.Plugins.GameOptionsSaverLoader.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<GameOptionsSaverLoader> _logger;

        public GameEventListener(ILogger<GameOptionsSaverLoader> logger)
        {
            _logger = logger;
        }

        [EventListener]
        public async ValueTask OnPlayerChat(IPlayerChatEvent e)
        {
            string serverResponse = "Command executed successfully";
            if(e.Game.GameState != GameStates.Started)
            {
                if(e.Message.StartsWith("/"))
                {
                    string[] s = e.Message.Split(' ');
                    string command = s[0];
                    if(!validateCommand(s))
                    {
                        serverResponse = "[ff0000ff]" + "Valid syntax: " + command + " filename.extension []";
                        await ServerMessage(e.ClientPlayer.Character, e.ClientPlayer.Character, serverResponse);
                        return;
                    }
                    
                    string fileName = s[1];
                    if(command == "/save")
                    {
                        using (BinaryWriter configWriter = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                        {
                            e.Game.Options.Serialize(configWriter, 3 /*or maybe e.Game.Version, but the options are 1, 2, or 3*/);
                            serverResponse = "[ff0000ff]" + "Saving game config file: " + fileName + "[]";
                            await ServerMessage(e.ClientPlayer.Character, e.ClientPlayer.Character, serverResponse);
                        }
                        if (!File.Exists(fileName))
                        {
                            serverResponse = "[ff0000ff]" + "Failed to save game config: " + fileName + "[]";
                            await ServerMessage(e.ClientPlayer.Character, e.ClientPlayer.Character, serverResponse);
                        }
                    }
                    else if(command == "/load")
                    {
                        if (File.Exists(fileName))
                        {
                            byte[] gameOptions = File.ReadAllBytes(fileName);
                            var memory = new ReadOnlyMemory<byte>(gameOptions);
                            e.Game.Options.Deserialize(memory);
                            serverResponse = "[ff0000ff]" + "Successfully loaded game config file: " + fileName + "[]";
                            await ServerMessage(e.ClientPlayer.Character, e.ClientPlayer.Character, serverResponse);
                        }
                        else
                        {
                            serverResponse = "[ff0000ff]" + "Failed to load game config: " + fileName + "[]";
                            await ServerMessage(e.ClientPlayer.Character, e.ClientPlayer.Character, serverResponse);
                        }
                    }
                }
            }
            else
            {
                //Broadcast error message that "Cannot use command during game"
                serverResponse = "[ff0000ff]" + "Command not allowed during active game." + "[]";
                await ServerMessage(e.ClientPlayer.Character, e.ClientPlayer.Character, serverResponse);
            } 
        }

        bool validateCommand(string[] commandParts)
        {
            if(commandParts.Length == 2)
            {
                return true;
            }
            else
            {
                return false;
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
    }
}