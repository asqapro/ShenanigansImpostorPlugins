using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Microsoft.Extensions.Logging;
using RolesManager;
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
        private Manager man;
        private CrewRoles crew;

        public GameEventListener(ILogger<SpecialRoles> logger)
        {
            _logger = logger;
            man = new Manager();
            crew = new CrewRoles(man);

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
                    _logger.LogInformation($"- {info.PlayerName} is a crewmate.");
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

            man.HandleChat(e);
        }
    }
}