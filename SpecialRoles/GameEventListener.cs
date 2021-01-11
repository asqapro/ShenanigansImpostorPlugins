using System;
using System.Collections.Generic;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Microsoft.Extensions.Logging;
using RolesManager;
using Roles;
using Roles.Crew;
using Roles.Neutral;

namespace Impostor.Plugins.SpecialRoles.Handlers
{

    public static class Shuffler
    {
        private static Random rng = new Random();  
        public static void Shuffle<T>(this IList<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }

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

        private ICollection<RoleTypes> calculateSpecialRoles(IGameStartedEvent e)
        {
            IList<RoleTypes> specialRoles = new List<RoleTypes>();

            int playerCount = e.Game.PlayerCount;
            int impostorRoleTotal = e.Game.Options.NumImpostors;

            int percentSpecialRole = 70;
            int specialRoleTotal = ((playerCount * percentSpecialRole - 1) / 100) + 1;

            int crewRoleTotal = specialRoleTotal - impostorRoleTotal;

            int totalSheriff = 0;
            int totalMedium = 0;
            int totalJester = 0;

            Random rng = new Random();  

            for (int iter = 0; iter < crewRoleTotal; iter++)
            {
                if (rng.Next(0, 10) == 1 && totalJester < Jester.TotalAllowed)
                {
                    specialRoles.Add(RoleTypes.Jester);
                }
                else if (totalSheriff < Sheriff.TotalAllowed)
                {
                    specialRoles.Add(RoleTypes.Sheriff);
                    totalSheriff++;
                }
                else if (totalMedium < Medium.TotalAllowed)
                {
                    specialRoles.Add(RoleTypes.Medium);
                    totalMedium++;
                }
            }

            for (int iter = 0; iter < impostorRoleTotal; iter++)
            {
                //add impostor roles
            }

            specialRoles.Shuffle();

            return specialRoles;
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

            HashSet<String> crewRoles = new HashSet<String>();
            HashSet<String> impostorRoles = new HashSet<String>();
            HashSet<String> neutralRoles = new HashSet<String>();

            calculateSpecialRoles(e);

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
                    if (!crewRoles.Contains(player.Character.PlayerInfo.PlayerName) && crewRoles.Count < 7)
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
                        crewRoles.Add(player.Character.PlayerInfo.PlayerName);
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