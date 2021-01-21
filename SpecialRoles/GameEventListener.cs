using System;
using System.IO;
using System.Collections.Generic;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Games;
using Impostor.Api.Innersloth;
using Microsoft.Extensions.Logging;
using Managers.Roles;
using Roles;

namespace Impostor.Plugins.SpecialRoles.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<SpecialRoles> _logger;
        private Dictionary<GameCode, IRolesManager> _manager;
        private Dictionary<GameCode, MemoryStream> preEditOptionsStream;
        private BinaryWriter preEditOptionsWriter;

        public GameEventListener(ILogger<SpecialRoles> logger)
        {
            _logger = logger;
            _manager = new Dictionary<GameCode, IRolesManager>();
            preEditOptionsStream = new Dictionary<GameCode, MemoryStream>();
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

            preEditOptionsStream[e.Game.Code] = new MemoryStream();
            preEditOptionsWriter = new BinaryWriter(preEditOptionsStream[e.Game.Code]);
            e.Game.Options.Serialize(preEditOptionsWriter, GameOptionsData.LatestVersion);

            _manager[e.Game.Code] = new RolesManager();

            List<RoleTypes> crewNeutralRoles = new List<RoleTypes> 
            {
                //RoleTypes.Crew,
                RoleTypes.Medium,
                /*RoleTypes.Sheriff,
                RoleTypes.Jester,
                RoleTypes.Deputy,
                RoleTypes.InsaneDeputy,
                RoleTypes.ConfusedDeputy,
                RoleTypes.Oracle,
                RoleTypes.Lightkeeper,
                RoleTypes.Doctor*/
            };

            List<RoleTypes> impostorRoles = new List<RoleTypes> 
            {
                RoleTypes.Impostor,
                RoleTypes.Hitman,
                RoleTypes.VoodooLady,
                RoleTypes.Arsonist
            };

            Random rng = new Random();
            foreach (var player in e.Game.Players)
            {
                var playerInfo = player.Character.PlayerInfo;
                if (playerInfo.IsImpostor)
                {
                    RoleTypes randImpostorRole = impostorRoles[rng.Next(0, impostorRoles.Count)];
                    _manager[e.Game.Code].RegisterRole(player.Client.Player, randImpostorRole);
                    _logger.LogInformation($"{playerInfo.PlayerName} is a {randImpostorRole.ToString()}");
                }
                else
                {
                    RoleTypes randCrewRole = crewNeutralRoles[rng.Next(0, crewNeutralRoles.Count)];
                    _manager[e.Game.Code].RegisterRole(player.Client.Player, randCrewRole);
                    _logger.LogInformation($"{playerInfo.PlayerName} is a {randCrewRole.ToString()}");
                }
            }
        }

        [EventListener]
        public void OnGameEnded(IGameEndedEvent e)
        {
            _logger.LogInformation($"Game has ended.");
            _manager.Remove(e.Game.Code);
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
            if (_manager.ContainsKey(e.Game.Code))
            {
                _manager[e.Game.Code].HandleEvent(e);
            }
        }

        [EventListener]
        public void OnPlayerExile(IPlayerExileEvent e)
        {
            _logger.LogInformation($"{e.PlayerControl.PlayerInfo.PlayerName} was exiled");
            
            if (_manager.ContainsKey(e.Game.Code))
            {
                _manager[e.Game.Code].HandleEvent(e);
            }
        }

        [EventListener]
        public void OnPlayerVoted(IPlayerVotedEvent e)
        {
            if (_manager.ContainsKey(e.Game.Code))
            {
                _manager[e.Game.Code].HandleEvent(e);
            }
        }

        [EventListener]
        public void OnMeetingStarted(IMeetingStartedEvent e)
        {
            if (_manager.ContainsKey(e.Game.Code))
            {
                _manager[e.Game.Code].HandleEvent(e);
            }
        }

        [EventListener]
        public void OnMeetingEnded(IMeetingEndedEvent e)
        {
            if (_manager.ContainsKey(e.Game.Code))
            {
                _manager[e.Game.Code].HandleEvent(e);
            }
        }

        [EventListener]
        public void OnPlayerMurder(IPlayerMurderEvent e)
        {
            if (_manager.ContainsKey(e.Game.Code))
            {
                _manager[e.Game.Code].HandleEvent(e);
            }
        }

        [EventListener]
        public void OnPlayerMovement(IPlayerMovementEvent e)
        {
            if (_manager.ContainsKey(e.Game.Code))
            {
                _manager[e.Game.Code].HandleEvent(e);
            }
        }
    }
}