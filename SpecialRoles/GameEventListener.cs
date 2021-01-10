using System;
using System.Collections.Generic;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Microsoft.Extensions.Logging;
using RolesManager;
using Roles;
using Roles.Crew;

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
        private IManager _manager;

        public GameEventListener(ILogger<SpecialRoles> logger)
        {
            _logger = logger;
            _manager = new Manager();
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

            HashSet<String> playerRoles = new HashSet<String>();
            Random rnd = new Random();

            // This prints out for all players if they are impostor or crewmate.
            foreach (var player in e.Game.Players)
            {
                var info = player.Character.PlayerInfo;
                var isImpostor = info.IsImpostor;
                if (isImpostor)
                {
                    _logger.LogInformation($"- {info.PlayerName} is an impostor.");
                }
                else
                {
                    if (!playerRoles.Contains(player.Character.PlayerInfo.PlayerName) && playerRoles.Count < 7)
                    {
                        if (rnd.Next(0, 1) == 1)
                        {
                            Medium med = new Medium(player.Character);
                            _manager.RegisterRole(e.Game.Code, med);
                            _logger.LogInformation($"{info.PlayerName} is a medium");
                        }
                        else
                        {
                            Sheriff sher = new Sheriff(player.Character);
                            _manager.RegisterRole(e.Game.Code, sher);
                            _logger.LogInformation($"{info.PlayerName} is a sheriff");
                        }
                        playerRoles.Add(player.Character.PlayerInfo.PlayerName);
                    }
                    _logger.LogInformation($"- {info.PlayerName} is a crewmate.");
                }
            }
        }

        [EventListener]
        public void OnGameEnded(IGameEndedEvent e)
        {
            _logger.LogInformation($"Game has ended.");

            _manager.CleanUpRoles(e.Game.Code);
        }

        [EventListener]
        public void OnPlayerChat(IPlayerChatEvent e)
        {
            _logger.LogInformation($"{e.PlayerControl.PlayerInfo.PlayerName} said {e.Message}");

            _manager.HandleChat(e.Game.Code, e);
        }
    }
}