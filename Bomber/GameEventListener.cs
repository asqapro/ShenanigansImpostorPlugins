using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Games;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;

namespace Impostor.Plugins.Bomber.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<Bomber> _logger;

        Dictionary<Api.Net.IClientPlayer, Timer> bombMap;

        public GameEventListener(ILogger<Bomber> logger)
        {
            _logger = logger;
            bombMap = new Dictionary<Api.Net.IClientPlayer, Timer>();
        }

        private void bombGoBoom(Object source, System.Timers.ElapsedEventArgs e)
        {
            Timer timer = (Timer) source;
            foreach(var x in bombMap)
            {
                if(x.Value.Equals(timer))
                {
                    x.Key.Character.SetExiledAsync();
                }
            }  
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            //modify this later once we can set plugin options via chat
            Random r = new Random();
            var player = e.Game.Players.ElementAt(r.Next(0, e.Game.PlayerCount-1));

            while(player.Character.PlayerInfo.IsImpostor)
            {
                player = e.Game.Players.ElementAt(r.Next(0, e.Game.PlayerCount-1));
            }
            
            //start timers for 25 seconds
            bombMap.Add(player, new Timer());
            bombMap.GetValueOrDefault(player).Elapsed += bombGoBoom;
            bombMap.GetValueOrDefault(player).Interval = 25000;
            bombMap.GetValueOrDefault(player).Start();
        }

        public void OnTaskComplete(IPlayerCompletedTaskEvent e)
        {
            if(bombMap.ContainsKey(e.ClientPlayer))
            {
                bombMap.GetValueOrDefault(e.ClientPlayer).Stop();

                foreach(var task in e.ClientPlayer.Character.PlayerInfo.Tasks)
                {
                    if(!task.Complete)
                    {
                        //set timer to 20 seconds and restart
                        bombMap.GetValueOrDefault(e.ClientPlayer).Interval = 20000;
                        bombMap.GetValueOrDefault(e.ClientPlayer).Start();
                        return;
                    }
                }
            }
        }

        
        [EventListener]
        public void OnPlayerChat(IPlayerChatEvent e)
        {
            if(e.Game.GameState == GameStates.NotStarted)
            {
                //call command parser

                //configure number of bombers
            }
        }

        bool validateCommand(string[] commandParts)
        {
            if(commandParts.Length == 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async ValueTask ServerMessage(IInnerPlayerControl sender, IInnerPlayerControl receiver, String message)
        {
            var currentColor = sender.PlayerInfo.ColorId;
            var currentName = sender.PlayerInfo.PlayerName;

            await sender.SetColorAsync(Impostor.Api.Innersloth.Customization.ColorType.White);
            await sender.SetNameAsync("Server");

            await sender.SendChatToPlayerAsync(message, receiver);

            await sender.SetColorAsync(currentColor);
            await sender.SetNameAsync(currentName);
        }

        public static Boolean isAlphaNumeric(string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9\.]*$");
            return rg.IsMatch(strToCheck);
        }
    }
}