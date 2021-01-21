using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Net.Inner.Objects.Components;
using Impostor.Api.Innersloth.Customization;
using Impostor.Api.Innersloth;

#nullable enable

namespace Roles
{
    public enum RoleTypes
    {
        Crew,
        Impostor,
        Medium,
        Sheriff,
        Jester,
        Hitman,
        VoodooLady,
        Deputy,
        InsaneDeputy,
        ConfusedDeputy,
        Oracle,
        Lightkeeper,
        Doctor,
        Arsonist
    }

    public enum ListenerTypes
    {
        OnPlayerChat,
        OnPlayerExile,
        OnPlayerVoted,
        OnMeetingStarted,
        OnMeetingEnded,
        OnPlayerMurder,
        OnPlayerMovement
    }

    public enum ResultTypes
    {
        NoAction,
        KillExile,
        KillMurder,
        Protect,
    }

    public class HandlerAction
    {
        public ResultTypes Action { get; set; }
        public ICollection<int>? AffectedPlayers { get; set; }
        public HandlerAction(ResultTypes playerAction, List<int>? affectedClient = null)
        {
            Action = playerAction;
            AffectedPlayers = affectedClient;
        }
    }

    public class InnerPlayerControlRole : IInnerPlayerControl
    {
        protected IInnerPlayerControl _player { get; set; }
        public byte PlayerId { get; private set; }
        public uint NetId { get; }
        public int OwnerId { get; }
        IInnerPlayerPhysics IInnerPlayerControl.Physics => Physics;
        IInnerCustomNetworkTransform IInnerPlayerControl.NetworkTransform => NetworkTransform;
        IInnerPlayerInfo IInnerPlayerControl.PlayerInfo => PlayerInfo;
        IInnerPlayerPhysics Physics { get; }
        IInnerCustomNetworkTransform NetworkTransform { get; }
        IInnerPlayerInfo PlayerInfo { get; }
        public HashSet<ListenerTypes> _listeners { get; set; }
        public static int TotalAllowed { get; set; }
        public RoleTypes RoleType { get; set; }
        protected MemoryStream? preEditOptionsStream { get; set; }
        protected BinaryWriter? preEditOptionsWriter {get; set; }
        public bool PreventNextDeath { get; set; }

        public InnerPlayerControlRole(IInnerPlayerControl parent)
        {
            _player = parent;
            _listeners = new HashSet<ListenerTypes>();
            Physics = parent.Physics;
            NetworkTransform = parent.NetworkTransform;
            PlayerInfo = parent.PlayerInfo;
            PreventNextDeath = false;
        }

        public async ValueTask SetNameAsync(string name)
        {
            await _player.SetNameAsync(name);
        }

        public async ValueTask SetColorAsync(byte colorId)
        {
            await _player.SetColorAsync(colorId);
        }

        public async ValueTask SetColorAsync(ColorType colorType)
        {
            await _player.SetColorAsync(colorType);
        }

        public async ValueTask SetHatAsync(uint hatId)
        {
            await _player.SetHatAsync(hatId);
        }

        public async ValueTask SetHatAsync(HatType hatType)
        {
            await _player.SetHatAsync(hatType);
        }

        public async ValueTask SetPetAsync(uint petId)
        {
            await _player.SetPetAsync(petId);
        }

        public async ValueTask SetPetAsync(PetType petType)
        {
            await _player.SetPetAsync(petType);
        }

        public async ValueTask SetSkinAsync(uint skinId)
        {
            await _player.SetSkinAsync(skinId);
        }

        public async ValueTask SetSkinAsync(SkinType skinType)
        {
            await _player.SetSkinAsync(skinType);
        }

        public async ValueTask SendChatAsync(string text)
        {
            await _player.SendChatAsync(text);
        }

        public async ValueTask SendChatToPlayerAsync(string text, IInnerPlayerControl? player = null)
        {
            await _player.SendChatToPlayerAsync(text, player);
        }

        public async ValueTask SetMurderedByAsync(IClientPlayer impostor)
        {
            await _player.SetMurderedByAsync(impostor);
        }

        public async ValueTask SetExiledAsync()
        {
            await _player.SetExiledAsync();
        }

        public async ValueTask SetAllTasksCompleteAsync()
        {
            await _player.SetAllTasksCompleteAsync();
        }

        protected void saveSettings(IGameEvent e)
        {
            preEditOptionsStream = new MemoryStream();
            preEditOptionsWriter = new BinaryWriter(preEditOptionsStream);
            e.Game.Options.Serialize(preEditOptionsWriter, GameOptionsData.LatestVersion);
        }

        protected void loadSettings(IGameEvent e)
        {
            if (preEditOptionsStream == null)
            {
                return;
            }
            byte[] gameOptions = preEditOptionsStream.GetBuffer();
            var memory = new ReadOnlyMemory<byte>(gameOptions);

            e.Game.Options.Deserialize(memory);
            e.Game.SyncSettingsAsync();
        }

        public virtual ValueTask<HandlerAction> HandlePlayerChat(IPlayerChatEvent e)
        {
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        public virtual ValueTask<HandlerAction> HandlePlayerExile(IPlayerExileEvent e)
        {
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        public virtual ValueTask<HandlerAction> HandlePlayerVote(IPlayerVotedEvent e)
        {
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        public virtual ValueTask<HandlerAction> HandleMeetingStart(IMeetingStartedEvent e)
        {
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        public virtual ValueTask<HandlerAction> HandleMeetingEnd(IMeetingEndedEvent e)
        {
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        public virtual ValueTask<HandlerAction> HandlePlayerMurder(IPlayerMurderEvent e)
        {
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }

        public virtual ValueTask<HandlerAction> HandlePlayerMovement(IPlayerMovementEvent e)
        {
            return ValueTask.FromResult(new HandlerAction(ResultTypes.NoAction));
        }
    }
}