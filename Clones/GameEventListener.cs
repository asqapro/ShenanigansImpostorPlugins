﻿using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Games;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Impostor.Plugins.Clones.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<Clones> _logger;

        private Api.Net.IClientPlayer imp;

        public GameEventListener(ILogger<Clones> logger)
        {
            _logger = logger;
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            _logger.LogInformation($"Game is starting.");
            int maxCloneCount = (e.Game.PlayerCount - 1) / 2 + 1;
            int cloneACount = 0;
            int cloneBCount = 0;
            Random rd = new Random();
            int rdMum;
            foreach (var player in e.Game.Players)
            {
                if (player.Character.PlayerInfo.IsImpostor)
                {
                    imp = player;
                }
                rdMum = rd.Next(0, 1);
                var info = player.Character.PlayerInfo;
                var playerEdit = player.Character;
                if ((rdMum == 0 && cloneACount < maxCloneCount) || cloneBCount >= maxCloneCount)
                {
                    playerEdit.SetNameAsync("A Clones");
                    playerEdit.SetColorAsync(1);
                    playerEdit.SetHatAsync(13);
                    playerEdit.SetSkinAsync(4);
                    playerEdit.SetPetAsync(1);
                    cloneACount++;
                }
                else
                {
                    playerEdit.SetNameAsync("B Clones");
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