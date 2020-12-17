using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Games;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Impostor.Plugins.Infected.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<InfectedPlugin> _logger;

        public GameEventListener(ILogger<InfectedPlugin> logger)
        {
            _logger = logger;
        }

        public bool someoneDied = false;
        public IInnerPlayerControl whoDied;

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            _logger.LogInformation($"Game is starting.");
            // This prints out for all players if they are impostor or crewmate.
            Random rd = new Random();
            int rd_num = rd.Next(0, e.Game.PlayerCount - e.Game.Options.NumImpostors - 1);
            int rd_perm_num = rd_num;
            foreach (var player in e.Game.Players)
            {
                var info = player.Character.PlayerInfo;
                var playerEdit = player.Character;
                var isImpostor = info.IsImpostor;
                if (isImpostor)
                {
                    if (rd_num == player.Client.Id)
                    {
                        rd_num = rd.Next(0, e.Game.PlayerCount - e.Game.Options.NumImpostors - 1);
                    }
                    _logger.LogInformation($"- {info.PlayerName} is an impostor.");
                    playerEdit.SetHatAsync(84);
                    playerEdit.SetColorAsync(2);
                    playerEdit.SetSkinAsync(0f);
                    playerEdit.SetNameAsync("Infected");
                }
                else
                {
                    if (rd_perm_num == player.Client.Id)
                    {
                        playerEdit.SetNameAsync("Crewmates chief");
                        playerEdit.SetColorAsync(1);
                        playerEdit.SetHatAsync(13);
                        playerEdit.SetSkinAsync(4);
                    }
                    else
                    {
                        playerEdit.SetHatAsync(10);
                        playerEdit.SetColorAsync(10);
                        playerEdit.SetSkinAsync(0f);
                        playerEdit.SetNameAsync("Crewmate");
                    }
                    _logger.LogInformation($"- {info.PlayerName} is a crewmate.");
                    _logger.LogInformation($"{rd_perm_num} is number 1");
                    _logger.LogInformation($"{player.Client.Id} is the id");
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
            _logger.LogInformation($"killed {e.Victim.PlayerInfo.PlayerName}");
            e.Victim.SetNameAsync("Infected");
            e.Victim.SetHatAsync(84);
            e.Victim.SetColorAsync(2);
            e.ClientPlayer.Game.Options.NumImpostors++;
            List<IInnerPlayerControl> pInfected = new List<IInnerPlayerControl>();
            pInfected.Add(e.Victim);
            e.Game.SetInfectedAsync(pInfected);
        }
    }
}