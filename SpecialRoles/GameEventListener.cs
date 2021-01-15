using System;
using System.Collections.Generic;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Games;
using Microsoft.Extensions.Logging;
using Managers.Roles;
using Roles;

namespace Impostor.Plugins.SpecialRoles.Handlers
{
    /// <summary>
    ///     A class that listens for two events.
    ///     It may be more but this is just an example.
    ///
    ///     Make sure your class implements <see cref="IEventListener"/>.
    /// </summary>
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<SpecialRoles> _logger;
        private Dictionary<GameCode, IRolesManager> _manager;

        public GameEventListener(ILogger<SpecialRoles> logger)
        {
            _logger = logger;
            _manager = new Dictionary<GameCode, IRolesManager>();
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

            _manager[e.Game.Code] = new RolesManager();

            // This prints out for all players if they are impostor or crewmate.
            foreach (var player in e.Game.Players)
            {
                var info = player.Character.PlayerInfo;
                var isImpostor = info.IsImpostor;
                Random rng = new Random();
                if (isImpostor)
                {
                    switch (rng.Next(1, 3))
                    {
                        case 1:
                            _manager[e.Game.Code].RegisterRole(player.Character, RoleTypes.Hitman);
                            _logger.LogInformation($"{info.PlayerName} is a hitman");
                            break;
                        case 2:
                            _manager[e.Game.Code].RegisterRole(player.Character, RoleTypes.VoodooLady);
                            _logger.LogInformation($"{info.PlayerName} is a voodoo lady");
                            break;
                        default:
                            _logger.LogInformation($"- {info.PlayerName} is an impostor.");
                            break;
                    }
                }
                else
                {
                    switch (rng.Next(1, 4))
                    {
                        case 1:
                        {
                            _manager[e.Game.Code].RegisterRole(player.Character, RoleTypes.Medium);
                            _logger.LogInformation($"{info.PlayerName} is a medium");
                            break;
                        }
                        case 2:
                            _manager[e.Game.Code].RegisterRole(player.Character, RoleTypes.Sheriff);
                            _logger.LogInformation($"{info.PlayerName} is a sheriff");
                            break;
                        case 3:
                            _manager[e.Game.Code].RegisterRole(player.Character, RoleTypes.Jester);
                            _logger.LogInformation($"{info.PlayerName} is a jester");
                            break;
                        default:
                            _logger.LogInformation($"{info.PlayerName} is a crewmate");
                            break;
                    }
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
        public void OnPlayerChat(IPlayerChatEvent e)
        {
            _logger.LogInformation($"{e.PlayerControl.PlayerInfo.PlayerName} said {e.Message}");

            _manager[e.Game.Code].HandleEvent(e);
        }

        [EventListener]
        public void OnPlayerExile(IPlayerExileEvent e)
        {
            _logger.LogInformation($"{e.PlayerControl.PlayerInfo.PlayerName} was exiled");
            
            _manager[e.Game.Code].HandleEvent(e);
        }

        [EventListener]
        public void OnPlayerVoted(IPlayerVotedEvent e)
        {
            _manager[e.Game.Code].HandleEvent(e);
        }

        [EventListener]
        public void OnMeetingEnded(IMeetingEndedEvent e)
        {
            _manager[e.Game.Code].HandleEvent(e);
        }
    }
}