using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Events.Player;
using CommandHandler;

namespace GameOptionsSaverLoader
{
    public class save : Command
    {
        public save() : base()
        {
        }

        public override void register()
        {
            HasTarget = true;
            HasOptions = false;
            Help = "/save <filename>";
            HostOnly = true;
            Enabled = true;
            Name = "/save";
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
        public load() : base()
        {
        }

        public override void register()
        {
            HasTarget = true;
            HasOptions = false;
            Help = "/load <filename>";
            HostOnly = true;
            Enabled = true;
            Name = "/load";
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

    public class GameOptionsSaverLoaderHandler
    {
        private CommandManager manager;
        public GameOptionsSaverLoaderHandler(ICommandManager manager)
        {
            var saveommand = new save();
            var loadCommand = new load();
            manager.RegisterCommand(saveommand);
            manager.RegisterCommand(loadCommand);
        }
    }
}