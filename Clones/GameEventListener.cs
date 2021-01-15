using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Microsoft.Extensions.Logging;
using System;

namespace Impostor.Plugins.Clones.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<Clones> _logger;

        public GameEventListener(ILogger<Clones> logger)
        {
            _logger = logger;
        }

        private IGame g;

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            g = e.Game;
            _logger.LogInformation($"Game is starting.");
            int maxCloneCount = (e.Game.PlayerCount - 1) / 2 + 1;
            int cloneACount = 0;
            int cloneBCount = 0;
            Random rd = new Random();
            int rdMum;
            foreach (var player in e.Game.Players)
            {
                rdMum = rd.Next(0, 1);
                var info = player.Character.PlayerInfo;
                var playerEdit = player.Character;
                if ((rdMum == 0 && cloneACount < maxCloneCount) || cloneBCount >= maxCloneCount)
                {
                    playerEdit.SetNameAsync("Cool Clones");
                    playerEdit.SetColorAsync(1);
                    playerEdit.SetHatAsync(13);
                    playerEdit.SetSkinAsync(4);
                    playerEdit.SetPetAsync(1);
                    cloneACount++;
                }
                else
                {
                    playerEdit.SetNameAsync("Cooler Clones");
                    playerEdit.SetHatAsync(10);
                    playerEdit.SetColorAsync(10);
                    playerEdit.SetSkinAsync(0f);
                    playerEdit.SetPetAsync(6);
                    cloneBCount++;
                }
            }
        }

        [EventListener]
        public void OnGameEnded(IGameEndedEvent e)
        {
            _logger.LogInformation($"Game has ended.");
        }

        [EventListener]
        public void OnPlayerChat(IPlayerChatEvent e)
        {
            _logger.LogInformation($"{e.PlayerControl.PlayerInfo.PlayerName} said {e.Message}");
        }
        [EventListener]
        public void OnGameCreated(IGameCreatedEvent e)
        {
            _logger.LogInformation($"Game created");
        }
        [EventListener]
        public void OnPlayerMurder(IPlayerMurderEvent e)
        {
        }
    }
}