using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using CommandHandler;

namespace GameOptionsSaverLoader
{
    public class save : Command
    {
        public save() : base()
        {
            commandParsePattern = @"^/save (\w+\.bin)$";
            Help = "/save <filename>.bin";
            HostOnly = true;
            Enabled = true;
            Name = "/save";
        }

        public async override ValueTask<ValidateResult> Handle(IPlayerChatEvent chatEvent)
        {
            var match = Regex.Match(chatEvent.Message, commandParsePattern);

            if (!match.Groups[1].Success)
            {
                return ValidateResult.MissingTarget;
            }

            var target = match.Groups[1].Value;

            using (BinaryWriter configWriter = new BinaryWriter(File.Open($"{target}", FileMode.Create)))
            {
                chatEvent.Game.Options.Serialize(configWriter, GameOptionsData.LatestVersion);
            }
            if (!File.Exists($"{target}"))
            {
                await ServerMessage(chatEvent.PlayerControl, $"Failed to save game config: {target}");
                return ValidateResult.CommandError;
            }

            return ValidateResult.Valid;
        }
    }

    public class load : Command
    {
        public load() : base()
        {
            commandParsePattern = @"^/load (\w+\.bin)$";
            Help = "/load <filename>";
            HostOnly = true;
            Enabled = true;
            Name = "/load";
        }

        public async override ValueTask<ValidateResult> Handle(IPlayerChatEvent chatEvent)
        {
            var match = Regex.Match(chatEvent.Message, commandParsePattern);

            if (!match.Groups[1].Success)
            {
                return ValidateResult.MissingTarget;
            }

            var target = match.Groups[2].Value;

            if (File.Exists($"{target}"))
            {
                byte[] gameOptions = File.ReadAllBytes(target);
                var memory = new ReadOnlyMemory<byte>(gameOptions);
                chatEvent.Game.Options.Deserialize(memory);
                await chatEvent.Game.SyncSettingsAsync();
            }
            else
            {
                await ServerMessage(chatEvent.PlayerControl, $"Failed to load game config: {target}.bin");
                return ValidateResult.CommandError;
            }

            return ValidateResult.Valid;
        }
    }
}