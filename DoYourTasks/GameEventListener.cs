using System;
using System.IO;
using System.Collections.Generic;
using Impostor.Api.Games;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Microsoft.Extensions.Logging;

namespace Impostor.Plugins.DoYourTasks.Handlers
{
    /// <summary>
    ///     A class that listens for two events.
    ///     It may be more but this is just an example.
    ///
    ///     Make sure your class implements <see cref="IEventListener"/>.
    /// </summary>
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<DoYourTasks> _logger;
        private Dictionary<GameCode, MemoryStream> preEditOptionsStream;
        private BinaryWriter preEditOptionsWriter;

        public GameEventListener(ILogger<DoYourTasks> logger)
        {
            _logger = logger;
            preEditOptionsStream = new Dictionary<GameCode, MemoryStream>();
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

            preEditOptionsStream[e.Game.Code] = new MemoryStream();
            preEditOptionsWriter = new BinaryWriter(preEditOptionsStream[e.Game.Code]);
            e.Game.Options.Serialize(preEditOptionsWriter, GameOptionsData.LatestVersion);

            e.Game.Options.CrewLightMod = 0.25f;
            e.Game.SyncSettingsAsync();
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
        public void OnGameDestroyed(IGameDestroyedEvent e)
        {
            preEditOptionsStream.Remove(e.Game.Code);
        }

        [EventListener]
        public void OnPlayerCompletedTask(IPlayerCompletedTaskEvent e)
        {
            e.Game.Options.CrewLightMod += 0.10f;
            e.Game.SyncSettingsToAsync(e.ClientPlayer);
        }
    }
}