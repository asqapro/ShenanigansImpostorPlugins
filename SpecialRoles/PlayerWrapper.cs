#nullable enable
using System.Threading.Tasks;
using Impostor.Api.Innersloth.Customization;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Net.Inner.Objects.Components;

namespace PlayerWrapper
{
    /*public class InnerPlayerControlDecorator : IInnerPlayerControl
    {
        public InnerPlayerControlDecorator(IInnerPlayerControl inner)
        {
            _inner = inner;
            Physics = inner.Physics;
            NetworkTransform = inner.NetworkTransform;
            PlayerInfo = inner.PlayerInfo;
        }

        public IInnerPlayerControl _inner;

        public bool ExiledByVote { get; }

        public byte PlayerId { get; }

        public IInnerPlayerPhysics Physics { get; }

        public IInnerCustomNetworkTransform NetworkTransform { get; }

        public IInnerPlayerInfo PlayerInfo { get; }

        public uint NetId { get; }

        public int OwnerId { get; }

        public ValueTask SetNameAsync(string name)
        {
            return _inner.SetNameAsync(name);
        }

        public ValueTask SetColorAsync(byte colorId)
        {
            return _inner.SetColorAsync(colorId);
        }

        public ValueTask SetColorAsync(ColorType colorType)
        {
            return _inner.SetColorAsync(colorType);
        }

        public ValueTask SetHatAsync(uint hatId)
        {
            return _inner.SetHatAsync(hatId);
        }

        public ValueTask SetHatAsync(HatType hatType)
        {
            return _inner.SetHatAsync(hatType);
        }

        public ValueTask SetPetAsync(uint petId)
        {
            return _inner.SetPetAsync(petId);
        }

        public ValueTask SetPetAsync(PetType petType)
        {
            return _inner.SetPetAsync(petType);
        }

        public ValueTask SetSkinAsync(uint skinId)
        {
            return _inner.SetSkinAsync(skinId);
        }

        public ValueTask SetSkinAsync(SkinType skinType)
        {
            return _inner.SetSkinAsync(skinType);
        }

        public ValueTask SendChatAsync(string text)
        {
            return _inner.SendChatAsync(text);
        }

        public ValueTask SendChatToPlayerAsync(string text, IInnerPlayerControl? player = null)
        {
            return _inner.SendChatToPlayerAsync(text, player);
        }

        public ValueTask SetMurderedByAsync(IClientPlayer impostor)
        {
            return _inner.SetMurderedByAsync(impostor);
        }

        public ValueTask SetExiledAsync()
        {
            return _inner.SetExiledAsync();
        }
    }*/
}