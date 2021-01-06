using System;
using System.Threading.Tasks;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Impostor.Plugins.GameOptionsSaverLoader.Handlers;
using Microsoft.Extensions.Logging;

namespace Impostor.Plugins.GameOptionsSaverLoader
{
    [ImpostorPlugin(
        package: "gg.impostor.GameOptionsSaverLoader",
        name: "GameOptionsSaverLoad",
        author: "Nitcholas",
        version: "1.0.0")]
    public class GameOptionsSaverLoader : PluginBase
    {
        private readonly ILogger<GameOptionsSaverLoader> _logger;
        private readonly IEventManager _eventManager;
        private IDisposable _unregister;

        public GameOptionsSaverLoader(ILogger<GameOptionsSaverLoader> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public override ValueTask EnableAsync()
        {
            _unregister = _eventManager.RegisterListener(new GameEventListener(_logger));
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _unregister.Dispose();
            return default;
        }
    }
}