using System;
using System.Numerics;
using System.Collections.Generic;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;
using Microsoft.Extensions.Logging;


namespace Impostor.Plugins.Infected.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<Infected> _logger;
        private HashSet<byte> infected;

        public GameEventListener(ILogger<Infected> logger)
        {
            _logger = logger;
            infected = new HashSet<byte>();
        }

        private void setInfected(IInnerPlayerControl toInfect)
        {
            toInfect.SetNameAsync("Infected");
            toInfect.SetHatAsync(84);
            toInfect.SetColorAsync(2);
            toInfect.SetSkinAsync(0f);
            infected.Add(toInfect.PlayerId);
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            _logger.LogInformation($"Game is starting.");
            var rng = new Random();
            var initialInfectedPlayer = rng.Next(0, e.Game.PlayerCount);
            var playerIdx = 0;
            foreach (var player in e.Game.Players)
            {
                if (playerIdx == initialInfectedPlayer)
                {
                    setInfected(player.Character);
                    var newSetting = e.Game;
                    newSetting.Options.CrewLightMod -= 0.50f;
                    newSetting.SyncSettingsAsync(player.Client.Player);
                    break;
                }
                playerIdx++;
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
        public void OnPlayerMovement(IPlayerMovementEvent e)
        {
            foreach (var player in e.Game.Players)
            {
                if (player.Client.Id == e.ClientPlayer.Client.Id)
                {
                    continue;
                }
                var distance = Vector2.Distance(e.PlayerControl.NetworkTransform.Position, player.Character.NetworkTransform.Position);
                if (distance < 0.6 && (infected.Contains(e.PlayerControl.PlayerId) || infected.Contains(player.Character.PlayerId)))
                {
                    if (!infected.Contains(e.PlayerControl.PlayerId))
                    {
                        setInfected(e.PlayerControl);
                        var newSetting = e.Game;
                        newSetting.Options.CrewLightMod -= 0.50f;
                        newSetting.SyncSettingsAsync(e.ClientPlayer);
                    }
                    if (!infected.Contains(player.Character.PlayerId))
                    {
                        setInfected(player.Character);
                        var newSetting = e.Game;
                        newSetting.Options.CrewLightMod -= 0.50f;
                        newSetting.SyncSettingsAsync(player.Client.Player);
                    }
                }
            }
        }
    }
}