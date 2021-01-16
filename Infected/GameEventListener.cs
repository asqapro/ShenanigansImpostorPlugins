using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
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
        private Dictionary<GameCode, MemoryStream> preEditOptionsStream;
        private BinaryWriter preEditOptionsWriter;

        public GameEventListener(ILogger<Infected> logger)
        {
            _logger = logger;
            infected = new Dictionary<GameCode, HashSet<IClientPlayer>>();
            preEditOptionsStream = new Dictionary<GameCode, MemoryStream>();
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
        public void OnPlayerSpawned(IPlayerSpawnedEvent e)
        {
            if (e.ClientPlayer.IsHost && preEditOptionsStream.ContainsKey(e.Game.Code))
            {
                byte[] gameOptions = preEditOptionsStream[e.Game.Code].GetBuffer();
                var memory = new ReadOnlyMemory<byte>(gameOptions);

                e.Game.Options.Deserialize(memory);
                e.Game.SyncSettingsAsync();
            }
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            _logger.LogInformation($"Game is starting.");

            infected[e.Game.Code] = new HashSet<IClientPlayer>();

            preEditOptionsStream[e.Game.Code] = new MemoryStream();
            preEditOptionsWriter = new BinaryWriter(preEditOptionsStream[e.Game.Code]);
            e.Game.Options.Serialize(preEditOptionsWriter, GameOptionsData.LatestVersion);

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
        public async ValueTask OnGameEnded(IGameEndedEvent e)
        {
            _logger.LogInformation($"Game has ended.");
            foreach (var player in e.Game.Players)
            {
                IClientPlayer origPlayerInfo = null;
                infected[e.Game.Code].TryGetValue(player, out origPlayerInfo);
                if (origPlayerInfo != null)
                {
                    await player.Character.SetNameAsync(origPlayerInfo.Character.PlayerInfo.PlayerName);
                    await player.Character.SetHatAsync(origPlayerInfo.Character.PlayerInfo.HatId);
                    await player.Character.SetColorAsync(origPlayerInfo.Character.PlayerInfo.ColorId);
                    await player.Character.SetSkinAsync(origPlayerInfo.Character.PlayerInfo.SkinId);
                }
            }
            infected.Remove(e.Game.Code);
        }

        [EventListener]
        public void OnGameDestroyed(IGameDestroyedEvent e)
        {
            preEditOptionsStream.Remove(e.Game.Code);
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
            if (e.PlayerControl == null)
            {
                return;
            }
            if (!infected.ContainsKey(e.Game.Code))
            {
                return;
            }
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
                    if (!infected[e.Game.Code].Contains(e.ClientPlayer) && infected[e.Game.Code].Contains(player.Client.Player))
                    {
                        setInfected(e.ClientPlayer, e.Game);
                    }
                    if (!infected[e.Game.Code].Contains(player) && infected[e.Game.Code].Contains(e.ClientPlayer))
                    {
                        setInfected(player.Client.Player, e.Game);
                    }
                }
            }
        }
    }
}