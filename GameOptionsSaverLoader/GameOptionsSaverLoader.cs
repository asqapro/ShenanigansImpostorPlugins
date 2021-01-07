using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CommandHandler;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Events.Player;

namespace Impostor.Plugins.GameOptionsSaverLoader
{
    public class save : Command
    {
        public save(String _name, bool _hastarget, bool _hasoptions, String _help, bool _hostonly, bool _enabled) : base(_name, _hastarget, _hasoptions, _help, _hostonly, _enabled)
        {
        }

        public override ValueTask<String> handle(IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent)
        {
            String success = "";
            Regex rg = new Regex(@"^\w+$");
            if (!rg.IsMatch(parsedCommand.Target))
            {
                return ValueTask.FromResult($"Invalid filename");
            }
            
            using (BinaryWriter configWriter = new BinaryWriter(File.Open($"{parsedCommand.Target}.bin", FileMode.Create)))
            {
                chatEvent.Game.Options.Serialize(configWriter, 3 /*or maybe e.Game.Version, but the options are 1, 2, or 3*/);
                success = $"Saving game config file: {parsedCommand.Target}.bin";
            }
            if (!File.Exists($"{parsedCommand.Target}.bin"))
            {
                success = $"Failed to save game config: {parsedCommand.Target}.bin";
            }
            return ValueTask.FromResult(success);
        }
    }

    public class load : Command
    {
        public load(String _name, bool _hastarget, bool _hasoptions, String _help, bool _hostonly, bool _enabled) : base(_name, _hastarget, _hasoptions, _help, _hostonly, _enabled)
        {
        }

        public override async ValueTask<String> handle(IInnerPlayerControl sender, ValidatedCommand parsedCommand, IPlayerChatEvent chatEvent)
        {
            Regex rg = new Regex(@"^\w+$");
            if (!rg.IsMatch(parsedCommand.Target))
            {
                return $"Invalid filename";
            }

            if (File.Exists($"{parsedCommand.Target}.bin"))
            {
                byte[] gameOptions = File.ReadAllBytes(parsedCommand.Target);
                var memory = new ReadOnlyMemory<byte>(gameOptions);
                chatEvent.Game.Options.Deserialize(memory);
                await chatEvent.Game.SyncSettingsAsync();
                return $"Successfully loaded game config file: {parsedCommand.Target}.bin";
            }
            else
            {
                return $"Failed to load game config: {parsedCommand.Target}.bin";
            }
        }
    }

    public class handler
    {
        private CommandManager manager = CommandManager.Instance;
        public handler()
        {
            var saveommand = new save("/save", true, true, "/save <filename>", true, true);
            var loadCommand = new load("/load", true, false, "/load <filename>", true, true);
            manager.RegisterManager(saveommand);
            manager.RegisterManager(loadCommand);
        }
    }
}