using System;
using System.Numerics;
using System.Collections.Generic;
using Impostor.Api.Games;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Net;
using Impostor.Api.Innersloth;
using Microsoft.Extensions.Logging;


namespace Impostor.Plugins.Infected.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<Infected> _logger;
        private Dictionary<GameCode, HashSet<IClientPlayer>> infected;
        private GameOptionsData preEditOptions;

        public GameEventListener(ILogger<Infected> logger)
        {
            _logger = logger;
            infected = new Dictionary<GameCode, HashSet<IClientPlayer>>();
        }

        private void setInfected(IClientPlayer toInfect, IGame game)
        {
            toInfect.Character.SetNameAsync("Infected");
            toInfect.Character.SetHatAsync(84);
            toInfect.Character.SetColorAsync(2);
            toInfect.Character.SetSkinAsync(0f);
            infected[game.Code].Add(toInfect);

            var moddedOptions = game;
            moddedOptions.Options.CrewLightMod = 0.25f;
            moddedOptions.Options.ImpostorLightMod = 0.25f;
            moddedOptions.Options.PlayerSpeedMod = 1.25f;
            moddedOptions.SyncSettingsToAsync(toInfect);
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            _logger.LogInformation($"Game is starting.");

            infected[e.Game.Code] = new HashSet<IClientPlayer>();

            preEditOptions = e.Game.Options;

            e.Game.Options.ImpostorLightMod = 1.0f;
            e.Game.Options.CrewLightMod = 1.0f;
            e.Game.Options.KillCooldown = 1000;
            e.Game.Options.PlayerSpeedMod = 2.0f;
            e.Game.SyncSettingsAsync();

            var rng = new Random();
            var initialInfectedPlayer = rng.Next(0, e.Game.PlayerCount);
            var playerIdx = 0;
            foreach (var player in e.Game.Players)
            {
                if (playerIdx == initialInfectedPlayer)
                {
                    setInfected(player.Client.Player, e.Game);
                    break;
                }
                playerIdx++;
            }
        }

        [EventListener]
        public void OnGameEnded(IGameEndedEvent e)
        {
            _logger.LogInformation($"Game has ended.");
            foreach (var player in e.Game.Players)
            {
                IClientPlayer origPlayerInfo = null;
                infected[e.Game.Code].TryGetValue(player, out origPlayerInfo);
                if (origPlayerInfo != null)
                {
                    player.Character.SetNameAsync(origPlayerInfo.Character.PlayerInfo.PlayerName);
                    player.Character.SetHatAsync(origPlayerInfo.Character.PlayerInfo.HatId);
                    player.Character.SetColorAsync(origPlayerInfo.Character.PlayerInfo.ColorId);
                    player.Character.SetSkinAsync(origPlayerInfo.Character.PlayerInfo.SkinId);
                }
            }
            e.Game.Options.ImpostorLightMod = preEditOptions.ImpostorLightMod;
            e.Game.Options.CrewLightMod = preEditOptions.CrewLightMod;
            e.Game.Options.KillCooldown = preEditOptions.KillCooldown;
            e.Game.Options.PlayerSpeedMod = preEditOptions.PlayerSpeedMod;
            e.Game.SyncSettingsAsync();
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
                if (player == null)
                {
                    continue;
                }
                if (player.Character.PlayerId == e.PlayerControl.PlayerId)
                {
                    continue;
                }
                var distance = Vector2.Distance(e.PlayerControl.NetworkTransform.Position, player.Character.NetworkTransform.Position);
                if (distance < 0.6)
                {
                    if (!infected[e.Game.Code].Contains(e.ClientPlayer))
                    {
                        setInfected(e.ClientPlayer, e.Game);
                    }
                    if (!infected[e.Game.Code].Contains(player))
                    {
                        setInfected(player.Client.Player, e.Game);
                    }
                }
            }
        }
    }
}