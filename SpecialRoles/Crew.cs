using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Inner.Objects;
using Microsoft.Extensions.Logging;
using RolesManager;

namespace Roles.Crew
{
    public class Medium : Role
    {
        public Medium(IInnerPlayerControl player) : base(player)
        {
            _listeners.Add(ListenerTypes.OnChat);
        }

        public override RoleTypes GetRoleType()
        {
            return RoleTypes.Medium;
        }

        public override ICollection<ListenerTypes> GetListenerTypes()
        {
            return _listeners;
        }

        public override int GetTotalAllowed()
        {
            return 2;
        }

        private async ValueTask hearDead(IInnerPlayerControl deadSender, IInnerPlayerControl mediumReceiver, String message)
        {
            var currentName = deadSender.PlayerInfo.PlayerName;
            await deadSender.SetNameAsync($"{currentName} (dead)");
            await deadSender.SendChatToPlayerAsync($"[0000ffff]{message}[]", mediumReceiver);
            await deadSender.SetNameAsync(currentName);
        }

        public override async void HandleChat(IPlayerChatEvent e)
        {
            if (e.PlayerControl.PlayerInfo.IsDead)
            {
                await hearDead(e.PlayerControl, _player, e.Message);
            }
        }
    }


    public class CrewRoles
    {
        Manager _manager;
        public CrewRoles(Manager manager)
        {
            _manager = manager;
        }

        public void RegisterRoles(IInnerPlayerControl player)
        {
            Medium med = new Medium(player);

            _manager.RegisterRole(med);
        }
    }
}